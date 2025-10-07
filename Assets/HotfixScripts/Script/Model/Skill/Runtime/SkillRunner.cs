using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using CharacterController;
using SkillRuntimeClip;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
namespace Fight
{
    public class SkillRunner : MonoBehaviour
    {
        [SerializeField]
        SerializedDictionary<string, DamageCollider> damageColliders;
        [SerializeField]
        public CharacterActor actor;
        public bool isFinish { get; private set; } = false;
        public AnimancerComponent anim;

        AudioSource audioSource;
        List<TrackRunner> trackRunners = new();

        [SerializeField]
        private CCAnimatorConfig animatorConfig;

        public UnityEvent onFinish;

        public void OnUpdate()
        {
            int trackCount = trackRunners.Count;
            if (isFinish)
            {
                return;
            }
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
        // public void OnClipUpdate(EventClipType clipType, object arg)
        // {
        //     if (clipType == EventClipType.Rotation)
        //     {
        //         float speed = (float)arg;
        //         var target = dataBase.GetData<Transform>("target");
        //         Vector3 direction = target.position - actor.Position;
        //         var targetLookingDirection = Vector3.ProjectOnPlane(direction, actor.Up).normalized;
        //         Quaternion targetDeltaRotation = Quaternion.FromToRotation(actor.Forward, targetLookingDirection);
        //         Quaternion currentDeltaDotation = Quaternion.Slerp(Quaternion.identity, targetDeltaRotation, speed * Time.deltaTime);
        //         actor.SetYaw(currentDeltaDotation * actor.Forward);
        //     }
        // }

        public void LoadTimeLineAsset(TimelineAsset asset)
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
                        clip = new AnimEventClip
                            (this, animatorConfig.weaponAnimators[WeaponType.Gloves].attackAnimations[e.displayName], anim);
                    }
                    else if (track is AudioTrack)
                    {
                        clip = new AudioEventClip(this, audioSource, e.displayName);
                    }
                    else if (track is SkillTrack)
                    {
                        clip = new SkillGenerateClip(this, e.displayName);
                    }
                    else if (track is ColliderTrack)
                    {
                        //clip = new ColliderEventClip(OnClipUpdate, damageColliders[e.displayName]);
                    }
                    else if (track is RootMotionTrack)
                    {
                        var c = e.asset as RootMotionClip;
                        clip = new SkillRuntimeClip.RootMotionClip
                            (this, c.usePositionRootMotion, c.useRotationRootMotion,
                            c.positionMultiplier, c.rotationMultiplier);
                    }
                    clip.StartTime = (float)e.start;
                    clip.EndTime = (float)e.end;
                    trackRunner.AddEvent(clip);
                }
                trackRunners.Add(trackRunner);
                isFinish = false;
            }
        }
        private void OnFinish()
        {
            trackRunners.Clear();
            onFinish.Invoke();
        }

        public void OnReset()
        {
            trackRunners.Clear();
            isFinish = false;
        }
    }
}