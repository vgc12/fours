using System.Runtime.InteropServices;
using Extensions;
using Singletons;
using UnityEngine;
using UnityEngine.Pool;

namespace Board
{
    public sealed class SquareCreationParams
    {
        public GridIndex Id { get; set; }
        public Color Color { get; set; }
        public bool Inactive { get; set; }
        public Transform Parent { get; set; }
    }
    
  
}