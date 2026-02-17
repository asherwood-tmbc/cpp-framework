using System;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;

namespace CPP.Framework.WindowsAzure.WebJobs
{
    /// <summary>
    /// Configuration provider class that connects the web job logs with the <see cref="Journal"/>.
    /// </summary>
    internal class AzureWebJobJournalConfigProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes the extension. Initialization should register any extension bindings with 
        /// the <see cref="IExtensionRegistry"/> instance, which can be obtained from the 
        /// <see cref="JobHostConfiguration"/> which is an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="context">The <see cref="ExtensionConfigContext"/></param>
        public void Initialize(ExtensionConfigContext context)
        {
            AzureWebJobJournalListener listener;
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(TraceWriter), context.Trace), 
            };
            if (!ServiceLocator.TryGetInstance(out listener, resolvers))
            {
                listener = new AzureWebJobJournalListener(context.Trace);
            }
            Journal.Listeners.Add(listener);
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="AzureWebJobJournalConfigProvider"/> class.
    /// </summary>
    public static class AzureWebJobJournalConfigProviderExtensions
    {
        /// <summary>
        /// Configures a <see cref="JobHost"/> to allow messages written to the <see cref="Journal"/> 
        /// to be written to the console log.
        /// </summary>
        /// <param name="configuration">The <see cref="JobHostConfiguration"/> for the job host.</param>
        public static void UseJournal(this JobHostConfiguration configuration)
        {
            ArgumentValidator.ValidateThisObj(() => configuration);
            Journal.ConfigureInstanceName(
                () =>
                    {
                        var webJobName = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
                        if (webJobName != null)
                        {
                            var siteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
                            if (!string.IsNullOrWhiteSpace(siteName)) siteName = $"{siteName}-";
                            webJobName = $"{siteName}{webJobName}";
                        }
                        return webJobName;
                    },
                true);
            var registry = configuration.GetService<IExtensionRegistry>();
            if (registry != null)
            {
                var provider = new AzureWebJobJournalConfigProvider();
                registry.RegisterExtension(typeof(IExtensionConfigProvider), provider);
            }
            configuration.Tracing.ConsoleLevel = Journal.SeverityLevel.AsTraceLevel();
        }
    }
}
