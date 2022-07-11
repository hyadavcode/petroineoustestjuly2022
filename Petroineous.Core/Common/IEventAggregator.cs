using Petroineous.Core.Dto;
using System;

namespace Petroineous.Core.Common
{
    public interface IEventAggregator
    {
        void Publish<T>(T dto);
        IObservable<T> Observe<T>();
        IObservable<ScheduledEvent> ObserveInterval(TimeSpan period, string eventName);
    }
}
