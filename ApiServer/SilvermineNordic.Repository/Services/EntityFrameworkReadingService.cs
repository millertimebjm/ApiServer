﻿using Microsoft.EntityFrameworkCore;
using SilvermineNordic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilvermineNordic.Repository.Services
{
    public class EntityFrameworkReadingService : IRepositoryReading
    {
        private readonly SilvermineNordicDbContext _dbContext;
        private readonly SilvermineNordicDbContextFactory _dbContextFactory;

        private const string INMEMORY = "Microsoft.EntityFrameworkCore.InMemory";
        public EntityFrameworkReadingService(
            SilvermineNordicDbContext dbContext,
            SilvermineNordicDbContextFactory dbContextFactory)
        {
            _dbContext = dbContext;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Reading> AddReadingAsync(Reading reading)
        {
            using var context = _dbContextFactory.Create();
            await context.Readings.AddAsync(reading);
            await context.SaveChangesAsync();
            return reading;
        }

        public async Task<IEnumerable<Reading>> GetLastNReadingAsync(
            ReadingTypeEnum type,
            int count,
            int? skip = 0)
        {
            using var context = _dbContextFactory.Create();
            if (context.Database.ProviderName == INMEMORY
                && (await context.Readings.FirstOrDefaultAsync()) == null)
            {
                await SeedData();
            }

            IQueryable<Reading> queryable = context
                .Readings.Where(_ => _.Type == type.ToString())
                .OrderByDescending(_ => _.Id);
            if ((skip ?? 0) > 0) queryable = queryable.Skip(skip.Value);
            return await queryable
                .Take(count)
                .ToListAsync();
        }

        private async Task SeedData()
        {
            using var context = _dbContextFactory.Create();
            await context.Readings.AddAsync(new Reading()
            {
                DateTimestampUtc = DateTime.UtcNow,
                Humidity = 20,
                InsertedDateTimestampUtc = DateTime.UtcNow,
                ReadingDateTimestampUtc = DateTime.UtcNow,
                TemperatureInCelcius = 30,
                Type = ReadingTypeEnum.Sensor.ToString(),
            });
            await context.Readings.AddAsync(new Reading()
            {
                DateTimestampUtc = DateTime.UtcNow,
                Humidity = 21,
                InsertedDateTimestampUtc = DateTime.UtcNow,
                ReadingDateTimestampUtc = DateTime.UtcNow,
                TemperatureInCelcius = 31,
                Type = ReadingTypeEnum.Weather.ToString(),
            });
            await context.SaveChangesAsync();
        }
    }
}
