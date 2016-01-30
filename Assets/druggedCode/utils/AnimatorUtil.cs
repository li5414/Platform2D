using UnityEngine;
using druggedcode.extensions.anim;

namespace druggedcode
{
    public class AnimatorUtil
    {
        public static void SetBool(Animator animator, string parameterName, bool value)
        {
            if (animator.HasParameterOfType(parameterName, AnimatorControllerParameterType.Bool))
                animator.SetBool(parameterName, value);
        }
        public static void SetFloat(Animator animator, string parameterName, float value)
        {
            if (animator.HasParameterOfType(parameterName, AnimatorControllerParameterType.Float))
                animator.SetFloat(parameterName, value);
        }
        public static void SetInteger(Animator animator, string parameterName, int value)
        {
            if (animator.HasParameterOfType(parameterName, AnimatorControllerParameterType.Int))
                animator.SetInteger(parameterName, value);
        }
    }
}