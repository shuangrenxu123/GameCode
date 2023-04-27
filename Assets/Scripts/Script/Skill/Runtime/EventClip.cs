using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventClip
{
    public bool isStart = true;
    public bool isFinished = false;
    public float StartTime;
    public float EndTime;
    public Transform transform;

    public EventClip(Transform transform)
    {
        this.transform = transform;
    }
    public virtual void Init()
    {
        isFinished = false;
    }
    public virtual void OnStart()
    {
        isStart = false;
    }
    public virtual void OnFinish()
    {

    }
    /// <summary>
    /// 有的自定义片段可能会需要我们计算相对值
    /// </summary>
    /// <param name="time"></param>
    public void OnUpdate(float time)
    {
        UpdateEvent(time);
    }
    protected virtual void UpdateEvent(float time)
    {

    }
    public virtual void OnReset()
    {

    }
}

class AnimEventClip : EventClip
{
    private Animator animator;
    private int clipHash;
    public AnimEventClip(string clipName,Transform transform,Animator anim): base(transform) 
    {
        animator = anim;
        clipHash = Animator.StringToHash("Base Layer"+"."+clipName);
    }
    public override void Init()
    {
        base.Init();
        if (animator == null)
        {
            return;
        }
    }
    public override void OnFinish()
    {
        animator.Play("Idle");
    }
    public override void OnStart()
    {
        animator.Play(clipHash);
        base.OnStart();
    }
}
class AudioEventClip : EventClip
{
    AudioSource source;
    AudioClip clip;
    public AudioEventClip(Transform transform,AudioSource s,string clipName):base(transform)
    {
        this.clip = Resources.Load<AudioClip>(clipName);
        this.source = s;
    }
    public override void OnStart()
    {
        source.clip = clip;
        source.Play();
        base.OnStart();
    }
    public override void OnFinish()
    {
        source.Stop();
        Debug.Log("停止播放音乐");
    }
}
class FxEventClip : EventClip
{
    /// <summary>
    /// 特效的名字
    /// </summary>
    private string name;
    public FxEventClip(Transform transform,string name):base(transform)
    {
        this.name = name;
        this.transform = transform;
    }
    public override void OnStart()
    {
        var go = transform.Find(name);
        go.gameObject.SetActive(true);
        base.OnStart();
    }
}
/// <summary>
/// ————————————————————————BUG——————————————————————————
/// </summary>
class TriggerEventClip : EventClip
{
    public TriggerEventClip(Transform trnas):base(trnas)
    {
        transform = trnas;
    }
    public override void OnStart()
    {
        var result = Physics.OverlapSphere(transform.position, 10);
        if (result.Length == 0)
        {
            Debug.Log("没有敌人");
        }
    }
}
