using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CPP.Framework.Services;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// A <see cref="LifetimeManager" /> that behaves similarly to the built-in
    /// <see cref="ContainerControlledLifetimeManager"/> in Unity, with the added behavior of
    /// calling an initialization routine when the instance is assigned, and a tear-down routine
    /// when the <see cref="CodeServiceSingletonLifetimeManager"/> container is disposed. For more
    /// information, please consult the documentation for the <see cref="CodeServiceSingleton"/>
    /// class, or the <see cref="ICodeServiceSingleton"/> interface.
    /// </summary>
    internal sealed class CodeServiceSingletonLifetimeManager : SynchronizedLifetimeManager, IDisposable
    {
        private static readonly ConcurrentDictionary<Type, InstanceReflectionHelper> InstanceHelperMap = new ConcurrentDictionary<Type, InstanceReflectionHelper>();
        private object _value;

        /// <summary>
        /// Invokes the tear down routine for a given service instance.
        /// </summary>
        /// <param name="instance">The service instance to tear down.</param>
        private void CleanupInstance(object instance)
        {
            GetReflectionHelper(instance?.GetType()).CleanupInstance(instance);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        private void Dispose()
        {
            if (_value != null)
            {
                GetReflectionHelper(_value.GetType()).CleanupInstance(_value);
                (_value as IDisposable)?.Dispose();
            }
            _value = null;
        }

        /// <inheritdoc />
        void IDisposable.Dispose() => this.Dispose();

        /// <summary>
        /// Gets the <see cref="InstanceReflectionHelper"/> instance for a given service
        /// implementation class.
        /// </summary>
        /// <param name="instanceType">The service implementation class type.</param>
        /// <returns>An <see cref="InstanceReflectionHelper"/> object.</returns>
        private static InstanceReflectionHelper GetReflectionHelper(Type instanceType)
        {
            var helper = default(InstanceReflectionHelper);
            if (instanceType != null)
            {
                helper = InstanceHelperMap.GetOrAdd(
                    (instanceType),
                    (type) => new InstanceReflectionHelper(type));
            }
            return (helper ?? InstanceReflectionHelper.Empty);
        }

        /// <inheritdoc />
        public override void RemoveValue()
        {
            this.Dispose();
            base.RemoveValue();
        }

        /// <summary>
        /// Invokes the initialization routine for a given service instance.
        /// </summary>
        /// <param name="instance">The service instance to initialize.</param>
        private void StartupInstance(object instance)
        {
            GetReflectionHelper(instance?.GetType()).StartupInstance(instance);
        }

        /// <inheritdoc />
        protected override object SynchronizedGetValue() => _value;

        /// <inheritdoc />
        protected override void SynchronizedSetValue(object newValue)
        {
            var oldValue = _value;
            if (!ReferenceEquals(oldValue, newValue))
            {
                this.CleanupInstance(oldValue);
                _value = null;
                this.StartupInstance(newValue);
            }
            _value = newValue;
        }

        #region InstanceInitializationHelper Class Declaration

        /// <summary>
        /// Helper class used to invoke the initialization and tear down routines for instances of
        /// a service implementation class.
        /// </summary>
        private sealed class InstanceReflectionHelper
        {
            private const BindingFlags SearchFlags = (BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            /// <summary>
            /// Prevents a default instance of the <see cref="InstanceReflectionHelper"/> class
            /// from being created.
            /// </summary>
            private InstanceReflectionHelper() => this.CleanupInstanceAction = this.StartupInstanceAction = ((o) => { });

            /// <summary>
            /// Gets the <see cref="Action{T1}"/> delegate used to invoke a service instance's
            /// tear down routine.
            /// </summary>
            private Action<object> CleanupInstanceAction { get; }

            /// <summary>
            /// Gets the <see cref="Action{T1}"/> delegate used to invoke a service instance's
            /// initialization routine.
            /// </summary>
            private Action<object> StartupInstanceAction { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="InstanceReflectionHelper"/> class.
            /// </summary>
            /// <param name="instanceType">The type of the service implementation class.</param>
            internal InstanceReflectionHelper(Type instanceType)
            {
                if (typeof(ICodeServiceSingleton).IsAssignableFrom(instanceType))
                {
                    var ti = instanceType;
                    while ((ti != null) && typeof(ICodeServiceSingleton).IsAssignableFrom(ti))
                    {
                        foreach (var mi in ti.GetMethods(SearchFlags).Where((mi) => (!mi.IsAbstract)))
                        {
                            if (mi.IsGenericMethod && mi.ContainsGenericParameters) continue;
                            if (mi.GetParameters().Any()) continue;
                            if (mi.HasCustomAttribute<SingletonInstanceStartupMethodAttribute>(true))
                            {
                                StartupInstanceAction = (StartupInstanceAction ?? GenerateDelegate(ti, mi));
                                continue;
                            }
                            if (mi.HasCustomAttribute<SingletonInstanceCleanupMethodAttribute>(true))
                            {
                                CleanupInstanceAction = (CleanupInstanceAction ?? GenerateDelegate(ti, mi));
                                continue;
                            }
                        }
                        if ((StartupInstanceAction != null) && (CleanupInstanceAction != null))
                        {
                            break;
                        }
                        ti = ti.BaseType;
                    }
                }
                StartupInstanceAction = (StartupInstanceAction ?? ((o) => { }));
                CleanupInstanceAction = (CleanupInstanceAction ?? ((o) => { }));
            }

            /// <summary>
            /// Gets a <see cref="InstanceReflectionHelper"/> instance that always does a no-op for
            /// the initialization and tear down routines (mostly for null service references).
            /// </summary>
            internal static InstanceReflectionHelper Empty { get; } = new InstanceReflectionHelper();

            /// <summary>
            /// Generates an <see cref="Action{T1}"/> delegate for an initialization or tear down
            /// method.
            /// </summary>
            /// <param name="ti">
            /// The <see cref="Type"/> for the class that has declared <paramref name="mi"/>.
            /// </param>
            /// <param name="mi">The <see cref="MethodInfo"/> for the target method.</param>
            /// <returns>An <see cref="Action{T1}"/> delegate instance.</returns>
            private static Action<object> GenerateDelegate(Type ti, MethodInfo mi)
            {
                Contract.Assert(mi.DeclaringType != null);  // needed to make the code analyzers happy (it's never null in practice)
                var args = Expression.Parameter(typeof(object), "instance");
                var conv = Expression.Convert(args, ti);
                var call = Expression.Call(conv, mi);
                return Expression.Lambda<Action<object>>(call, args).Compile();
            }

            /// <summary>
            /// Invokes the tear down routine for a service instance.
            /// </summary>
            /// <param name="instance">The service instance.</param>
            internal void CleanupInstance(object instance)
            {
                if (instance != null) this.CleanupInstanceAction(instance);
            }

            /// <summary>
            /// Invokes the initialization routine for a service instance.
            /// </summary>
            /// <param name="instance">The service instance.</param>
            internal void StartupInstance(object instance)
            {
                if (instance != null) this.StartupInstanceAction(instance);
            }
        }

        #endregion
    }
}
