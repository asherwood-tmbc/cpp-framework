using System;
using System.Reflection;
using System.Threading;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Services;
using CPP.Framework.Threading;

namespace CPP.Framework
{
    /// <summary>
    /// Abstract base class for objects that provide abstracted wrapper for static system service
    /// classes.
    /// </summary>
    [Obsolete("Please derive from the CodeServiceSingleton class instead.")]
    public abstract class SingletonServiceBase : CodeServiceSingleton
    {
        /// <summary>
        /// Called by the base class to cleanup the current instance prior to it being destroyed.
        /// </summary>
        protected internal override void CleanupInstance() { }

        /// <summary>
        /// Called by the base class to perform any initialization tasks when the instance is being
        /// created.
        /// </summary>
        protected internal override void StartupInstance() { }

        #region ServiceInstance Class Declaration

        /// <summary>
        /// Manages an instance reference for a service class that is registered with the
        /// <see cref="ServiceLocator"/> class.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        [Obsolete("Please use the CodeServiceProvider class instead.")]
        protected sealed class ServiceInstance<TService>
            where TService : SingletonServiceBase
        {
            /// <summary>
            /// The delegate to call when creating instances of the service if automatic resolution
            /// with the <see cref="ServiceLocator"/> fails.
            /// </summary>
            private readonly Func<TService> _factory;

            /// <summary>
            /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
            /// multiple threads.
            /// </summary>
            private readonly MultiAccessLock _syncLock;

            /// <summary>
            /// The current instance of the service.
            /// </summary>
            private TService _instance;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInstance{TService}"/> class. 
            /// </summary>
            public ServiceInstance() : this(null, LockRecursionPolicy.NoRecursion) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInstance{TService}"/> class 
            /// using a creation factory delegate. 
            /// </summary>
            /// <param name="factory">
            /// A custom factory delegate that is used to create an instance of the service class 
            /// directly if the <see cref="ServiceLocator"/> is unable to resolve the service type.
            /// </param>
            public ServiceInstance(Func<TService> factory) : this(factory, LockRecursionPolicy.NoRecursion) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInstance{TService}"/> class. 
            /// </summary>
            /// <param name="recursionPolicy">
            /// The default recursion policy for the lock the protects access to the service object 
            /// reference.
            /// </param>
            public ServiceInstance(LockRecursionPolicy recursionPolicy) : this(null, recursionPolicy) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInstance{TService}"/> class 
            /// using a creation factory delegate. 
            /// </summary>
            /// <param name="factory">
            /// A custom factory delegate that is used to create an instance of the service class 
            /// directly if the <see cref="ServiceLocator"/> is unable to resolve the service type.
            /// </param>
            /// <param name="recursionPolicy">
            /// The default recursion policy for the lock the protects access to the service object 
            /// reference.
            /// </param>
            public ServiceInstance(Func<TService> factory, LockRecursionPolicy recursionPolicy)
            {
                ServiceLocator.Unloaded += (sender, args) =>
                {
                    using (_syncLock.GetWriterAccess())
                    {
                        _instance?.CleanupInstance();
                        _instance = null;
                    }
                };
                _factory = (factory ?? (() =>
                {
                    const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var ctor = typeof(TService).GetConstructor(SearchFlags, null, Type.EmptyTypes, null);
                    var instance = ((ctor == null)
                        ? ServiceLocator.GetInstance<TService>()
                        : ((TService)ctor.Invoke(new object[0])));
                    return instance;
                }));
                _syncLock = new MultiAccessLock(recursionPolicy);
            }

            /// <summary>
            /// Gets a reference to the current instance of the service.
            /// </summary>
            /// <returns>A reference to the service instance.</returns>
            public TService GetInstance()
            {
                using (_syncLock.GetReaderAccess())
                {
                    if (_instance != null) return _instance;
                }
                using (_syncLock.GetWriterAccess())
                {
                    if (_instance == null)
                    {
                        if (_factory == null)
                        {
                            _instance = ServiceLocator.GetInstance<TService>();
                        }
                        else if (!ServiceLocator.TryGetInstance(out _instance))
                        {
                            _instance = _factory();
                        }
                        _instance.StartupInstance();
                    }
                    return _instance;
                }
            }
        }

        #endregion // ServiceInstance Class Declaration
    }
}
