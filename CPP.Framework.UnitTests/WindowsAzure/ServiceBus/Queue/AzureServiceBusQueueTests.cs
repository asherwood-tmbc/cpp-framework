using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public partial class AzureServiceBusQueueTests
    {
        private ExtendendContext CustomContext { get; set; }

        public TestContext TestContext
        {
            get => this.CustomContext;
            set => this.CustomContext = new ExtendendContext(value);
        }

        #region ExtendendContext Class Declaration

        private sealed class ExtendendContext : CPP.Framework.Diagnostics.Testing.ExtendedContext
        {
            public ExtendendContext(TestContext innerContext) : base(innerContext) { }
            public HashSet<SampleMessageModel> Received { get; } = new HashSet<SampleMessageModel>();
        }

        #endregion // ExtendendContext Class Declaration
    }
}
