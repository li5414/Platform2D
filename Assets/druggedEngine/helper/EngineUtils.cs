using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using druggedcode.extensions.anim;

public class EngineUtils
{

    //animator 의 파라메터를 변경
    public static void UpdateAnimatorBool(Animator animator, string parameterName,bool value)
    {
        if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Bool))
            animator.SetBool(parameterName,value);
    }

    public static void UpdateAnimatorFloat(Animator animator, string parameterName,float value)
    {
        if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Float))
            animator.SetFloat(parameterName,value);
    }
    public static void UpdateAnimatorInteger(Animator animator, string parameterName,int value)
    {
        if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Int))
            animator.SetInteger(parameterName,value);
    }
}
