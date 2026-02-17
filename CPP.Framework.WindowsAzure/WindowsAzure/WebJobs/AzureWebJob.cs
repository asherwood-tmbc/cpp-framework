using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CPP.Framework.Configuration;
using CPP.Framework.Diagnostics;
using CPP.Framework.WindowsAzure.ServiceBus;
using CPP.Framework.WindowsAzure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace CPP.Framework.WindowsAzure.WebJobs
{
    /// <summary>
    /// Delegate used by the <see cref="AzureWebJob"/> class to get a subscription for a topic.
    /// </summary>
    /// <param name="topic">The topic associated with the subscription.</param>
    /// <returns>An <see cref="AzureServiceBusSubscription"/> object.</returns>
    public delegate AzureServiceBusSubscription GetAzureSubscriptionDelegate(AzureServiceBusTopic topic);

    /// <summary>
    /// Represents the host process for a Windows Azure Web Job.
    /// </summary>
    public class AzureWebJob : IDisposable
    {
        private readonly HashSet<AzureStorageQueue> _StorageQueues = new HashSet<AzureStorageQueue>();
        private readonly HashSet<AzureServiceBusSubscription> _Subscriptions = new HashSet<AzureServiceBusSubscription>();

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="serviceBus">The <see cref="AzureStorageAccount"/> associated with the web job.</param>
        public AzureWebJob(AzureServiceBus serviceBus)
        {
            ArgumentValidator.ValidateNotNull(() => serviceBus);
            this.ServiceBus = serviceBus;
        }

        /// <summary>
        /// Gets the <see cref="AzureStorageAccount"/> associated with the web job.
        /// </summary>
        public AzureServiceBus ServiceBus { get; private set; }

        /// <summary>
        /// Registers a storage queue with the web job host.
        /// </summary>
        /// <param name="queueName">The name of the storage queue.</param>
        /// <returns>A reference to the current <see cref="AzureWebJob"/> object.</returns>
        public virtual AzureWebJob AppendStorageQueue(string queueName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => queueName);
            _StorageQueues.Add(this.ServiceBus.Account.GetStorageQueue(queueName));
            return this;
        }

        /// <summary>
        /// Registers a storage queue with the web job host.
        /// </summary>
        /// <param name="queue">The storage queue to register.</param>
        /// <returns>A reference to the current <see cref="AzureWebJob"/> object.</returns>
        public virtual AzureWebJob AppendStorageQueue(AzureStorageQueue queue)
        {
            ArgumentValidator.ValidateNotNull(() => queue);
            Journal.WriteDebug("Registering StorageQueue \"{0}\"", queue.ObjectName);
            _StorageQueues.Add(queue);
            return this;
        }

        /// <summary>
        /// Registers a subscription with the web job host.
        /// </summary>
        /// <param name="topicName">The name of the service bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="eventName">The name of the message event to monitor.</param>
        /// <returns>A reference to the current <see cref="AzureWebJob"/> object.</returns>
        public virtual AzureWebJob AppendSubscription(string topicName, string subscriptionName, string eventName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => topicName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);

            var topic = this.ServiceBus.GetTopic(topicName);
            var subscription = topic.GetSubscription(subscriptionName, eventName);
            this.AppendSubscription(subscription);

            return this;
        }

        /// <summary>
        /// Registers a subscription with the web job host.
        /// </summary>
        /// <param name="subscription">The subscription to register.</param>
        /// <returns>A reference to the current <see cref="AzureWebJob"/> object.</returns>
        public virtual AzureWebJob AppendSubscription(AzureServiceBusSubscription subscription)
        {
            ArgumentValidator.ValidateNotNull(() => subscription);
            
            Journal.CreateSource<AzureWebJob>()
                .WriteDebug("Registering Subscription \"{0}\"", subscription.ObjectName)
                .WriteVerbose("Topic: {0}", subscription.Topic.ObjectName)
                .WriteVerbose("Event: {0}", subscription.EventName);
            var target = subscription;
            if (target.ServiceBus != this.ServiceBus)
            {
                var topic = this.ServiceBus.GetTopic(subscription.Topic.ObjectName);
                target = topic.GetSubscription(subscription.ObjectName, subscription.EventName);
            }
            _Subscriptions.Add(target);

            return this;
        }

        /// <summary>
        /// Automatically discovers and registers declared message handlers in the current 
        /// <see cref="AppDomain"/> using reflection.
        /// </summary>
        protected internal virtual void AutoRegisterMethods()
        {
            try
            {
                var assemblyMethods = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => (asm.GetTypes()))
                    .Where(ti => (ti.IsClass))
                    .SelectMany(ti => ti.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(mi => (!mi.IsAbstract));
                foreach (var methodInfo in assemblyMethods)
                {
                    this.AutoRegisterMethodTriggers(methodInfo);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loadex = ex.LoaderExceptions.FirstOrDefault();
                if (null != loadex)
                {
                    throw loadex;
                }
                throw;
            }
        }

        /// <summary>
        /// Detects any service bus or storage queue triggers on the target method, and 
        /// automatically registers any related subscription/queue with the webjob host.
        /// </summary>
        /// <param name="methodInfo">The target method to run discovery against.</param>
        protected internal void AutoRegisterMethodTriggers(MethodInfo methodInfo)
        {
            foreach (var parameter in methodInfo.GetParameters())
            {
                // look for subscription triggers first.
                var trigger = parameter.GetCustomAttributes(typeof(ServiceBusTriggerAttribute))
                    .OfType<ServiceBusTriggerAttribute>()
                    .Where(pi => (!String.IsNullOrWhiteSpace(pi.TopicName)))
                    .Where(pi => (!String.IsNullOrWhiteSpace(pi.SubscriptionName)))
                    .FirstOrDefault();
                if (null != trigger)
                {
                    // now see if there an event name as well; if not, default to any ("*").
                    var eventName = parameter.GetCustomAttributes(typeof(ServiceBusTriggerEventAttribute))
                        .OfType<ServiceBusTriggerEventAttribute>()
                        .Select(ai => ai.EventName)
                        .DefaultIfEmpty(AzureServiceBusTopic.AnyEventName)
                        .FirstOrDefault();
                    var subscriptionName = trigger.SubscriptionName.Trim('%');
                    var topicName = trigger.TopicName;
                    this.AppendSubscription(topicName, subscriptionName, eventName);
                    break;  // complete processing and exit
                }

                // if no subscription triggers were found on the current method, then look for
                // a storage queue trigger instead.
                var queueName = methodInfo.GetParameters()
                    .SelectMany(pi => pi.GetCustomAttributes(typeof(QueueTriggerAttribute)))
                    .OfType<QueueTriggerAttribute>()
                    .Where(ai => (!String.IsNullOrWhiteSpace(ai.QueueName)))
                    .Select(ai => ai.QueueName)
                    .FirstOrDefault();
                if (null != queueName)
                {
                    // register the queue with the webjob host.
                    this.AppendStorageQueue(queueName.Trim('%'));
                    break;  // complete processing and exit
                }
            }
        }

        /// <summary>
        /// Executes an on-demand web job method.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the method to execute.</param>
        public virtual void CallOnce(Expression<Action<TextWriter>> expression)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            var methodInfo = expression.GetMethodInfo();

            // the webjobs SDK does not currently support debugging on-demand web jobs, so we have
            // to work around it by invoking the method directly ourselves using reflection.
            if (Debugger.IsAttached)
            {
                var parameters = new object[]
                {
                    Console.Out,
                };
                var task = (methodInfo.Invoke(null, parameters) as Task);

                if (task != null) task.Wait();
                Console.Out.Write("Press any key to continue...");
                Console.ReadKey(true);
            }
            else this.CreateJobHost().Call(methodInfo);
        }

        /// <summary>
        /// Executes an on-demand web job method.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the method to execute.</param>
        /// <param name="context">An optional context object to pass to the method call.</param>
        public virtual void CallOnce<TContext>(Expression<Action<TContext, TextWriter>> expression, TContext context)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            var methodInfo = expression.GetMethodInfo();

            // the webjobs SDK does not currently support debugging on-demand web jobs, so we have
            // to work around it by invoking the method directly ourselves using reflection.
            if (Debugger.IsAttached)
            {
                var parameters = new object[]
                {
                    context,
                    Console.Out,
                };
                var task = (methodInfo.Invoke(null, parameters) as Task);

                if (task != null) task.Wait();
                Console.Out.Write("Press any key to continue...");
                Console.ReadKey(true);
            }
            else
            {
                var parameters = new Dictionary<string, object>
                {
                    { methodInfo.GetParameters()[0].Name, context }
                };
                this.CreateJobHost().Call(methodInfo, parameters);
            }
        }

        /// <summary>
        /// Creates and configures a new <see cref="JobHost"/> instance for the current application.
        /// </summary>
        /// <returns>A <see cref="JobHost"/> object.</returns>
        private JobHost CreateJobHost()
        {
            Journal.WriteDebug("Configuring WebJob Host");
            var configuration = new JobHostConfiguration
            {
                DashboardConnectionString = this.ServiceBus.Account.ConnectionString,
                StorageConnectionString = this.ServiceBus.Account.ConnectionString,
                NameResolver = this.ServiceBus.NameResolver,
            };

            configuration.Queues.BatchSize = ConfigSettingProvider.Current.GetSetting(
                ConfigSettingKey.WebJobQueueBatchSize, Int32.Parse);
            configuration.Queues.MaxDequeueCount = ConfigSettingProvider.Current.GetSetting(
                ConfigSettingKey.WebJobQueueMaxDequeueCount, Int32.Parse);
            configuration.Queues.MaxPollingInterval = ConfigSettingProvider.Current.GetSetting(
                ConfigSettingKey.WebJobQueueMaxPollingInterval, (s => TimeSpan.FromSeconds(Int32.Parse(s))));
            configuration.UseServiceBus(new ServiceBusConfiguration
            {
                ConnectionString = this.ServiceBus.ConnectionString,
                PrefetchCount = ConfigSettingProvider.Current.GetSetting("ServiceBusPrefetchCount", (s => Int32.Parse(s)), "0"),
                MessageOptions = new OnMessageOptions
                {
                    AutoComplete = true,
                    AutoRenewTimeout = ConfigSettingProvider.Current.GetSetting("ServiceBusAutoRenewTimeout", TimeSpan.Parse, "3:00:00"),
                    MaxConcurrentCalls = ConfigSettingProvider.Current.GetSetting("ServiceBusMaxConcurrentCalls", (s => Int32.Parse(s)), "16"),
                },
            });
            configuration.UseJournal();

            return new JobHost(configuration);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            foreach(var subscription in _Subscriptions)
            {
                ((IDisposable)subscription).Dispose();
            }
        }

        /// <summary>
        /// Starts the job host and waits for subscription events on the service bus.
        /// </summary>
        public virtual void Run() { this.Run(false); }

        /// <summary>
        /// Starts the job host and waits for subscription events on the service bus.
        /// </summary>
        /// <param name="autoRegister">True to automatically discover message triggers and create subscriptions or handlers for them; otherwise, false.</param>
        public virtual void Run(bool autoRegister)
        {
            if (autoRegister)
            {
                this.AutoRegisterMethods();
            }
            var host = this.CreateJobHost();

            foreach (var queue in _StorageQueues)
            {
                var options = queue.RequestOptions.CreateOptions<QueueRequestOptions>();
                queue.GetCloudQueue().CreateIfNotExists(options);
            }
            foreach (var subscription in _Subscriptions)
            {
                subscription.Listen();
            }
            try
            {
                Journal.WriteInfo("Starting WebJob Host");
                host.RunAndBlock();
            }
            finally { Journal.WriteInfo("WebJob Host Stopped"); }
        }

        /// <summary>
        /// Automatically discovers and registers any message handlers in the application domain,
        /// and then starts the service bus host to wait for subscription or storage queue events.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account to use for the webjob host.</param>
        public static void Run(string storageAccountName)
        {
            using (var account = AzureStorageAccount.GetInstance(storageAccountName))
            using (var host = account.GetServiceBus().CreateWebJob())
            {
                host.Run(true); // start the web job message pump and wait
            }
        }
    }
}
