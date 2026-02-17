using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CPP.Framework.IO
{
    /// <summary>
    /// Abstract base class for classes that implement accessing command-separated data in a stream.
    /// </summary>
    public abstract class CommaSeparatedStream : IDisposable
    {
        /// <summary>
        /// The default character used to delimit a string field value in a CSV file.
        /// </summary>
        protected const char QuoteChar = '\"';

        /// <summary>
        /// The default delimiter used to separate field values in a CSV file.
        /// </summary>
        protected const char Separator = ',';

        /// <summary>
        /// The data for the columns on the current line.
        /// </summary>
        private readonly Dictionary<string, string> _columnData;

        /// <summary>
        /// The flag that indicates whether or not the current object has been disposed.
        /// </summary>
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedStream"/> class. 
        /// </summary>
        /// <param name="stream">
        /// A reference to the underlying stream object that contains the data.
        /// </param>
        /// <param name="encoding">
        /// The text encoding for the file contents.
        /// </param>
        protected CommaSeparatedStream(Stream stream, Encoding encoding)
        {
            ArgumentValidator.ValidateNotNull(() => stream);
            ArgumentValidator.ValidateNotNull(() => encoding);
            _columnData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.ColumnNames = Enumerable.Empty<string>().ToList();
            this.DataStream = stream;
            this.TextEncoding = encoding;
        }

        /// <summary>
        /// Destroys an instance of the class and frees its resources.
        /// </summary>
        ~CommaSeparatedStream()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    this.Dispose(false);
                }
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        /// <summary>
        /// Gets the name of the columns in the header.
        /// </summary>
        public IReadOnlyList<string> ColumnNames { get; private set; }

        /// <summary>
        /// Gets a reference to the underlying stream object that contains the data.
        /// </summary>
        protected Stream DataStream { get; }

        /// <summary>
        /// Gets a reference to the text encoding for the stream data.
        /// </summary>
        public Encoding TextEncoding { get; }

        /// <summary>
        /// Gets or sets the value of a column in the current row of data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <returns>The value for <paramref name="columnName"/>.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="columnName"/> is not defined in the data.</exception>
        protected string this[string columnName]
        {
            get
            {
                ArgumentValidator.ValidateNotNullOrWhiteSpace(() => columnName);
                if (!this.TryGetColumnValue(columnName, out var value))
                {
                    throw new KeyNotFoundException();
                }
                return value;
            }
            set
            {
                ArgumentValidator.ValidateNotNullOrWhiteSpace(() => columnName);
                if (!_columnData.ContainsKey(columnName))
                {
                    throw new KeyNotFoundException();
                }
                _columnData[columnName] = (value ?? string.Empty);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    this.Dispose(true);
                }
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Clears the data for the current row and initializes the column values to empty strings.
        /// </summary>
        protected void InitColumnData()
        {
            this.InitColumnData(this.ColumnNames);
        }

        /// <summary>
        /// Clears the data for the current row and initializes the column values to empty strings.
        /// </summary>
        /// <param name="columnNames">
        /// An <see cref="IEnumerable{T}"/> of strings that contains the column header names.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="columnNames"/> does not match the current column headers, and the file 
        /// has already been written to, or the column headers are otherwise read-only.
        /// </exception>
        protected void InitColumnData(IEnumerable<string> columnNames)
        {
            var columnNamesList = columnNames.ToList();

            var oldColumnNames = new HashSet<string>(_columnData.Keys, _columnData.Comparer);
            var newColumnNames = new HashSet<string>(columnNamesList, _columnData.Comparer);
            if (!newColumnNames.SetEquals(_columnData.Keys) && (!this.CanUpdateColumnNames()))
            {
                throw new InvalidOperationException(ErrorStrings.CannotUpdateColumnNames);
            }

            foreach (var columnName in newColumnNames)
            {
                oldColumnNames.Remove(columnName);
                _columnData[columnName] = string.Empty;
            }
            foreach (var columnName in oldColumnNames)
            {
                _columnData.Remove(columnName);
            }
            this.ColumnNames = columnNamesList;
        }

        /// <summary>
        /// Called by the base class to determine whether or not the column name can be updated in
        /// the object's current state.
        /// </summary>
        /// <returns>True if the column names can be updated; otherwise, false.</returns>
        protected virtual bool CanUpdateColumnNames() => true;

        /// <summary>
        /// Determines whether or not a column exists in the data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <returns>True if <paramref name="columnName"/> exists in the data; otherwise, false.</returns>
        public bool Contains(string columnName) => _columnData.ContainsKey(columnName);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed manually; otherwise, false if it is being finalized.</param>
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Gets the value of a column for the current row of data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <returns>The value for <paramref name="columnName"/>.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="columnName"/> is not defined in the data.</exception>
        public string GetColumnValue(string columnName) => this[columnName];

        /// <summary>
        /// Gets the data for all of the columns in the current row.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance that can be used to iterate over the sequence.</returns>
        protected IEnumerable<string> GetCurrentRow()
        {
            foreach (var columnName in this.ColumnNames)
            {
                yield return this.GetColumnValue(columnName);
            }
        }

        /// <summary>
        /// Moves to the next row of data in the stream.
        /// </summary>
        /// <returns>True if the move was successful; otherwise, false if the end of the stream has been reached.</returns>
        public abstract bool MoveNext();

        /// <summary>
        /// Tries to get the value of a column for the current row of data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <param name="value">A variable that receives the column value on success.</param>
        /// <returns>True if the column data was retrieved successfully; otherwise, false if <paramref name="columnName"/> is not defined in the data.</returns>
        public bool TryGetColumnValue(string columnName, out string value)
        {
            return _columnData.TryGetValue(columnName, out value);
        }
    }
}
