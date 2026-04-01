using Attributes;
using Extensions;
using UnityEngine;

namespace Board
{
    [CreateAssetMenu(fileName = "Square Factory", menuName = "Factories/Square Factory", order = 0)]
    public sealed class SquareFactory : ScriptableObject, IFactory<Square, SquareCreationParams>
    {
        [Required, SerializeField] public SquareConfig squareConfig;

        public Square Create(SquareCreationParams parameters)
        {
            // Create main square object
            var squareObject = new GameObject($"Square{parameters.Id}", typeof(Square));
            squareObject.transform.SetParent(parameters.Parent, false);

            var square = squareObject.GetOrAdd<Square>();

            square.SpriteRenderer = square.GetOrAdd<SpriteRenderer>();
            square.SpriteRenderer.sprite = squareConfig.squareSprite;
            square.squareConfig = squareConfig;

         
            if (!parameters.Inactive)
            {
                // Create outline
                
                square.OutlineRenderer = CreateChildRenderer("Outline", squareObject.transform,
                    squareConfig.squareSprite, squareConfig.spriteColor, new Vector3(1.1f, 1.1f, 1f), 1f);

                // Create highlight
                square.HighlightRenderer = CreateChildRenderer("Highlight", squareObject.transform,
                    squareConfig.squareSprite, squareConfig.highlightColor, Vector3.one, 1f);
                square.HighlightRenderer.enabled = false;
            }


            square.Initialize(parameters.Id, parameters.Color, parameters.Inactive);

            return square;
        }

        private static SpriteRenderer CreateChildRenderer(
            string name, Transform parent,
            Sprite sprite, Color color, Vector3 scale, float zOffset
        )
        {
            var obj = new GameObject(name, typeof(SpriteRenderer));
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = scale;
            obj.transform.localPosition = new Vector3(0, 0, zOffset);

            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;

            return renderer;
        }
    }
}