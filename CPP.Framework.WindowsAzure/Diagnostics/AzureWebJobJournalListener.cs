using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.WebJobs.Host;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// An <see cref="IJournalListener"/> that writes messages to the web job host logs.
    /// </summary>
    public class AzureWebJobJournalListener : IJournalListener
    {
        #region ScopeContext Class Declaration

        [ExcludeFromCodeCoverage]
        private sealed class ScopeContext : IDisposable
        {
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting 
            /// unmanaged resources.
            /// </summary>
            public void Dispose() { AzureWebJobJournalListener.ExecutionScopeWriter = null; }
        }

        #endregion // ScopeContext Class Declaration

        private readonly static ConcurrentDictionary<Guid, TraceWriter> _ScopeWriterMap = new ConcurrentDictionary<Guid, TraceWriter>();
        private readonly TraceWriter _TraceWriter;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="trace">The <see cref="TraceWriter"/> for the web job host.</param>
        protected internal AzureWebJobJournalListener(TraceWriter trace)
        {
            ArgumentValidator.ValidateNotNull(() => trace);
            _TraceWriter = trace;
        }

        /// <summary>
        /// Gets or sets a <see cref="TraceWriter"/> specific to the current 
        /// execution scope.
        /// </summary>
        protected internal static TraceWriter ExecutionScopeWriter
        {
            get
            {
                var writer = default(TraceWriter);
                if (!_ScopeWriterMap.TryGetValue(Journal.Scope.ID, out writer))
                {
                    return null;
                }
                return writer;
            }
            set
            {
                var writer = value;
                if (null == writer)
                {
                    _ScopeWriterMap.TryRemove(Journal.Scope.ID, out writer);
                }
                else _ScopeWriterMap.AddOrUpdate(Journal.Scope.ID, writer, (id, existing) => writer);
            }
        }

        /// <summary>
        /// Creates an execution scope and assigns it to the listener.
        /// </summary>
        /// <param name="trace">The <see cref="TraceWriter"/> to use for the scope.</param>
        /// <returns>An <see cref="IDisposable"/> object that should be disposed when the execution context goes out of scope.</returns>
        public static IDisposable CreateScope(TraceWriter trace)
        {
            AzureWebJobJournalListener.ExecutionScopeWriter = trace;
            return new ScopeContext();
        }

        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The name of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, Guid source, string message)
        {
            var traceWriter = AzureWebJobJournalListener.ExecutionScopeWriter;
            if (null == traceWriter) traceWriter = _TraceWriter;

            var traceEvent = new TraceEvent(severity.AsTraceLevel(), message)
            {
                Source = source.ToString(),
            };
            traceWriter.Level = Journal.SeverityLevel.AsTraceLevel();
            traceWriter.Trace(traceEvent);
            traceWriter.Flush();
        }
    }
}
