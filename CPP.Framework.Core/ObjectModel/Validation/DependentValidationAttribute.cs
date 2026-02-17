using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CPP.Framework.ObjectModel.Validation.Resources;

namespace CPP.Framework.ObjectModel.Validation
{
    /// <summary>
    /// Abstract base class for validation attributes that depend on the value of another property
    /// that is defined on the same object.
    /// </summary>
    public abstract class DependentValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentValidationAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of the property or field on which the attribute depends.
        /// </param>
        protected DependentValidationAttribute(string memberName) : this(memberName, () => ValidationResources.DependentValidationAttribute_ValidationError) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentValidationAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of the property or field on which the attribute depends.
        /// </param>
        /// <param name="errorMessage">
        /// A non-localized error message to use in
        /// <see cref="ValidationAttribute.ErrorMessageString"/>.
        /// </param>
        protected DependentValidationAttribute(string memberName, string errorMessage) : this(memberName, () => errorMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentValidationAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of the property or field on which the attribute depends.
        /// </param>
        /// <param name="errorMessageAccessor">The <see cref="Func{T}"/> that will return an error message.</param>
        protected DependentValidationAttribute(string memberName, Func<string> errorMessageAccessor) : base(errorMessageAccessor)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => memberName);
            this.SecondaryMemberName = memberName;
        }

        /// <inheritdoc />
        public override bool RequiresValidationContext => true;

        /// <summary>
        /// Gets the name of the secondary member on which the current attribute depends.
        /// </summary>
        public string SecondaryMemberName { get; }

        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name, this.SecondaryMemberName);
        }

        /// <summary>
        /// Creates a <see cref="ValidationResult"/> for a failed validation attempt.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <returns>A <see cref="ValidationResult"/> object.</returns>
        protected virtual ValidationResult GenerateErrorResult(ValidationContext context)
        {
            var members = new[] { context.MemberName };
            return new ValidationResult(this.FormatErrorMessage(context.MemberName), members);
        }

        /// <summary>
        /// Gets the value of the secondary field on which the current attribute depends.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <returns>An object that contains the member value.</returns>
        protected virtual object GetSecondaryMemberValue(ValidationContext context)
        {
            return context.GetMemberValue(this.SecondaryMemberName);
        }
    }
}
