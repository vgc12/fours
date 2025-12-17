using UnityEngine;

namespace EventBus
{

    public struct SwordThrownEvent : IEvent
    {
        public float AnimationTime;
        public Vector2 ThrowForce;
        public bool AllowRaiseCamera;
        public SwordThrownEvent(bool allowRaiseCamera, float animationTime, Vector2 throwForce)
        {
            AllowRaiseCamera = allowRaiseCamera;
            AnimationTime = animationTime;
            ThrowForce = throwForce;
        }
       
    }
    
    public struct SwordInAirEvent : IEvent
    {

        
    }
}