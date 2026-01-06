using EventBus;

namespace UI
{
    public struct UIEvent : IEvent
    {
        public enum UIEventType
        {
            InGame,
            InMenu
        }
        
        public UIEventType Type;
        
        public UIEvent(UIEventType type)
        {
            Type = type;
        }
    }
}