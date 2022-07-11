namespace Petroineous.Core.Configuration
{
    public interface IConfiguration
    {
        int IntradayExtractScheduleInterval { get; }
        string PowerExtractOutputDirectory { get; }
    }
}
