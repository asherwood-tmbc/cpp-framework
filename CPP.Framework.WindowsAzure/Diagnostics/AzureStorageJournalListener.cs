using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;
using CPP.Framework.WindowsAzure.Storage;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides an <see cref="IJournalListener"/> implementation that will write to blob storage.
    /// </summary>
    public class AzureStorageJournalListener : IJournalListener
    {
        private readonly MultiAccessLock _SyncLock = new MultiAccessLock();
        private readonly object _WriteLock = new object();

        private readonly string _AzureAccountName;
        private readonly string _BaseFileName;
        private readonly string _LogContainerName;
        private readonly int _MaxLogFileSize;
        private readonly string _RoleInstanceId;

        private StringBuilder _Contents;
        private DateTime _LastGeneratedDate;
        private string _LogFileName;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="accountName">The name of the Azure Storage account where the log file is stored, or null to use the default account.</param>
        public AzureStorageJournalListener(string accountName) : this(accountName, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="accountName">The name of the Azure Storage account where the log file is stored, or null to use the default account.</param>
        /// <param name="baseFileName">The name to use as the base location of the log file, or null to use the value from the application configuration.</param>
        protected AzureStorageJournalListener(string accountName, string baseFileName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accountName);
            if (String.IsNullOrWhiteSpace(baseFileName))
            {
                try
                {
                    baseFileName = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.LogFileName);
                }
                catch (ConfigurationErrorsException)
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        baseFileName = ConfigSettingProvider.Current.GetSetting("logFileName");
                    }
                    catch (Exception) { }
                    try
                    {
                        if (String.IsNullOrWhiteSpace(baseFileName) && RoleEnvironmentService.Current.IsAvailable)
                        {
                            var instance = RoleEnvironmentService.Current.CurrentRoleInstance;
                            if (instance != null) baseFileName = instance.Role.Name;
                        }
                    }
                    catch (Exception) { }
                    // ReSharper restore EmptyGeneralCatchClause
                    
                    if (!String.IsNullOrWhiteSpace(baseFileName))
                    {
                        baseFileName = AzureStoragePath.RemoveInvalidChars(baseFileName);
                    }
                    if (String.IsNullOrWhiteSpace(baseFileName)) throw;
                }
            }
#if DEBUG
            if (Debugger.IsAttached)
            {
                baseFileName = AzureStoragePath.Combine(baseFileName, Environment.MachineName);
            }
#endif // DEBUG
            _BaseFileName = baseFileName;

            if (RoleEnvironmentService.Current.IsAvailable)
            {
                var instance = RoleEnvironmentService.Current.CurrentRoleInstance;
                if (instance != null) _RoleInstanceId = instance.Id;
            }
            else
            {
                _RoleInstanceId = String.Empty;
            }

            _AzureAccountName = accountName;
            _LogContainerName = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.LogContainer);
            _MaxLogFileSize = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.LogFileSizeMax, Int32.Parse);
            _MaxLogFileSize *= 1024;    // Convert from KB to Bytes
        }

        /// <summary>
        /// Generates a new file name for the log storage blob.
        /// </summary>
        /// <param name="timestamp">The timestamp to use for the log file name.</param>
        /// <returns>A string that contains the file name.</returns>
        private string GenerateFileName(DateTime timestamp)
        {
            var sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(_RoleInstanceId))
            {
                sb.AppendFormat("{0}-", _RoleInstanceId);
            }
            sb.AppendFormat("{0}-{1:yyyyMMdd-hhmmss}", Environment.MachineName, timestamp);
            sb.AppendFormat(".txt");
            return AzureStoragePath.Combine(_BaseFileName, AzureStoragePath.RemoveInvalidChars(sb.ToString()));
        }

        /// <summary>
        /// Gets a reference to the <see cref="AzureStorageBlob"/> for the log file.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the file is stored.</param>
        /// <returns>An <see cref="AzureStorageBlob"/> object.</returns>
        private AzureStorageBlob GetStorageBlob(AzureStorageAccount account)
        {
            using (_SyncLock.GetWriterAccess())
            {
                var blobfile = default(AzureStorageBlob);
                do
                {
                    // generate a new log file name if: (a) this is the first log message, (b) the
                    // log file is over 24 hours old, or (c) the blob file isn't null (which means
                    // that the size has gone over the threshold).
                    var now = DateTimeService.Current.UtcNow;
                    if ((String.IsNullOrWhiteSpace(_LogFileName)) ||                // file hasn't been written to yet
                        (_LastGeneratedDate.Add(TimeSpan.FromHours(24)) <= now) ||  // file is over 24 hours old
                        (blobfile != null))                                         // file size exceeds the threshold
                    {
                        _LastGeneratedDate = DateTimeService.Current.UtcNow;
                        _LogFileName = GenerateFileName(_LastGeneratedDate);
                    }
                    blobfile = account.GetStorageBlockBlob(_LogContainerName, _LogFileName);
                }
                while (blobfile.Exists() && (blobfile.GetLength() >= _MaxLogFileSize));
                return blobfile;
            }
        }

        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The id of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, Guid source, string message)
        {
            using (var account = ServiceLocator.GetInstance<AzureStorageAccount>(_AzureAccountName))
            {
                var blobfile = this.GetStorageBlob(account);
                lock (_WriteLock)   // don't allow multiple writes at the same time
                {
                    if (blobfile.Exists())
                    {
                        if (_Contents == null)
                        {
                            using (var reader = blobfile.OpenRead())
                            using (var stream = new StreamReader(reader, Encoding.UTF8))
                            {
                                var existing = stream.ReadToEnd();
                                _Contents = new StringBuilder(existing);
                            }
                        }
                    }
                    else _Contents = new StringBuilder();

                    _Contents.AppendLine(message);
                    blobfile.UpdateFromString(_Contents.ToString());
                }
            }
        }
    }
}
