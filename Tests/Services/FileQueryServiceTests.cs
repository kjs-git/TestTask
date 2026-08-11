using Application.Models;
using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Services
{
    public class FileQueryServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetResultsAsync_WithValueFilter_ShouldReturnOnlyMatchingRecords()
        {
            var dbContext = GetDbContext();

            dbContext.Results.AddRange(
                new FileResult { FileName = "file1.csv", AverageValue = 10.0, DeltaTime = 0, MinDate = DateTime.UtcNow, AverageExecutionTime = 1, MaxValue = 10, MinValue = 10, MedianValue = 10 },
                new FileResult { FileName = "file2.csv", AverageValue = 25.0, DeltaTime = 0, MinDate = DateTime.UtcNow, AverageExecutionTime = 1, MaxValue = 25, MinValue = 25, MedianValue = 25 },
                new FileResult { FileName = "file3.csv", AverageValue = 50.0, DeltaTime = 0, MinDate = DateTime.UtcNow, AverageExecutionTime = 1, MaxValue = 50, MinValue = 50, MedianValue = 50 }
            );
            await dbContext.SaveChangesAsync();

            var service = new FileQueryService(dbContext);

            var filter = new ResultFilterDto
            {
                MinAverageValue = 20.0,
                MaxAverageValue = 30.0
            };

            var results = await service.GetResultsAsync(filter);

            results.Should().HaveCount(1);
            results.First().FileName.Should().Be("file2.csv");
        }

        [Fact]
        public async Task GetLastValuesAsync_ShouldReturnTop10OrderedByDateDescending()
        {
            var dbContext = GetDbContext();
            var fileName = "test.csv";

            for (int i = 1; i <= 15; i++)
            {
                dbContext.Values.Add(new ValueRecord
                {
                    FileName = fileName,
                    Date = new DateTime(2024, 1, i, 12, 0, 0, DateTimeKind.Utc),
                    ExecutionTime = 10,
                    Value = i * 1.5
                });
            }
            await dbContext.SaveChangesAsync();

            var service = new FileQueryService(dbContext);

            var results = await service.GetLastValuesAsync(fileName);

            results.Should().HaveCount(10); 
            results.First().Date.Day.Should().Be(15);
            results.Last().Date.Day.Should().Be(6);
        }
    }
}
