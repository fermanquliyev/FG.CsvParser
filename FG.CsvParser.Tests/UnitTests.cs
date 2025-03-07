using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace FG.CsvParser.Tests
{
    public partial class UnitTests
    {
        private string _filePath = string.Empty;
        private string _readFilePath;

        [SetUp]
        public void Setup()
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory + $"/testFile{DateTime.Now.Ticks}.csv";
            _readFilePath = AppDomain.CurrentDomain.BaseDirectory + $"/TestReadData.csv";
        }

        [Test]
        public async Task Should_Write_To_File()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";


            using (var parser = CsvParser.OpenFile(_filePath)) {
                await parser.WriteAsync(testContent);
            }
            
            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo(testContent));
        }

        [Test]
        public async Task Should_Write_List_To_File()
        {
            Console.WriteLine(new { _filePath });
            var testContent = new List<TestModel> { new() { Id = 1, Name = "Test 1" }, new() { Id = 2, Name = "Test 2" } };


            using (var parser = CsvParser.OpenFile(_filePath))
            {
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo(CsvHelper.ConvertToCsv(testContent, false)));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.OpenFile(_filePath, new CsvParserConfiguration { HasHeader = true }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}{Environment.NewLine}{testContent}"));
        }

        [Test]
        public async Task Should_Write_List_To_File_With_Header()
        {
            Console.WriteLine(new { _filePath });
            var testContent = new List<TestModel> { new() { Id = 1, Name = "Test 1" }, new() { Id = 2, Name = "Test 2" } };


            using (var parser = CsvParser.OpenFile(_filePath, true))
            {
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo(CsvHelper.ConvertToCsv(testContent, true)));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header_And_Row_Splitter()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.OpenFile(_filePath, new CsvParserConfiguration { HasHeader = true, RowSplitter = Environment.NewLine }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}{Environment.NewLine}{testContent}"));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header_And_Row_Splitter_And_Delimiter()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.OpenFile(_filePath, new CsvParserConfiguration { HasHeader = true, RowSplitter = Environment.NewLine, Delimitter = ';' }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}{Environment.NewLine}{testContent}"));
        }

        [Test]
        public void Should_Convert_CsvTextToDictionary()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new TestModel { Id = 1, Name = "Test" } });
            Console.WriteLine(csv);
            var data = CsvHelper.CsvTextToDictionary(csv,hasHeader:true, rowSplitter: Environment.NewLine);
            Console.WriteLine(new { data, data.Count, First = data[0] });
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0]["Id"], Is.EqualTo("1"));
            Assert.That(data[0]["Name"], Is.EqualTo("Test"));
        }

        [Test]
        public void Should_Convert_CsvTextToList()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new TestModel { Id = 1, Name = "Test" } });
            var data = CsvHelper.CsvTextToList<TestModel>(csv, hasHeader: true, rowSplitter: Environment.NewLine);
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Id, Is.EqualTo(1));
            Assert.That(data[0].Name, Is.EqualTo("Test"));
        }

#if DEBUG
        [Test]
        public async Task Should_Query_CsvData()
        {
            var csv = CsvParser.OpenFile(_readFilePath, new CsvParserConfiguration { HasHeader = true });
            var data = await csv.Query<TestModel>(t=>t.Id==3).ToListAsync();
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Id, Is.EqualTo(3));
        }
#endif
        [Test]
        public void Should_Convert_CsvTextToJson()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new() { Id = 1, Name = "Test" } });
            var data = CsvHelper.CsvTextToJson(csv, hasHeader: true, rowSplitter: Environment.NewLine);
            Assert.That(data, Is.EqualTo("[{\"Id\":\"1\",\"Name\":\"Test\"}]"));
        }

        [Test]
        public void Should_Convert_ToCsv()
        {
            var data = new List<TestModel> { new() { Id = 1, Name = "Test" } };
            var csv = CsvHelper.ConvertToCsv(data);
            Assert.That(csv, Is.EqualTo($"Id,Name{Environment.NewLine}1,Test"));
        }

        [Test]
        public void Should_Convert_Dictionary_To_Csv()
        {
            var data = new List<Dictionary<string, string>> { new() { { "Id", "1" }, { "Name", "Test" } } };
            var csv = CsvHelper.ConvertToCsv(data);
            Assert.That(csv, Is.EqualTo($"Id,Name{Environment.NewLine}1,Test"));
        }

        [TearDown]
        public void TestCleanup()
        {
            if (_filePath != null && File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Debug.Print($"File Deleted: {_filePath}");
            }
        }
    }
}