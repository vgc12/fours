using System;

namespace EventBus
{
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        public Action<T> OnEventRaised { get; set; }
        public Action OnEventNoArgs { get; set; } = () => { };

        public EventBinding(Action<T> onEvent) => OnEventRaised = onEvent;
        
        public EventBinding(Action onEventNoArgs) => OnEventNoArgs = onEventNoArgs;
        
        public void Add(Action onEvent) => OnEventNoArgs += onEvent;
        
        public void Remove(Action onEvent) => OnEventNoArgs -= onEvent;
        
        public void Add(Action<T> onEvent) => OnEventRaised += onEvent;
        
        public void Remove(Action<T> onEvent) => OnEventRaised -= onEvent;
    }
}