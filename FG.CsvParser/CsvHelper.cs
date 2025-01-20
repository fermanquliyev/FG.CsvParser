using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;

namespace FG.CsvParser
{
    public static class CsvHelper
    {

        /// <summary>
        /// Convert CSV text to a list of dictionaries. Each dictionary represents a row in the CSV text.
        /// </summary>
        /// <param name="textContent">The CSV text content.</param>
        /// <param name="hasHeader">Indicates if the CSV text has a header row.</param>
        /// <param name="rowSplitter">The string used to split rows in the CSV text.</param>
        /// <param name="columnSplitter">The character used to split columns in the CSV text.</param>
        /// <returns>A list of dictionaries where each dictionary represents a row in the CSV text.</returns>
        public static List<Dictionary<string, object>> CsvTextToDictionary(string textContent, bool hasHeader = false, string rowSplitter = "", char columnSplitter = ',')
        {
            if (string.IsNullOrEmpty(rowSplitter))
            {
                rowSplitter = Environment.NewLine;
            }
            var list = new List<Dictionary<string, object>>();
            var rows = textContent.Split(rowSplitter).ToList();
            var headers = new List<string>();
            if (hasHeader)
            {
                var headerRow = rows.First();
                rows.RemoveAt(0);
                headers = headerRow.Split(columnSplitter).ToList();
                if (headers.Count == 0)
                {
                    hasHeader = false;
                }
            }
            foreach (var row in rows)
            {
                var columns = row.Split(columnSplitter);
                var dictionary = new Dictionary<string, object>();
                for (var i = 0; i < columns.Length; i++)
                {
                    if (hasHeader)
                    {
                        dictionary[headers[i]] = columns[i];
                    }
                    else
                    {
                        dictionary[i.ToString()] = columns[i];
                    }
                }
                list.Add(dictionary);
            }
            return list;
        }

        /// <summary>
        /// Convert CSV text to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to convert each row to.</typeparam>
        /// <param name="textContent">The CSV text content.</param>
        /// <param name="hasHeader">Indicates if the CSV text has a header row.</param>
        /// <param name="rowSplitter">The string used to split rows in the CSV text.</param>
        /// <param name="columnSplitter">The character used to split columns in the CSV text.</param>
        /// <returns>A list of objects of type T where each object represents a row in the CSV text.</returns>
        public static List<T> CsvTextToList<T>(string textContent, bool hasHeader = false, string rowSplitter = "", char columnSplitter = ',')
        {
            var list = new List<T>();
            if (string.IsNullOrEmpty(rowSplitter))
            {
                rowSplitter = Environment.NewLine;
            }
            var rows = textContent.Split(rowSplitter).ToList();
            var headers = new List<string>();
            var properties = typeof(T).GetProperties();
            if (hasHeader)
            {
                var headerRow = rows.First();
                rows.RemoveAt(0);
                headers = headerRow.Split(columnSplitter).ToList();
                if (headers.Count == 0)
                {
                    hasHeader = false;
                }
            }
            foreach (var row in rows)
            {
                T instance = ParseCsvLine<T>(hasHeader, columnSplitter, headers, row, properties);
                list.Add(instance);
            }
            return list;
        }

