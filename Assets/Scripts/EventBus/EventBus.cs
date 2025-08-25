using System.Collections.Generic;
using UnityEngine;

namespace EventChannel
{
    public static class EventBus<T> where T : IEvent
    {
        public static readonly List<IEventBinding<T>> Bindings = new();
        

        public static void Register(IEventBinding<T> binding)
        {
            Bindings.Add(binding);
        }

        public static void Deregister(IEventBinding<T> binding)
        {
            Bindings.Remove(binding);
        }


        public static void Raise(T eventToRaise)
        {
            foreach (var binding in Bindings)
            {
                binding.OnEventRaised?.Invoke(eventToRaise);
                binding.OnEventNoArgs?.Invoke();
            }
        }
    }


}