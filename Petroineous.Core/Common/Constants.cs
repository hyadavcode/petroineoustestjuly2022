namespace Petroineous.Core
{
    public static class Constants
    {
        public const string EventLogSourceName = "PetroineousExtractService";
        public const string EventLogName = "Application";

        public const string PowerExtractOutputDirectoryConfigKey = "OutputDirectory";
        public const string PowerExtractIntervalConfigKey = "IntradayPowerPositionExtractIntervalMinutes";

        public class ExtractType
        {
            public const string IntradayPowerPosition = "IntradayPowerPosition";
        }        
    }
}
