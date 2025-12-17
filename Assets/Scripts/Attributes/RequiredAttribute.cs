using UnityEngine;

namespace Attributes
{
    public sealed class RequiredAttribute : PropertyAttribute
    {
        public RequiredAttribute()
        {
            ErrorMessage = "This field must be assigned!";
        }

        public string ErrorMessage { get; }
    }
}