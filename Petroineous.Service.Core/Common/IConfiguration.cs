namespace Petroineous.Service.Core.Configuration
{
    public interface IConfiguration
    {
        int IntradayExtractScheduleInterval { get; }
        string PowerExtractOutputDirectory { get; }
        int ExtractOrchestratorStopWaitTimespan { get; }
        string ExtractsLogFilename { get; }
    }
}
