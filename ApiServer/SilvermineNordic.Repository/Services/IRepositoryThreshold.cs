using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilvermineNordic.Repository.Services
{
    public interface IRepositoryThreshold
    {
        public Task<Threshold> UpdateThreshold(Threshold threshold);
        public Task<IEnumerable<Threshold>> GetThresholds();
        public Task SeedData();
    }
}
