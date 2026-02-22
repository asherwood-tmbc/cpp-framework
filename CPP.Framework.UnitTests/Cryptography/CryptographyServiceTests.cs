using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using CPP.Framework.Configuration;
using CPP.Framework.Cryptography.Bundles;
using CPP.Framework.DependencyInjection;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

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
            string certBase64 =
                "MIIGyAIBAzCCBoQGCSqGSIb3DQEHAaCCBnUEggZxMIIGbTCCA74GCSqGSIb3DQEHAaCCA68EggOrMIIDpz" +
                "CCA6MGCyqGSIb3DQEMCgECoIICtjCCArIwHAYKKoZIhvcNAQwBAzAOBAgtcF4e4YYM8QICB9AEggKQB+U0" +
                "kugkYIQ0j3mnl1ktTGouOYhTbkCjg5saQTPQ0MCmRlOxBUUw1pSjyGlwlfX7TvDDagFd7t4aTKC3ipMl0q" +
                "LFAA62b7Eo8wLfAMLDWaPRbI9h2mpUJq9sIIOugFTAg3mgx0t9pO8exMyc0nFOpAUZMpiS1IEkKQbPc+Ee" +
                "7y08S7ouvTjqYIDJvRY62bbqcmFXCmUhp2hcPQNUAp6jVybvsWGulrbbeUiLOs4It3N+eMvQYXALVZqo4w" +
                "iRNDYRRhBMpMNcQRyn3hOFztS2dbxPUecGExy+n26W+2S75Q+UJlYTYcvmeq3S0NGDunvDKRoP3c90QkFv" +
                "JB+Y03fmGDc9yPiHC91vw+rOqBCIv1UUVjpFmtko7AsHzxZzl9lfeh+DL5R3Szw+OHDvR510fjxwt5yOD6" +
                "drdlvw30FcQXhvU6N8UcJGkVAFSAuxmGKRf2Ph0BeWfJ+aX64OJYSs8WYPj4lowoWRtwVlLI5ZwFh8ZdIc" +
                "Yk7StPoHCwHNMh4DBsFAIWcQPBd0Yt+F9acs8DWz6iHa3SGKNyUy2/dLv5BTyhhIvtJDYdjT9Tnbc1jfxj" +
                "SXvfKrsStAcmghHXAv4bCXtAWEoXUA51uLeJDmnWG2/BvC6rEdg/mbGRxxpkN3iXrHFfwej32/Oy1fzopT" +
                "msN2kFj8ub28J5SYKRwCMhcr25Cj90E/7BXeEGddQS2u7ZRtRHs08vVrekWbo5SNR3kCkxNbgeUx2Y9T+G" +
                "f16d1NAEmOxn/5LyqFiRdpM6OvAZZRvyPP+9LseaFdWIhUhegYbn3DTGMFBfXNxesoTQnJzMlxrKjqabGZ" +
                "rHU5pDn7cp4zUf00iyWAuyKchS7sM/DaG8xLutP8eNhr2JPW5nAxgdkwEwYJKoZIhvcNAQkVMQYEBAEAAA" +
                "AwVwYJKoZIhvcNAQkUMUoeSAAzADgAZgA1ADIAMQA5ADAALQAyAGMANAA4AC0ANABmADIAZAAtADkAMgBj" +
                "ADkALQA3AGQAZgA4ADUAYwA1AGEAMQA3ADEAMTBpBgkrBgEEAYI3EQExXB5aAE0AaQBjAHIAbwBzAG8AZg" +
                "B0ACAAUgBTAEEAIABTAEMAaABhAG4AbgBlAGwAIABDAHIAeQBwAHQAbwBnAHIAYQBwAGgAaQBjACAAUABy" +
                "AG8AdgBpAGQAZQByMIICpwYJKoZIhvcNAQcGoIICmDCCApQCAQAwggKNBgkqhkiG9w0BBwEwHAYKKoZIhv" +
                "cNAQwBBjAOBAiudY4qywy9DAICB9CAggJg5hIQT9AvjwapQUg6m9Bdi0i3AH06Ok2FyGHuyLDlcZN8Q3Ip" +
                "pTbQy8GXqY0MgMar9NfVxCnKml7E/vE3Xketd6ha9of/pxSiahHaIeoC935VEh0cnXX6ob0yq75YJYtwub" +
                "uIuVhFvocaXmSBFd/j+gL3IzZcBOVIUnATCubWGRffjigFB6VkbIQHdOF4oXc6eV6I2x5tv8GaUIcR6GuJ" +
                "lSXwOWeLmTP1CZPn612Tb/fYTW4qnVGxZTA8ZCavWDpjzGQNOBCYf5YNGcVduqNXSsS5P8MvehS8wRsNzZ" +
                "8PyIGCP9ijB1ApsBme/27uFQRPbMvoMkt/+heozOSrueZD+cq242MaktLfZtFOQboZMC+8r+XgGO+wYtYF" +
                "nhU8Xnm0+B975w3EPpSRKsX2EXmrtVYWZ0M7bIZm/iGDq9WW3SDNMrJL/aF+GLLE+U16a/3fceQ961GJHT" +
                "bmtywLuplz2x90v3fIql8vu7+XaK1RAD8+HgpNL80twbA0VDrZS7WJV+Z/F/ktFcgNUHwD1bLuG52dyh1K" +
                "p/Gmy9/Df+ozeKHiE7bW4DxPDzMTlo33GjTKsY3tCs8HqhjqZys/QG20G+QsgvIv5tpB2rebGULCmm8RsP" +
                "blDTOQBzCECORyHLrGOpMFNfnkp5vR4aORqCKMo/HJ5pRBETBgy1keSUcZxBREzudDr6OAEQst6tez0QTM" +
                "LpS0B+RmUxa0CaTWnqxptnIOL/PKKPBY4crPMnncaXbXHVJJnk1kljlqJbYRAh1SsGKAnrnVh+uZVq9Ef4" +
                "aqcbrnT3j5vzSe+PsrnSO7t78wOzAfMAcGBSsOAwIaBBQ57UmlHpKkaN41tzsuurxJ8hoGLgQUOv+UuNtF" +
                "rEZFE6btHOB4+k+BGJ4CAgfQ";
            byte[] certBytes = Convert.FromBase64String(certBase64);

            _certificate = new X509Certificate2(certBytes, "PASSword2012");
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

            SetupCryptoStubs();
            var actual = CryptographyService.Decrypt(SourceValue);

            actual.Should().Be(expect);
        }

        [TestMethod]
        public void DecryptValueWithLegacyBundle()
        {
            const string SourceValue = "tYiib2xZSTyqlYZtpNsER/PUjRLhseLylj7S9kOPuVxg7cwmCd0GqaJcJnAxsid/VpaCknw9awcxdgUi7yHcQGus1AiFy+1wHobIuS2Ee1NA9YdNNTSSc/SybTKEOQkCDOXRQyAzUH8ejtI5N6FlrdJ9/u499hX6nGyMNIgEojc=";
            const string expect = "rhoy@cpp.com";

            SetupCryptoStubs();
            var actual = CryptographyService.Decrypt(SourceValue);

            actual.Should().Be(expect);
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

            SetupCryptoStubs();
            var actual = CryptographyService.Encrypt(expect);

            actual.Should().NotBeNullOrWhiteSpace();
            actual.Should().StartWith($"{CryptoBundle.CryptoBundleTokenChar}");
            actual.Should().NotBe(expect);
            CryptographyService.Decrypt(actual).Should().Be(expect);
        }

        [TestMethod]
        public void EncryptValueWithLegacyBundle()
        {
            const string expect = "rhoy@cpp.com";

            SetupCryptoStubs();
            var actual = CryptographyService.Encrypt(expect);

            actual.Should().NotBeNullOrWhiteSpace();
            actual.Should().NotStartWith($"{CryptoBundle.CryptoBundleTokenChar}");
            actual.Should().NotBe(expect);
            CryptographyService.Decrypt(actual).Should().Be(expect);
        }

        [TestMethod]
        public void EncryptValueWithoutLegacy()
        {
            const string expect = "rhoy@cpp.com";

            SetupCryptoStubs();
            var actual = CryptographyService.Encrypt(expect, false);

            actual.Should().NotBeNullOrWhiteSpace();
            actual.Should().StartWith($"{CryptoBundle.CryptoBundleTokenChar}");
            actual.Should().NotBe(expect);
            CryptographyService.Decrypt(actual).Should().Be(expect);
        }

        #region Test Class Helper Methods

        private void SetupCryptoStubs()
        {
            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting("EncryptionCertificate", EncryptionCertificateThumbprint)
                .RegisterServiceStub();

            // NSubstitute uses last-wins for matching setups, so wildcard must be set up
            // BEFORE the specific argument match.
            var certProvider = Substitute.For<CertificateProvider>();
            certProvider.GetCertificate(Arg.Any<string>()).Throws<NotImplementedException>();
            // Use Configure() to suppress the wildcard Throws during this setup call.
            certProvider.Configure().GetCertificate(EncryptionCertificateThumbprint).Returns(_certificate);
            certProvider.RegisterServiceStub();
        }

        #endregion // Test Class Helper Methods
    }
}
