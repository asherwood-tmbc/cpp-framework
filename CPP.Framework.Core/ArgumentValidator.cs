using System;
using System.Linq;
using System.Linq.Expressions;
using CPP.Framework.Configuration;
using CPP.Framework.ObjectModel.Validation;

namespace CPP.Framework
{
    /// <summary>
    /// Helper Class for Argument Validation.
    /// </summary>
    public static class ArgumentValidator
    {
        #region ConfigSettingKey Argument Validation

        /// <summary>
        /// Validates that a <see cref="ConfigSettingKey"/> value is assigned to an expected 
        /// configuration category.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <param name="expected">The expected <see cref="ConfigSettingTarget"/> of the argument value.</param>
        /// <exception cref="ArgumentException">The argument value is <see cref="Guid.Empty"/>.</exception>
        public static void ValidateConfigCategory(Expression<Func<ConfigSettingKey>> expression, ConfigSettingTarget expected)
        {
            var argument = new MethodArgument<ConfigSettingKey>(expression);
            ArgumentValidator.ValidateConfigCategory(argument, expected);
        }

        /// <summary>
        /// Validates that a <see cref="ConfigSettingKey"/> value is assigned to an expected 
        /// configuration category.
        /// </summary>
        /// <param name="argument">The <see cref="MethodArgument{T}"/> value for the argument to validate.</param>
        /// <param name="expected">The expected <see cref="ConfigSettingTarget"/> of the argument value.</param>
        /// <exception cref="ArgumentException">The argument value is <see cref="Guid.Empty"/>.</exception>
        private static void ValidateConfigCategory(MethodArgument<ConfigSettingKey> argument, ConfigSettingTarget expected)
        {
            if (argument.Value.GetTarget() != expected)
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(argument, null, ErrorStrings.InvalidConfigSettingKeyTarget, argument.Value, expected);
            }
        }

        #endregion // ConfigSettingKey Argument Validation

        #region Custom Argument Validation

        /// <summary>
        /// Validates an argument variable using a custom validator.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <param name="validator">The custom validator for the argument.</param>
        /// <exception cref="ArgumentException">The argument value is not valid.</exception>
        public static void ValidateCustom<T>(Expression<Func<T>> expression, ICustomArgumentValidator<T> validator)
        {
            if (validator == null) return;
            var argument = new MethodArgument<T>(expression);
            validator.ValidateArgument(argument.Name, argument.Value);
        }

        #endregion // Custom Argument Validation

        #region Data Model Validation

        /// <summary>
        /// Validates that the data model is valid for a method argument, and that the caller has
        /// permission to access it. Additionally, if the argument type implements the
        /// <see cref="ICustomArgumentValidator{TValue}"/> interface, this method will also invoke
        /// it do any custom validation.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        public static void ValidateModel<T>(Expression<Func<T>> expression)
        {
            var argument = new MethodArgument<T>(expression);
            ArgumentValidator.ValidateModel(argument, true);
        }

        /// <summary>
        /// Validates that the data model is valid for a method argument, and optionally whether or
        /// not the caller has permission to access it. Additionally, if the argument type
        /// implements the <see cref="ICustomArgumentValidator{TValue}"/> interface, this method
        /// will also invoke it do any custom validation.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <param name="validateAccess">
        /// <b>True</b> to validate the model security; otherwise, <b>false</b>.
        /// </param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        public static void ValidateModel<T>(Expression<Func<T>> expression, bool validateAccess)
        {
            var argument = new MethodArgument<T>(expression);
            ArgumentValidator.ValidateModel(argument, validateAccess);
        }

        /// <summary>
        /// Validates that the data model is valid for a method argument, and optionally whether or
        /// not the caller has permission to access it. Additionally, if the argument type
        /// implements the <see cref="ICustomArgumentValidator{TValue}"/> interface, this method
        /// will also invoke it do any custom validation.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="argument">The <see cref="MethodArgument{T}"/> value for the argument to validate.</param>
        /// <param name="validateAccess">
        /// <b>True</b> to validate the model security; otherwise, <b>false</b>.
        /// </param>
        private static void ValidateModel<T>(MethodArgument<T> argument, bool validateAccess)
        {
            if (!ObjectValidator.IsValid(argument.Value, out var results))
            {
                throw new ArgumentException(results.First().ErrorMessage, argument.Name);
            }
            if (argument.Value is ICustomArgumentValidator<T> validator)
            {
                validator.ValidateArgument(argument.Name, argument.Value);
            }
            if (validateAccess) ObjectValidator.DemandAccess(argument.Value);
        }

