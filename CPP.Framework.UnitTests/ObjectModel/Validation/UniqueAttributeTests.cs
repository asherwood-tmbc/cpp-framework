using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.ObjectModel.Validation
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UniqueAttributeTests
    {
        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Validation_WithCircularReference_ExpectFailure()
        {
            var sample = new MockObject();
            ObjectValidator.Validate(sample, (o) => o.PropertyB);
        }

        [ExpectedException(typeof(ValidationException))]
        [TestMethod]
        public void Validate_WithMatchingValue_ExpectFailure()
        {
            var value = Guid.NewGuid();
            var sample = new MockObject
            {
                PropertyA = value,
                PropertyB = value,
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyA);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void Validate_WithMismatchedTypes_ExpectFailure()
        {
            var sample = new MockObject();
            ObjectValidator.Validate(sample, (o) => o.PropertyC);
        }

        [TestMethod]
        public void Validate_WithUniqueValue_ExpectSuccess()
        {
            var sample = new MockObject
            {
                PropertyA = Guid.NewGuid(),
                PropertyB = Guid.NewGuid(),
            };
            ObjectValidator.Validate(sample, (o) => o.PropertyA);
        }

        [ExpectedException(typeof(ValidationException))]
        [TestMethod]
        public void Validate_EntireObject_ExpectFailure()
        {
            var value = Guid.NewGuid();
            var sample = new MockModel
            {
                PropertyA = value,
                PropertyB = value,
            };
            ObjectValidator.Validate(sample);
        }

        [TestMethod]
        public void Validate_EntireObject_ExpectSuccess()
        {
            var sample = new MockModel
            {
                PropertyA = Guid.NewGuid(),
                PropertyB = Guid.NewGuid(),
            };
            ObjectValidator.Validate(sample);
        }

        #region MockModel Class Declaration

        private sealed class MockModel
        {
            [Unique(nameof(PropertyB))]
            public Guid PropertyA { get; set; }

            public Guid PropertyB { get; set; }
        }

        #endregion // MockModel Class Declaration

        #region MockObject Class Declaration

        private sealed class MockObject
        {
            [Unique(nameof(PropertyB))]
            public Guid PropertyA { get; set; }

            [Unique(nameof(PropertyB))]
            public Guid PropertyB { get; set; }

            [Unique(nameof(PropertyB))]
            public bool PropertyC { get; set; }
        }

        #endregion // MockObject Class Declaration
    }
}
