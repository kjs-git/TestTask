using Application.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using FluentValidation;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IFileProcessingService
    {
        Task ProcessFileAsync(Stream fileStream, string fileName);
    }

    public class FileProcessingService : IFileProcessingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<CsvRowDto> _validator;

        public FileProcessingService(ApplicationDbContext dbContext, IValidator<CsvRowDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task ProcessFileAsync(Stream fileStream, string fileName)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<CsvRowMap>();

            var rows = new List<CsvRowDto>();
            int rowCount = 0;


            await foreach (var record in csv.GetRecordsAsync<CsvRowDto>())
            {
                rowCount++;
                if (rowCount > 10000)
                    throw new Exception("Количество строк не может быть больше 10 000."); 

                var validationResult = await _validator.ValidateAsync(record);
                if (!validationResult.IsValid)
                    throw new Exception($"Ошибка в строке {rowCount}: {validationResult.Errors.First().ErrorMessage}"); 

                rows.Add(record);
            }

            if (rowCount < 1)
                throw new Exception("Количество строк не может быть меньше 1."); 

            var valueRecords = rows.Select(r => new ValueRecord
            {
                FileName = fileName,
                Date = r.Date.ToUniversalTime(),
                ExecutionTime = r.ExecutionTime,
                Value = r.Value
            }).ToList();

            var fileResult = CalculateMetrics(fileName, rows);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                bool fileExists = await _dbContext.Results.AnyAsync(x => x.FileName == fileName);
                if (fileExists)
                {
                    await _dbContext.Values.Where(x => x.FileName == fileName).ExecuteDeleteAsync();
                    await _dbContext.Results.Where(x => x.FileName == fileName).ExecuteDeleteAsync();
                }

                await _dbContext.Values.AddRangeAsync(valueRecords);
                await _dbContext.Results.AddAsync(fileResult);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private FileResult CalculateMetrics(string fileName, List<CsvRowDto> rows)
        {
            var dates = rows.Select(r => r.Date).ToList();
            var executionTimes = rows.Select(r => r.ExecutionTime).ToList();
            var values = rows.Select(r => r.Value).OrderBy(v => v).ToList();

            double median = values.Count % 2 == 0
                ? (values[(values.Count / 2) - 1] + values[values.Count / 2]) / 2.0
                : values[values.Count / 2];

            return new FileResult
            {
                FileName = fileName,
                DeltaTime = (dates.Max() - dates.Min()).TotalSeconds,
                MinDate = dates.Min().ToUniversalTime(),
                AverageExecutionTime = executionTimes.Average(),
                AverageValue = values.Average(),
                MedianValue = median,
                MaxValue = values.Max(),
                MinValue = values.Min()
            };
        }
    }
}
