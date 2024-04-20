using Animancer;
using System;
using UnityEngine;

namespace SkillRuntimeClip
{
    public abstract class EventClip
    {
        public bool isStart = true;
        public bool isFinished = false;
        public float StartTime;
        public float EndTime;
        protected Action<EventClipType, object> onUpdateAction;
        protected abstract EventClipType clipType { get;}
        public EventClip(Action<EventClipType, object> action)
        {
            onUpdateAction = action;
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
        public abstract void OnUpdate();
        public virtual void OnReset()
        {

        }
    }

    class AnimEventClip : EventClip
    {
        private AnimancerComponent animator;
        private ClipTransition clip;
        public AnimEventClip(Action<EventClipType, object> action, ClipTransition clip, AnimancerComponent anim):base(action)
        {
            animator = anim;
            this.clip = clip;
        }

        protected override EventClipType clipType => EventClipType.Animator;

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

        public override void OnUpdate()
        {
            
        }
    }
    class AudioEventClip : EventClip
    {
        AudioSource source;
        AudioClip clip;

        protected override EventClipType clipType => EventClipType.Audio;

        public AudioEventClip(Action<EventClipType, object> action,AudioSource s, string clipName):base(action)
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

        public override void OnUpdate()
        {
           
        }
    }
    class FxEventClip : EventClip
    {
        /// <summary>
        /// 特效的名字
        /// </summary>
        private string name;
        public FxEventClip(Action<EventClipType, object> action,string name):base(action) 
        {
            this.name = name;

        }

        protected override EventClipType clipType => throw new NotImplementedException();

        public override void OnStart()
        {
            //go.gameObject.SetActive(true);
            base.OnStart();
        }

        public override void OnUpdate()
        {
           
        }
    }
    class ColliderEventClip : EventClip
    {
        private DamageCollider collider;

        protected override EventClipType clipType => EventClipType.Collider;

        public ColliderEventClip(Action<EventClipType, object>action, DamageCollider collider) : base(action)
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

        public override void OnUpdate()
        {
            onUpdateAction(clipType,null);
        }
    }
    class RotationEventClip : EventClip {
        private float speed;
        private CharacterActor actor;
        protected override EventClipType clipType => EventClipType.Rotation;
        public RotationEventClip(Action<EventClipType,object> action,CharacterActor actor, float speed) :base(action) 
        {
            this.speed = speed;
            this.actor = actor; 
        }
        public override void OnStart()
        {
            actor.SetUpRootMotion(true,false);
        }
        public override void OnUpdate()
        {
            onUpdateAction?.Invoke(clipType,speed);
        }
    }

    public enum EventClipType
    {
        Audio,
        Rotation,
        Collider,
        Animator,
    }
}