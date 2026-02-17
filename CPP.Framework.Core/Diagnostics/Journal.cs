using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Threading;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides methods to write trace messages to one or more locations.
    /// </summary>
    public class Journal : SingletonServiceBase
    {
        /// <summary>
        /// The default text for exception stack lines.
        /// </summary>
        private const string DefaultIndentText = ">>";

        /// <summary>
        /// The default message format for internal <see cref="Journal"/> errors.
        /// </summary>
        internal const string JournalErrorMessageFormat = "*** JOURNAL ERROR *** {0}.{1} Failure: {2}";

        /// <summary>
        /// The namespace of the logical task scope for an executing request.
        /// </summary>
        private const string ScopeLogicalDataName = "CPP.Framework.Diagnostics.Journal.Scope";

        /// <summary>
        /// The default source for log messages.
        /// </summary>
        private static readonly JournalSource _DefaultSource = new JournalSource();

        /// <summary>
        /// The collection of listeners attached to the current <see cref="Journal"/> instance.
        /// </summary>
        private static readonly JournalListenerCollection _Listeners = new JournalListenerCollection();

        /// <summary>
        /// The reference to the shared instance of the service for the application.
        /// </summary>
        private static readonly ServiceInstance<Journal> _ServiceInstance = new ServiceInstance<Journal>();

        /// <summary>
        /// The current minimum severity level for the trace messages.
        /// </summary>
        private static readonly Lazy<JournalSeverity> _SeverityLevel = new Lazy<JournalSeverity>(
            () =>
                {
#if DEBUG
                    const JournalSeverity DefaultSeverity = JournalSeverity.Debug;
#else
                    const JournalSeverity DefaultSeverity = JournalSeverity.Warning;
#endif
                    var level = ConfigSettingProvider.Current.GetSetting(
                        ConfigSettingKey.CurrentTraceLevel,
                        value =>
                            {
                                // ReSharper disable once InlineOutVariableDeclaration
                                var parsed = default(JournalSeverity);
                                if (!Enum.TryParse(value, true, out parsed))
                                {
                                    parsed = DefaultSeverity;
                                }
                                return parsed;
                            },
                        DefaultSeverity.ToString("G"));
                    return level;
                },
            LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// The lock used to synchronize access to the <see cref="ApplicationName"/> property 
        /// across multiple threads.
        /// </summary>
        private static readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The name of the application that is added to the telemetry for each message that is 
        /// written to the journal.
        /// </summary>
        private static string _applicationName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Journal"/> class. 
        /// </summary>
        protected Journal()
        {
            if (ConfigSettingProvider.Current.GetSetting("UseEventLogListener", Convert.ToBoolean, "true"))
            {
                // only add the listener for the event log if the application hasn't been 
                // specifically configurated to suppress it.
                _Listeners.Add(ServiceLocator.GetInstance<EventLogJournalListener>());
            }
            _Listeners.Add(new DebugJournalListener());
        }

        /// <summary>
        /// Raised when a write request to a listener fails.
        /// </summary>
        public static event JournalListenerEvent WriteFailure;

        /// <summary>
        /// Raised when a write request to a listener succeeds.
        /// </summary>
        public static event JournalListenerEvent WriteSuccess;

        /// <summary>
        /// Gets the name of the application that is added to the telemetry for each message that 
        /// is written to the journal.
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                using (_SyncLock.GetReaderAccess())
                {
                    return _applicationName;
                }
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="IJournalListener"/> objects for the trace messages.
        /// </summary>
        public static JournalListenerCollection Listeners => _Listeners;

        /// <summary>
        /// Gets or sets the <see cref="JournalScope"/> object assigned to the logical execution 
        /// context for the current call stack.
        /// </summary>
        public static JournalScope Scope
        {
            get
            {
                if (!(CallContext.LogicalGetData(ScopeLogicalDataName) is JournalScope scope))
                {
                    scope = new JournalScope();
                    CallContext.LogicalSetData(ScopeLogicalDataName, scope);
                }
                return scope;
            }
            set => CallContext.LogicalSetData(ScopeLogicalDataName, value);
        }

        /// <summary>
        /// Gets the current minimum severity level for the trace messages.
        /// </summary>
        public static JournalSeverity SeverityLevel => _SeverityLevel.Value;

        /// <summary>
        /// Determines whether or not a message with a given severity can be written to the journal.
        /// </summary>
        /// <param name="severity">The severity to validate.</param>
        /// <returns>True if <paramref name="severity"/> is valid; otherwise, false.</returns>
        protected virtual bool CanWrite(JournalSeverity severity)
        {
            return (severity <= Journal.SeverityLevel);
        }

        /// <summary>
        /// Configures the <see cref="ApplicationName"/> using an explicit string value. Please 
        /// note that once the name has been configured to a non-whitespace, non-null value, any 
        /// subsequent calls to this method will have no effect unless <paramref name="force"/> 
        /// is set to true.
        /// <paramref name="force"/> is set to true.
        /// </summary>
        /// <param name="instanceName">
        /// The name to set for the application.
        /// </param>
        /// <param name="force">
        /// True to force setting the application name, even if the value was already configured;
        /// otherwise, false to set it only if it has not already been assigned (default).
        /// </param>
        /// <returns>True if the application name has been configured; otherwise, false.</returns>
        public static bool ConfigureInstanceName(string instanceName, bool force = false)
        {
            return ConfigureInstanceName(() => instanceName, force);
        }

        /// <summary>
        /// Configures the <see cref="ApplicationName"/> using the value from a callback delegate. 
        /// Please note that once the name has been configured to a non-whitespace, non-null value, 
        /// any subsequent calls to this method will have no effect unless <paramref name="force"/> 
        /// is set to true.
        /// </summary>
        /// <param name="selector">
        /// An optional delegate that returns the name of the application instance, or a null value
        /// to leave the name unconfigured.
        /// </param>
        /// <param name="force">
        /// True to force setting the application name, even if the value was already configured;
        /// otherwise, false to set it only if it has not already been assigned (default).
        /// </param>
        /// <returns>True if the application name has been configured; otherwise, false.</returns>
        public static bool ConfigureInstanceName(Func<string> selector = null, bool force = false)
        {
            using (_SyncLock.GetWriterAccess())
            {
                if (force || string.IsNullOrWhiteSpace(_applicationName))
                {
                    if (selector != null)
                    {
                        try
                        {
                            var name = selector();
                            _applicationName = (string.IsNullOrWhiteSpace(name) ? _applicationName : name);
                        }
                        catch (Exception ex)
                        {
                            _applicationName = null;
                            Debug.WriteLine(JournalErrorMessageFormat, typeof(Journal).FullName, nameof(ConfigureInstanceName), ex);
                        }
                    }
                }
                return (!string.IsNullOrWhiteSpace(_applicationName));
            }
        }

        /// <summary>
        /// Creates new scope for log messages written from the current logical execution context.
        /// </summary>
        public static void CreateScope() { Journal.Scope = new JournalScope(); }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <param name="source">The id of the trace source.</param>
        /// <param name="sourceName">The name of the source writing to the journal.</param>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual JournalSource CreateSource(Guid source, string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                sourceName = null;
            }
            if (ServiceLocator.IsRegistered(typeof(JournalSource)))
            {
                var resolvers = new ServiceResolver[]
                {
                    new ParameterResolver("source", source),
                    new ParameterResolver("sourceName", sourceName),
                };
                return ServiceLocator.GetInstance<JournalSource>(resolvers);
            }
            return new JournalSource(source, sourceName);
        }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        [ExcludeFromCodeCoverage]
        public static JournalSource CreateSource()
        {
            var sourceName = default(string);
            try
            {
                var frame = new StackFrame(1, false);
                var methodInfo = frame.GetMethod();
                var parentType = methodInfo?.DeclaringType;
                sourceName = $"{parentType?.Name ?? "???"}.{methodInfo?.Name ?? "???"}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(JournalErrorMessageFormat, typeof(Journal).FullName, nameof(Write), ex);
            }
            return GetInstance().CreateSource(Guid.NewGuid(), sourceName);
        }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <param name="objectName">
        /// The name of the source object that is generating the message, or null if it there isn't
        /// an applicable object name available.
        /// </param>
        /// <param name="sourceName">
        /// The name of the source writing to the journal. This value is automatically provided by
        /// the compiler, and does not require any explicit values to be passed. However, one can
        /// be passed in order to override the default value (which is the calling method name).
        /// </param>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        public static JournalSource CreateSource(string objectName, [CallerMemberName] string sourceName = null)
        {
            var source = GuidGeneratorService.Current.NewGuid();
            var caller = (string.IsNullOrWhiteSpace(objectName)
                ? sourceName
                : $"{objectName}.{sourceName}");
            return GetInstance().CreateSource(source, caller);
        }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="sourceName">
        /// The name of the source writing to the journal. This value is automatically provided by
        /// the compiler, and does not require any explicit values to be passed. However, one can
        /// be passed in order to override the default value (which is the calling method name).
        /// </param>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        [ExcludeFromCodeCoverage]
        public static JournalSource CreateSource<TSource>([CallerMemberName] string sourceName = null)
        {
            return Journal.CreateSource(typeof(TSource).Name, sourceName);
        }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the source object.
        /// </typeparam>
        /// <param name="sourceObject">
        /// The source object that generated the message.
        /// </param>
        /// <param name="sourceName">
        /// The name of the source writing to the journal. This value is automatically provided by
        /// the compiler, and does not require any explicit values to be passed. However, one can
        /// be passed in order to override the default value (which is the calling method name).
        /// </param>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        [ExcludeFromCodeCoverage]
        public static JournalSource CreateSource<TSource>(TSource sourceObject, [CallerMemberName] string sourceName = null)
        {
            return Journal.CreateSource(typeof(TSource).Name, sourceName);
        }

        /// <summary>
        /// Formats an <see cref="Exception"/> object into a sequence of trace messages.
        /// </summary>
        /// <param name="exception">The exception to format.</param>
        /// <param name="indentText">An optional value that should be used to indent the text of each line of the message.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        protected internal virtual IEnumerable<string> FormatException(Exception exception, string indentText = null)
        {
            indentText = (indentText ?? string.Empty);
            var message = (exception.StackTrace ?? "<stack unavailable>")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.TrimEnd('\r'));
            yield return ($"{indentText}[{exception.GetType()}: {exception.Message}]");

            foreach (var line in message)
            {
                yield return (indentText + line);
            }
            if (exception.InnerException != null)
            {
                indentText += DefaultIndentText;
                yield return (indentText + "===== Nested Inner Exception ====");
                foreach (var line in this.FormatException(exception.InnerException, indentText))
                {
                    yield return line;
                }
                yield return (indentText + "===== Finish Inner Exception ====");
            }
        }

        /// <summary>
        /// Formats the contents of a trace message.
        /// </summary>
        /// <param name="timestamp">The <see cref="DateTime"/> value to use for the message timestamp.</param>
        /// <param name="severity">The severity of the trace message.</param>
        /// <param name="source">The id of the source for the trace message.</param>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A string that contains the formatted message.</returns>
        protected internal virtual string FormatMessage(DateTime timestamp, JournalSeverity severity, Guid source, string format, params object[] formatArgs)
        {
            var block = format.SafeFormatWith(formatArgs).Split('\n').Select(s => s.TrimEnd('\r'));
            var scope = Journal.Scope.ID;

            var message = new StringBuilder();
            foreach (var line in block)
            {
                if (message.Length >= 1) message.AppendLine();
                message.AppendFormat("[{0:yyyy-MM-dd HH:mm:ss.fff}] ", timestamp);
                message.AppendFormat("{0} ", this.FormatSeverity(severity));
                message.AppendFormat("{0:X8} ", Thread.CurrentThread.ManagedThreadId);
                message.AppendFormat("<{0:B}> ({1:B}) : {2}", scope, source, line);
            }
            return message.ToString();
        }

        /// <summary>
        /// Formats the message text for a given <see cref="JournalSeverity"/> value.
        /// </summary>
        /// <param name="severity">The severity value to format.</param>
        /// <returns>A string that contains the formatted text.</returns>
        protected virtual string FormatSeverity(JournalSeverity severity)
        {
            switch (severity)
            {
                case JournalSeverity.Critical: return "CRT";
                case JournalSeverity.Debug: return "DBG";
                case JournalSeverity.Error: return "ERR";
                case JournalSeverity.Information: return "INF";
                case JournalSeverity.Verbose: return "VRB";
                case JournalSeverity.Warning: return "WRN";
            }
            return "UNK";
        }

        /// <summary>
        /// Gets a reference to the current <see cref="Journal"/> instance for the application.
        /// </summary>
        /// <returns>A <see cref="Journal"/> value.</returns>
        internal static Journal GetInstance() { return _ServiceInstance.GetInstance(); }

        /// <summary>
        /// Creates a new <see cref="JournalSource"/> for writing messages to the current
        /// <see cref="Journal"/> instance.
        /// </summary>
        /// <param name="source">The id of the trace source.</param>
        /// <returns>A <see cref="JournalSource"/> object.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use CreateSource(string) instead.")]
        public static JournalSource GetSource(Guid source) => throw new NotSupportedException();

        /// <summary>
        /// Raises the <see cref="WriteFailure"/> event.
        /// </summary>
        /// <param name="listener">The listener that triggered the failure.</param>
        /// <param name="message">The message being processed at the time of the failure.</param>
        /// <param name="exception">The exception that was caught.</param>
        /// <returns>True if the failure was handled and does not require further action; otherwise, false.</returns>
        protected bool OnWriteFailure(IJournalListener listener, JournalMessage message, Exception exception)
        {
            try
            {
                var args = new JournalListenerEventArgs(listener, message, exception);
                Journal.WriteFailure?.Invoke(this, args);
                return args.Handled;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Raises the <see cref="WriteSuccess"/> event.
        /// </summary>
        /// <param name="listener">The listener that triggered the failure.</param>
        /// <param name="message">The message being processed at the time of the failure.</param>
        protected void OnWriteSuccess(IJournalListener listener, JournalMessage message)
        {
            try
            {
                var args = new JournalListenerEventArgs(listener, message, null);
                Journal.WriteSuccess?.Invoke(this, args);
            }
            catch
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Publishes a message to each <see cref="IJournalListener"/> that is currently registered 
        /// with the Journal.
        /// </summary>
        /// <param name="message">The <see cref="JournalMessage"/> to publish</param>
        protected void PublishMessage(JournalMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            try
            {
                var contents = message;         // prevent "implicitly captured closure: this" warnings
                var written = true;             // assume success
                var hasEventListener = false;   // assume there is no event log listener

                Lazy<List<string>> legacyMessageContent;
                switch (contents)
                {
                    case JournalExceptionMessage exception:
                        {
                            legacyMessageContent = new Lazy<List<string>>(() =>
                            {
                                var result = this.FormatException(exception.Exception)
                                    .Select(line => this.FormatMessage(contents.Timestamp, contents.Severity, contents.Source, line))
                                    .ToList();
                                return result;
                            });
                        }
                        break;
                    case JournalFormattedMessage formatted:
                        {
                            legacyMessageContent = new Lazy<List<string>>(() => new List<string> { formatted.Message });
                        }
                        break;
                    default:
                        {
                            legacyMessageContent = new Lazy<List<string>>(() => new List<string>());
                        }
                        break;
                }

                Parallel.ForEach(
                    Journal.Listeners,
                    listener =>
                    {
                        try
                        {
                            if (!hasEventListener)
                            {
                                hasEventListener = (listener is EventLogJournalListener);
                            }
                            if (listener is IJournalListenerEx extended)
                            {
                                extended.Write(contents);
                            }
                            else legacyMessageContent.Value.ForEach(line => listener?.Write(contents.Severity, contents.Source, line));

                            // notify the any handlers that the write call succeeded, just in case the
                            // current listener had failed previously.
                            this.OnWriteSuccess(listener, contents);
                        }
                        catch (Exception ex)
                        {
                            written = (this.OnWriteFailure(listener, contents, ex) && written);
                        }
                    });

                // if the message was not written to all of the listeners for some reason, or 
                // the write failure was not handled by the application, then attempt to write 
                // it to the system event log as well, but only if there isn't already an event
                // log listener in the collection.
                if (!written && (!hasEventListener)) EventLogProvider.Current.Write(contents);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(JournalErrorMessageFormat, typeof(Journal).FullName, nameof(Write), ex);
            }
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the journal.
        /// </summary>
        /// <param name="timestamp">The <see cref="DateTime"/> value to use for the message timestamp.</param>
        /// <param name="severity">The severity of the trace message.</param>
        /// <param name="source">The name of the source for the trace message.</param>
        /// <param name="telemetry">The telemetry data associated with the message.</param>
        /// <param name="exception">The exception to write.</param>
        protected internal void Write(DateTime timestamp, JournalSeverity severity, Guid source, JournalTelemetry telemetry, Exception exception)
        {
            ArgumentValidator.ValidateNotNull(() => exception);
            if (!this.CanWrite(severity)) return;
            var message = new JournalExceptionMessage(timestamp, severity, source, telemetry, exception);
            this.PublishMessage(message);
        }

        /// <summary>
        /// Writes a trace message to the journal.
        /// </summary>
        /// <param name="timestamp">The <see cref="DateTime"/> value to use for the message timestamp.</param>
        /// <param name="severity">The severity of the trace message.</param>
        /// <param name="source">The id of the source for the trace message.</param>
        /// <param name="telemetry">The telemetry data associated with the message.</param>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        protected internal void Write(DateTime timestamp, JournalSeverity severity, Guid source, JournalTelemetry telemetry, string format, params object[] formatArgs)
        {
            ArgumentValidator.ValidateNotNull(() => format);
            if (!this.CanWrite(severity)) return;
            var formatted = this.FormatMessage(timestamp, severity, source, format, formatArgs);
            var message = new JournalFormattedMessage(timestamp, severity, source, telemetry, formatted);
            this.PublishMessage(message);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Critical"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteCritical instead by calling the CreateSource method first.")]
        public static JournalSource WriteCritical(Exception exception)
        {
            return _DefaultSource.WriteCritical(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Critical"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteCritical instead by calling the CreateSource method first.")]
        public static JournalSource WriteCritical(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteCritical(format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Debug"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteDebug instead by calling the CreateSource method first.")]
        public static JournalSource WriteDebug(Exception exception)
        {
            return _DefaultSource.WriteDebug(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Debug"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteDebug instead by calling the CreateSource method first.")]
        public static JournalSource WriteDebug(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteDebug(format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Error"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteError instead by calling the CreateSource method first.")]
        public static JournalSource WriteError(Exception exception)
        {
            return _DefaultSource.WriteError(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Error"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteError instead by calling the CreateSource method first.")]
        public static JournalSource WriteError(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteError(format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Information"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteInfo instead by calling the CreateSource method first.")]
        public static JournalSource WriteInfo(Exception exception)
        {
            return _DefaultSource.WriteInfo(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Information"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteInfo instead by calling the CreateSource method first.")]
        public static JournalSource WriteInfo(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteInfo(format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Verbose"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteVerbose instead by calling the CreateSource method first.")]
        public static JournalSource WriteVerbose(Exception exception)
        {
            return _DefaultSource.WriteVerbose(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Verbose"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteVerbose instead by calling the CreateSource method first.")]
        public static JournalSource WriteVerbose(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteVerbose(format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Warning"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteWarning instead by calling the CreateSource method first.")]
        public static JournalSource WriteWarning(Exception exception)
        {
            return _DefaultSource.WriteWarning(exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Warning"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        [Obsolete("Please use JournalSource.WriteWarning instead by calling the CreateSource method first.")]
        public static JournalSource WriteWarning(string format, params object[] formatArgs)
        {
            return _DefaultSource.WriteWarning(format, formatArgs);
        }
    }
}
