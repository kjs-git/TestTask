using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class FileResult
    {
        public string FileName { get; set; } = string.Empty;

        public double DeltaTime { get; set; } // Дельта времени Date в секундах
        public DateTime MinDate { get; set; } // Минимальное дата и время
        public double AverageExecutionTime { get; set; } // Среднее время выполнения
        public double AverageValue { get; set; } // Среднее значение по показателям
        public double MedianValue { get; set; } // Медиана по показателям
        public double MaxValue { get; set; } // Максимальное значение показателя
        public double MinValue { get; set; } // Минимальное значение показателя
    }
}
