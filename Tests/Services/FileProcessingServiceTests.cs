using Application.Models;
using Application.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class FileProcessingServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SqliteConnection _connection;

        public FileProcessingServiceTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();
        }

        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public async Task ProcessFileAsync_WithValidData_ShouldCalculateMetricsCorrectly()
        {
            var validatorMock = new Mock<IValidator<CsvRowDto>>();

            var successResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(It.IsAny<CsvRowDto>())).Returns(successResult);
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CsvRowDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(successResult);

            var service = new FileProcessingService(_dbContext, validatorMock.Object);

            var csvContent = @"Date;ExecutionTime;Value
2024-01-01T10-00-00.0000000Z;10;15.0
2024-01-01T10-05-00.0000000Z;20;25.0
2024-01-01T10-10-00.0000000Z;30;35.0";
            var stream = GenerateStreamFromString(csvContent);
            var fileName = "test_metrics.csv";

            await service.ProcessFileAsync(stream, fileName);

            var result = await _dbContext.Results.FirstOrDefaultAsync(x => x.FileName == fileName);

            result.Should().NotBeNull();
            result!.AverageExecutionTime.Should().Be(20);
            result.AverageValue.Should().Be(25);
            result.MedianValue.Should().Be(25);
            result.MinValue.Should().Be(15);
            result.MaxValue.Should().Be(35);
            result.DeltaTime.Should().Be(600);

            var valuesCount = await _dbContext.Values.CountAsync(x => x.FileName == fileName);
            valuesCount.Should().Be(3);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenValidationFails_ShouldThrowExceptionAndRollback()
        {
            var validatorMock = new Mock<IValidator<CsvRowDto>>();

            var validationFailure = new ValidationFailure("Value", "Значение не может быть отрицательным");
            var failedResult = new ValidationResult(new[] { validationFailure });

            validatorMock.Setup(v => v.Validate(It.IsAny<CsvRowDto>())).Returns(failedResult);
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CsvRowDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(failedResult);

            var service = new FileProcessingService(_dbContext, validatorMock.Object);

            var csvContent = @"Date;ExecutionTime;Value
2024-01-01T10-00-00.0000000Z;10;-5.0";
            var stream = GenerateStreamFromString(csvContent);

            Func<Task> action = async () => await service.ProcessFileAsync(stream, "bad_file.csv");
            await action.Should().ThrowAsync<Exception>().WithMessage("*Значение не может быть отрицательным*");

            var valuesCount = await _dbContext.Values.CountAsync();
            valuesCount.Should().Be(0);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }
    }
}