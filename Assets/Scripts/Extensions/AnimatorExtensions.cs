using UnityEngine;

namespace Extensions
{
    public static class AnimatorExtensions
    {
        public static float GetAnimationLength(this Animator animator, int animationHash, int layerIndex = 0)
        {
            var clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);

            foreach (var clipInfo in clipInfos)
                if (Animator.StringToHash(clipInfo.clip.name) == animationHash)
                    return clipInfo.clip.length;

            // If not found in current clips, search all clips in the controller
            var controller = animator.runtimeAnimatorController;
            foreach (var clip in controller.animationClips)
                if (Animator.StringToHash(clip.name) == animationHash)
                    return clip.length;

            Debug.LogWarning($"Animation with hash {animationHash} not found");
            return 0f;
        }
    }
}