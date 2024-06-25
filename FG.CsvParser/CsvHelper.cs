using System.Reflection;
using System.Text;
using System.Text.Json;

namespace FG.CsvParser
{
    public static class CsvHelper
    {
        public static List<Dictionary<string, object>> CsvTextToList(string textContent, bool hasHeader = false, string rowSplitter = "\r\n", char columnSplitter = ',')
        {
            var list = new List<Dictionary<string, object>>();
            var rows = textContent.Split(rowSplitter).ToList();
            var headers = new List<string>();
            if (hasHeader)
            {
                var headerRow = rows.First();
                rows.RemoveAt(0);
                headers = [.. headerRow.Split(columnSplitter)];
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

        public static List<T> CsvTextToList<T>(string textContent, bool hasHeader = false, string rowSplitter = "\r\n", char columnSplitter = ',')
        {
            var list = new List<T>();
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
                T? instance = ParseCsvLine<T>(hasHeader, columnSplitter, headers, row);
                list.Add(instance);
            }
            return list;

            
        }

        public static T? ParseCsvLine<T>(bool hasHeader, char columnSplitter, List<string> headers, string? row)
        {
            if (string.IsNullOrEmpty(row))
            {
                return default;
            }
            var columns = row.Split(columnSplitter);
            var instance = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();
            for (var i = 0; i < columns.Length; i++)
            {
                var value = columns[i];
                if (hasHeader)
                {
                    var header = headers[i];
                    var property = properties.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
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

        static void SetPropertyOfType<U>(U? instance, string value, PropertyInfo? property)
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
        public static string CsvTextToJson(string textContent, bool hasHeader = false, string rowSplitter = "\r\n", char columnSplitter = ',')
        {
            var list = CsvTextToList(textContent, hasHeader, rowSplitter, columnSplitter);
            return JsonSerializer.Serialize(list, list.GetType());
        }

        public static string ConvertToCsv<T>(IList<T> dataList, bool hasHeader = true)
        {
            if (dataList == null || dataList.Count == 0)
            {
                throw new ArgumentException("Data list is null or empty.");
            }

            if(typeof(T)==typeof(Dictionary<string, object>))
            {
                return ConvertDictionaryListToCsv(dataList.Cast<Dictionary<string, object>>().ToList(), hasHeader);
            } else if (typeof(T) == typeof(Dictionary<string, string>))
            {
                return ConvertDictionaryListToCsv(dataList.Cast<Dictionary<string, string>>().ToList(), hasHeader);
            }

            StringBuilder csvBuilder = new();

            // Get properties of the object type T
            PropertyInfo[] properties = typeof(T).GetProperties();

            if (hasHeader)
            {
                // Write CSV header using property names
                foreach (PropertyInfo prop in properties)
                {
                    csvBuilder.Append($"{prop.Name},");
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

            StringBuilder csvBuilder = new();

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
