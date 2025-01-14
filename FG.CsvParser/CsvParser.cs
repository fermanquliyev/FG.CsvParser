using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.CsvParser
{
    public class CsvParser : IDisposable
    {
        private FileStream _fileStream;
        private bool _hasHeader;
        private string _rowSplitter = "\r\n";
        private char _delimiter = ',';
        private bool disposedValue;
        private Encoding _encoding = Encoding.UTF8;

        internal CsvParser()
        {
            Debug.WriteLine("FileStream is not instantiated. Use SetFilePath to instantiate.");
        }
        internal CsvParser(string filePath)
        {
            SetFilePath(filePath);
        }
        internal CsvParser(CsvParserConfiguration configuration) : this()
        {
            SetRowSplitter(configuration.RowSplitter);
            SetDelimiter(configuration.Delimitter);
            SetHasHeader(configuration.HasHeader);
            SetEncoding(configuration.Encoding);
        }

        internal CsvParser(string filePath, CsvParserConfiguration configuration) : this(configuration)
        {
            SetFilePath(filePath);
        }

        /// <summary>
        /// Opens a CSV file and returns a CsvParser instance.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>A CsvParser instance.</returns>
        public static CsvParser OpenFile(string filePath)
        {
            var writer = new CsvParser(filePath);
            return writer;
        }

        /// <summary>
        /// Opens a CSV file and returns a CsvParser instance with the specified header setting.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="hasHeader">A boolean indicating whether the CSV file has a header row.</param>
        /// <returns>A CsvParser instance.</returns>
        public static CsvParser OpenFile(string filePath, bool hasHeader)
        {
            var writer = new CsvParser(filePath);
            writer.SetHasHeader(hasHeader);
            return writer;
        }

        /// <summary>
        /// Opens a CSV file and returns a CsvParser instance with the specified configuration.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="configuration">The configuration settings for the CSV parser.</param>
        /// <returns>A CsvParser instance.</returns>
        public static CsvParser OpenFile(string filePath, CsvParserConfiguration configuration)
        {
            var writer = new CsvParser(filePath, configuration);
            return writer;
        }

        /// <summary>
        /// Sets the file path for the CSV parser and initializes the file stream.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>The current CsvParser instance.</returns>
        public CsvParser SetFilePath(string filePath)
        {
            this._fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Debug.WriteLine("FileStream is instantiated with " + filePath);
            return this;
        }

        /// <summary>
        /// Sets the encoding for the CSV parser.
        /// </summary>
        /// <param name="encoding">The encoding to use for the CSV file.</param>
        /// <returns>The current CsvParser instance.</returns>
        public CsvParser SetEncoding(Encoding encoding)
        {
            this._encoding = encoding;
            Debug.WriteLine("Encoding is set to " + encoding);
            return this;
        }

        /// <summary>
        /// Sets whether the CSV file has a header row.
        /// </summary>
        /// <param name="hasHeader">A boolean indicating whether the CSV file has a header row.</param>
        /// <returns>The current CsvParser instance.</returns>
        public CsvParser SetHasHeader(bool hasHeader)
        {
            this._hasHeader = hasHeader;
            Debug.WriteLine("HasHeader is set to " + hasHeader);
            return this;
        }

        /// <summary>
        /// Sets the row splitter for the CSV parser.
        /// </summary>
        /// <param name="rowSplitter">The string used to split rows in the CSV file.</param>
        /// <returns>The current CsvParser instance.</returns>
        public CsvParser SetRowSplitter(string rowSplitter)
        {
            this._rowSplitter = rowSplitter;
            Debug.WriteLine("RowSplitter is set to " + rowSplitter);
            return this;
        }

        /// <summary>
        /// Sets the delimiter for the CSV parser.
        /// </summary>
        /// <param name="delimiter">The character used to delimit fields in the CSV file.</param>
        /// <returns>The current CsvParser instance.</returns>
        public CsvParser SetDelimiter(char delimiter)
        {
            this._delimiter = delimiter;
            Debug.WriteLine("Delimiter is set to " + delimiter);
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether the CSV file has a header row.
        /// </summary>
        public bool HasHeader => _hasHeader;

        /// <summary>
        /// Writes the csv content to the file stream.
        /// </summary>
        /// <param name="csvContent"></param>
        /// <param name="append">A boolean indicating whether to append the data to the existing content (true) or overwrite it (false).</param>
        /// <returns></returns>
        public async Task WriteAsync(string csvContent, bool append)
        {
            var data = _encoding.GetBytes(csvContent);
            if (!append)
            {
                _fileStream.Seek(0, SeekOrigin.Begin);
            }

            if (_fileStream.Position > 0)
            {
                var rowSplitterBytes = _encoding.GetBytes(_rowSplitter.ToString());
                await _fileStream.WriteAsync(rowSplitterBytes, 0, rowSplitterBytes.Length);
            }

            await _fileStream.WriteAsync(data, 0, data.Length);
            await _fileStream.FlushAsync();
        }
        /// <summary>
        /// Writes the list of data to the file stream asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the data in the list.</typeparam>
        /// <param name="data">The list of data to write to the file stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync<T>(List<T> data) => WriteAsync(data, true);
        /// <summary>
        /// Writes the list of data to the file stream asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the data in the list.</typeparam>
        /// <param name="data">The list of data to write to the file stream.</param>
        /// <param name="append">A boolean indicating whether to append the data to the existing content (true) or overwrite it (false).</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync<T>(List<T> data, bool append) => WriteAsync(CsvHelper.ConvertToCsv(data, _hasHeader), append);
        /// <summary>
        /// Writes the CSV content to the file stream asynchronously.
        /// </summary>
        /// <param name="csvContent">The CSV content to write to the file stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync(string csvContent) => WriteAsync(csvContent, true);
        /// <summary>
        /// Reads the CSV content from the file stream and converts it to a JSON string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The task result contains the JSON string representation of the CSV content, or null if the file stream is not initialized.</returns>
        public async Task<string?> ReadAsJson()
        {
            if (_fileStream == null) return null;
            var bytes = new byte[_fileStream.Length];
            await _fileStream.ReadAsync(bytes);
            var textContent = _encoding.GetString(bytes);
            return CsvHelper.CsvTextToJson(textContent, _hasHeader, _rowSplitter, _delimiter);
        }

        /// <summary>
        /// Reads the CSV content from the file stream and converts it to a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the data in the list.</typeparam>
        /// <returns>A task that represents the asynchronous read operation. The task result contains the list of data of the specified type.</returns>
        public async Task<List<T>> ReadAs<T>()
        {
            if (_fileStream == null)
            {
                Debug.WriteLine("FileStream is null");
                return null;
            }
            var bytes = new byte[_fileStream.Length];
            await _fileStream.ReadAsync(bytes);
            var textContent = _encoding.GetString(bytes);
            return CsvHelper.CsvTextToList<T>(textContent, _hasHeader, _rowSplitter, _delimiter);
        }

        /// <summary>
        /// Queries the CSV file and returns an asynchronous stream of items that match the specified filter.
        /// </summary>
        /// <typeparam name="T">The type of the items to query.</typeparam>
        /// <param name="filter">A function to filter the items.</param>
        /// <returns>An asynchronous stream of items that match the specified filter.</returns>
        public async IAsyncEnumerable<T> Query<T>(Func<T, bool> filter)
        {
            if (_fileStream == null)
            {
                Debug.WriteLine("FileStream is null");
                yield break;
            }
            using var reader = new StreamReader(_fileStream, _encoding);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var headers = new List<string>();
            var properties = typeof(T).GetProperties();
            if (HasHeader)
            {
                var headerRow = await reader.ReadLineAsync();
                if (headerRow is null) yield break;
                headers = headerRow.Split(this._delimiter).ToList();
            }
            string? line;
            while ((line = await reader.ReadLineAsync())
                != null)
            {
                var item = CsvHelper.ParseCsvLine<T>(this.HasHeader, this._delimiter, headers, line, properties);
                if (item != null && filter(item))
                {
                    yield return item;
                }
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStream?.Close();
                    _fileStream?.Dispose();
                }

                _fileStream = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
