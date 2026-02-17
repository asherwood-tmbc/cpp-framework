using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Rhino.Mocks
{
    /// <summary>
    /// Extension methods for the <see cref="MethodInvocation"/> class.
    /// </summary>
    public static class MethodInvocationExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, Delegate> _BaseMethodMap = new ConcurrentDictionary<MethodInfo, Delegate>();

        /// <summary>
        /// Calls the original base method of the currently stubbed action.
        /// </summary>
        /// <typeparam name="TInstance">The type of the stubbed object.</typeparam>
        /// <param name="methodInvocation">
        /// The <see cref="MethodInvocation"/> object that contains the method and argument
        /// information that was passed to the stub.
        /// </param>
        /// <param name="instance">The instance being invoked.</param>
        public static void CallOriginalMethod<TInstance>(this MethodInvocation methodInvocation, TInstance instance)
        {
            var methodInfo = methodInvocation.Method;
            var baseMethod = methodInfo.GetBaseDefinition();
            if (baseMethod.ContainsGenericParameters)
            {
                baseMethod = baseMethod.MakeGenericMethod(methodInfo.GetGenericArguments());
            }
            var basePointr = baseMethod.MethodHandle.GetFunctionPointer();

            var paramTypes = baseMethod.GetParameters()
                .Select(pi => pi.ParameterType)
                .ToArray();
            var actionType = ((baseMethod.ReturnType == typeof(void))
                ? Type.GetType($"System.Action`{paramTypes.Length}")
                : Type.GetType($"System.Func`{paramTypes.Length + 1}"));
            Contract.Assert(actionType != null);
            actionType = actionType.MakeGenericType(paramTypes);

            // This method uses the following Delegate class constructor to create a
            // delegate that directly calls the function pointer of the base class,
            // instead of calling the derived class's override.
            //
            //    public Delegate(object target, IntPtr ftn)
            ////
            var callback = (Delegate)Activator.CreateInstance(actionType, instance, basePointr);
            methodInvocation.ReturnValue = callback.DynamicInvoke(methodInvocation.Arguments);
        }
    }
}
