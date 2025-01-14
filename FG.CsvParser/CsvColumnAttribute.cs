using System;
using System.Collections.Generic;
using System.Text;

namespace FG.CsvParser
{
    /// <summary>
    /// Attribute to specify the name of the column in the CSV file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CsvColumnAttribute: Attribute
    {
        /// <summary>
        /// Gets the Name of the column in the CSV file.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the order of the column in the CSV file.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the column in the CSV file.</param>
        /// <param name="order">(Optional) Order of the column when writing into the CSV file.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CsvColumnAttribute(string name, int order = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.Name = name;
            this.Order = order;
        }
    }
}
