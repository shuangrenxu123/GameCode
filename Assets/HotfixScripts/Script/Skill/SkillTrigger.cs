using System;
using Animancer;
using UnityEngine;
using UnityEngine.Timeline;

public class SkillTrigger : MonoBehaviour
{
    public CCAnimatorConfig animatorConfig;
    public SkillRunner runner;
    public AnimancerComponent anim;
    public AudioSource AudioSource;

    private bool canStart;
    public bool isFinish
    {
        get  
        {
            if (runner != null)
                return runner.isFinish;
            else return false;
        }
    }
    public virtual void Start()
    {
        runner = new SkillRunner(anim, AudioSource, animatorConfig, transform);
        anim.Play(animatorConfig.linearMixerAnimators["move"]);
    }

    public void LoadConfig(TimelineAsset asset)
    {
        runner.LoadConfig(asset,this);
        canStart = true;
    }
    public virtual void OnStart()
    {

    }
    public virtual void OnEventUpdate(float time)
    {
    }

    public void OnUpdate()
    {
        runner.Update();
    }
    public virtual void OnFinish()
    {

    }

    public void OnReset()
    {
        runner.Reset();
    }

}
