using Application.Models;
using Domain.Entities;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IFileQueryService
    {
        Task<List<FileResult>> GetResultsAsync(ResultFilterDto filter);
        Task<List<ValueRecord>> GetLastValuesAsync(string fileName);
    }

    public class FileQueryService : IFileQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public FileQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FileResult>> GetResultsAsync(ResultFilterDto filter)
        {
            var query = _dbContext.Results.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FileName))
                query = query.Where(x => x.FileName == filter.FileName);

            if (filter.StartDate.HasValue)
            {
                var startUtc = filter.StartDate.Value.ToUniversalTime();
                query = query.Where(x => x.MinDate >= startUtc);
            }
            if (filter.EndDate.HasValue)
            {
                var endUtc = filter.EndDate.Value.ToUniversalTime();
                query = query.Where(x => x.MinDate <= endUtc);
            }

            if (filter.MinAverageExecutionTime.HasValue)
                query = query.Where(x => x.AverageExecutionTime >= filter.MinAverageExecutionTime.Value);
            if (filter.MaxAverageExecutionTime.HasValue)
                query = query.Where(x => x.AverageExecutionTime <= filter.MaxAverageExecutionTime.Value);

            if (filter.MinAverageValue.HasValue)
                query = query.Where(x => x.AverageValue >= filter.MinAverageValue.Value);
            if (filter.MaxAverageValue.HasValue)
                query = query.Where(x => x.AverageValue <= filter.MaxAverageValue.Value);

            return await query.ToListAsync();
        }

        public async Task<List<ValueRecord>> GetLastValuesAsync(string fileName)
        {
            return await _dbContext.Values
                .Where(x => x.FileName == fileName)
                .OrderByDescending(x => x.Date)
                .Take(10)
                .ToListAsync();
        }
    }
}
