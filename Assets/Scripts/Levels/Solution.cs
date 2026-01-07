using System.Collections.Generic;
using Board;
using UnityEngine;

namespace Levels
{
    [System.Serializable]
    public struct Solution
    {
        public enum Group
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            TopMiddle,
            BottomMiddle,
            LeftMiddle,
            RightMiddle,
            Center
            // Add more groups as needed
            
        }
        [SerializeField]  public List<SolutionData> steps;

        public override string ToString()
        {
            var result = "Solution Steps:\n";
            foreach (var step in steps)
            {
                result += $"{step.group}: Rotate {step.rotationDirection} {step.times} times\n";
            }
            return result;
        }
        
        [System.Serializable]
        public struct SolutionData
        {
            public Group group;
            public RotationDirection rotationDirection;
            public int times;
        }
    }

 
    
    
}