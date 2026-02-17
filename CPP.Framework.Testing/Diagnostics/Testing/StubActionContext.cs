using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Helper class used to wrap a service interface while one of its actions are being stubbed.
    /// </summary>
    /// <typeparam name="TService">The type of the service interface.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class StubActionContext<TService> where TService : class
    {
        private readonly Action<TService> _Action;
        private readonly TService _Service;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="service">The service whose action is being stubbed.</param>
        /// <param name="action">An expession that evaluates to the method being stubbed.</param>
        internal StubActionContext(TService service, Action<TService> action)
        {
            ArgumentValidator.ValidateNotNull(() => service);
            ArgumentValidator.ValidateNotNull(() => action);
            _Action = action;
            _Service = service;
        }

        /// <summary>
        /// Instructs the stub to call the original method implementation.
        /// </summary>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService CallOriginalMethod() { return this.CallOriginalMethod(OriginalCallOptions.NoExpectation); }

        /// <summary>
        /// Instructs the stub to call the original method implementation, bypassing the mocking 
        /// layers.
        /// </summary>
        /// <param name="options">An <see cref="OriginalCallOptions"/> value that sets the expection for the method call.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService CallOriginalMethod(OriginalCallOptions options)
        {
            _Service.Stub(_Action).CallOriginalMethod(options);
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed. Please note that the delegate must match the signature of the method being stubbed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Do(Delegate callback)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            _Service.Stub(_Action).Do(callback);
            return _Service;
        }

        /// <summary>
        /// Instructs the stub to do nothing and exit immediately when the method is called.
        /// </summary>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService DoNothing()
        {
            _Service.Stub(_Action).WhenCalled(mi => { });
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled(Action<MethodInvocation> callback)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            _Service.Stub(_Action).WhenCalled(callback);
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <typeparam name="TArg">The type of the callback argument.</typeparam>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <param name="arg">An optional object to pass to the <paramref name="callback"/> delegate.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled<TArg>(Action<MethodInvocation, TArg> callback, TArg arg)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            _Service.Stub(_Action).WhenCalled(mi => callback(mi, arg));
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled(Action<MethodInvocation, TService> callback)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            var cb = callback;
            _Service.Stub(_Action).WhenCalled((mi) => cb(mi, _Service));
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <typeparam name="TArg">The type of the callback argument.</typeparam>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <param name="arg">An optional object to pass to the <paramref name="callback"/> delegate.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled<TArg>(Action<MethodInvocation, TService, TArg> callback, TArg arg)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            var cb = callback;
            _Service.Stub(_Action).WhenCalled((mi) => cb(mi, _Service, arg));
            return _Service;
        }

        /// <summary>
        /// Throws the specified exception when the method is called.
        /// </summary>
        /// <typeparam name="T">The type of the exception.</typeparam>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Throw<T>() where T : Exception
        {
            _Service.Stub(svc => svc.Equals(Arg<object>.Is.Anything));
            var instance = StubFactory.CreateInstance<T>();
            return this.Throw(instance);
        }

        /// <summary>
        /// Throws the specified exception when the method is called.
        /// </summary>
        /// <typeparam name="T">The type of the exception.</typeparam>
        /// <param name="instance">The exception instance to throw.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Throw<T>(T instance) where T : Exception
        {
            _Service.Stub(_Action).Throw(instance);
            return _Service;
        }
    }

    /// <summary>
    /// Helper class used to wrap a service interface while one of its actions are being stubbed.
    /// </summary>
    /// <typeparam name="TService">The type of the service interface.</typeparam>
    /// <typeparam name="TReturn">The type of the action's return value.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class StubActionContext<TService, TReturn> where TService : class
    {
        private readonly Function<TService, TReturn> _Action;
        private readonly Action<TReturn> StubReturnValue;
        private readonly TService _Service;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="service">The service whose action is being stubbed.</param>
        /// <param name="expression">An expession that evaluates to the property or method being stubbed.</param>
        internal StubActionContext(TService service, Expression<Function<TService, TReturn>> expression)
        {
            ArgumentValidator.ValidateNotNull(() => service);
            ArgumentValidator.ValidateNotNull(() => expression);
            _Action = expression.Compile();
            _Service = service;

            if (expression.NodeType == ExpressionType.Lambda)
            {
                var propertyInfo = (expression.GetMemberInfo() as PropertyInfo);
                if ((propertyInfo != null) && propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    StubReturnValue = (value => propertyInfo.SetValue(_Service, value));
                }
            }
            StubReturnValue = (StubReturnValue ?? (value => _Service.Stub(_Action).Return(value)));
        }

        /// <summary>
        /// Instructs the stub to call the original method implementation.
        /// </summary>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService CallOriginalMethod() { return this.CallOriginalMethod(OriginalCallOptions.NoExpectation); }

        /// <summary>
        /// Instructs the stub to call the original method implementation.
        /// </summary>
        /// <param name="options">A <see cref="OriginalCallOptions"/> value that specificies the expection for the method call within the mock framework.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService CallOriginalMethod(OriginalCallOptions options)
        {
            _Service.Stub(_Action).CallOriginalMethod(options);
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Do(Action callback)
        {
            _Service.Stub(_Action).Do(callback);
            return _Service;
        }

        /// <summary>
        /// Set the return value for the method when it is called.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Return(TReturn value)
        {
            StubReturnValue(value);
            return _Service;
        }

        /// <summary>
        /// Set the return value for the method when it is called.
        /// </summary>
        /// <param name="expression">An expression that is passed the service stub reference, and returns the value.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Return(Func<TService, TReturn> expression)
        {
            var value = ((expression == null)
                ? default(TReturn)
                : expression(_Service));
            return this.Return(value);
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled(Action<MethodInvocation> callback)
        {
            _Service.Stub(_Action).WhenCalled(callback).Return(default(TReturn));
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <typeparam name="TArg">The type of the callback argument.</typeparam>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <param name="arg">An optional object to pass to the <paramref name="callback"/> delegate.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled<TArg>(Action<MethodInvocation, TArg> callback, TArg arg)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            _Service.Stub(_Action).WhenCalled(mi => callback(mi, arg)).Return(default(TReturn));
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled(Action<MethodInvocation, TService> callback)
        {
            _Service.Stub(_Action).WhenCalled(mi => callback(mi, _Service)).Return(default(TReturn));
            return _Service;
        }

        /// <summary>
        /// Set a delegate to be called when the expectation is matched.
        /// </summary>
        /// <typeparam name="TArg">The type of the callback argument.</typeparam>
        /// <param name="callback">The delegate to call when the stubbed action is executed.</param>
        /// <param name="arg">An optional object to pass to the <paramref name="callback"/> delegate.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService WhenCalled<TArg>(Action<MethodInvocation, TService, TArg> callback, TArg arg)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            var cb = callback;
            _Service.Stub(_Action).WhenCalled((mi) => cb(mi, _Service, arg)).Return(default(TReturn));
            return _Service;
        }

        /// <summary>
        /// Throws the specified exception when the method is called.
        /// </summary>
        /// <typeparam name="T">The type of the exception.</typeparam>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Throw<T>() where T : Exception
        {
            var instance = StubFactory.CreateInstance<T>();
            return this.Throw(instance);
        }

        /// <summary>
        /// Throws the specified exception when the method is called.
        /// </summary>
        /// <param name="instance">The exception instance to throw.</param>
        /// <returns>The reference to the service being stubbed.</returns>
        public TService Throw(Exception instance)
        {
            _Service.Stub(_Action).Throw(instance);
            return _Service;
        }
    }
}
