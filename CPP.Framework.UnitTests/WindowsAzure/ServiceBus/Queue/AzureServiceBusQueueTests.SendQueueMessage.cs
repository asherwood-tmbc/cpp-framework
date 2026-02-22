using System;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.UnitTests.Testing;
using CPP.Framework.WindowsAzure.Storage;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

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

            var account = Substitute.For<AzureStorageAccount>();
            account.Configure().LoadAzureAccountName().Returns(TestAccountName);
            account.Configure().LoadConnectionString().Throws<InvalidOperationException>();
            account.Configure().GetServiceBus().Returns(callInfo => CreateServiceBusStub(account));
            account.Configure().GetServiceBus(Arg.Any<string>()).Throws<InvalidOperationException>();
            account.RegisterServiceStub(TestAccountName);
            AzureServiceBusQueue.SendQueueMessage(expect);

            this.CustomContext.Received.Count.Should().Be(1);
            this.CustomContext.Received.Contains(expect).Should().BeTrue();
        }

        #region Internal Helper Functions

        private AzureServiceBus CreateServiceBusStub(AzureStorageAccount account)
        {
            var instance = Substitute.For<AzureServiceBus>(account, "ignored");
            instance.Configure().GetQueue(Arg.Any<string>()).Throws<InvalidOperationException>();
            instance.Configure().GetQueue(Arg.Is(TargetQueueName))
                .Returns(callInfo => CreateServiceBusQueueStub(instance, callInfo.ArgAt<string>(0)));
            return instance;
        }

        private AzureServiceBusQueue CreateServiceBusQueueStub(AzureServiceBus serviceBus, string queueName)
        {
            var instance = Substitute.For<AzureServiceBusQueue>(serviceBus, queueName);
            instance.When(stub => stub.SendMessage(Arg.Any<SampleMessageModel>(), Arg.Any<DateTime?>()))
                .Do(callInfo =>
                    {
                        if (callInfo.ArgAt<object>(0) is SampleMessageModel model)
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
