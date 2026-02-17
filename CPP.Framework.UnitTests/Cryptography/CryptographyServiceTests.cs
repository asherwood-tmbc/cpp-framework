using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

using CPP.Framework.Configuration;
using CPP.Framework.Cryptography.Bundles;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;

namespace CPP.Framework.Cryptography
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class CryptographyServiceTests
    {
        private const string EncryptionCertificateThumbprint = "3EE3FDDD47D45EB04F78D9C325352E0D5A4C3F0F";
        private static X509Certificate2 _certificate;

        [ClassInitialize]
        public static void OnCaseStartup(TestContext context)
        {
            _certificate = new X509Certificate2("CPPEncryption.pfx", "PASSword2012");
        }

        [TestCleanup]
        public void OnTestCleanup() => ServiceLocator.Unload();

        [TestMethod]
        public void DecryptValueWithAesCryptBundle()
        {
            const string SourceValue =
                "#AQCAAAAAgAAAAI+H1gXV/ai7D9lVgupa2O5yEXSMEjp8gPRzPgTvI6Gm3UzHfn2HS8UUZnqh583L9tVAJ" +
                "q3GANCv7E/YOIDwhXiI+qoDFcBjqe+SMqJGOdrkFlBnbumTPC/4TCQXNc/l/6ZkyNK1eIr6B3YqgnwIP4e" +
                "hoTZl3yRL/QGXVrv8l8R1R3y+qD/RDop5Gsd1zVxDdi/fHzVrr9Cpg1TjogWxsNNwTl0ZRFpg0aX4YCoIx" +
                "OHFfs0H8uUYmMfK+7ZViLnl4Ha6mOnZ6LNkI1z7ctwvqY8jKG7TgGlg8+ODozgJSRpzZBZjmxx0qKLge9B" +
                "nOUnO1zN0uvykIvtdjBqEB5M1QI5eul6W2SYmeBd8PWrMRawo7QMb7naqHnKzfC9Wwe0enuR8WP0QPnWve" +
                "MSZAIMwiEu5hRFlc9EZXu/5QIrGkVQkp5eVofb+7+EVkrQ4DGDVZi+qCuRoTyO1UGvYLXi95ZUicYb8H3t" +
                "J1Ign10Afu5+goxWVCCF1mNsMOl/llX1Ie8D/K1h7ePkJ6yWEqSamzDndvZbn2HA0tJ78+SoL2xM2d3I9f" +
                "t2L0j6kl5IlB/UuCOo5h8R7qNY6/zgH3qdjtwhDoFXE7rHndhDJ6vG4/NLicjTKteTQiCr5pjJAdkC+5Z4" +
                "jzzUnYYOtPE6QJuaUjynjh6/DSj8so7ddTQl5z0vJi5UiPizqouP2PX+AFGmK3dShDQib0U2aeXCDpIKsk" +
                "p/SNxfZUYXvKpBGnqWlBr5v5yXcW39UQg95wz1bVVk/Kd9+V9zJSTjRzFmqlfLWdXpHXeI0J/qnBGRQbNf" +
                "jtgH2+aoynD1j5AYqvN2EdHfaeZ+EHkMbHAm+8bMoTZgHAzoo9NLQ0t2vuRHOENwYBpLOnHU+FcD9B/zSe" +
                "d/EvAosEa2xBpCrnhyQpq5Vg4sCkeaJC9jRGIEz8kf/DVgyYUE/2wLzCFS+zGdMCUwiW/mz2uE8+O21Qna" +
                "GUHP6A4CAFX8R+ZvzVL0Xc1LszEqwdb080lLR67lx3NSbD1RMNhJ50A==";
            const string expect =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In volutpat in purus et " +
                "sodales. Integer rutrum justo sapien, ut pharetra mauris efficitur vitae. Mauris " +
                "laoreet, tellus id bibendum facilisis, orci velit volutpat neque, tempor congue " +
                "nibh lacus vitae tellus. Pellentesque in massa erat. Donec tempus, eros in " +
                "vulputate vehicula, enim dui elementum ligula, in euismod eros magna eget nisl. " +
                "Morbi consectetur sem sit amet imperdiet placerat. Aenean vel nibh rhoncus, varius " +
                "nulla ut massa nunc.";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();
            StubFactory.CreateStub<CertificateProvider>()
                .StubAction(stub => stub.GetCertificate(Arg.Is(EncryptionCertificateThumbprint))).Return(_certificate)
                .StubAction(stub => stub.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>()
                .RegisterServiceStub();
            var actual = CryptographyService.Decrypt(SourceValue);

            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void DecryptValueWithLegacyBundle()
        {
            const string SourceValue = "tYiib2xZSTyqlYZtpNsER/PUjRLhseLylj7S9kOPuVxg7cwmCd0GqaJcJnAxsid/VpaCknw9awcxdgUi7yHcQGus1AiFy+1wHobIuS2Ee1NA9YdNNTSSc/SybTKEOQkCDOXRQyAzUH8ejtI5N6FlrdJ9/u499hX6nGyMNIgEojc=";
            const string expect = "rhoy@cpp.com";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();
            StubFactory.CreateStub<CertificateProvider>()
                .StubAction(stub => stub.GetCertificate(Arg.Is(EncryptionCertificateThumbprint))).Return(_certificate)
                .StubAction(stub => stub.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>()
                .RegisterServiceStub();
            var actual = CryptographyService.Decrypt(SourceValue);
            
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void EncryptValueWithLatestBundle()
        {
            const string expect =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In volutpat in purus et " +
                "sodales. Integer rutrum justo sapien, ut pharetra mauris efficitur vitae. Mauris " + 
                "laoreet, tellus id bibendum facilisis, orci velit volutpat neque, tempor congue " + 
                "nibh lacus vitae tellus. Pellentesque in massa erat. Donec tempus, eros in " +
                "vulputate vehicula, enim dui elementum ligula, in euismod eros magna eget nisl. " +
                "Morbi consectetur sem sit amet imperdiet placerat. Aenean vel nibh rhoncus, varius " +
                "nulla ut massa nunc.";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();
            StubFactory.CreateStub<CertificateProvider>()
                .StubAction(stub => stub.GetCertificate(Arg.Is(EncryptionCertificateThumbprint))).Return(_certificate)
                .StubAction(stub => stub.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>()
                .RegisterServiceStub();
            var actual = CryptographyService.Encrypt(expect);

            Verify.IsFalse(string.IsNullOrWhiteSpace(actual));
            Verify.IsTrue(actual?.StartsWith($"{CryptoBundle.CryptoBundleTokenChar}") ?? false);
            Verify.AreNotEqual(expect, actual);
            Verify.AreEqual(expect, CryptographyService.Decrypt(actual));
        }

        [TestMethod]
        public void EncryptValueWithLegacyBundle()
        {
            const string expect = "rhoy@cpp.com";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();
            StubFactory.CreateStub<CertificateProvider>()
                .StubAction(stub => stub.GetCertificate(Arg.Is(EncryptionCertificateThumbprint))).Return(_certificate)
                .StubAction(stub => stub.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>()
                .RegisterServiceStub();
            var actual = CryptographyService.Encrypt(expect);
            
            Verify.IsFalse(string.IsNullOrWhiteSpace(actual));
            Verify.IsFalse(actual?.StartsWith($"{CryptoBundle.CryptoBundleTokenChar}") ?? false);
            Verify.AreNotEqual(expect, actual);
            Verify.AreEqual(expect, CryptographyService.Decrypt(actual));
        }

        [TestMethod]
        public void EncryptValueWithoutLegacy()
        {
            const string expect = "rhoy@cpp.com";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();
            StubFactory.CreateStub<CertificateProvider>()
                .StubAction(stub => stub.GetCertificate(Arg.Is(EncryptionCertificateThumbprint))).Return(_certificate)
                .StubAction(stub => stub.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>()
                .RegisterServiceStub();
            var actual = CryptographyService.Encrypt(expect, false);

            Verify.IsFalse(string.IsNullOrWhiteSpace(actual));
            Verify.IsTrue(actual?.StartsWith($"{CryptoBundle.CryptoBundleTokenChar}") ?? false);
            Verify.AreNotEqual(expect, actual);
            Verify.AreEqual(expect, CryptographyService.Decrypt(actual));
        }
    }
}
