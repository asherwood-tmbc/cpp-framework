using System;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;

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
            var account = StubFactory.CreatePartial<AzureStorageAccount>("UseDevelopmentStorage=true", $"{Guid.NewGuid():N}");
            account
                .StubAction(stub => stub.GetServiceBus())
                .WhenCalled((GetServiceBus) =>
                    {
                        var serviceBus = StubFactory.CreateStub<AzureServiceBus>(account, account.ConnectionString);
                        serviceBus.StubAction(stub => stub.GetTopic(Arg<string>.Is.Equal(TestTopicName)))
                            .WhenCalled((GetTopic) =>
                                {
                                    var topic = StubFactory.CreateStub<AzureServiceBusTopic>(serviceBus, GetTopic.Arguments[0])
                                        .StubAction(stub => stub.SendEventMessage(Arg.Is(TestEventName), Arg.Is(model)))
                                        .DoNothing()
                                        .StubAction(stub => stub.SendEventMessage(Arg<string>.Is.Anything, Arg.Is(model)))
                                        .Throw(new InvalidOperationException());
                                    GetTopic.ReturnValue = topic;
                                });
                        GetServiceBus.ReturnValue = serviceBus;
                    })
                .RegisterServiceStub();
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
