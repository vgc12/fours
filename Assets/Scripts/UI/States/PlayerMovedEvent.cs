using System;
using Board;
using EventBus;

namespace UI.States
{
    public class PlayerMovedEvent : IEvent
    {
        public string CurrentGrid { get; }
        public int MovesRemaining { get; }

        public PlayerMovedEvent(string currentGrid, int movesRemaining)
        {
            CurrentGrid = currentGrid;
            MovesRemaining = movesRemaining;
        }
    }
}