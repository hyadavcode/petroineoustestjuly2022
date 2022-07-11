using System;

namespace Petroineous.Core.Dto
{
    public class ScheduledEvent
    {
        private readonly DateTime _timestamp = new DateTime();
        private readonly string _eventName;

        public ScheduledEvent(string eventName)
        {
            this._eventName = eventName;
        }

        public string EventName => _eventName;
        public DateTime Timestamp => _timestamp;
    }
}
