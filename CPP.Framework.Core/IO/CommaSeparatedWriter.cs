using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CPP.Framework.IO
{
    /// <summary>
    /// Class used to write comma-separated string data to an output stream.
    /// </summary>
    public sealed class CommaSeparatedWriter : CommaSeparatedStream
    {
        /// <summary>
        /// A flag used to indicate whether or not the file has been written to.
        /// </summary>
        private bool _hasWrittenData;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedWriter"/> class. 
        /// </summary>
        /// <param name="stream">
        /// A reference to the underlying stream object that contains the data.
        /// </param>
        /// <param name="encoding">
        /// The text encoding for the file contents.
        /// </param>
        public CommaSeparatedWriter(Stream stream, Encoding encoding) : base(stream, encoding) { }

        /// <summary>
        /// Called by the base class to determine whether or not the column name can be updated in
        /// the object's current state.
        /// </summary>
        /// <returns>True if the column names can be updated; otherwise, false.</returns>
        protected override bool CanUpdateColumnNames() => (!_hasWrittenData);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed manually; otherwise, false if it is being finalized.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_hasWrittenData && this.ColumnNames.Any())
            {
                this.WriteHeader();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Escapes any required characters in a value string, quoting the value if needed.
        /// </summary>
        /// <param name="value">The string value to escape.</param>
        /// <returns>The escaped value of <paramref name="value"/>.</returns>
        private static string Escape(string value)
        {
            var sb = new StringBuilder();
            var quoted = false;
            foreach (var ch in value.ToCharArray())
            {
                switch (ch)
                {
                    case '\"':
                        {
                            quoted = true;
                            sb.Append(ch);
                        }
                        goto default;
                    case ',':
                    case '\r':
                    case '\n':
                        {
                            quoted = true;
                        }
                        goto default;
                    default:
                        {
                            sb.Append(ch);
                        }
                        break;
                }
                if (char.IsWhiteSpace(ch)) quoted = true;
            }
            return (quoted ? ("\"" + sb + "\"") : sb.ToString());
        }

        /// <summary>
        /// Moves to the next row of data in the stream.
        /// </summary>
        /// <returns>True if the move was successful; otherwise, false if the end of the stream has been reached.</returns>
        public override bool MoveNext()
        {
            if (!_hasWrittenData)
            {
                this.WriteHeader();
            }

            this.WriteValues();
            this.DataStream.Flush();
            this.InitColumnData();

            return true;
        }

        /// <summary>
        /// Sets the name of the columns in the data. This method can only be called before the 
        /// first call to <see cref="CommaSeparatedStream.MoveNext"/>.
        /// </summary>
        /// <param name="columnNames">An <see cref="IEnumerable{T}"/> that can be used to iterate over the column names.</param>
        /// <exception cref="InvalidOperationException">Data has already been written to the stream.</exception>
        public void SetColumnNames(IEnumerable<string> columnNames)
        {
            this.InitColumnData(columnNames);
        }

        /// <summary>
        /// Sets the value of a column for the current row of data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <param name="value">The new value of the target column.</param>
        public void SetColumnValue(string columnName, string value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => columnName);
            this[columnName] = (value ?? string.Empty);
        }

        /// <summary>
        /// Sets the value of a column for the current row of data.
        /// </summary>
        /// <param name="columnName">The name of the target column.</param>
        /// <param name="format">The format string for the column value.</param>
        /// <param name="formatArgs">A variable list of one or more arguments for <paramref name="format"/>.</param>
        public void SetColumnValue(string columnName, string format, params object[] formatArgs)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => columnName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => format);
            this.SetColumnValue(columnName, string.Format(format, formatArgs));
        }

        /// <summary>
        /// Writes the column names to the stream.
        /// </summary>
        private void WriteHeader()
        {
            var bom = this.TextEncoding.GetPreamble();
            if ((bom != null) && (bom.Length >= 1))
            {
                this.DataStream.Write(bom, 0, bom.Length);
            }
            this.WriteLine(this.ColumnNames);
        }

        /// <summary>
        /// Writes a line of values to the stream, separated by commas, and terminated by a carriage
        /// return/line feed sequence (CRLF, or "\r\n").
        /// </summary>
        /// <param name="values">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the list of values.</param>
        private void WriteLine(IEnumerable<string> values)
        {
            var index = 0;
            foreach (var value in values)
            {
                if (index++ >= 1)
                {
                    this.WriteString(",");
                }
                this.WriteString(Escape(value));
            }
            this.WriteString("\r\n");
        }

        /// <summary>
        /// Writes a string to the stream.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        private void WriteString(string value)
        {
            var bytes = this.TextEncoding.GetBytes(value);
            this.DataStream.Write(bytes, 0, bytes.Length);
            _hasWrittenData = true;
        }

        /// <summary>
        /// Writes the column values for the current row of data to the stream.
        /// </summary>
        private void WriteValues() => this.WriteLine(this.GetCurrentRow());
    }
}
