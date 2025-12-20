using EventBus;

namespace Board
{
    public class RotationEvent : IEvent
    {
        public SquareGroup SquareGroup;
        public RotationDirection Direction;
    }
}