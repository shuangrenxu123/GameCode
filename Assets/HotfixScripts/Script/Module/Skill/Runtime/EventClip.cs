using System;
using Animancer;
using Fight;
using UnityEngine;
using static Fight.Number.CombatNumberBox;

namespace SkillRuntimeClip
{
    public abstract class EventClip
    {
        public bool isStart = true;
        public bool isFinished = false;
        public float StartTime;
        public float EndTime;
        protected SkillRunner runner;
        protected abstract EventClipType clipType { get; }
        public EventClip(SkillRunner skillRunner)
        {
            this.runner = skillRunner;
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
        private AnimationClip clip;
        public AnimEventClip(SkillRunner skillRunner, AnimationClip clip, AnimancerComponent anim)
        : base(skillRunner)
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

        public AudioEventClip(SkillRunner skillRunner, AudioSource s, string clipName) : base(skillRunner)
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

    class RootMotionClip : EventClip
    {
        bool usePositionRootMotion;
        bool useRotationRootMotion;
        public int rotationMultiplier = 100;
        public int positionMultiplier = 100;


        protected override EventClipType clipType => EventClipType.RootMotion;
        public RootMotionClip(SkillRunner skillRunner,
            bool usePositionRootMotion,
            bool useRotationRootMotion,
            int positionMultiplier, int rotationMultiplier) : base(skillRunner)
        {
            this.positionMultiplier = positionMultiplier;
            this.rotationMultiplier = rotationMultiplier;
            this.usePositionRootMotion = usePositionRootMotion;
            this.useRotationRootMotion = useRotationRootMotion;
        }
        Guid speedChange, rotationChange;
        public override void OnStart()
        {
            base.OnStart();
            runner.actor.SetUpRootMotion(usePositionRootMotion, useRotationRootMotion);

            var properties = runner.actor.GetComponent<CombatEntity>().properties;
            speedChange = properties.AddModifier(PropertyType.SpeedMultiplier
                  , positionMultiplier
                  , Fight.Number.ModifierType.Percent
                  , Fight.Number.PropertySourceTypes.Buff);
            rotationChange = properties.AddModifier(PropertyType.RotationMultiplier
                   , rotationMultiplier
                   , Fight.Number.ModifierType.Percent
                   , Fight.Number.PropertySourceTypes.Buff);
        }
        public override void OnFinish()
        {
            base.OnFinish();
            runner.actor.SetUpRootMotion(true, true);
            var properties = runner.actor.GetComponent<CombatEntity>().properties;
            properties.RemoveModifier(PropertyType.SpeedMultiplier, speedChange);
            properties.RemoveModifier(PropertyType.RotationMultiplier, rotationChange);

        }
        public override void OnUpdate()
        {

        }
    }
    class SkillGenerateClip : EventClip
    {
        /// <summary>
        /// 特效的名字
        /// </summary>
        private string name;
        private SkillSystem system;
        public SkillGenerateClip(SkillRunner skillRunner, string name) : base(skillRunner)
        {
            this.name = name;
        }

        protected override EventClipType clipType => EventClipType.Skill;

        public override void OnStart()
        {
            system.GenerateSkill(name);
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

        public ColliderEventClip(SkillRunner skillRunner, DamageCollider collider)
            : base(skillRunner)
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
        }
    }

    public enum EventClipType
    {
        Audio,
        RootMotion,
        Collider,
        Animator,
        Skill,
    }
}