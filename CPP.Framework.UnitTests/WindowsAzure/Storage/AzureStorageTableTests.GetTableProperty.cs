using System;
using CPP.Framework.DependencyInjection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

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
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetTablePropertyWithEmptyName()
        {
            Action act = () =>
            {
                using (var account = CreateStorageAccountStub(true))
                {
                    var target = account.GetStorageTable<SampleEntity>();
                    target.GetTableProperty<Guid>("");
                }
            };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("propertyName");
        }

        [TestMethod]
        public void GetTablePropertyWithInvalidName()
        {
            Action act = () =>
            {
                using (var account = CreateStorageAccountStub(true))
                {
                    var target = account.GetStorageTable<SampleEntity>();
                    target.GetTableProperty<Guid>("missing");
                }
            };
            act.Should().Throw<AzureTablePropertyNotFoundException>();
        }

        [TestMethod]
        public void GetTablePropertyWithNullName()
        {
            Action act = () =>
            {
                using (var account = CreateStorageAccountStub(true))
                {
                    var target = account.GetStorageTable<SampleEntity>();
                    target.GetTableProperty<Guid>(null);
                }
            };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyName");
        }

        [TestMethod]
        public void GetTablePropertyWithInvalidType()
        {
            const string TestPropertyName = "SampleProperty";
            var expected = Guid.NewGuid();

            Action act = () =>
            {
                using (var account = CreateStorageAccountStub(true))
                {
                    var target = account.GetStorageTable<SampleEntity>()
                        .StubMetadataValue(TestPropertyName, expected);
                    target.GetTableProperty<bool>(TestPropertyName);
                }
            };
            act.Should().Throw<InvalidCastException>();
        }
    }
}
