using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CPP.Framework.IO
{
    /// <summary>
    /// Class used to read comma-separated string data from an input stream.
    /// </summary>
    public class CommaSeparatedReader : CommaSeparatedStream
    {
        /// <summary>
        /// The underlying <see cref="StreamReader"/> for the file.
        /// </summary>
        private readonly StreamReader _streamReader;

        /// <summary>
        /// The flag that indicates whether or not any data has been read from the file.
        /// </summary>
        private bool _hasReadData;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedReader"/> class. 
        /// </summary>
        /// <param name="stream">
        /// A reference to the underlying stream object that contains the data.
        /// </param>
        /// <param name="encoding">
        /// The text encoding for the file contents.
        /// </param>
        public CommaSeparatedReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
            ArgumentValidator.ValidateNotNull(() => stream);
            ArgumentValidator.ValidateNotNull(() => encoding);
            _streamReader = new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Destroys an instance of the class and frees its resources.
        /// </summary>
        ~CommaSeparatedReader()
        {
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed manually; otherwise, false if it is being finalized.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamReader?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Moves to the next row of data in the stream.
        /// </summary>
        /// <returns>True if the move was successful; otherwise, false if the end of the stream has been reached.</returns>
        public override bool MoveNext()
        {
            if (_streamReader.EndOfStream) return false;
            try
            {
                // if this is our first read operation, then we need to read the column headers
                // first, which should be the first line.
                if (!_hasReadData)
                {
                    this.InitColumnData(this.ParseNextLine());
                    _hasReadData = true;
                }
                else this.InitColumnData(); // otherwise, just clear the data for the current line

                // parse the next line of data from the stream.
                var fieldValues = default(string[]);
                do
                {
                    // keep parsing until we hit a non-blank line, or the end of the stream.
                    fieldValues = this.ParseNextLine().ToArray();
                }
                while ((fieldValues.Length == 0) && (!_streamReader.EndOfStream));

                // ensure that we have read the same number of fields as we have columns, otherwise
                // the file may be malformed.
                if (fieldValues.Length != this.ColumnNames.Count)
                {
                    if (_streamReader.EndOfStream && (fieldValues.Length == 0))
                    {
                        return false;   // the last line was just blank, so the file isn't malformed
                    }
                    throw new InvalidStreamFormatException();
                }

                // now copy the field values for each column into the data for the current line.
                for (var idx = 0; idx < fieldValues.Length; idx++)
                {
                    var columnName = this.ColumnNames[idx];
                    this[columnName] = fieldValues[idx];
                }
                return true;    // return success
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the field values from the next line of text in the stream.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the parsed fields.</returns>
        protected virtual IEnumerable<string> ParseNextLine()
        {
            var buffer = new StringBuilder();
            var fields = 0;
            var quoted = false;

            for (var ch = _streamReader.Read();; ch = _streamReader.Read())
            {
                switch (ch)
                {
                    case CommaSeparatedStream.QuoteChar:
                        {
                            if (quoted)
                            {
                                if ((ch = _streamReader.Peek()) == QuoteChar)
                                {
                                    // two quotation marks back to back within a quoted string 
                                    // represent an escaped quotation mark character that has been
                                    // embedded within the string.
                                    _streamReader.Read();   // consume the extra quotation mark
                                    goto default;           // ... and append one to the string
                                }

                                quoted = false; // otherwise, the current field is not longer quoted
                            }
                            else quoted = true;
                        }
                        break;
                    case '\r':
                        {
                            // if the field is currently quoted, then append the character value to
                            // the buffer.
                            if (quoted) goto default;
                        }
                        break;  // otherwise, ignore it
                    case '\n':
                        {
                            // if the current field is quoted, then this is an embedded value, and
                            // should be appended to the buffer.
                            if (quoted) goto default;
                        }
                        goto case CommaSeparatedStream.Separator;
                    case CommaSeparatedStream.Separator:
                        {
                            if (quoted) goto default;
                        }
                        goto case -1;
                    case -1:    // this means the end of the stream was reached
                        {
                            if ((fields == 0) && (_streamReader.EndOfStream) && (buffer.Length == 0))
                            {
                                yield break;
                            }
                            fields++;

                            yield return buffer.ToString();
                            buffer.Length = 0;

                            if (_streamReader.EndOfStream || (ch == '\n')) yield break;
                        }
                        break;
                    default:
                        {
                            buffer.Append((char)ch);
                        }
                        break;
                }
            }
        }
    }
}
