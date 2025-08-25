using System;

namespace EventChannel
{
    public interface IEventBinding<T>
    {
        public Action<T> OnEventRaised { get; set; }
        public Action OnEventNoArgs { get; set; }
    }
}