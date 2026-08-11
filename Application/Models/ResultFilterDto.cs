using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Models
{
    public class ResultFilterDto
    {
        public string? FileName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? MinAverageExecutionTime { get; set; }
        public double? MaxAverageExecutionTime { get; set; }
        public double? MinAverageValue { get; set; }
        public double? MaxAverageValue { get; set; }
    }
}
