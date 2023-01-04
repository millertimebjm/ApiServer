
namespace SilvermineNordic.Repository
{
    public class SilvermineNordicConfigurationService : ISilvermineNordicConfiguration
    {
        public string StorageConnectionString { get; set; }
        public string StorageName { get; set; }
        public string SqlConnectionString { get; set; }
        public string OpenWeatherApiKey { get; set; }
        public string AzureSmsConnectionString { get; set; }
        public string AzureSmsFromPhone { get; set; }
        public string SilvermineNordicApiUrl { get; set; }
        public string ZoneNotificationPhoneNumbers { get; set; }
        public string EmailServiceConnectionString { get; set; }
        public string InMemoryDatabaseName { get; set; }
        public string Protocol { get; set; }

        public SilvermineNordicConfigurationService()
        {

        }

        public string GetSqlConnectionString()
        {
            return SqlConnectionString;
        }

        public string GetStorageConnectionString()
        {
            return StorageConnectionString;
        }

        public string GetStorageName()
        {
            return StorageName;
        }

        public string GetOpenWeatherApiKey()
        {
            return OpenWeatherApiKey;
        }

        public string GetAzureSmsConnectionString()
        {
            return AzureSmsConnectionString;
        }

        public string GetAzureSmsFromPhone()
        {
            return AzureSmsFromPhone;
        }

        public string GetSilvermineNordicApiUrl()
        {
            return SilvermineNordicApiUrl;
        }

        public string GetZoneNotificationPhoneNumbers()
        {
            return ZoneNotificationPhoneNumbers;
        }

        public string GetEmailServiceConnectionString()
        {
            return EmailServiceConnectionString;
        }

        public string GetInMemoryDatabaseName()
        {
            return InMemoryDatabaseName;
        }

        public string GetProtocol()
        {
            return Protocol;
        }
    }
}
