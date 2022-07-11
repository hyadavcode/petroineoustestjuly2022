using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineous.Core.Common
{
    public class Logger : ILogger
    {
        private readonly EventLog _eventLog = new EventLog();

        public Logger()
        {
            Init();
        }

        private void Init()
        {
            if (!EventLog.SourceExists(Constants.EventLogSourceName))
            {
                EventLog.CreateEventSource(Constants.EventLogSourceName, Constants.EventLogName);
            }

            _eventLog.Source = Constants.EventLogSourceName;
            _eventLog.Log = Constants.EventLogName;
        }

        public async Task LogError(string error, Exception ex = null)
        {
            await Task.Factory.StartNew(() =>
            {
                var msg = $@"{error}. {ex?.StackTrace}";
                _eventLog.WriteEntry(msg, EventLogEntryType.Error);
            });
        }

        public async Task LogInfo(string info)
        {
            await Task.Factory.StartNew(() =>
            {
                _eventLog.WriteEntry(info, EventLogEntryType.Information);
            });
        }
    }
}
