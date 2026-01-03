using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace Board
{
    public sealed class SquareGroup
    {
        public Guid Id;

        public GridIndex TopLeftIndex;


        public Vector2 CenterPoint;
        public Square TopLeft;
        public Square TopRight;
        public Square BottomLeft;
        public Square BottomRight;
        public Dot AttachedDot;
        public bool Selected => Squares.Any(s => s.HighlightRenderer.enabled);
        
        public IReadOnlyList<Square> Squares => new[]
        {
            TopLeft, TopRight, BottomLeft, BottomRight
        };

        public bool AnyAreNull => TopLeft == null || TopRight == null || BottomLeft == null || BottomRight == null;


        public SquareGroup(
            Square topLeft, Square topRight, Square bottomLeft, Square bottomRight, GridIndex topLeftIndex
        )
        {
            TopLeftIndex = topLeftIndex;

            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            CenterPoint = (TopLeft.transform.position + BottomRight.transform.position + TopRight.transform.position +
                           BottomLeft.transform.position) / 4f;

            Id = Guid.NewGuid();
        }

        public async UniTask RotateClockwise(CancellationTokenSource cancellationTokenSource = null)
        {
            await RotateAsync(RotationDirection.Clockwise, cancellationTokenSource);

            var temp = TopLeft;

            TopLeft = BottomLeft;
            BottomLeft = BottomRight;
            BottomRight = TopRight;
            TopRight = temp;
        }
        


        public async UniTask RotateAsync(
            RotationDirection direction, CancellationTokenSource cancellationTokenSource = null
        )
        {
            AddSortingOrder(10);

            var originalParent = TopLeft.transform.parent;
            SetGroupParents(AttachedDot.transform);

            var totalDegrees = 90f * (int)direction;

            var originalScale = AttachedDot.transform.localScale;
            var scale = new Vector3(1.2f, 1.2f, 1.0f);
            const float scaleSpeed = .23f;
            const Ease scaleEase = Ease.InOutCubic;
            const float rotationSpeed = .55f;
            await Sequence
                 .Create()
                 .Chain(Tween.Scale(AttachedDot.transform, new TweenSettings<Vector3>(scale, duration: scaleSpeed, ease: scaleEase)))
                 .Chain(Tween.LocalRotation(AttachedDot.transform,
                      AttachedDot.transform.localEulerAngles + new Vector3(0, 0, totalDegrees), duration: rotationSpeed))
                 .Chain(Tween.Scale(AttachedDot.transform, new TweenSettings<Vector3>(originalScale, duration: scaleSpeed, ease:scaleEase)));


            SetGroupParents(originalParent);

            AddSortingOrder(-10);
        }

        public void AddSortingOrder(int order)
        {
            var renderers = new[]
            {
                TopLeft.SpriteRenderer, TopRight.SpriteRenderer, BottomLeft.SpriteRenderer, BottomRight.SpriteRenderer,
                AttachedDot.GetComponent<SpriteRenderer>()
            };
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder += order;
                foreach (Transform t in renderer.transform)
                {
                    if (t.TryGetComponent<SpriteRenderer>(out var childRenderer))
                    {
                        childRenderer.sortingOrder += order;
                    }
                }
            }
        }

        public void SetGroupParents(Transform parent, bool worldPositionStays = true)
        {
            TopLeft.transform.SetParent(parent, worldPositionStays);
            TopRight.transform.SetParent(parent, worldPositionStays);
            BottomLeft.transform.SetParent(parent, worldPositionStays);
            BottomRight.transform.SetParent(parent, worldPositionStays);
        }

        public void Select()
        {
            TopLeft.Select();
            TopRight.Select();
            BottomLeft.Select();
            BottomRight.Select();
            
        }
        
        public async UniTask Deselect()
        {
            var tasks = new[]
            {
                TopLeft.Deselect(),
                TopRight.Deselect(),
                BottomLeft.Deselect(),
                BottomRight.Deselect()
            };
            await UniTask.WhenAll(tasks);
           
      
        }

        public async Task RotateCounterClockwise(CancellationTokenSource cancellationTokenSource = null)
        {
            await RotateAsync(RotationDirection.CounterClockwise, cancellationTokenSource);
            var temp = TopLeft;
            TopLeft = TopRight;
            TopRight = BottomRight;
            BottomRight = BottomLeft;
            BottomLeft = temp;
        }
    }
}