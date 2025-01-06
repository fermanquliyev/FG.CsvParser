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
### Reading CSV Content

You can read the CSV content and convert it to JSON or a list of objects:
### Writing CSV Content

You can write CSV content to a file:
### Querying CSV Content

You can query the CSV content with a custom filter:
## Configuration

The `CsvParserConfiguration` class allows you to configure various options for the CSV parser:
## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
