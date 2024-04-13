using Animancer;
using UnityEngine;

namespace SkillRuntimeClip
{


    public class EventClip
    {
        public bool isStart = true;
        public bool isFinished = false;
        public float StartTime;
        public float EndTime;

        public EventClip()
        {
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
        private AnimancerComponent animator;
        private ClipTransition clip;
        public AnimEventClip(ClipTransition clip, AnimancerComponent anim)
        {
            animator = anim;
            this.clip = clip;
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
        }
        public override void OnStart()
        {
            animator.Play(clip);
            base.OnStart();
        }
    }
    class AudioEventClip : EventClip
    {
        AudioSource source;
        AudioClip clip;
        public AudioEventClip(AudioSource s, string clipName)
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
        public FxEventClip(string name)
        {
            this.name = name;

        }
        public override void OnStart()
        {
            //go.gameObject.SetActive(true);
            base.OnStart();
        }
    }
    /// <summary>
    /// ————————————————————————BUG——————————————————————————
    /// </summary>
    class TriggerEventClip : EventClip
    {
        private SkillTrigger trigger;
        public TriggerEventClip(SkillTrigger trigger)
        {
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
            trigger.OnEventUpdate(time);
        }
    }
    class ColliderEventClip : EventClip
    {
        private DamageCollider collider;
        public ColliderEventClip(DamageCollider collider)
        {
            this.collider = collider;
        }
        public override void OnStart()
        {
            collider.EnableDamageCollider();
        }
        public override void OnFinish()
        {
            collider.DisableDamageCollider();
        }
    }
}