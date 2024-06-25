using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.CsvParser
{
    public class CsvParserConfiguration
    {
        public bool HasHeader { get; set; }
        public char Delimitter { get; set; } = ',';
        public string RowSplitter { get; set; } = "\r\n";

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}
