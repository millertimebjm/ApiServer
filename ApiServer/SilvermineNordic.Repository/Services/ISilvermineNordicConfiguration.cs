
namespace SilvermineNordic.Repository
{
    public interface ISilvermineNordicConfiguration
    {
        public string GetStorageConnectionString();
        public string GetStorageName();
        public string GetSqlConnectionString();
        public string GetOpenWeatherApiKey();
        public string GetAzureSmsConnectionString();
        public string GetAzureSmsFromPhone();
        public string GetSilvermineNordicApiUrl();
        public string GetZoneNotificationPhoneNumbers();
        public string GetEmailServiceConnectionString();
        public string GetInMemoryDatabaseName();
        public string GetProtocol();
    }
}