using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Models
{
    public sealed class CsvRowMap : ClassMap<CsvRowDto>
    {
        public CsvRowMap()
        {
            Map(m => m.Date).Index(0).TypeConverterOption.Format("yyyy-MM-ddTHH-mm-ss.fffffffZ");
            Map(m => m.ExecutionTime).Index(1);
            Map(m => m.Value).Index(2);
        }
    }
}
