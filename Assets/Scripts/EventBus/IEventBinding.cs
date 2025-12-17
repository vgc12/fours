using System;

namespace EventBus
{
    public interface IEventBinding<T>
    {
        public Action<T> OnEventRaised { get; set; }
        public Action OnEventNoArgs { get; set; }
    }
}