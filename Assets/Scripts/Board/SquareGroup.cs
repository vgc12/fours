using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Board
{
    public class SquareGroup
    {

        public GUID Id;
      
        public GridIndex TopLeftIndex;
        
        
        public Vector2 CenterPoint { get; private set; }
        public Square TopLeft;
        public Square TopRight;
        public Square BottomLeft;
        public Square BottomRight;
        



        public SquareGroup(Square topLeft, Square topRight, Square bottomLeft, Square bottomRight, GridIndex topLeftIndex)
        {
    
            TopLeftIndex = topLeftIndex;

            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            CenterPoint = (TopLeft.transform.position + BottomRight.transform.position + TopRight.transform.position + BottomLeft.transform.position) / 4f;
            
            Id = GUID.Generate();
        }

        public async Task RotateClockwise()
        {
            await RotateAsync(RotationDirection.Clockwise);
            
            var temp = TopLeft;
            
            TopLeft = BottomLeft;
            BottomLeft = BottomRight;
            BottomRight = TopRight;
            TopRight = temp;



        }

        
        public async Task RotateAsync( RotationDirection direction)
        {
            
            var totalDegrees = 90f * (int)direction;
    
            float rotatedDegrees = 0f;
    
      
            while (Mathf.Abs(rotatedDegrees) < Mathf.Abs(totalDegrees))
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

                await Awaitable.NextFrameAsync();
            }
        
        
  
        }


        public async Task RotateCounterClockwise()
        {
            await RotateAsync(RotationDirection.CounterClockwise);
        }
    }
    
    
}