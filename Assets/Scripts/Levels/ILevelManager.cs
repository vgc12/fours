using System.Collections.Generic;

namespace Levels
{
    public interface ILevelManager
    {
        public LevelData CurrentLevelData { get; }
        public List<LevelData> Levels { get; }
        void LoadLevel(LevelData level);
    }
}