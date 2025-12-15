using UnityEngine;

namespace Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool Contains<T>(this T obj, int layer) where T : Component
        {
            return Contains(obj.gameObject.layer, layer);
        }

        public static bool Contains(this LayerMask layerMask, int layer)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}