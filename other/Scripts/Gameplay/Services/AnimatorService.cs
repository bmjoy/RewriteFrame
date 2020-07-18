using System.Collections.Generic;
using UnityEngine;

public class AnimatorService
{
    private readonly List<Animator> m_AnimatorList = new List<Animator>();

    public AnimatorService(Animator[] animators)
    {
        m_AnimatorList.AddRange(animators);
    }

    public void SetFloat(string key, float value)
    {
        foreach (var animator in m_AnimatorList)
        {
            if (animator.isActiveAndEnabled 
                && HasParameter(animator, key, AnimatorControllerParameterType.Float)
                && animator.GetFloat(key) != value)
            {
                animator.SetFloat(key, value);
            }
        }
    }

    private bool HasParameter(Animator animator, string key, AnimatorControllerParameterType type)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == key && parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }
}
