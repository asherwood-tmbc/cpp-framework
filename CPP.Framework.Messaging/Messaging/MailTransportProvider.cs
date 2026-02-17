using System;
using System.Net;
using System.Threading;
using CPP.Framework.Configuration;
using CPP.Framework.Threading;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Provides functionality to send email messages.
    /// </summary>
    public abstract class MailTransportProvider : SingletonServiceBase, ICustomArgumentValidator<IMailMessage>
    {
        #region MailServerConfig Class Declaration

        /// <summary>
        /// Contains the configuration information for a mail server connection.
        /// </summary>
        protected class MailServerConfig
        {
            /// <summary>
            /// Initializes an instance of the class.
            /// </summary>
            /// <param name="hostname">The DNS host name for the destination mail server.</param>
            /// <param name="hostport">The TCP/IP port for the destination mail server.</param>
            /// <param name="username">The username for authentication with the mail server.</param>
            /// <param name="password">The password to use for authentication with the mail server.</param>
            /// <param name="replyTo">The default reply-to address for messages sent through the provider.</param>
            public MailServerConfig(string hostname, ushort hostport, string username, string password, string replyTo)
            {
                username = (String.IsNullOrWhiteSpace(username) ? String.Empty : username);
                password = (String.IsNullOrWhiteSpace(password) ? String.Empty : password);
                if (!String.IsNullOrWhiteSpace(username))
                {
                    this.Credentials = new NetworkCredential(username, password);
                }
                this.ReplyTo = (String.IsNullOrWhiteSpace(replyTo)
                    ? ConfigSettingKey.CPPFromEmailAddress.GetDefaultValue()
                    : replyTo);
                this.HostName = (String.IsNullOrWhiteSpace(hostname) ? String.Empty : hostname);
                this.HostPort = ((hostport == 0) ? ((ushort)25) : hostport);
            }

            /// <summary>
            /// Gets the network credentials for the mail server connection.
            /// </summary>
            public NetworkCredential Credentials { get; private set; }

            /// <summary>
            /// Gets the DNS host name for the destination mail server.
            /// </summary>
            public string HostName { get; private set; }

            /// <summary>
            /// Gets the TCP/IP port for the destination mail server.
            /// </summary>
            public ushort HostPort { get; private set; }

            /// <summary>
            /// Gets the default reply-to address for messages sent through the provider.
            /// </summary>
            public string ReplyTo { get; private set; }
        }

        #endregion // MailServerConfig Class Declaration

        private static readonly ServiceInstance<MailTransportProvider> _ServiceInstance = new ServiceInstance<MailTransportProvider>();
        private readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
        private MailServerConfig _ServerConfig;

        /// <summary>
        /// Gets the current instance of the provider for the application.
        /// </summary>
        public static MailTransportProvider Current { get { return _ServiceInstance.GetInstance(); } }

        /// <summary>
        /// Gets the default from address for all email messages.
        /// </summary>
        protected string DefaultSender
        {
            get { return this.GetServerConfig(cfg => cfg.ReplyTo); }
        }

        /// <summary>
        /// Gets the DNS host name of the mail server.
        /// </summary>
        protected string HostName
        {
            get { return this.GetServerConfig(cfg => cfg.HostName); }
        }

        /// <summary>
        /// Gets the TCP/IP port of the mail server.
        /// </summary>
        protected ushort HostPort
        {
            get { return this.GetServerConfig(cfg => cfg.HostPort); }
        }

        /// <summary>
        /// Gets the default authentication credentials for the mail server.
        /// </summary>
        protected NetworkCredential ServerCredentials
        {
            get { return this.GetServerConfig(cfg => cfg.Credentials); }
        }

        /// <summary>
        /// Called by the base class to cleanup the current instance prior to it being destroyed.
        /// </summary>
        protected override void CleanupInstance()
        {
            using (_SyncLock.GetWriterAccess())
            {
                _ServerConfig = null;
            }
            base.CleanupInstance();
        }

        /// <summary>
        /// Ensures that the configuration information for the mail server is loaded and returns a
        /// configuration value the configuration information for the mail server.
        /// </summary>
        protected TValue GetServerConfig<TValue>(Func<MailServerConfig, TValue> selector)
        {
            using (_SyncLock.GetReaderAccess())
            {
                if (_ServerConfig != null) return selector(_ServerConfig);
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (_ServerConfig == null)
                {
                    var config = this.LoadServerConfig();
                    this.ValidateServerConfig(config);
                    _ServerConfig = config;
                }
                return selector(_ServerConfig);
            }
        }

        /// <summary>
        /// Called by the base class to load the configuration settings for the mail server.
        /// </summary>
        /// <returns>A <see cref="MailServerConfig"/> object.</returns>
        protected virtual MailServerConfig LoadServerConfig()
        {
            var config = new MailServerConfig
            (
                ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MailHostName, String.Empty),
                ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MailHostPort, UInt16.Parse, "25"),
                ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MailUserName, String.Empty),
                ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MailPassword, String.Empty),
                ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CPPFromEmailAddress)
            );
            return config;
        }

        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="message">An <see cref="IMailMessage"/> object that contains the message contents.</param>
        public abstract void Send(IMailMessage message);

        /// <summary>
        /// Called by the <see cref="ArgumentValidator"/> class to perform custom validation of an
        /// argument value;
        /// </summary>
        /// <param name="paramName">The name of the argument in the parameter list.</param>
        /// <param name="paramValue">The value of the argument being validated.</param>
        void ICustomArgumentValidator<IMailMessage>.ValidateArgument(string paramName, IMailMessage paramValue)
        {
            this.ValidateMessage(paramName, paramValue);
        }

        /// <summary>
        /// Called by the <see cref="ArgumentValidator"/> class to perform custom validation of an
        /// argument value;
        /// </summary>
        /// <param name="paramName">The name of the argument in the parameter list.</param>
        /// <param name="paramValue">The value of the argument being validated.</param>
        protected virtual void ValidateMessage(string paramName, IMailMessage paramValue)
        {
            if ((paramValue.Recipients == null) || (paramValue.Recipients.Count == 0))
            {
                throw new InvalidMailRecipientsException();
            }
        }

        /// <summary>
        /// Called by the base class to validate the mail server configuration.
        /// </summary>
        /// <param name="config">The <see cref="MailServerConfig"/> object to validate.</param>
        protected virtual void ValidateServerConfig(MailServerConfig config)
        {
            if (String.IsNullOrWhiteSpace(config.HostName) || (config.HostPort == 0))
            {
                throw new InvalidMailServerException();
            }
        }
    }
}
