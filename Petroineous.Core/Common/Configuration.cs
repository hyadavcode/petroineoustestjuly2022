using System.Configuration;

namespace Petroineous.Core.Configuration
{
    public class Configuration : IConfiguration
    {
        private readonly AppSettingsReader _appSettingsReader = new AppSettingsReader();

        public int IntradayExtractScheduleInterval => (int)_appSettingsReader.GetValue(Constants.PowerExtractIntervalConfigKey, typeof(int));
        public string PowerExtractOutputDirectory => (string)_appSettingsReader.GetValue(Constants.PowerExtractOutputDirectoryConfigKey, typeof(string));
    }
}
