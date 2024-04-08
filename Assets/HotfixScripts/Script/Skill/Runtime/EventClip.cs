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
    public virtual void UpdateEvent(float time)
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
    public AnimEventClip(string clipName, Transform transform, Animator anim) : base(transform)
    {
        animator = anim;
        clipHash = Animator.StringToHash("Base Layer" + "." + clipName);
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
        animator.Play("idle");
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
    public AudioEventClip(Transform transform, AudioSource s, string clipName) : base(transform)
    {
        //this.clip = Resources.Load<AudioClip>(clipName);
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
    public FxEventClip(Transform transform, string name) : base(transform)
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
    private SkillTrigger trigger;
    public TriggerEventClip(Transform trnas, SkillTrigger trigger) : base(trnas)
    {
        transform = trnas;
        this.trigger = trigger;
    }
    public override void OnStart()
    {
        trigger.OnStart();
    }
    public override void OnFinish()
    {
        base.OnFinish();
        trigger.OnFinish();
    }
    public override void UpdateEvent(float time)
    {
        trigger.OnUpdate(time);
    }
}
