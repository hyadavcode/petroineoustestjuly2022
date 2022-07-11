using Petroineous.Service.Core.Configuration;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Petroineous.Service.Core.Common
{
    public class Logger : ILogger
    {
        private readonly IConfiguration _configuration;
        private EventLog _eventLog;         

        public Logger(IConfiguration configuration)
        {
            this._configuration = configuration;
            Init();
        }

        private void Init()
        {
            SetupEventLogging();
            SetupFileBasedLogging();

            if(_eventLog != null)
            {
                Log.Information("Event logging is functional");
            }
            else
            {
                Log.Error("Event logging is non-functional");
            }
        }

        public async Task LogError(string error, Exception ex = null)
        {
            await Task.Factory.StartNew(() =>
            {
                var msg = $@"{error}. {ex?.StackTrace}";
                _eventLog?.WriteEntry(msg, EventLogEntryType.Error);
                Log.Error(msg);
            });
        }

        public async Task LogInfo(string info)
        {
            await Task.Factory.StartNew(() =>
            {
                _eventLog?.WriteEntry(info, EventLogEntryType.Information);
                Log.Information(info);
            });
        }

        private void SetupEventLogging()
        {
            try
            {
                if (!EventLog.SourceExists(Constants.EventLogSourceName))
                    EventLog.CreateEventSource(Constants.EventLogSourceName, Constants.EventLogName);

                _eventLog = new EventLog()
                {
                    Source = Constants.EventLogSourceName,
                    Log = Constants.EventLogName
                };
            }
            catch (Exception ex)
            {
                // This user must have permissions to access and create EventLog -  so if this exception is thrown do not use event log
                // Instead let all logs flow into alternative logger 
                _eventLog = null;
            }
        }

        private void SetupFileBasedLogging()
        {
            string logFilename = _configuration.ExtractsLogFilename;

            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            //.WriteTo.Console()
                            .WriteTo.File($"logs/{logFilename}.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();
        }
    }
}
