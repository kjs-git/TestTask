using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Models
{
    public class CsvRowDto
    {
        public DateTime Date { get; set; }
        public int ExecutionTime { get; set; }
        public double Value { get; set; }
    }
}
