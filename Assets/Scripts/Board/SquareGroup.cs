using System;
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

        public void Scale(Vector3 newScale)
        {
            TopLeft.transform.localScale = newScale;
            TopRight.transform.localScale = newScale;
            BottomLeft.transform.localScale = newScale;
            BottomRight.transform.localScale = newScale;
        }


        public async UniTask RotateAsync(
            RotationDirection direction, CancellationTokenSource cancellationTokenSource = null
        )
        {
            AddSortingOrder(10);

            SetGroupParents(AttachedDot.transform);

            var totalDegrees = 90f * (int)direction;

            var originalScale = AttachedDot.transform.localScale;
            var scale = new Vector3(1.2f, 1.2f, 1.0f);
            var scaleSpeed = .23f;
            var scaleEase = Ease.InOutCubic;
            var rotationSpeed = .55f;
            await Sequence
                 .Create()
                 .Chain(Tween.Scale(AttachedDot.transform, new TweenSettings<Vector3>(scale, duration: scaleSpeed, ease: scaleEase)))
                 .Chain(Tween.LocalRotation(AttachedDot.transform,
                      AttachedDot.transform.localEulerAngles + new Vector3(0, 0, totalDegrees), duration: rotationSpeed))
                 .Chain(Tween.Scale(AttachedDot.transform, new TweenSettings<Vector3>(originalScale, duration: scaleSpeed, ease:scaleEase)));


            SetGroupParents(null);

            AddSortingOrder(-10);
        }

        public void AddSortingOrder(int order)
        {
            var renderers = new[]
            {
                TopLeft.spriteRenderer, TopRight.spriteRenderer, BottomLeft.spriteRenderer, BottomRight.spriteRenderer,
                AttachedDot.GetComponent<SpriteRenderer>()
            };
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = order;
                foreach (Transform t in renderer.transform)
                {
                    if (t.TryGetComponent<SpriteRenderer>(out var childRenderer))
                    {
                        childRenderer.sortingOrder = order;
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

        public void AddRgbOffset(Color colorOffset)
        {
            TopLeft.AddRgbOffset(colorOffset);
            TopRight.AddRgbOffset(colorOffset);
            BottomLeft.AddRgbOffset(colorOffset);
            BottomRight.AddRgbOffset(colorOffset);
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