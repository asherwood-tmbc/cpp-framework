using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Security
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class SecurityAuthorizationContextTests
    {
        [TestInitialize]
        public void TestStartUp() => ServiceLocator.Unload();

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        [TestMethod]
        public void GetCurrentPrincipalWithoutInjection()
        {
            var existing = Thread.CurrentPrincipal;
            try
            {
                var expect = Thread.CurrentPrincipal = StubFactory.CreatePrincipal("basic")
                    .GrantUserName("testuser");
                var manager = CodeServiceProvider.GetService<SecurityAuthorizationManager>();
                var context = new SecurityAuthorizationContext(manager, null);
                var actual = context.CurrentPrincipal;
                Verify.AreSame(expect, actual);

                context = SecurityAuthorizationContext.Create();
                actual = context.CurrentPrincipal;
                Verify.AreSame(expect, actual);
            }
            catch (Exception)
            {
                try
                {
                    Thread.CurrentPrincipal = existing;
                }
                catch { /* ignored */ }
                throw;
            }
        }
    }
}
