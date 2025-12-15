using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EventBus
{
    public static class EventBusUtil{
        public static IReadOnlyList<Type> EventTypes { get; private set; }
        public static IReadOnlyList<Type> EventBusTypes { get; private set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            PlayModeState = obj;
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllBuses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            EventBusTypes = InitializeEventBusTypes();
        }
        
        public static List<Type>InitializeEventBusTypes()
        {
            var typedef = typeof(EventBus<>);
         
            return EventTypes.Select(eventType => typedef.MakeGenericType(eventType)).ToList();
            
        }
        
        public static void ClearAllBuses()
        {
            foreach (var eventBusType in EventBusTypes)
            {
                var clearMethod = eventBusType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
                if (clearMethod != null) clearMethod.Invoke(null, null);
            }
        }
    }
}