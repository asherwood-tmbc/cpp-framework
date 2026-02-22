using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using CPP.Framework.UnitTests.Testing;
using CPP.Framework.WindowsAzure.Storage;

using FluentAssertions;

using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

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

            var account = Substitute.For<AzureStorageAccount>();
            account.Configure().LoadAzureAccountName().Returns(TestAccountName);
            account.Configure().LoadConnectionString().Throws<InvalidOperationException>();
            account.Configure().GetServiceBus().Returns(callInfo => CreateServiceBusStub(account));
            account.Configure().GetServiceBus(Arg.Any<string>()).Throws<InvalidOperationException>();
            account.RegisterServiceStub(TestAccountName);
            AzureServiceBusTopic.SendEventMessage(expect);

            this.CustomContext.Received.Count.Should().Be(1);
            this.CustomContext.Received.Contains(expect).Should().BeTrue();
        }

        [TestMethod]
        public void SendQueueMessageWithCustomProperties()
        {
            var expect = new SampleMessageModelWithProperties();

            var account = Substitute.For<AzureStorageAccount>();
            account.Configure().LoadAzureAccountName().Returns(TestAccountName);
            account.Configure().LoadConnectionString().Throws<InvalidOperationException>();
            account.Configure().GetServiceBus().Returns(callInfo => CreateServiceBusStub(account));
            account.Configure().GetServiceBus(Arg.Any<string>()).Throws<InvalidOperationException>();
            account.RegisterServiceStub(TestAccountName);
            AzureServiceBusTopic.SendEventMessage(expect);

            this.CustomContext.Received.Count.Should().Be(1);
            this.CustomContext.Received.Contains(expect).Should().BeTrue();
            this.CustomContext.Received.First().Should().BeOfType<SampleMessageModelWithProperties>();

            this.CustomContext.Messages.Count.Should().Be(1);
            this.CustomContext.Messages[0].Properties.ContainsKey(SampleGuidValueName).Should().BeTrue();
            this.CustomContext.Messages[0].Properties[SampleGuidValueName].Should().Be(expect.GuidValue.ToString("B"));
            this.CustomContext.Messages[0].Properties.ContainsKey(SampleLongValueName).Should().BeTrue();
            this.CustomContext.Messages[0].Properties[SampleLongValueName].Should().Be(expect.LongValue);
            this.CustomContext.Messages[0].Properties.ContainsKey(SampleStaticValName).Should().BeTrue();
            this.CustomContext.Messages[0].Properties[SampleStaticValName].Should().Be(SampleStaticVal);
        }

        #region Internal Helper Functions

        private AzureServiceBus CreateServiceBusStub(AzureStorageAccount account)
        {
            var instance = Substitute.For<AzureServiceBus>(account, "ignored");
            instance.Configure().GetTopic(Arg.Is(TargetTopicName))
                .Returns(callInfo => CreateServiceBusTopicStub(instance, callInfo.ArgAt<string>(0)));
            instance.Configure().GetQueue(Arg.Any<string>()).Throws<InvalidOperationException>();
            return instance;
        }

        private AzureServiceBusTopic CreateServiceBusTopicStub(AzureServiceBus serviceBus, string topicName)
        {
            var instance = Substitute.ForPartsOf<AzureServiceBusTopic>(serviceBus, topicName);
            instance.Configure().CreateIfNotExists().Returns(true);
            instance.When(stub => stub.SendEventMessage(Arg.Is(TargetEventName), Arg.Any<SampleMessageModel>(), Arg.Any<DateTime?>()))
                .Do(callInfo =>
                    {
                        this.CustomContext.Received.Add(callInfo.ArgAt<SampleMessageModel>(1));
                    });
            instance.When(stub => stub.SendEventMessage(Arg.Is(TargetEventName), Arg.Any<SampleMessageModelWithProperties>(), Arg.Any<DateTime?>()))
                .Do(callInfo =>
                    {
                        this.CustomContext.Received.Add(callInfo.ArgAt<SampleMessageModelWithProperties>(1));
                    });
            instance.When(stub => stub.SendMessage(Arg.Any<BrokeredMessage>())).DoNotCallBase();
            instance.When(stub => stub.SendMessage(Arg.Any<BrokeredMessage>()))
                .Do(callInfo =>
                    {
                        this.CustomContext.Messages.Add(callInfo.ArgAt<BrokeredMessage>(0));
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