        internal static T ParseCsvLine<T>(bool hasHeader, char columnSplitter, List<string> headers, string? row, PropertyInfo[] properties)
        {
            if (string.IsNullOrEmpty(row) || properties is null || properties.Length == 0)
            {
                return default;
            }
            var columns = row.Split(columnSplitter);
            var instance = Activator.CreateInstance<T>();
            for (var i = 0; i < columns.Length; i++)
            {
                var value = columns[i];
                if (hasHeader)
                {
                    var header = headers[i];
                    var property = properties.FirstOrDefault(p => (p.GetCustomAttribute<CsvColumnAttribute>()?.Name.Equals(header, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                    SetPropertyOfType(instance, value, property);
                }
                else
                {
                    if (i < properties.Length)
                    {
                        var property = properties[i];
                        SetPropertyOfType(instance, value, property);
                    }
                }
            }

            return instance;
        }

        private static void SetPropertyOfType<U>(U instance, string value, PropertyInfo? property)
        {
            if (property == null)
            {
                return;
            }
            try
            {
                property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
            }
            catch (InvalidCastException ex)
            {
                // Handle the specific exception for invalid cast
                // You can choose to log the error, throw a custom exception, or take any other appropriate action
                Console.WriteLine($"Invalid cast exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other general exceptions
                // You can choose to log the error, throw a custom exception, or take any other appropriate action
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
        }


        /// <summary>
        /// Convert CSV text to JSON format.
        /// </summary>
        /// <param name="textContent">The CSV text content.</param>
        /// <param name="hasHeader">Indicates if the CSV text has a header row.</param>
        /// <param name="rowSplitter">The string used to split rows in the CSV text. Default value <see cref="Environment.NewLine"/> will be set if it is left null or empty. </param>
        /// <param name="columnSplitter">The character used to split columns in the CSV text.</param>
        /// <returns>A JSON string representing the CSV data.</returns>
        public static string CsvTextToJson(string textContent, bool hasHeader = false, string rowSplitter = "", char columnSplitter = ',')
        {
            if (string.IsNullOrEmpty(rowSplitter))
            {
                rowSplitter = Environment.NewLine;
            }
            var list = CsvTextToDictionary(textContent, hasHeader, rowSplitter, columnSplitter);
            return JsonSerializer.Serialize(list, list.GetType());
        }

        /// <summary>
        /// Converts a list of objects to a CSV formatted string.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list.</typeparam>
        /// <param name="dataList">The list of objects to convert to CSV format.</param>
        /// <param name="hasHeader">Indicates whether to include a header row in the CSV output.</param>
        /// <returns>A CSV formatted string representing the list of objects.</returns>
        /// <exception cref="ArgumentException">Thrown when the data list is null or empty.</exception>
        public static string ConvertToCsv<T>(IList<T> dataList, bool hasHeader = true)
        {
            if (dataList == null || dataList.Count == 0)
            {
                throw new ArgumentException("Data list is null or empty.");
            }

            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                return ConvertDictionaryListToCsv(dataList.Cast<Dictionary<string, object>>().ToList(), hasHeader);
            }
            else if (typeof(T) == typeof(Dictionary<string, string>))
            {
                return ConvertDictionaryListToCsv(dataList.Cast<Dictionary<string, string>>().ToList(), hasHeader);
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Get properties of the object type T
            PropertyInfo[] properties = typeof(T).GetProperties().OrderBy(x=> x.GetCustomAttribute<CsvColumnAttribute>()?.Order ?? 0).ToArray();

            if (hasHeader)
            {
                // Write CSV header using property names
                foreach (PropertyInfo prop in properties)
                {
                    var customAttributeName = prop.GetCustomAttribute<CsvColumnAttribute>()?.Name;
                    csvBuilder.Append($"{customAttributeName ?? prop.Name},");
                }
                csvBuilder.Length--; // Remove the last comma
                csvBuilder.AppendLine(); // Move to next line after writing header
            }

            // Write CSV data rows
            foreach (T item in dataList)
            {
                foreach (PropertyInfo prop in properties)
                {
                    object value = prop.GetValue(item);
                    // Handle null values by writing an empty string
                    string? valueString = value != null ? value.ToString() : "";
                    csvBuilder.Append($"{EscapeCsvValue(valueString ?? string.Empty)},");
                }
                csvBuilder.Length--; // Remove the last comma
                csvBuilder.AppendLine(); // Move to next line after writing a row
            }

            // Remove the last appended new line
            csvBuilder.Length -= Environment.NewLine.Length;

            return csvBuilder.ToString();
        }

        private static string ConvertDictionaryListToCsv<T>(IList<Dictionary<string, T>> dataList, bool hasHeader = true)
        {
            if (dataList == null || dataList.Count == 0)
            {
                throw new ArgumentException("Data list is null or empty.");
            }

            StringBuilder csvBuilder = new StringBuilder();

            if (hasHeader)
            {
                // Write CSV header using dictionary keys
                var headers = dataList.First().Keys;
                foreach (var header in headers)
                {
                    csvBuilder.Append($"{header},");
                }
                csvBuilder.Length--; // Remove the last comma
                csvBuilder.AppendLine(); // Move to next line after writing header
            }

            // Write CSV data rows
            foreach (var row in dataList)
            {
                foreach (var value in row.Values)
                {
                    // Handle null values by writing an empty string
                    var valueString = value != null ? value.ToString() : "";
                    csvBuilder.Append($"{EscapeCsvValue(valueString ?? "")},");
                }
                csvBuilder.Length--; // Remove the last comma
                csvBuilder.AppendLine(); // Move to next line after writing a row
            }

            // Remove the last appended new line
            csvBuilder.Length -= Environment.NewLine.Length;

            return csvBuilder.ToString();
        }

        private static string EscapeCsvValue(string value)
        {
            // Escape double quotes by doubling them
            return $"{value.Replace("\"", "\"\"")}";
        }
    }
}
