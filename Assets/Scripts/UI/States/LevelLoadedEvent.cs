using EventBus;
using Levels;

namespace UI.States
{
    public class LevelLoadedEvent : IEvent
    {
        public LevelData LevelData { get; }

        public LevelLoadedEvent(LevelData levelData)
        {
            LevelData = levelData;
        }
   
    }
}