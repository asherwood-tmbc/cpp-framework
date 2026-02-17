using CPP.Framework.DependencyInjection;

using JetBrains.Annotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Abstract base class for all objects that define unit test or integration test methods.
    /// </summary>
    [TestClass]
    public class TestSuite
    {
        /// <summary>
        /// Gets or sets the <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.TestContext"/>
        /// reference for the current test suite. Please note that this is set automatically by the
        /// testing framework, and does not require any modifications.
        /// </summary>
        [UsedImplicitly]
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Called by the testing framework just prior to executing a test method.
        /// </summary>
        [TestInitialize]
        protected virtual void OnTestStartup() { }

        /// <summary>
        /// Called by the testing framework just after executing a test method.
        /// </summary>
        [TestCleanup]
        protected virtual void OnTestCleanup() => ServiceLocator.Unload();
    }
}
