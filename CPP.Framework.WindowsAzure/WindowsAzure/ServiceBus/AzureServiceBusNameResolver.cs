using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Service class used to resolve the name of service bus subscriptions for a Windows Azure
    /// Web Job.
    /// </summary>
    public class AzureServiceBusNameResolver : SingletonServiceBase, INameResolver
    {
        private static readonly ServiceInstance<AzureServiceBusNameResolver> _ServiceInstance = new ServiceInstance<AzureServiceBusNameResolver>();

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        protected AzureServiceBusNameResolver() { }

        /// <summary>
        /// Gets a reference to the current instance for the application.
        /// </summary>
        public static AzureServiceBusNameResolver Current { get { return _ServiceInstance.GetInstance(); } }

        /// <summary>
        /// Resolve a %name% to a value. Resolution is not recursive.
        /// </summary>
        /// <param name="name">The name to resolve (without the %... %)</param>
        /// <returns>The value to which the name resolves, if the name is supported; otherwise <see langword="null"/>.</returns>
        public string Resolve(string name)
        {
            if (Debugger.IsAttached && (!String.IsNullOrWhiteSpace(name)))
            {
                name = String.Format("{0}.{1}", name, Environment.MachineName);
            }
            return name;
        }
    }
}