        #endregion // Data Model Validation

        #region Guid Argument Validation

        /// <summary>
        /// Validates that a <see cref="Guid"/> method argument value is not <see cref="Guid.Empty"/>.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <exception cref="ArgumentException">The argument value is <see cref="Guid.Empty"/>.</exception>
        public static void ValidateNotEmpty(Expression<Func<Guid>> expression)
        {
            var argument = new MethodArgument<Guid>(expression);
            ArgumentValidator.ValidateNotEmpty(argument);
        }

        /// <summary>
        /// Validates that a <see cref="Guid"/> method argument value is not <see cref="Guid.Empty"/>.
        /// </summary>
        /// <param name="argument">The <see cref="MethodArgument{T}"/> value for the argument to validate.</param>
        /// <exception cref="ArgumentException">The argument value is <see cref="Guid.Empty"/>.</exception>
        private static void ValidateNotEmpty(MethodArgument<Guid> argument)
        {
            if (Guid.Empty.Equals(argument.Value))
            {
                throw new ArgumentException(ErrorStrings.EmptyGuidValue, argument.Name);
            }
        }

        #endregion // Guid Value Validation

        #region Object Reference Validation

        /// <summary>
        /// Validates that a reference argument value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        public static void ValidateNotNull<T>(Expression<Func<T>> expression) where T : class
        {
            var argument = new MethodArgument<T>(expression);
            ArgumentValidator.ValidateNotNull(argument);
        }

        /// <summary>
        /// Validates that a <see cref="Nullable{T}"/> argument value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        public static void ValidateNotNull<T>(Expression<Func<T?>> expression) where T : struct
        {
            var argument = new MethodArgument<T?>(expression);
            ArgumentValidator.ValidateNotNull(argument);
        }

        /// <summary>
        /// Validates that a <see cref="MethodArgument{T}"/> is not a null value.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="argument">The argument to validate.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        private static void ValidateNotNull<T>(MethodArgument<T> argument) where T : class
        {
            if (argument.Value == null) throw new ArgumentNullException(argument.Name);
        }

        /// <summary>
        /// Validates that a <see cref="MethodArgument{T}"/> is not a null value.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="argument">The argument to validate.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        private static void ValidateNotNull<T>(MethodArgument<T?> argument) where T : struct
        {
            if (argument.Value == null) throw new ArgumentNullException(argument.Name);
        }

        /// <summary>
        /// Validates that the "this" object passed to an extension method is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <exception cref="NullReferenceException">The value referenced by <paramref name="expression"/> is null.</exception>
        public static void ValidateThisObj<T>(Expression<Func<T>> expression) where T : class
        {
            var argument = new MethodArgument<T>(expression);
            ArgumentValidator.ValidateThisObj(argument);
        }

        /// <summary>
        /// Validates that the "this" object passed to an extension method is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <exception cref="NullReferenceException">The value referenced by <paramref name="expression"/> is null.</exception>
        public static void ValidateThisObj<T>(Expression<Func<T?>> expression) where T : struct
        {
            var argument = new MethodArgument<T?>(expression);
            ArgumentValidator.ValidateThisObj(argument);
        }

        /// <summary>
        /// Validates that the "this" object passed to an extension method is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="argument">The argument to validate.</param>
        /// <exception cref="NullReferenceException">The value referenced by <paramref name="argument"/> is null.</exception>
        [JetBrains.Annotations.AssertionMethod]
        private static void ValidateThisObj<T>(MethodArgument<T> argument) where T : class
        {
            if (argument.Value == null) throw new NullReferenceException();
        }

        /// <summary>
        /// Validates that the "this" object passed to an extension method is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="argument">The argument to validate.</param>
        /// <exception cref="NullReferenceException">The value referenced by <paramref name="argument"/> is null.</exception>
        [JetBrains.Annotations.AssertionMethod]
        private static void ValidateThisObj<T>(MethodArgument<T?> argument) where T : struct
        {
            if (!argument.Value.HasValue) throw new NullReferenceException();
        }

        #endregion // Object Reference Validation

        #region String Reference Validation

