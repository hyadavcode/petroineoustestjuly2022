namespace Petroineous.Service.Core
{
    public static class Constants
    {
        public const string EventLogSourceName = "PetroineousExtractService";
        public const string EventLogName = "Application";

        public const string ExtractsLogFilenameConfigKey = "ExtractsLogFilename";
        public const string PowerExtractOutputDirectoryConfigKey = "OutputDirectory";
        public const string PowerExtractIntervalConfigKey = "IntradayPowerPositionExtractIntervalSeconds";
        public const string ExtractOrchestratorStopWaitTimespan = "ServiceStopWaitTimeSeconds";

        public class ExtractType
        {
            public const string IntradayPowerPosition = "IntradayPowerPosition";
        }        
    }
}
