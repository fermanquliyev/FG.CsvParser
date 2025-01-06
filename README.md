# FG.CsvParser

FG.CsvParser is a .NET Standard 2.1 library for parsing and writing CSV files. It provides a flexible and easy-to-use API for reading, writing, and querying CSV data.

## Features

- Read CSV files and convert them to JSON or a list of objects.
- Write CSV content to files.
- Configure CSV parsing options such as delimiter, row splitter, and encoding.
- Query CSV files with custom filters.

## Installation

To install FG.CsvParser, add the following package to your project:
## Usage

### Creating a CsvParser Instance

You can create a `CsvParser` instance using one of the static `OpenFile` methods:
1. **Open a CSV file with default settings:**
```c#
var parser = CsvParser.OpenFile("path/to/your/file.csv");
```
2. **Open a CSV file and specify if it has a header row:**
```c#
var parser = CsvParser.OpenFile("path/to/your/file.csv", hasHeader: true);
```
3. **Open a CSV file with a custom configuration:**
```c#
var configuration = new CsvParserConfiguration
{
    HasHeader = true,
    Delimitter = ',',
    RowSplitter = "&#x0a;",
    Encoding = Encoding.UTF8
};
var parser = CsvParser.OpenFile("path/to/your/file.csv", configuration);
```

### Reading CSV Content

You can read the CSV content and convert it to JSON or a list of objects:

1. **Reading CSV Content as JSON**
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

string? jsonContent = await parser.ReadAsJson();
Console.WriteLine(jsonContent);
```
2. **Reading CSV Content as a List of Objects**
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

List<MyDataClass> dataList = await parser.ReadAs<MyDataClass>();
foreach (var data in dataList)
{
    Console.WriteLine(data);
}
```
### Writing CSV Content

You can write CSV content to a file:
1. **Writing CSV Content as a String**
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

string csvContent = "Column1,Column2\nValue1,Value2\nValue3,Value4";
await parser.WriteAsync(csvContent, append: false);

Console.WriteLine("CSV content written to file.");
```
2. **Writing a List of Objects to CSV**
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

var dataList = new List<MyDataClass>
{
    new MyDataClass { Column1 = "Value1", Column2 = 1 },
    new MyDataClass { Column1 = "Value2", Column2 = 2 }
};

await parser.WriteAsync(dataList, append: false);

Console.WriteLine("List of objects written to CSV file.");
```
3. **Appending CSV Content**
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

string csvContent = "Value5,Value6\nValue7,Value8";
await parser.WriteAsync(csvContent, append: true);

Console.WriteLine("CSV content appended to file.");
```
### Querying CSV Content

You can query the CSV content with a custom filter:
```c#
var filePath = "path/to/your/csvfile.csv";
using var parser = CsvParser.OpenFile(filePath, new CsvParserConfiguration { HasHeader = true, Delimitter = ',', RowSplitter = "\r\n", Encoding = Encoding.UTF8 });

await foreach (var item in parser.Query<MyDataClass>(data => data.Column2 > 100))
{
    Console.WriteLine(item);
}
```
## Configuration

The `CsvParserConfiguration` class allows you to configure various options for the CSV parser:
```c#
var filePath = "path/to/your/csvfile.csv";

// Create a configuration for the CSV parser
var configuration = new CsvParserConfiguration
{
    HasHeader = true,
    Delimitter = ';',
    RowSplitter = "\n",
    Encoding = Encoding.UTF8
};

// Open the CSV file with the specified configuration
using var parser = CsvParser.OpenFile(filePath, configuration);

// Example: Reading CSV content as JSON
string? jsonContent = await parser.ReadAsJson();
Console.WriteLine("CSV content as JSON:");
Console.WriteLine(jsonContent);

// Example: Writing CSV content
var dataList = new List<MyDataClass>
{
    new MyDataClass { Column1 = "Value1", Column2 = 1 },
    new MyDataClass { Column1 = "Value2", Column2 = 2 }
};
await parser.WriteAsync(dataList, append: false);
Console.WriteLine("List of objects written to CSV file.");
```
## License

This project is licensed under the MIT License. See the [LICENSE](https://opensource.org/license/mit) file for details.
