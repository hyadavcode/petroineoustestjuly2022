using Petroineous.Service.Core.Dto;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Petroineous.Service.Core.Common
{
    public class EventAggregator : IEventAggregator
    {
        private readonly BehaviorSubject<object> _stream = new BehaviorSubject<object>(null);
        private readonly ILogger _logger;

        public EventAggregator(ILogger logger)
        {
            this._logger = logger;
        }

        public void Publish<T>(T dto)
        {
            try
            {
                _stream.OnNext(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error in {nameof(EventAggregator.Publish)}", ex);
            }
        }

        public IObservable<T> Observe<T>()
        {
            return _stream.OfType<T>();
        }

        public IObservable<ScheduledEvent> ObserveInterval(TimeSpan period, string eventName)
        {
            return Observable.Interval(period).Select(x => new ScheduledEvent(eventName));            
        }
    }
}
