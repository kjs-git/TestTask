using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ValueRecord> Values { get; set; }
        public DbSet<FileResult> Results { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Values
            modelBuilder.Entity<ValueRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Date).IsRequired();
                entity.HasIndex(e => new { e.FileName, e.Date });
            });

            // Настройка таблицы Results
            modelBuilder.Entity<FileResult>(entity =>
            {
                entity.HasKey(e => e.FileName);
                entity.Property(e => e.FileName).HasMaxLength(255);

                entity.HasIndex(e => e.MinDate);
                entity.HasIndex(e => e.AverageValue);
                entity.HasIndex(e => e.AverageExecutionTime);
            });
        }
    }
}