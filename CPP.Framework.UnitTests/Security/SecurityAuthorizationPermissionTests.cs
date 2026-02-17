using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.Security.Policies;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static CPP.Framework.Security.SecurityAuthorizationPermissionTests.SampleAccessRights;
using static CPP.Framework.Security.SecurityAuthorizationPermissionTests.SampleFeatureNames;

namespace CPP.Framework.Security
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class SecurityAuthorizationPermissionTests
    {
        private const string SeSampleAccessRightNameA = "SeSampleAccessRightNameA";
        private const string SeSampleAccessRightNameB = "SeSampleAccessRightNameB";
        private const string SeSampleAccessRightNameC = "SeSampleAccessRightNameC";

        private const string SeSampleFeatureNameNameA = "SeSampleFeatureNameNameA";
        private const string SeSampleFeatureNameNameB = "SeSampleFeatureNameNameB";
        private const string SeSampleFeatureNameNameC = "SeSampleFeatureNameNameC";

        [TestInitialize]
        public void TestStartUp() => ServiceLocator.Unload();

        [TestMethod]
        public void CallGrantAccessRights()
        {
            var policy = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            var identity = (ClaimsIdentity) StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.GrantAccessRight(SeSampleAccessRightNameA);
            Assert.IsTrue(identity.CheckAccess(policy));
        }


        [TestMethod]
        public void CallRevokeAccessRights()
        {
            var policy = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            var identity = (ClaimsIdentity)StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.GrantAccessRight(SeSampleAccessRightNameA);
            Assert.IsTrue(identity.CheckAccess(policy));

            identity.RevokeAccessRight(SeSampleAccessRightNameA);
            Assert.IsFalse(identity.CheckAccess(policy));
        }

        [TestMethod]
        public void CallRevokeAccessRightsNotAssigned()
        {
            var policy = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            var identity = (ClaimsIdentity)StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.RevokeAccessRight(SeSampleAccessRightNameA);
            Assert.IsFalse(identity.CheckAccess(policy));
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void CallComboSecuredMethodWithAuthAndNoAccessRights()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            this.SecuredComboAccessMethod();
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void CallComboSecuredMethodWithAuthAndFeatureButNoAccessRights()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameB)
                .RegisterPrincipal();
            this.SecuredComboAccessMethod();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            this.SecuredAccessRightMethod();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            this.SecuredFeatureNameMethod();
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void CallSecuredMethodWithAuthAndNoAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            this.SecuredAccessRightMethod();
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void CallSecuredMethodWithAuthAndNoFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            this.SecuredFeatureNameMethod();
        }

        [ExpectedException(typeof(SecurityAuthenticationException))]
        [TestMethod]
        public void CallSecuredMethodWithNoAuthAndNoClaims()
        {
            StubFactory.CreatePrincipal(null)
                .RegisterPrincipal();
            this.SecuredAccessRightMethod();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            Verify.IsTrue(actual);
        }

        [TestMethod]
        public void CheckAccessWithAuthAndFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleFeatureNameA);
            Verify.IsTrue(actual);
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            Verify.IsFalse(actual);
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoClaims()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            Verify.IsFalse(actual);
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleFeatureNameA);
            Verify.IsFalse(actual);
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "Reviewed. Suppression is OK here.")]
        [TestMethod]
        public void CheckAccessWithImplicitAuth()
        {
            var existing = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = StubFactory.CreatePrincipal("basic")
                    .GrantUserName("testuser")
                    .GrantAccessRight(SeSampleAccessRightNameA);
                var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
                Verify.IsTrue(actual);
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

        [TestMethod]
        public void CheckAccessWithNoAuthAndNoClaims()
        {
            StubFactory.CreatePrincipal(null)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            Verify.IsFalse(actual);
        }

        [TestMethod]
        public void CheckAccessWithNoAuthAndExplicitPrincipal()
        {
            var principal = StubFactory.CreatePrincipal(null);
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA, principal);
            Verify.IsFalse(actual);
        }

        [TestMethod]
        public void ComplexDemandWithAuthAndAccessRightsAndFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .GrantAccessRight(SeSampleAccessRightNameC)
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            var condition = (SeSampleFeatureNameA & SeSampleAccessRightA & (SeSampleAccessRightB | SeSampleAccessRightC));
            SecurityAuthorizationPermission.Demand(condition);
        }

        [TestMethod]
        public void ImperativeDemandWithAnonAndAccessRight()
        {
            StubFactory.CreatePrincipal(null)
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            SeSampleAccessRightPermissionA.Demand(false);
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA, false);
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void ImperativeDemandWithAuthAndNoAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            SeSampleAccessRightPermissionA.Demand();
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleFeatureNameA);
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void SimpleDemandWithAuthAndNoAccessRight()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void SimpleDemandWithAuthAndNoClaims()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
        }

        [ExpectedException(typeof(SecurityAuthorizationException))]
        [TestMethod]
        public void SimpleDemandWithAuthAndNoFeatureName()
        {
            StubFactory.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleFeatureNameA);
        }

        [ExpectedException(typeof(SecurityAuthenticationException))]
        [TestMethod]
        public void SimpleDemandWithNoAuthAndNoClaims()
        {
            StubFactory.CreatePrincipal(null)
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA | SeSampleFeatureNameA);
        }

        #region Internal Helper Methods

        [SecurityAuthorizationPermission(
            AccessRights = (SeSampleAccessRightNameA + "," + SeSampleAccessRightNameB + "," + SeSampleAccessRightNameC),
            FeatureNames = (SeSampleFeatureNameNameA + "," + SeSampleFeatureNameNameB + "," + SeSampleFeatureNameNameC))]
        private void SecuredComboAccessMethod() { }

        [SecurityAuthorizationPermission(AccessRights = SeSampleAccessRightNameA)]
        private void SecuredAccessRightMethod() { }

        [SecurityAuthorizationPermission(FeatureNames = SeSampleFeatureNameNameA)]
        private void SecuredFeatureNameMethod() { }

        #endregion // Internal Helper Methods

        #region Sample Authorization Policy Classes

        internal static class SampleAccessRights
        {
            internal static readonly SecurityClaimPolicy SeSampleAccessRightA = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            internal static readonly SecurityClaimPolicy SeSampleAccessRightB = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameB);
            internal static readonly SecurityClaimPolicy SeSampleAccessRightC = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameC);

            internal static readonly SecurityAuthorizationPermission SeSampleAccessRightPermissionA = new SecurityAuthorizationPermission(SeSampleAccessRightA);
        }

        internal static class SampleFeatureNames
        {
            internal static readonly SecurityClaimPolicy SeSampleFeatureNameA = SecurityFeatureNamePolicy.Create(SeSampleFeatureNameNameA);
            internal static readonly SecurityClaimPolicy SeSampleFeatureNameB = SecurityFeatureNamePolicy.Create(SeSampleFeatureNameNameB);
            internal static readonly SecurityClaimPolicy SeSampleFeatureNameC = SecurityFeatureNamePolicy.Create(SeSampleFeatureNameNameC);
        }

        #endregion // Sample Authorization Policy Classes
    }
}
