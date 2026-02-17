using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using CPP.Framework.Cryptography.Bundles;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics;
using CPP.Framework.Services;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Provides cryptographic operations for the application, including encrypting and decrypting
    /// data or messages.
    /// </summary>
    [AutoRegisterService]
    public class CryptographyService : CodeServiceSingleton
    {
        private ReadOnlyDictionary<ushort, CryptoBundle> _cryptoBundleMap;
        private CryptoBundle _latest, _legacy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographyService"/> class.
        /// </summary>
        [ServiceLocatorConstructor]
        protected CryptographyService() { }

        /// <summary>
        /// Gets a reference to the current instance of the service for the application.
        /// </summary>
        public static CryptographyService Instance => CodeServiceProvider.GetService<CryptographyService>();

        /// <summary>
        /// Decrypts a message string value.
        /// </summary>
        /// <param name="message">The encrypted data as a Base-64 encoded string.</param>
        /// <returns>A <see langword="string"/> that contains the decrypted value.</returns>
        public static string Decrypt(string message) => Instance.DecryptValue(message);

        /// <summary>
        /// Decrypts the data from an encoded message.
        /// </summary>
        /// <param name="message">The encoded message that contains the data to decrypt.</param>
        /// <returns>A <see langword="string"/> that contains the decrypted value.</returns>
        /// <exception cref="CryptographicException"><paramref name="message"/> could not be decrypted.</exception>
        public virtual string DecryptValue(string message)
        {
            try
            {
                var payload = CryptoBundle.DecodeMessage(message, out var startAt, out var version);
                return this.GetCryptoBundle(version).DecryptValue(payload, startAt);
            }
            catch (Exception ex) when (!(ex is CryptographicException))
            {
                throw new CryptographicException(ErrorStrings.CannotDecryptMessagePayload, ex);
            }
        }

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input value to encrypt.</param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        /// <exception cref="CryptographicException"><paramref name="input"/> could not be encrypted.</exception>
        public static string Encrypt(string input) => Instance.EncryptValue(input, true);

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input value to encrypt.</param>
        /// <param name="includeLegacy">
        /// <c>True</c> to allow the legacy bundle to encrypt <paramref name="input"/> if the size
        /// is below its maximum threshold. Otherwise, <c>false</c> to only use a versioned bundle.
        /// </param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        /// <exception cref="CryptographicException"><paramref name="input"/> could not be encrypted.</exception>
        public static string Encrypt(string input, bool includeLegacy) => Instance.EncryptValue(input, includeLegacy);

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input value to encrypt.</param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        /// <exception cref="CryptographicException"><paramref name="input"/> could not be encrypted.</exception>
        public string EncryptValue(string input) => this.EncryptValue(input, true);

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input value to encrypt.</param>
        /// <param name="includeLegacy">
        /// <c>True</c> to allow the legacy bundle to encrypt <paramref name="input"/> if the size
        /// is below its maximum threshold. Otherwise, <c>false</c> to only use a versioned bundle.
        /// </param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        /// <exception cref="CryptographicException"><paramref name="input"/> could not be encrypted.</exception>
        public string EncryptValue(string input, bool includeLegacy)
        {
            try
            {
                var encode = default(Encoding);
                var source = default(byte[]);

                foreach (var bundle in _cryptoBundleMap.Select(kvp => kvp.Value))
                {
                    if (!includeLegacy && (bundle.Version == CryptoBundle.LegacyVersion))
                    {
                        continue;
                    }
                    if (bundle.CanEncrypt(input, ref source, ref encode))
                    {
                        input = bundle.EncryptValue(source);
                        return input;
                    }
                }
                throw new CryptographicException(ErrorStrings.SuitableCryptoBundleUnavailable);
            }
            catch (Exception ex) when (!(ex is CryptographicException))
            {
                throw new CryptographicException(ErrorStrings.CannotEncryptMessagePayload, ex);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="CryptoBundle"/> assigned to a given version number.
        /// </summary>
        /// <param name="version">
        /// The target bundle version number, or <see cref="LatestVersion"/> to get the latest.
        /// </param>
        /// <returns>A <see cref="CryptoBundle"/> object.</returns>
        /// <exception cref="KeyNotFoundException">
        /// <paramref name="version"/> was not found in the available list of bundles.
        /// </exception>
        protected internal CryptoBundle GetCryptoBundle(ushort version)
        {
            if (!this.TryGetCryptoBundle(version, out var bundle))
            {
                var message = string.Format(ErrorStrings.UnknownCryptoBundleVersion, version);
                throw new KeyNotFoundException(message);
            }
            return bundle;
        }

        /// <summary>
        /// Called by the application framework to perform any initialization tasks when the 
        /// instance is being created.
        /// </summary>
        protected internal override void StartupInstance()
        {
            // find all of the CryptoBundle definitions in the current assembly, and add them to
            // out local registry map.
            var reflectedTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(ti => typeof(CryptoBundle).IsAssignableFrom(ti))
                .Where(ti => !ti.IsAbstract);
            var latest = default(CryptoBundle);
            var legacy = default(CryptoBundle);
            var loaded = new SortedList<ushort, CryptoBundle>(CryptoBundleVersionComparer.Default);

            foreach (var bundleType in reflectedTypes)
            {
                if (CodeServiceProvider.GetService(bundleType) is CryptoBundle service)
                {
                    if (loaded.TryGetValue(service.Version, out var existing))
                    {
                        Journal.CreateSource(this).WriteWarning(
                            "{0} cannot be mapped to bundle version {1} because it is already mapped to {2}",
                            service.GetType().Name,
                            service.Version,
                            existing.GetType().Name);
                        continue;
                    }
                    if ((legacy == null) && (service.Version == CryptoBundle.LegacyVersion))
                    {
                        legacy = service;
                    }
                    if ((latest == null) || (latest.Version < service.Version))
                    {
                        latest = service;
                    }
                    loaded.Add(service.Version, service);
                }
                _legacy = legacy;
                _latest = latest;
            }
            _cryptoBundleMap = new ReadOnlyDictionary<ushort, CryptoBundle>(loaded);

            // always call the base class.
            base.StartupInstance();
        }

        /// <summary>
        /// Attempts to retrieve a <see cref="CryptoBundle"/> assigned to a given version number.
        /// </summary>
        /// <param name="version">
        /// The target bundle version number, or <see cref="CryptoBundle.LatestVersion"/> to get
        /// the latest.
        /// </param>
        /// <param name="bundle">
        /// An output variable that assigned to the <see cref="CryptoBundle"/> on success.
        /// </param>
        /// <returns><c>True</c> if the bundle was found; otherwise, <c>false</c></returns>
        protected internal bool TryGetCryptoBundle(ushort version, out CryptoBundle bundle)
        {
            bundle = default(CryptoBundle);
            switch (version)
            {
                case CryptoBundle.LatestVersion:
                    bundle = _latest;
                    break;
                case CryptoBundle.LegacyVersion:
                    bundle = _legacy;
                    break;
            }
            if (bundle == null)
            {
                return _cryptoBundleMap.TryGetValue(version, out bundle);
            }
            return true;
        }

        #region CryptoBundleVersionComparer Class Declaration

        /// <summary>
        /// An <see cref="IComparer{T}"/> for <see cref="CryptoBundle"/> that sorts the values by
        /// version number descending, and with the legacy version at the top of the list.
        /// </summary>
        private sealed class CryptoBundleVersionComparer : IComparer<ushort>
        {
            public static readonly CryptoBundleVersionComparer Default = new CryptoBundleVersionComparer();

            /// <summary>
            /// Prevents a default instance of the <see cref="CryptoBundleVersionComparer"/> class
            /// from being created.
            /// </summary>
            private CryptoBundleVersionComparer() { }

            /// <inheritdoc />
            public int Compare(ushort x, ushort y)
            {
                if (y == CryptoBundle.LegacyVersion) return 1;
                if (x == CryptoBundle.LegacyVersion) return -1;
                return y.CompareTo(x);
            }
        }

        #endregion // CryptoBundleVersionComparer Class Declaration
    }
}
