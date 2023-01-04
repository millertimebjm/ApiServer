using Microsoft.EntityFrameworkCore;
using SilvermineNordic.Models;
using System;

namespace SilvermineNordic.Repository.Services
{
    public class SilvermineNordicDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }

        public DbSet<Threshold> Thresholds { get; set; }
        public DbSet<CommunicationLog> CommunicationLogs { get; set; }


        public DbSet<SensorReading> SensorReadings { get; set; }
        private readonly ISilvermineNordicConfiguration _configuration;

        public SilvermineNordicDbContext(ISilvermineNordicConfiguration configuration)
        : base()
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var sqlConnectionString = _configuration.GetSqlConnectionString();
            if (!string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                optionsBuilder.UseSqlServer(sqlConnectionString);
                return;
            }

            var inMemoryDatabaseName = _configuration.GetInMemoryDatabaseName();
            if (!string.IsNullOrWhiteSpace(inMemoryDatabaseName))
            {
                optionsBuilder.UseInMemoryDatabase(inMemoryDatabaseName);
                return;
            }

            throw new NotImplementedException();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReading>().ToTable("SensorReading");
            modelBuilder.Entity<Threshold>().ToTable("SensorThreshold");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserOtp>().ToTable("UserOtp");
        }
    }
}

