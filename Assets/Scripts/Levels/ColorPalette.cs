#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(menuName = "Create Color Palette", fileName = "Color Palette", order = 0)]
    public sealed class ColorPalette : ScriptableObject
    {
        public List<Color> colors = new List<Color>();
    }
}
#endif