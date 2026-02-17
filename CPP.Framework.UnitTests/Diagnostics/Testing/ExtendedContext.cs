using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class ExtendedContext : TestContext
    {
        protected ExtendedContext(TestContext innerContext) => this.InnerContext = innerContext;

        public override sealed DbConnection DataConnection => this.InnerContext.DataConnection;
        public override sealed DataRow DataRow => this.InnerContext.DataRow;
        private TestContext InnerContext { get; }
        public override sealed IDictionary Properties => this.InnerContext.Properties;

        public override sealed void AddResultFile(string fileName) => this.InnerContext.AddResultFile(fileName);
        public override sealed void BeginTimer(string timerName) => this.InnerContext.BeginTimer(timerName);
        public override sealed void EndTimer(string timerName) => this.InnerContext.EndTimer(timerName);
        public override sealed void WriteLine(string format, params object[] args) => this.InnerContext.WriteLine(format, args);
    }
}
