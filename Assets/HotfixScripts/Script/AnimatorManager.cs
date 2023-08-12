using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public Animator anim;
    public bool canRotate;
    public virtual void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public virtual void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        anim.applyRootMotion = isInteracting;
        anim.SetBool("isInteracting", isInteracting);
        anim.SetBool("canRotate", false);
        anim.CrossFade(targetAnim, 0.1f);
    }
    public bool GetBool(string name)
    {
        return anim.GetBool(name);
    }
    public float GetFloat(string name)
    {
        return anim.GetFloat(name);
    }

}
