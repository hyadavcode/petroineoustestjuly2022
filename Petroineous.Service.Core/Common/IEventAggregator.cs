using Petroineous.Service.Core.Dto;
using System;

namespace Petroineous.Service.Core.Common
{
    public interface IEventAggregator
    {
        void Publish<T>(T dto);
        IObservable<T> Observe<T>();
        IObservable<ScheduledEvent> ObserveInterval(TimeSpan period, string eventName);
    }
}
