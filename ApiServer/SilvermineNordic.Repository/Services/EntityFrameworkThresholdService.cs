using Microsoft.EntityFrameworkCore;
using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilvermineNordic.Repository.Services
{
    public class EntityFrameworkThresholdService : IRepositoryThreshold
    {
        private readonly SilvermineNordicDbContext _dbContext;
        public EntityFrameworkThresholdService(SilvermineNordicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Threshold>> GetThresholds()
        {
            return await _dbContext.Thresholds.ToListAsync();
        }

        public async Task<Threshold> UpdateThreshold(Threshold threshold)
        {
            _dbContext.Update(threshold);
            await _dbContext.SaveChangesAsync();
            return threshold;
        }

        public async Task SeedData()
        {
            await _dbContext.Thresholds.AddAsync(new Threshold()
            {
                Id = 1,
                TemperatureInCelciusLowThreshold = -10m,
                TemperatureInCelciusHighThreshold = -8m,
                HumidityLowThreshold = 0m,
                HumidityHighThreshold = 10m,
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}
