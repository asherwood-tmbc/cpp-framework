using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.ObjectModel.Validation
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequiredIfAttributeTests
    {
        private const string EnableLicenseKeyString = "{010F61AC-3D0D-4E13-9980-D670195F96B5}";
        private static readonly Guid EnabledLicenseKey = Guid.Parse(EnableLicenseKeyString);
        
        [TestMethod]
        public void Validate_WithBoolGateAndValidationDisabled_ExpectSuccess()
        {
            var sample = new MockObject
            {
                CanValidate = false,
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyWithBoolPropertyGate);
        }

        [ExpectedException(typeof(ValidationException))]
        [TestMethod]
        public void Validate_WithBoolGateAndValidationEnabled_ExpectFailure()
        {
            var sample = new MockObject
            {
                CanValidate = true,
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyWithBoolPropertyGate);
        }

        [TestMethod]
        public void Validate_WithGuidGateAndValidationDisabled_ExpectSuccess()
        {
            var sample = new MockObject
            {
                LicenceKey = Guid.Empty,
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyWithGuidPropertyGate);
        }

        [ExpectedException(typeof(ValidationException))]
        [TestMethod]
        public void Validate_WithGuidGateAndValidationEnabled_ExpectFailure()
        {
            var sample = new MockObject
            {
                LicenceKey = EnabledLicenseKey,
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyWithGuidPropertyGate);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Validate_WithInvalidMemberName_ExpectFailure()
        {
            var sample = new MockObject();
            ObjectValidator.Validate(sample, (o) => o.PropertyWithInvalidMemberName);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Validate_WithInvalidMemberType_ExpectFailure()
        {
            var sample = new MockObject();
            ObjectValidator.Validate(sample, (o) => o.PropertyWithInvalidMemberType);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Validate_WithWriteOnlyProperty_ExpectFailure()
        {
            var sample = new MockObject();
            ObjectValidator.Validate(sample, (o) => o.PropertyWithWriteOnlyMember);
        }

        #region InvalidMethodClass Declaration

        //// ReSharper disable UnusedAutoPropertyAccessor.Local
        private sealed class MockObject
        {
            public MockObject()
            {
                PropertyWithInvalidMemberName = $"{Guid.NewGuid()}";
                PropertyWithInvalidMemberType = $"{Guid.NewGuid()}";
                PropertyWithWriteOnlyMember = $"{Guid.NewGuid()}";
            }

            public bool CanValidate { get; set; }

            public Guid LicenceKey { get; set; }

            [RequiredIf(nameof(CanValidate))]
            public string PropertyWithBoolPropertyGate { get; set; }

            [RequiredIf(nameof(LicenceKey), EnableLicenseKeyString)]
            public string PropertyWithGuidPropertyGate { get; set; }
            
            [RequiredIf("Bogus")]
            public string PropertyWithInvalidMemberName { get; set; }

            [RequiredIf(nameof(MethodWithParams))]
            public string PropertyWithInvalidMemberType { get; set; }

            [RequiredIf(nameof(WriteOnlyProperty))]
            public string PropertyWithWriteOnlyMember { get; set; }

            // ReSharper disable once ValueParameterNotUsed
            private string WriteOnlyProperty { set { } }

            private string MethodWithParams(object arg) => $"{Guid.NewGuid()}";
        }
        //// ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion
    }
}
