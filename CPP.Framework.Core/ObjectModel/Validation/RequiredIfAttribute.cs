using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using CPP.Framework.ObjectModel.Validation.Resources;

namespace CPP.Framework.ObjectModel.Validation
{
    /// <summary>
    /// Validation attribute to indicate that a property field or parameter is required, but only
    /// if the value of another property or field matches a given value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class RequiredIfAttribute : DependentValidationAttribute
    {
        private static readonly Lazy<CultureInfo> EnglishCulture = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(1033));

        private readonly string _sourceValue;
        private object _expected;
        private bool _isValueSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate, which should return a
        /// boolean value.
        /// </param>
        public RequiredIfAttribute(string memberName) : this(memberName, true)
        {
            _expected = true;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, bool value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, byte value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, char value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, DateTime value) : this(memberName, value.ToString("O", EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, decimal value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, double value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, float value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, int value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, long value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, sbyte value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, short value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, string value) : base(memberName, ValidationResources.RequiredIfAttribute_ValidationError)
        {
            _sourceValue = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, uint value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, ulong value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="memberName">
        /// The name of a field or property on the same class to evaluate.
        /// </param>
        /// <param name="value">
        /// The value to compare against <paramref name="memberName"/>, as a string.
        /// </param>
        public RequiredIfAttribute(string memberName, ushort value) : this(memberName, value.ToString(EnglishCulture.Value))
        {
            _expected = value;
            _isValueSet = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute should allow empty strings.
        /// </summary>
        public bool AllowEmptyStrings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to invert the condition (i.e. required-
        /// if-not instead of required-if).
        /// </summary>
        public bool Not { get; set; }

        /// <inheritdoc />
        public override bool RequiresValidationContext => true;

        /// <summary>
        /// Verifies whether or not validation operation can proceed.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <returns><b>True</b> if validation is enabled; otherwise, <c>false</c>.</returns>
        protected virtual bool CanValidate(ValidationContext context)
        {
            if (!_isValueSet)
            {
                _expected = context.ConvertValue(this.SecondaryMemberName, _sourceValue, typeof(string));
                _isValueSet = true;
            }
            return (Equals(_expected, context.GetMemberValue(this.SecondaryMemberName)) || this.Not);
        }

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            // only check string length if empty strings are not allowed
            var result = (value != null);
            if (result && (value is string stringValue))
            {
                result = (this.AllowEmptyStrings) || (!string.IsNullOrWhiteSpace(stringValue));
            }
            return result;
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            ArgumentValidator.ValidateNotNull(() => context);

            var result = ValidationResult.Success;
            if (this.CanValidate(context) && (!this.IsValid(value)))
            {
                result = this.GenerateErrorResult(context);
            }
            return result;
        }
    }
}
