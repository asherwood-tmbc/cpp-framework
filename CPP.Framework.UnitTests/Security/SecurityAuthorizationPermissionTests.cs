using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;

using CPP.Framework.DependencyInjection;
using CPP.Framework.UnitTests.Testing;
using CPP.Framework.Security.Policies;

using FluentAssertions;

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
            var identity = (ClaimsIdentity) ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.GrantAccessRight(SeSampleAccessRightNameA);
            identity.CheckAccess(policy).Should().BeTrue();
        }


        [TestMethod]
        public void CallRevokeAccessRights()
        {
            var policy = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            var identity = (ClaimsIdentity)ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.GrantAccessRight(SeSampleAccessRightNameA);
            identity.CheckAccess(policy).Should().BeTrue();

            identity.RevokeAccessRight(SeSampleAccessRightNameA);
            identity.CheckAccess(policy).Should().BeFalse();
        }

        [TestMethod]
        public void CallRevokeAccessRightsNotAssigned()
        {
            var policy = SecurityAccessRightPolicy.Create(SeSampleAccessRightNameA);
            var identity = (ClaimsIdentity)ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .Identity;

            identity.RevokeAccessRight(SeSampleAccessRightNameA);
            identity.CheckAccess(policy).Should().BeFalse();
        }

        [TestMethod]
        public void CallComboSecuredMethodWithAuthAndNoAccessRights()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => this.SecuredComboAccessMethod();
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void CallComboSecuredMethodWithAuthAndFeatureButNoAccessRights()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameB)
                .RegisterPrincipal();
            Action act = () => this.SecuredComboAccessMethod();
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            this.SecuredAccessRightMethod();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            this.SecuredFeatureNameMethod();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndNoAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => this.SecuredAccessRightMethod();
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void CallSecuredMethodWithAuthAndNoFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => this.SecuredFeatureNameMethod();
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void CallSecuredMethodWithNoAuthAndNoClaims()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal(null)
                .RegisterPrincipal();
            Action act = () => this.SecuredAccessRightMethod();
            act.Should().Throw<SecurityAuthenticationException>();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleFeatureNameA);
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoClaims()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void CheckAccessWithAuthAndNoFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleFeatureNameA);
            actual.Should().BeFalse();
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "Reviewed. Suppression is OK here.")]
        [TestMethod]
        public void CheckAccessWithImplicitAuth()
        {
            var existing = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                    .GrantUserName("testuser")
                    .GrantAccessRight(SeSampleAccessRightNameA);
                var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
                actual.Should().BeTrue();
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
            ClaimsPrincipalTestExtensions.CreatePrincipal(null)
                .RegisterPrincipal();
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA);
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void CheckAccessWithNoAuthAndExplicitPrincipal()
        {
            var principal = ClaimsPrincipalTestExtensions.CreatePrincipal(null);
            var actual = SecurityAuthorizationPermission.CheckAccess(SeSampleAccessRightA, principal);
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void ComplexDemandWithAuthAndAccessRightsAndFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
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
            ClaimsPrincipalTestExtensions.CreatePrincipal(null)
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            SeSampleAccessRightPermissionA.Demand(false);
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA, false);
        }

        [TestMethod]
        public void ImperativeDemandWithAuthAndNoAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => SeSampleAccessRightPermissionA.Demand();
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantAccessRight(SeSampleAccessRightNameA)
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .GrantFeatureName(SeSampleFeatureNameNameA)
                .RegisterPrincipal();
            SecurityAuthorizationPermission.Demand(SeSampleFeatureNameA);
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndNoAccessRight()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndNoClaims()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => SecurityAuthorizationPermission.Demand(SeSampleAccessRightA);
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void SimpleDemandWithAuthAndNoFeatureName()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal("basic")
                .GrantUserName("testuser")
                .RegisterPrincipal();
            Action act = () => SecurityAuthorizationPermission.Demand(SeSampleFeatureNameA);
            act.Should().Throw<SecurityAuthorizationException>();
        }

        [TestMethod]
        public void SimpleDemandWithNoAuthAndNoClaims()
        {
            ClaimsPrincipalTestExtensions.CreatePrincipal(null)
                .RegisterPrincipal();
            Action act = () => SecurityAuthorizationPermission.Demand(SeSampleAccessRightA | SeSampleFeatureNameA);
            act.Should().Throw<SecurityAuthenticationException>();
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
