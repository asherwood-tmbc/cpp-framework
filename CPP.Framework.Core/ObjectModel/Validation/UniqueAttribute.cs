using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using CPP.Framework.ObjectModel.Validation.Resources;

namespace CPP.Framework.ObjectModel.Validation
{
    /// <summary>
    /// Validates that the property or field value does not match another property or field value
    /// on the same object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class UniqueAttribute : DependentValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueAttribute"/> class.
        /// </summary>
        /// <param name="memberName">The name of the property or field to compare against.</param>
        public UniqueAttribute(string memberName) : base(memberName, ValidationResources.UniqueAttribute_ValidationError) { }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            ArgumentValidator.ValidateNotNull(() => context);

            var result = ValidationResult.Success;
            var sourceType = context.GetMemberType(this.SecondaryMemberName);
            var other = this.GetSecondaryMemberValue(context);
            other = context.ConvertValue(other, sourceType);

            if (this.SecondaryMemberName == context.MemberName)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    ValidationResources.UniqueAttribute_Property_Cannot_Validate_Against_Itself,
                    context.MemberName);
                throw new InvalidOperationException(message);
            }
            if ((other != null) && (Equals(value, other)))
            {
                result = this.GenerateErrorResult(context);
            }
            return result;
        }
    }
}
