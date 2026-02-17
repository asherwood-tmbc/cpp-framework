using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Security.Principal;

using CPP.Framework.DependencyInjection;
using CPP.Framework.ObjectModel.Validation.Resources;
using CPP.Framework.Security;
using CPP.Framework.Security.Policies;
using CPP.Framework.Services;

using JetBrains.Annotations;

namespace CPP.Framework.ObjectModel.Validation
{
    /// <summary>
    /// Helper class used to validate the data for an object.
    /// </summary>
    [AutoRegisterService]
    public class ObjectValidator : CodeServiceSingleton
    {
        private static readonly ConcurrentDictionary<Type, ObjectValidator> _Validators = new ConcurrentDictionary<Type, ObjectValidator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValidator"/> class. 
        /// </summary>
        [ServiceLocatorConstructor]
        protected ObjectValidator() { }

        /// <summary>
        /// Checks whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <remarks>
        /// In order to participate in access validation, <paramref name="instance"/> must either
        /// implement the <see cref="ISupportsObjectAccessPolicy"/> interface, or derive from the
        /// <see cref="SecuredObject"/> class.
        /// </remarks>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        public static bool CheckAccess(object instance)
        {
            return GetValidator(instance).OnCheckAccess(instance, null);
        }

        /// <summary>
        /// Checks whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <param name="principal">The <see cref="IPrincipal"/> to check against.</param>
        /// <remarks>
        /// In order to participate in access validation, <paramref name="instance"/> must either
        /// implement the <see cref="ISupportsObjectAccessPolicy"/> interface, or derive from the
        /// <see cref="SecuredObject"/> class.
        /// </remarks>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="principal"/> is null.</exception>
        public static bool CheckAccess(object instance, IPrincipal principal)
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            return GetValidator(instance).OnCheckAccess(instance, principal);
        }

        /// <summary>
        /// Verifies whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <remarks>
        /// In order to participate in access validation, <paramref name="instance"/> must either
        /// implement the <see cref="ISupportsObjectAccessPolicy"/> interface, or derive from the
        /// <see cref="SecuredObject"/> class.
        /// </remarks>
        /// <exception cref="SecurityException">
        /// <paramref name="instance"/> could not be validated, or the principal is not authorized.
        /// </exception>
        public static void DemandAccess(object instance)
        {
            GetValidator(instance).OnDemandAccess(instance, null);
        }

