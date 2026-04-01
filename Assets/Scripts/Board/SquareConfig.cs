using PrimeTween;
using UnityEngine;

namespace Board
{
    [CreateAssetMenu(fileName = "Square Config", menuName = "Configs/Square Config", order = 0)]
    public sealed class SquareConfig : ScriptableObject
    {
        public Sprite squareSprite;
        public Color spriteColor = Color.HSVToRGB(34 / 360f, 41 / 100f, 66 / 100f);
        public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
        public TweenSettings<Color> selectTween = new (new Color(1, 1, 1, .5f), duration: 0.1f, ease: Ease.InOutCubic);
        public TweenSettings<Color> deselectTween = new (new Color(1, 1, 1, 0f), duration: 0.1f, ease: Ease.InOutCubic);
    }
}