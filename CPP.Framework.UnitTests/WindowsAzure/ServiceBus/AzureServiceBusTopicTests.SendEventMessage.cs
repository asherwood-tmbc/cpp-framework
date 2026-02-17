using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rhino.Mocks;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public partial class AzureServiceBusTopicTests
    {
        private const string SampleGuidValueName = "GuidValue";
        private const string SampleLongValueName = "LongValue";
        private const string SampleStaticVal = "This is a sample value";
        private const string SampleStaticValName = "StaticVal";

        private const string TestAccountName = "TestAzureAccount";
        private const string TargetTopicName = "SampleQueue";
        private const string TargetEventName = "TopicTestEvent";

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
            AzureServiceBusTopic.SendEventMessage(expect);

            Verify.AreEqual(1, this.CustomContext.Received.Count);
            Verify.IsTrue(this.CustomContext.Received.Contains(expect));
        }

        [TestMethod]
        public void SendQueueMessageWithCustomProperties()
        {
            var expect = new SampleMessageModelWithProperties();

            StubFactory.CreatePartial<AzureStorageAccount>()
                .StubAction(stub => stub.LoadAzureAccountName()).Return(TestAccountName)
                .StubAction(stub => stub.LoadConnectionString()).Throw<InvalidOperationException>()
                .StubAction(stub => stub.GetServiceBus()).Return(CreateServiceBusStub)
                .StubAction(stub => stub.GetServiceBus(Arg<string>.Is.Anything)).Throw<InvalidOperationException>()
                .RegisterServiceStub(TestAccountName);
            AzureServiceBusTopic.SendEventMessage(expect);

            Verify.AreEqual(1, this.CustomContext.Received.Count);
            Verify.IsTrue(this.CustomContext.Received.Contains(expect));
            Verify.IsInstanceOfType(this.CustomContext.Received.First(), typeof(SampleMessageModelWithProperties));

            Verify.AreEqual(1, this.CustomContext.Messages.Count);
            Verify.IsTrue(this.CustomContext.Messages[0].Properties.ContainsKey(SampleGuidValueName));
            Verify.AreEqual(expect.GuidValue.ToString("B"), this.CustomContext.Messages[0].Properties[SampleGuidValueName]);
            Verify.IsTrue(this.CustomContext.Messages[0].Properties.ContainsKey(SampleLongValueName));
            Verify.AreEqual(expect.LongValue, this.CustomContext.Messages[0].Properties[SampleLongValueName]);
            Verify.IsTrue(this.CustomContext.Messages[0].Properties.ContainsKey(SampleStaticValName));
            Verify.AreEqual(SampleStaticVal, this.CustomContext.Messages[0].Properties[SampleStaticValName]);
        }

        #region Internal Helper Functions

        private AzureServiceBus CreateServiceBusStub(AzureStorageAccount account)
        {
            var instance = StubFactory.CreatePartial<AzureServiceBus>(account, "ignored");
            instance.StubAction(stub => stub.GetTopic(Arg.Is(TargetTopicName)))
                .WhenCalled(
                    (mi) =>
                    {
                        mi.ReturnValue = CreateServiceBusTopicStub(instance, ((string)mi.Arguments[0]));
                    })
                .StubAction(stub => stub.GetQueue(Arg<string>.Is.Anything)).Throw<InvalidOperationException>();
            return instance;
        }

        private AzureServiceBusTopic CreateServiceBusTopicStub(AzureServiceBus serviceBus, string topicName)
        {
            var instance = StubFactory.CreatePartial<AzureServiceBusTopic>(serviceBus, topicName)
                .StubAction(stub => stub.SendEventMessage(Arg.Is(TargetEventName), Arg<SampleMessageModel>.Is.Anything, Arg<DateTime?>.Is.Anything))
                .WhenCalled(
                    (mi, sbt) =>
                        {
                            this.CustomContext.Received.Add(mi.Arguments[1]);
                            mi.CallOriginalMethod(sbt);
                        })
                .StubAction(stub => stub.SendEventMessage(Arg.Is(TargetEventName), Arg<SampleMessageModelWithProperties>.Is.Anything, Arg<DateTime?>.Is.Anything))
                .WhenCalled(
                    (mi, sbt) =>
                        {
                            this.CustomContext.Received.Add(mi.Arguments[1]);
                            mi.CallOriginalMethod(sbt);
                        })
                .StubAction(stub => stub.SendMessage(Arg<BrokeredMessage>.Is.Anything))
                .WhenCalled(
                    (mi, sbt) =>
                        {
                            this.CustomContext.Messages.Add((BrokeredMessage)mi.Arguments[0]);
                        });
            return instance;
        }

        #endregion // Internal Helper Functions

        #region SampleMessageModel Class Declaration

        [AzureAccountName(TestAccountName)]
        [AzureServiceBusTopic(TargetTopicName, TargetEventName)]
        private class SampleMessageModel : IEquatable<SampleMessageModel>
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

        #region SampleMessageModelWithProperties Class Delcaration
        
        [AzureAccountName(TestAccountName)]
        [AzureMessageProperty(SampleGuidValueName, "{" + nameof(GuidValue) + "}", FormatString = "{0:B}")]
        [AzureMessageProperty(SampleLongValueName, "{" + nameof(LongValue) + "}")]
        [AzureMessageProperty(SampleStaticValName, SampleStaticVal)]
        [AzureServiceBusTopic(TargetTopicName, TargetEventName)]
        private sealed class SampleMessageModelWithProperties : IEquatable<SampleMessageModelWithProperties>
        {
            private Guid ID { get; } = Guid.NewGuid();
            public bool Equals(SampleMessageModelWithProperties that)
            {
                if (ReferenceEquals(this, that)) return true;
                if (ReferenceEquals(null, that)) return false;
                return (this.ID == that.ID);
            }
            [JsonIgnore]
            public long LongValue { get; } = new Random(0).Next();
            [JsonIgnore]
            public Guid GuidValue { get; } = Guid.NewGuid();
            public override bool Equals(object that) => this.Equals(that as SampleMessageModelWithProperties);
            public override int GetHashCode() => this.ID.GetHashCode();
        }

        #endregion // SampleMessageModelWithProperties Class Delcaration
    }
}
