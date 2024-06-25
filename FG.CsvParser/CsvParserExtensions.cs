using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.CsvParser
{
    public static class CsvParserExtensions
    {
        public static IServiceCollection AddCsvParser(this IServiceCollection services, CsvParserConfiguration csvParserConfiguration)
        {
            ArgumentNullException.ThrowIfNull(csvParserConfiguration);
            services.AddScoped((p) =>
            {
                var parser = new CsvParser(csvParserConfiguration);
                return parser;
            });
            return services;
        }

        public static IServiceCollection AddCsvParser(this IServiceCollection services)
        {
            services.AddScoped((p) =>
            {
                var parser = new CsvParser();
                return parser;
            });
            return services;
        }
    }
}
