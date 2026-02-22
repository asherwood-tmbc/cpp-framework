using System;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.UnitTests.Testing;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AzureServiceBusSubscriptionQueueTests
    {
        private const string TestTopicName = "TestTopic";
        private const string TestEventName = "TestEvent";

        [TestMethod]
        public void EnqueueWithAttributedEventName()
        {
            var model = new AttributedtMessageModel();
            var account = Substitute.ForPartsOf<AzureStorageAccount>("UseDevelopmentStorage=true", $"{Guid.NewGuid():N}");
            account.Configure().GetServiceBus()
                .Returns(callInfo =>
                    {
                        var serviceBus = Substitute.For<AzureServiceBus>(account, account.ConnectionString);
                        serviceBus.GetTopic(Arg.Is(TestTopicName))
                            .Returns(getTopicCallInfo =>
                                {
                                    var topic = Substitute.For<AzureServiceBusTopic>(serviceBus, getTopicCallInfo.ArgAt<string>(0));
                                    topic.When(t => t.SendEventMessage(Arg.Any<string>(), Arg.Is(model))).Throw<InvalidOperationException>();
                                    topic.When(t => t.SendEventMessage(Arg.Is(TestEventName), Arg.Is(model))).Do(ci => { });
                                    return topic;
                                });
                        return serviceBus;
                    });
            account.RegisterServiceStub();
            TestServiceBusQueue.Current.Enqueue(new AttributedtMessageModel());
        }

        #region Internal Test Class Declarations

        private abstract class AbstractMessageModel { }

        [DefaultEventName(TestEventName)]
        private class AttributedtMessageModel : AbstractMessageModel { }

        private class TestServiceBusQueue : AzureServiceBusSubscriptionQueue<AbstractMessageModel>
        {
            private static readonly ServiceInstance<TestServiceBusQueue> _ServiceInstance = new ServiceInstance<TestServiceBusQueue>();

            protected TestServiceBusQueue() : base(null, TestTopicName) { }

            public static TestServiceBusQueue Current => _ServiceInstance.GetInstance();
        }

        #endregion // Internal Test Class Declarations
    }
}
