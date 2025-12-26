using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Board
{
    public sealed class SquareGroup
    {

        public Guid Id;
      
        public GridIndex TopLeftIndex;
        
        
        public Vector2 CenterPoint { get; }
        public Square TopLeft;
        public Square TopRight;
        public Square BottomLeft;
        public Square BottomRight;
        public bool AnyAreNull => TopLeft == null || TopRight == null || BottomLeft == null || BottomRight == null;



        public SquareGroup(Square topLeft, Square topRight, Square bottomLeft, Square bottomRight, GridIndex topLeftIndex)
        {
    
            TopLeftIndex = topLeftIndex;

            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            CenterPoint = (TopLeft.transform.position + BottomRight.transform.position + TopRight.transform.position + BottomLeft.transform.position) / 4f;

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

        
        public async UniTask RotateAsync( RotationDirection direction, CancellationTokenSource cancellationTokenSource = null)
        {
            
            var totalDegrees = 90f * (int)direction;
    
            var rotatedDegrees = 0f;
    
            var renderers = new[]
            {
                TopLeft.GetComponent<SpriteRenderer>(),
                TopRight.GetComponent<SpriteRenderer>(),
                BottomLeft.GetComponent<SpriteRenderer>(),
                BottomRight.GetComponent<SpriteRenderer>()
            };
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder += 10; 
                foreach (Transform t in renderer.transform)
                {
                    if (t.TryGetComponent<SpriteRenderer>(out var childRenderer))
                    {
                        childRenderer.sortingOrder += 10;
                    }
                }
            }
            while (Mathf.Abs(rotatedDegrees) < Mathf.Abs(totalDegrees) || cancellationTokenSource?.IsCancellationRequested == true)
            {
                var rotationThisFrame = 200 * Time.deltaTime * (int)direction;

                if (Mathf.Abs(rotatedDegrees + rotationThisFrame) > Mathf.Abs(totalDegrees))
                {
                    rotationThisFrame = totalDegrees - rotatedDegrees;
                }

       
                TopLeft.transform.RotateAround(CenterPoint, Vector3.forward, rotationThisFrame);
                TopRight.transform.RotateAround(CenterPoint, Vector3.forward, rotationThisFrame);
                BottomLeft.transform.RotateAround(CenterPoint, Vector3.forward, rotationThisFrame);
                BottomRight.transform.RotateAround(CenterPoint, Vector3.forward, rotationThisFrame);

                rotatedDegrees += rotationThisFrame;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationTokenSource?.Token ?? CancellationToken.None);
            }
        
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder -= 10; 
                foreach (Transform t in renderer.transform)
                {
                    if (t.TryGetComponent<SpriteRenderer>(out var childRenderer))
                    {
                        childRenderer.sortingOrder -= 10;
                    }
                }
            }
            
  
        }


        public async Task RotateCounterClockwise(CancellationTokenSource cancellationTokenSource = null)
        {
            await RotateAsync(RotationDirection.CounterClockwise, cancellationTokenSource);
        }
    }
}