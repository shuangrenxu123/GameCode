using Animancer;
using AYellowpaper.SerializedCollections;
using SkillRuntimeClip;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
namespace Fight
{
    /// <summary>
    /// 该类为具体的技能执行者，可以理解为Timeline的Runtime版本
    /// 目前只针对角色而言使用该类，如果后续有其他的复杂技能可以参考该类实现
    /// 理论上来说，应该玩家所有的攻击动作都由该类实现。
    /// </summary>
    /// todo 修改属性字段，将角色与敌人的分开
    public class SkillRunner : MonoBehaviour
    {
        SkillSystem SkillSystem=>enemyAIControl.skillSystem;
        public SerializedDictionary<string, DamageCollider> damageColliders;
        public CharacterActor actor;
        public bool isFinish = false;
        public AnimancerComponent anim;
        private AudioSource AudioSource;
        private List<TrackRunner> trackRunners = new();
        [SerializeField]
        private CCAnimatorConfig animatorConfig;
        [SerializeField]
        private EnemyAIControl enemyAIControl;
        private DataBase dataBase => enemyAIControl.dataBase;
        public void OnUpdate()
        {
            int trackCount = trackRunners.Count;
            if (trackCount == 0)
            {
                isFinish = true;
            }
            else
            {
                for (int i = trackCount - 1; i >= 0; i--)
                {
                    trackRunners[i].UpdateEvent();
                    if (trackRunners[i].hasFinished)
                    {
                        trackRunners.RemoveAt(i);
                    }
                }
            }
            if (isFinish)
            {
                OnFinish();
            }
        }
        public void OnClipUpdate(EventClipType clipType, object arg)
        {
            if (clipType == EventClipType.Rotation)
            {
                float speed = (float)arg;
                var target = dataBase.GetData<Transform>("target");
                Vector3 direction = target.position - actor.Position;
                var targetLookingDirection = Vector3.ProjectOnPlane(direction, actor.Up).normalized;
                Quaternion targetDeltaRotation = Quaternion.FromToRotation(actor.Forward, targetLookingDirection);
                Quaternion currentDeltaDotation = Quaternion.Slerp(Quaternion.identity, targetDeltaRotation, speed * Time.deltaTime);
                actor.SetYaw(currentDeltaDotation * actor.Forward);
            }
        }
        public void LoadConfig(TimelineAsset asset)
        {
            TimelineAsset playable = asset;
            var tracks = playable.GetOutputTracks();
            foreach (var track in tracks)
            {
                var trackRunner = new TrackRunner();
                var events = track.GetClips();  
                foreach (var e in events)
                {
                    EventClip clip = null;
                    if (track is AnimationTrack)
                    {
                        clip = new AnimEventClip(OnClipUpdate, animatorConfig.clipAnimators[e.displayName], anim);
                    }
                    else if (track is AudioTrack)
                    {
                        clip = new AudioEventClip(OnClipUpdate, AudioSource, e.displayName);
                    }
                    else if (track is SkillTrack)
                    {
                        clip = new SkillGenerateClip(OnClipUpdate, e.displayName, SkillSystem);
                    }
                    else if (track is ColliderTrack)
                    {
                        clip = new ColliderEventClip(OnClipUpdate, damageColliders[e.displayName]);
                    }
                    else if (track is RotationTrack)
                    {
                        var c = e.asset as RotationClip;
                        clip = new RotationEventClip(OnClipUpdate, actor, c.RotationSpeed);
                    }
                    clip.StartTime = (float)e.start;
                    clip.EndTime = (float)e.end;
                    trackRunner.AddEvent(clip);
                }
                trackRunners.Add(trackRunner);
            }
        }
        private void OnFinish()
        {
            trackRunners.Clear();
        }

        public void OnReset()
        {
            trackRunners.Clear();
            isFinish = false;
        }
    }
}