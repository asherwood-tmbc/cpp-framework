using System;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public partial class AzureServiceBusQueueTests
    {
        private const string TestAccountName = "TestAzureAccount";
        private const string TargetQueueName = "SampleQueue";

        [TestMethod]
        public void SendQueueMessage()
        {
            var expect = new SampleMessageModel();

            StubFactory.CreatePartial<AzureStorageAccount>()
                .StubAction(stub => stub.LoadAzureAccountName()).Return(TestAccountName)
                .StubAction(stub => stub.LoadConnectionString()).Throw<InvalidOperationException>()
                .StubAction(stub => stub.GetServiceBus()).Return(CreateServiceBusStub)
                .StubAction(stub => stub.GetServiceBus(Arg<string>.Is.Anything)).Throw<InvalidOperationException>()
                .RegisterServiceStub(TestAccountName);
            AzureServiceBusQueue.SendQueueMessage(expect);

            Verify.AreEqual(1, this.CustomContext.Received.Count);
            Verify.IsTrue(this.CustomContext.Received.Contains(expect));
        }

        #region Internal Helper Functions

        private AzureServiceBus CreateServiceBusStub(AzureStorageAccount account)
        {
            var instance = StubFactory.CreatePartial<AzureServiceBus>(account, "ignored");
            instance.StubAction(stub => stub.GetQueue(Arg.Is(TargetQueueName)))
                .WhenCalled(
                    (mi) =>
                        {
                            mi.ReturnValue = CreateServiceBusQueueStub(instance, ((string)mi.Arguments[0]));
                        })
                .StubAction(stub => stub.GetQueue(Arg<string>.Is.Anything)).Throw<InvalidOperationException>();
            return instance;
        }

        private AzureServiceBusQueue CreateServiceBusQueueStub(AzureServiceBus serviceBus, string queueName)
        {
            var instance = StubFactory.CreatePartial<AzureServiceBusQueue>(serviceBus, queueName)
                .StubAction(stub => stub.SendMessage(Arg<SampleMessageModel>.Is.Anything, Arg<DateTime?>.Is.Anything))
                .WhenCalled(
                    (mi) =>
                        {
                            if (mi.Arguments[0] is SampleMessageModel model)
                            {
                                this.CustomContext.Received.Add(model);
                            }
                        });
            return instance;
        }

        #endregion // Internal Helper Functions

        #region SampleMessageModel Class Declaration

        [AzureAccountName(TestAccountName)]
        [AzureServiceBusQueue(TargetQueueName)]
        private sealed class SampleMessageModel : IEquatable<SampleMessageModel>
        {
            private Guid ID { get; } = Guid.NewGuid();
            public bool Equals(SampleMessageModel that)
            {
                if (ReferenceEquals(this, that)) return true;
                if (ReferenceEquals(null, that)) return false;
                return (this.ID == that.ID);
            }
            public override bool Equals(object that) => this.Equals(that as SampleMessageModel);
            public override int GetHashCode() => this.ID.GetHashCode();
        }

        #endregion // SampleMessageModel Class Declaration
    }
}
