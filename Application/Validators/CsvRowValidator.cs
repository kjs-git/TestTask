using Application.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validators
{
    public class CsvRowValidator : AbstractValidator<CsvRowDto>
    {
        public CsvRowValidator()
        {
            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(new DateTime(2000, 1, 1)).WithMessage("Дата не может быть раньше 01.01.2000.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Дата не может быть позже текущей.");

            RuleFor(x => x.ExecutionTime)
                .GreaterThanOrEqualTo(0).WithMessage("Время выполнения не может быть меньше 0.");

            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Значение показателя не может быть меньше 0.");
        }
    }
}