        /// <summary>
        /// Validates that a string argument to a method is not a null reference or an empty string
        /// value.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        /// <exception cref="ArgumentException">The argument value is an empty string.</exception>
        public static void ValidateNotNullOrEmpty(Expression<Func<string>> expression)
        {
            var argument = new MethodArgument<string>(expression);
            ArgumentValidator.ValidateNotNullOrEmpty(argument);
        }

        /// <summary>
        /// Validates that a string argument to a method is not a null reference or an empty string
        /// value.
        /// </summary>
        /// <param name="argument">The <see cref="MethodArgument{T}"/> for the argument to validate.</param>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        /// <exception cref="ArgumentException">The argument value is an empty string.</exception>
        private static void ValidateNotNullOrEmpty(MethodArgument<string> argument)
        {
            ArgumentValidator.ValidateNotNull(argument);
            if (string.IsNullOrEmpty(argument.Value))
            {
                throw new ArgumentException(ErrorStrings.EmptyStringValue, argument.Name);
            }
        }

        /// <summary>
        /// Validates that a string argument to a method is not a null reference, an empty string
        /// value, or all whitespace characters.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the argument to validate.</param>
        /// <exception cref="ArgumentException">
        ///     <para>The argument value is an empty string.</para>
        ///     <para>-or-</para>
        ///     <para>The argument value is all whitespace characters.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        public static void ValidateNotNullOrWhiteSpace(Expression<Func<string>> expression)
        {
            var argument = new MethodArgument<string>(expression);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(argument);
        }

        /// <summary>
        /// Validates that a string argument to a method is not a null reference, an empty string
        /// value, or all whitespace characters.
        /// </summary>
        /// <param name="argument">The <see cref="MethodArgument{T}"/> for the argument to validate.</param>
        /// <exception cref="ArgumentException">
        ///     <para>The argument value is an empty string.</para>
        ///     <para>-or-</para>
        ///     <para>The argument value is all whitespace characters.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">The argument value is null.</exception>
        private static void ValidateNotNullOrWhiteSpace(MethodArgument<string> argument)
        {
            ArgumentValidator.ValidateNotNullOrEmpty(argument);
            if (string.IsNullOrWhiteSpace(argument.Value))
            {
                throw new ArgumentException(ErrorStrings.WhiteSpaceStringValue, argument.Name);
            }
        }

        #endregion // String Reference Validation

        #region Generic Helper Methods

        /// <summary>
        /// Generates a custom <see cref="ArgumentException"/> for a given method argument.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="argument">The argument for which to throw the exception.</param>
        /// <param name="innerException">The exception that caused the <see cref="ArgumentException"/> to be thrown.</param>
        /// <param name="format">The format string for the exception message.</param>
        /// <param name="formatArgs">One or more optional arguments for the <paramref name="format"/> string.</param>
        /// <returns>An <see cref="ArgumentException"/> value</returns>
        private static ArgumentException CreateArgumentExceptionFor<T>(MethodArgument<T> argument, Exception innerException, string format, params object[] formatArgs)
        {
            var message = string.Format(format, formatArgs);
            var exception = ((innerException != null)
                ? new ArgumentException(message, argument.Name, innerException)
                : new ArgumentException(message, argument.Name));
            return exception;
        }

        /// <summary>
        /// Generates a custom <see cref="ArgumentException"/> for a given method argument.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <param name="format">The format string for the exception message.</param>
        /// <returns>An <see cref="ArgumentException"/> value</returns>
        /// <param name="formatArgs">One or more optional arguments for the <paramref name="format"/> string.</param>
        public static ArgumentException CreateArgumentExceptionFor<T>(Expression<Func<T>> expression, string format, params object[] formatArgs)
        {
            var argument = new MethodArgument<T>(expression);
            return ArgumentValidator.CreateArgumentExceptionFor(argument, null, format, formatArgs);
        }

        /// <summary>
        /// Generates a custom <see cref="ArgumentException"/> for a given method argument.
        /// </summary>
        /// <typeparam name="T">The type of the argument variable.</typeparam>
        /// <param name="expression">An expression that evaluates to the argument being validated.</param>
        /// <param name="innerException">The exception that caused the <see cref="ArgumentException"/> to be thrown.</param>
        /// <param name="format">The format string for the exception message.</param>
        /// <param name="formatArgs">One or more optional arguments for the <paramref name="format"/> string.</param>
        /// <returns>An <see cref="ArgumentException"/> value</returns>
        public static ArgumentException CreateArgumentExceptionFor<T>(Expression<Func<T>> expression, Exception innerException, string format, params object[] formatArgs)
        {
            var argument = new MethodArgument<T>(expression);
            return ArgumentValidator.CreateArgumentExceptionFor(argument, innerException, format, formatArgs);
        }

