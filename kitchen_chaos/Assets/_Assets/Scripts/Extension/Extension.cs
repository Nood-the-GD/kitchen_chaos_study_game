using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.CSharp;
using UnityEngine;

public static class Extension
{
    public static bool IsInRange(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    public static bool IsInRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    public static Vector3 ToVector3XZ(this Vector2 source, float y = 0)
    {
        return new Vector3(source.x, y, source.y);
    }

}

public static class AnimatorExtension
{
    public static float GetCurrentAnimationLength(this Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
    public static float GetAnimationLength(this Animator animator, string animationName)
    {
        try
        {
            return animator.runtimeAnimatorController.animationClips.First(clip => clip.name == animationName).length;
        }
        catch
        {
            Debug.LogError($"Animation {animationName} not found");
            return 0;
        }
    }
}
