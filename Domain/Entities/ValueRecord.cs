using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ValueRecord
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty; // Для привязки записи к конкретному файлу
        public DateTime Date { get; set; } // Время начала операции
        public int ExecutionTime { get; set; } // Время выполнения в секундах
        public double Value { get; set; } // Показатель
    }
}
