using System.Collections.Generic;

namespace Levels
{
    public interface ILevelManager
    {
        public LevelData CurrentLevel { get; }
        public List<LevelData> Levels { get; }
        
        LevelData NextLevel {get;}
        
        LevelData PreviousLevel {get; }
        
        bool HasNextLevel { get; }
        bool HasPreviousLevel { get; }
        
        void LoadLevel(LevelData level);
    }
}