        /// <summary>
        /// Verifies whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <param name="principal">The <see cref="IPrincipal"/> to check against.</param>
        /// <remarks>
        /// In order to participate in access validation, <paramref name="instance"/> must either
        /// implement the <see cref="ISupportsObjectAccessPolicy"/> interface, or derive from the
        /// <see cref="SecuredObject"/> class.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="principal"/> is null.</exception>
        /// <exception cref="SecurityException">
        /// <paramref name="instance"/> could not be validated, or the principal is not authorized.
        /// </exception>
        public static void DemandAccess(object instance, IPrincipal principal)
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            GetValidator(instance).OnDemandAccess(instance, principal);
        }

        /// <summary>
        /// Gets the security access policy for an object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <param name="context">The context for the access check request.</param>
        /// <param name="policy">An output parameter that receives the access policy on success.</param>
        /// <returns><b>True</b> if the policy was retrieved; otherwise, <b>false</b>.</returns>
        protected internal virtual bool GetAccessPolicy(object instance, SecurityAuthorizationContext context, out SecurityAuthorizationPolicy policy)
        {
            policy = default(SecurityAuthorizationPolicy);
            if (instance is ISupportsObjectAccessPolicy custom)
            {
                try
                {
                    policy = custom.GetAccessPolicy(context);
                    return (policy != null);
                }
                catch (NotImplementedException)
                {
                    /* the object doesn't have any access policies, so do nothing */
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the <see cref="ObjectValidator"/> instance for a given object, and ensures that
        /// any associated metadata types for it have been registered.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <returns>A <see cref="ObjectValidator"/> object.</returns>
        private static ObjectValidator GetValidator(object instance)
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            var validator = _Validators.GetOrAdd(
                (instance.GetType()),
                (type) =>
                    {
                        // we are going to return a single shared instance of the actual validator
                        // class (since the implementation isn't dependent on the input type), but
                        // we do need to make sure any metadata types for the target type has been
                        // registered, otherwise the instances may not validate properly.
                        var attribute = type.GetCustomAttributes(typeof(MetadataTypeAttribute), false)
                            .OfType<MetadataTypeAttribute>()
                            .FirstOrDefault();
                        if (attribute != null)
                        {
                            var provider = new AssociatedMetadataTypeTypeDescriptionProvider(type, attribute.MetadataClassType);
                            TypeDescriptor.AddProviderTransparent(provider, type);
                        }
                        return CodeServiceProvider.GetService<ObjectValidator>();
                    });
            return validator;
        }

        /// <summary>
        /// Tests whether the given object instance is valid.
        /// </summary>
        /// <param name="instance">The object instance to test. It cannot be null.</param>
        /// <param name="memberNames">
        /// An optional list of member names that specify which properties to validate. If this
        /// list is empty, then all available properties are validated.
        /// </param>
        /// <returns><c>true</c> if the object is valid, <c>false</c> if any validation errors are encountered.</returns>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object
        /// instance's type. It also checks to ensure all properties marked with
        /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> are set. This method will also evaluate the
        /// <see cref="ValidationAttribute"/>s for all the immediate properties of this object.
        /// This process is not recursive.
        /// <para>
        /// For any given property, if it has a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> that fails
        /// validation, no other validators will be evaluated for that property.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool IsValid(object instance, params string[] memberNames)
        {
            return GetValidator(instance).OnTryValidate(instance, null, memberNames);
        }

        /// <summary>
        /// Tests whether the given object instance is valid.
        /// </summary>
        /// <param name="instance">The object instance to test. It cannot be null.</param>
        /// <param name="validationResults">
        /// An array of <see cref="ValidationResult"/> objects that receives the list of validation
        /// errors on failure.
        /// </param>
        /// <param name="memberNames">
        /// An optional list of member names that specify which properties to validate. If this
        /// list is empty, then all available properties are validated.
        /// </param>
        /// <returns><c>true</c> if the object is valid, <c>false</c> if any validation errors are encountered.</returns>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object
        /// instance's type. It also checks to ensure all properties marked with
        /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> are set. This method will also evaluate the
        /// <see cref="ValidationAttribute"/>s for all the immediate properties of this object.
        /// This process is not recursive.
        /// <para>
        /// For any given property, if it has a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> that fails
        /// validation, no other validators will be evaluated for that property.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool IsValid(object instance, out ValidationResult[] validationResults, params string[] memberNames)
        {
            var aggregate = new List<ValidationResult>();
            var succeeded = GetValidator(instance).OnTryValidate(instance, aggregate, memberNames);
            validationResults = aggregate.ToArray();
            return succeeded;
        }
        
        /// <summary>
        /// Tests whether the given object instance is valid.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="instance">The object instance to test. It cannot be null.</param>
        /// <param name="memberExpressions">
        /// An optional list of member-access expressions that specify which properties to validate.
        /// If this list is empty, then all available properties are validated.
        /// </param>
        /// <returns><c>true</c> if the object is valid, <c>false</c> if any validation errors are encountered.</returns>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object
        /// instance's type. It also checks to ensure all properties marked with
        /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> are set. This method will also evaluate the
        /// <see cref="ValidationAttribute"/>s for all the immediate properties of this object.
        /// This process is not recursive.
        /// <para>
        /// For any given property, if it has a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> that fails
        /// validation, no other validators will be evaluated for that property.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool IsValid<TObject>(TObject instance, params Expression<Func<TObject, object>>[] memberExpressions)
        {
            return GetValidator(instance).OnTryValidate(instance, null, UnpackMemberExpressions(memberExpressions));
        }

        /// <summary>
        /// Tests whether the given object instance is valid.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="instance">The object instance to test. It cannot be null.</param>
        /// <param name="validationResults">
        /// An array of <see cref="ValidationResult"/> objects that receives the list of validation
        /// errors on failure.
        /// </param>
        /// <param name="memberExpressions">
        /// An optional list of member-access expressions that specify which properties to validate.
        /// If this list is empty, then all available properties are validated.
        /// </param>
        /// <returns><c>true</c> if the object is valid, <c>false</c> if any validation errors are encountered.</returns>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object
        /// instance's type. It also checks to ensure all properties marked with
        /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> are set. This method will also evaluate the
        /// <see cref="ValidationAttribute"/>s for all the immediate properties of this object.
        /// This process is not recursive.
        /// <para>
        /// For any given property, if it has a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> that fails
        /// validation, no other validators will be evaluated for that property.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool IsValid<TObject>(TObject instance, out ValidationResult[] validationResults, params Expression<Func<TObject, object>>[] memberExpressions)
        {
            var aggregate = new List<ValidationResult>();
            var succeeded = GetValidator(instance).OnTryValidate(instance, aggregate, UnpackMemberExpressions(memberExpressions));
            validationResults = aggregate.ToArray();
            return succeeded;
        }
                
        /// <summary>
        /// Checks whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <param name="principal">
        /// The <see cref="IPrincipal"/> to check against, or null to use the current principal for
        /// the request thread.
        /// </param>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        protected internal virtual bool OnCheckAccess(object instance, IPrincipal principal)
        {
            var context = SecurityAuthorizationContext.Create(principal);
            if (this.GetAccessPolicy(instance, context, out var policy))
            {
                return policy.CheckAccess(context);
            }
            return true;
        }
        
        /// <summary>
        /// Verifies whether or not the <see cref="IPrincipal"/> for the current execution context
        /// has access to the given object instance.
        /// </summary>
        /// <param name="instance">The object instance to test.</param>
        /// <param name="principal">
        /// The <see cref="IPrincipal"/> to check against, or null to use the current principal for
        /// the request thread.
        /// </param>
        /// <exception cref="SecurityException">
        /// <paramref name="instance"/> could not be validated, or the principal is not authorized.
        /// </exception>
        protected internal virtual void OnDemandAccess(object instance, IPrincipal principal)
        {
            if (this.OnCheckAccess(instance, principal))
            {
                return;
            }
            throw new SecurityAuthorizationException();
        }

        /// <summary>
        /// Tests whether the given object instance is valid.
        /// </summary>
        /// <param name="instance">The object instance to test. It cannot be null.</param>
        /// <param name="validationResults">
        /// An array of <see cref="ValidationResult"/> objects that receives the list of validation
        /// errors on failure. This value can be null if the caller does not need the results.
        /// </param>
        /// <param name="memberNames">
        /// An optional list of member names that specify which properties to validate. If this
        /// list is empty, then all available properties are validated.
        /// </param>
        /// <returns><c>true</c> if the object is valid, <c>false</c> if any validation errors are encountered.</returns>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object
        /// instance's type. It also checks to ensure all properties marked with
        /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> are set. This method will also evaluate the
        /// <see cref="ValidationAttribute"/>s for all the immediate properties of this object.
        /// This process is not recursive.
        /// <para>
        /// For any given property, if it has a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> that fails
        /// validation, no other validators will be evaluated for that property.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        protected internal virtual bool OnTryValidate([NotNull] object instance, [CanBeNull] ICollection<ValidationResult> validationResults, [CanBeNull] IEnumerable<string> memberNames)
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ArgumentValidator.ValidateNotNull(() => memberNames);

            var validated = false;
            var context = new ValidationContext(instance, null, null);
            var success = true;

            foreach (var name in (memberNames?.Where(mn => (!string.IsNullOrWhiteSpace(mn))) ?? Enumerable.Empty<string>()))
            {
                context.MemberName = name;
                if (!Validator.TryValidateProperty(context.GetMemberValue(name), context, validationResults))
                {
                    // only set this false if we have not failed yet. this is to prevent accidently
                    // reporting success because a later property evaluation succeeded even after a
                    // previous one had failed.
                    success = false;
                }
                validated = true;
            }
            if (!validated)
            {
                success = Validator.TryValidateObject(instance, context, validationResults, true);
            }
            return success;
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> if the given object instance is not valid.
        /// </summary>
        /// <param name="instance">The object instance to test.  It cannot be null.</param>
        /// <param name="memberNames">
        /// An optional list of member names that specify which properties to validate. If this
        /// list is empty, then all available properties are validated.
        /// </param>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object's
        /// type, as well as all the object's properties.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="instance"/> is found to be invalid.</exception>
        protected internal virtual void OnValidate([NotNull] object instance, [CanBeNull] IEnumerable<string> memberNames)
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ArgumentValidator.ValidateNotNull(() => memberNames);

            var validated = false;
            var context = new ValidationContext(instance);

            foreach (var name in (memberNames?.Where(e => (!string.IsNullOrWhiteSpace(e))) ?? Enumerable.Empty<string>()))
            {
                context.MemberName = name;
                Validator.ValidateProperty(context.GetMemberValue(name), context);
                validated = true;
            }
            if (!validated) Validator.ValidateObject(instance, context, true);
        }

        /// <summary>
        /// Extracts the property or field names from a sequence of member-access expressions.
        /// </summary>
        /// <typeparam name="TObject">The type of the parent object.</typeparam>
        /// <param name="expressions">The sequence of expressions to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the list of extracted names.</returns>
        protected static IEnumerable<string> UnpackMemberExpressions<TObject>(IEnumerable<Expression<Func<TObject, object>>> expressions)
        {
            foreach (var exp in expressions.Select(exp => exp.Body))
            {
                var memberExpression = default(MemberExpression);
                switch (exp.NodeType)
                {
                    case ExpressionType.Convert:
                        {
                            var unary = ((UnaryExpression)exp);
                            if (unary.Operand.NodeType == ExpressionType.MemberAccess)
                            {
                                memberExpression = ((MemberExpression)unary.Operand);
                            }
                        }
                        break;
                    case ExpressionType.MemberAccess:
                        {
                            memberExpression = ((MemberExpression)exp);
                        }
                        break;
                }
                if (memberExpression == null)
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        ValidationResources.ObjectValidator_Expression_Type_Is_Invalid,
                        exp.NodeType);
                    throw new ArgumentException(message, nameof(expressions));
                }
                if (memberExpression.Expression.Type != typeof(TObject))
                {
                    throw new ArgumentException();
                }
                yield return memberExpression.Member.Name;
            }
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> if the given object instance is not valid.
        /// </summary>
        /// <param name="instance">The object instance to test.  It cannot be null.</param>
        /// <param name="memberNames">
        /// An optional list of member names that specify which properties to validate. If this
        /// list is empty, then all available properties are validated.
        /// </param>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object's
        /// type, as well as all the object's properties.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="instance"/> is found to be invalid.</exception>
        public static void Validate(object instance, params string[] memberNames)
        {
            GetValidator(instance).OnValidate(instance, memberNames);
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> if the given object instance is not valid.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="instance">The object instance to test.  It cannot be null.</param>
        /// <param name="memberExpressions">
        /// An optional list of member-access expressions that specify which properties to validate.
        /// If this list is empty, then all available properties are validated.
        /// </param>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s attached to the object's
        /// type, as well as all the object's properties.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        /// <exception cref="ValidationException"><paramref name="instance"/> is found to be invalid.</exception>
        public static void Validate<TObject>(TObject instance, params Expression<Func<TObject, object>>[] memberExpressions)
        {
            GetValidator(instance).OnValidate(instance, UnpackMemberExpressions(memberExpressions));
        }
    }
}
