using System;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.WindowsAzure.Storage
{
    public partial class AzureStorageTableTests
    {
        [TestMethod]
        public void SetTableProperty()
        {
            const string TestPropertyName = "SampleProperty";
            Guid expected = Guid.NewGuid(), actual;

            using(var account = CreateStorageAccountStub(true))
            {
                var table = account.GetStorageTable<SampleEntity>();
                table.SetTableProperty(TestPropertyName, expected);
                actual = table.GetTableProperty<Guid>(TestPropertyName);
            }
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetTablePropertyWithMultipleTypes()
        {
            const string TestPropertyName1 = "SampleProperty1";
            const string TestPropertyName2 = "SampleProperty2";

            bool expected1 = true, actual1;
            Guid expected2 = Guid.NewGuid(), actual2;

            using (var account = CreateStorageAccountStub(true))
            {
                var table = account.GetStorageTable<SampleEntity>();
                table.SetTableProperty(TestPropertyName1, expected1);
                actual1 = table.GetTableProperty<bool>(TestPropertyName1);
                table.SetTableProperty(TestPropertyName2, expected2);
                actual2 = table.GetTableProperty<Guid>(TestPropertyName2);
            }

            Verify.AreEqual(expected1, actual1);
            Verify.AreEqual(expected2, actual2);
        }
    }
}
