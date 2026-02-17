using System;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Rhino.Mocks;

namespace CPP.Framework.WindowsAzure.Storage
{
    public partial class AzureStorageTableTests
    {
        [TestMethod]
        public void GetTableProperty()
        {
            const string TestPropertyName = "SampleProperty";
            Guid expected = Guid.NewGuid(), actual;

            using (var account = CreateStorageAccountStub(true))
            {
                var target = account.GetStorageTable<SampleEntity>()
                    .StubMetadataValue(TestPropertyName, expected);
                actual = target.GetTableProperty<Guid>(TestPropertyName);
            }
            Verify.AreEqual(expected, actual);
        }

        [ExpectedArgumentException("propertyName")]
        [TestMethod]
        public void GetTablePropertyWithEmptyName()
        {
            using (var account = CreateStorageAccountStub(true))
            {
                var target = account.GetStorageTable<SampleEntity>();
                target.GetTableProperty<Guid>("");
            }
        }

        [ExpectedException(typeof(AzureTablePropertyNotFoundException))]
        [TestMethod]
        public void GetTablePropertyWithInvalidName()
        {
            using (var account = CreateStorageAccountStub(true))
            {
                var target = account.GetStorageTable<SampleEntity>();
                target.GetTableProperty<Guid>("missing");
            }
        }

        [ExpectedArgumentNullException("propertyName")]
        [TestMethod]
        public void GetTablePropertyWithNullName()
        {
            using (var account = CreateStorageAccountStub(true))
            {
                var target = account.GetStorageTable<SampleEntity>();
                target.GetTableProperty<Guid>(null);
            }
        }

        [ExpectedException(typeof(InvalidCastException))]
        [TestMethod]
        public void GetTablePropertyWithInvalidType()
        {
            const string TestPropertyName = "SampleProperty";
            var expected = Guid.NewGuid();

            using (var account = CreateStorageAccountStub(true))
            {
                var target = account.GetStorageTable<SampleEntity>()
                    .StubMetadataValue(TestPropertyName, expected);
                target.GetTableProperty<bool>(TestPropertyName);
            }
        }
    }
}
