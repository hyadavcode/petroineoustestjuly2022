using System.Configuration;

namespace Petroineous.Service.Core.Configuration
{
    public class Configuration : IConfiguration
    {
        private readonly AppSettingsReader _appSettingsReader = new AppSettingsReader();

        public int IntradayExtractScheduleInterval => (int)_appSettingsReader.GetValue(Constants.PowerExtractIntervalConfigKey, typeof(int));
        public string PowerExtractOutputDirectory => (string)_appSettingsReader.GetValue(Constants.PowerExtractOutputDirectoryConfigKey, typeof(string));
        public int ExtractOrchestratorStopWaitTimespan => (int)_appSettingsReader.GetValue(Constants.ExtractOrchestratorStopWaitTimespan, typeof(string));
        public string ExtractsLogFilename => (string)_appSettingsReader.GetValue(Constants.ExtractsLogFilenameConfigKey, typeof(string));

    }
}
