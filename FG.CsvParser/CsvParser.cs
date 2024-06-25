using System.Collections;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;

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

        public static CsvParser Open(string filePath)
        {
            var writer = new CsvParser(filePath);
            return writer;
        }

        public static CsvParser Open(string filePath, bool hasHeader)
        {
            var writer = new CsvParser(filePath);
            writer.SetHasHeader(hasHeader);
            return writer;
        }

        public static CsvParser Open(string filePath, CsvParserConfiguration configuration)
        {
            var writer = new CsvParser(filePath, configuration);
            return writer;
        }

        public CsvParser SetFilePath(string filePath)
        {
            this._fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Debug.WriteLine("FileStream is instantiated with " + filePath);
            return this;
        }

        public CsvParser SetEncoding(Encoding encoding)
        {
            this._encoding = encoding;
            Debug.WriteLine("Encoding is set to " + encoding);
            return this;
        }

        public CsvParser SetHasHeader(bool hasHeader)
        {
            this._hasHeader = hasHeader;
            Debug.WriteLine("HasHeader is set to " + hasHeader);
            return this;
        }

        public CsvParser SetRowSplitter(string rowSplitter)
        {
            this._rowSplitter = rowSplitter;
            Debug.WriteLine("RowSplitter is set to " + rowSplitter);
            return this;
        }

        public CsvParser SetDelimiter(char delimiter)
        {
            this._delimiter = delimiter;
            Debug.WriteLine("Delimiter is set to " + delimiter);
            return this;
        }

        public bool HasHeader => _hasHeader;

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

        public Task WriteAsync<T>(List<T> data) => WriteAsync(data, true);
        public Task WriteAsync<T>(List<T> data, bool append) => WriteAsync(CsvHelper.ConvertToCsv(data, _hasHeader), append);
        public Task WriteAsync(string csvContent) => WriteAsync(csvContent, true);
        public async Task<string?> ReadAsJson()
        {
            if (_fileStream == null) return null;
            var bytes = new byte[_fileStream.Length];
            await _fileStream.ReadAsync(bytes);
            var textContent = _encoding.GetString(bytes);
            return CsvHelper.CsvTextToJson(textContent, _hasHeader, _rowSplitter, _delimiter);
        }

        public async Task<List<T>> ReadAs<T>()
        {
            if (_fileStream == null) return null;
            var bytes = new byte[_fileStream.Length];
            await _fileStream.ReadAsync(bytes);
            var textContent = _encoding.GetString(bytes);
            return CsvHelper.CsvTextToList<T>(textContent, _hasHeader, _rowSplitter, _delimiter);
        }

        public async IAsyncEnumerable<T> Query<T>(Func<T, bool> filter)
        {
            if (_fileStream == null) yield break;
            using var reader = new StreamReader(_fileStream, _encoding);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var headers = new List<string>();
            if (HasHeader)
            {
                var headerRow = await reader.ReadLineAsync();
                if (headerRow is null) yield break;
                headers = headerRow.Split(this._delimiter).ToList();
            }
            string line;
            while ((line = await reader.ReadLineAsync())
                != null)
            {
                var item = CsvHelper.ParseCsvLine<T>(this.HasHeader, this._delimiter, headers, line);
                if (item is not null && filter(item))
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
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
