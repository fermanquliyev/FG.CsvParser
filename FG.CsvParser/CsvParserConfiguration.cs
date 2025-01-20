using System;
using System.Text;

namespace FG.CsvParser
{
    public class CsvParserConfiguration
    {
        public bool HasHeader { get; set; }
        public char Delimitter { get; set; } = ',';
        public string RowSplitter { get; set; } = Environment.NewLine;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}
