using EventBus;

namespace Board
{
    public struct GroupRotatedEvent : IEvent
    {
        public readonly Dot SquareGroup;
        public readonly string GridSnapshot;
        public GroupRotatedEvent(Dot squareGroup, string gridSnapshot)
        {
            SquareGroup = squareGroup;
            GridSnapshot = gridSnapshot;
        }
    }
}