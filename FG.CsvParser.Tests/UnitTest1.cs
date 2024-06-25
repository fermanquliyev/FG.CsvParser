using System.Diagnostics;
using System.Reflection;

namespace FG.CsvParser.Tests
{
    public partial class Tests
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


            using (var parser = CsvParser.Open(_filePath)) {
                await parser.WriteAsync(testContent);
            }
            
            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo(testContent));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.Open(_filePath, new CsvParserConfiguration { HasHeader = true }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}\r\n{testContent}"));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header_And_Row_Splitter()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.Open(_filePath, new CsvParserConfiguration { HasHeader = true, RowSplitter = "\r\n" }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}\r\n{testContent}"));
        }

        [Test]
        public async Task Should_Write_To_File_With_Header_And_Row_Splitter_And_Delimiter()
        {
            Console.WriteLine(new { _filePath });
            var testContent = "testContent";
            var header = "header";

            using (var parser = CsvParser.Open(_filePath, new CsvParserConfiguration { HasHeader = true, RowSplitter = "\r\n", Delimitter = ';' }))
            {
                await parser.WriteAsync(header);
                await parser.WriteAsync(testContent);
            }

            var afterRead = await File.ReadAllTextAsync(_filePath);
            Assert.That(afterRead, Is.EqualTo($"{header}\r\n{testContent}"));
        }

        [Test]
        public void Should_Convert_CsvTextToDictionary()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new TestModel { Id = 1, Name = "Test" } });
            var data = CsvHelper.CsvTextToList(csv,hasHeader:true);
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0]["Id"], Is.EqualTo("1"));
            Assert.That(data[0]["Name"], Is.EqualTo("Test"));
        }

        [Test]
        public void Should_Convert_CsvTextToList()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new TestModel { Id = 1, Name = "Test" } });
            var data = CsvHelper.CsvTextToList<TestModel>(csv, hasHeader: true);
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Id, Is.EqualTo(1));
            Assert.That(data[0].Name, Is.EqualTo("Test"));
        }

        [Test]
        public async Task Should_Query_CsvData()
        {
            var csv = CsvParser.Open(_readFilePath, new CsvParserConfiguration { HasHeader = true });
            var data = await csv.Query<TestModel>(t=>t.Id==3).ToListAsync();
            Assert.That(data.Count, Is.EqualTo(1));
            Assert.That(data[0].Id, Is.EqualTo(3));
        }

        [Test]
        public void Should_Convert_CsvTextToJson()
        {
            var csv = CsvHelper.ConvertToCsv(new List<TestModel> { new() { Id = 1, Name = "Test" } });
            var data = CsvHelper.CsvTextToJson(csv, hasHeader: true);
            Assert.That(data, Is.EqualTo("[{\"Id\":\"1\",\"Name\":\"Test\"}]"));
        }

        [Test]
        public void Should_Convert_ToCsv()
        {
            var data = new List<TestModel> { new() { Id = 1, Name = "Test" } };
            var csv = CsvHelper.ConvertToCsv(data);
            Assert.That(csv, Is.EqualTo("Id,Name\r\n1,Test"));
        }

        [Test]
        public void Should_Convert_Dictionary_To_Csv()
        {
            var data = new List<Dictionary<string, string>> { new() { { "Id", "1" }, { "Name", "Test" } } };
            var csv = CsvHelper.ConvertToCsv(data);
            Assert.That(csv, Is.EqualTo("Id,Name\r\n1,Test"));
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