        #endregion // Generic Helper Methods

        #region Legacy Terrace validators

        /// <summary>
        /// Validates that the specified parameter value is not null.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="paramValue">The value of the parameter to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown when the parameter value is null.</exception>
        public static void ValidateNotNull<TParam>(TParam paramValue, string paramName)
        {
            if (ReferenceEquals(null, paramValue))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Validates the specified parameter using multiple validation actions.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="expression">An expression that evaluates to the parameter to validate.</param>
        /// <param name="failureAction">The action to perform when validation fails.</param>
        /// <param name="validateActions">The validation actions to perform.</param>
        /// <exception cref="ArgumentNullException">Thrown when the failure action or validation actions are null.</exception>
        public static void ValidateAll<TParam>(Expression<Func<TParam>> expression, Func<TParam, string, Exception> failureAction, params Func<TParam, bool>[] validateActions)
        {
            var param = ArgumentValidator.Evaluate(expression);
            ArgumentValidator.ValidateAll(param.Value, param.Name, failureAction, validateActions);
        }

        /// <summary>
        /// Validates the specified parameter using multiple validation actions.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="paramValue">The value of the parameter to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="failureAction">The action to perform when validation fails.</param>
        /// <param name="validateActions">The validation actions to perform.</param>
        /// <exception cref="ArgumentNullException">Thrown when the failure action or validation actions are null.</exception>
        public static void ValidateAll<TParam>(TParam paramValue, string paramName, Func<TParam, string, Exception> failureAction, params Func<TParam, bool>[] validateActions)
        {
            ArgumentValidator.ValidateNotNull(failureAction, "failureAction");
            ArgumentValidator.ValidateNotNull(validateActions, "validateActions");

            foreach (var isValidAction in validateActions)
            {
                if (isValidAction == null) continue;
                if (!isValidAction(paramValue))
                {
                    var exception = failureAction(paramValue, paramName);
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Evaluates the specified expression and returns the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>An <see cref="ExpressionResult{TValue}"/> containing the name and value of the evaluated expression.</returns>
        private static ExpressionResult<TValue> Evaluate<TValue>(Expression<Func<TValue>> expression)
        {
            var paramName = ((expression.Body.NodeType != ExpressionType.MemberAccess)
                ? "<expression>"
                : ((MemberExpression)expression.Body).Member.Name);
            var paramValue = expression.Compile()();
            return new ExpressionResult<TValue>(paramName, paramValue);
        }

        #region MethodArgument Class Declaration

        /// <summary>
        /// Represents a parameter to a method.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        private sealed class MethodArgument<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodArgument{T}"/> class. 
            /// </summary>
            /// <param name="expression">
            /// An expression that evaluates to a method parameter variable.
            /// </param>
            internal MethodArgument(Expression<Func<T>> expression)
            {
                if (expression?.Body == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }
                if (expression.Body.NodeType != ExpressionType.MemberAccess)
                {
                    throw new ArgumentException(ErrorStrings.InvalidMemberAccessExpression, nameof(expression));
                }
                this.Name = ((MemberExpression)expression.Body).Member.Name;
                this.Value = (expression.Compile())();
            }

            /// <summary>
            /// Gets the name of the parameter variable.
            /// </summary>
            internal string Name { get; }

            /// <summary>
            /// Gets the current value of the parameter.
            /// </summary>
            internal T Value { get; }
        }

        #endregion // MethodArgument Class Declaration

        /// <summary>
        /// Represents the result of evaluating an expression.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        private class ExpressionResult<TValue>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionResult{TValue}"/> class.
            /// </summary>
            /// <param name="name">The name of the parameter.</param>
            /// <param name="value">The value of the parameter.</param>
            internal ExpressionResult(string name, TValue value)
            {
                this.Name = name;
                this.Value = value;
            }

            /// <summary>Gets the name of the parameter.</summary>
            internal string Name { get; private set; }

            /// <summary>Gets the value of the parameter.</summary>
            internal TValue Value { get; private set; }
        }
        #endregion
    }
}
