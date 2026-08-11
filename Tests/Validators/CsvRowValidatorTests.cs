using Application.Models;
using Application.Validators;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Validators
{
    public class CsvRowValidatorTests
    {
        private readonly CsvRowValidator _validator;

        public CsvRowValidatorTests()
        {
            _validator = new CsvRowValidator();
        }

        [Fact]
        public void Validate_WhenDataIsValid_ShouldNotHaveErrors()
        {
            var row = new CsvRowDto
            {
                Date = new DateTime(2023, 1, 1),
                ExecutionTime = 10,
                Value = 15.5
            };

            var result = _validator.Validate(row);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenDateIsTooOld_ShouldHaveError()
        {
            var row = new CsvRowDto { Date = new DateTime(1999, 12, 31), ExecutionTime = 10, Value = 15.5 };

            var result = _validator.Validate(row);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage.Contains("Дата не может быть раньше 01.01.2000"));
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(10, -5.5)]
        public void Validate_WhenNumbersAreNegative_ShouldHaveError(int executionTime, double value)
        {
            var row = new CsvRowDto { Date = new DateTime(2023, 1, 1), ExecutionTime = executionTime, Value = value };

            var result = _validator.Validate(row);

            result.IsValid.Should().BeFalse();
        }
    }
}
