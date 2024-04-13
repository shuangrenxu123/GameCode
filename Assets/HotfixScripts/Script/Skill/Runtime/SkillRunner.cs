using System.Collections.Generic;
using Animancer;
using SkillRuntimeClip;
using UnityEngine;
using UnityEngine.Timeline;

public class SkillRunner
{
    public bool isSubSkill;
    private GameObject SkillOwner;
    private List<TrackRunner> trackRunners;
    public bool isFinish = false;
    public AnimancerComponent anim;
    private AudioSource AudioSource;
    private Transform transform;
    private CCAnimatorConfig animatorConfig;
    public SkillRunner(AnimancerComponent anim, AudioSource audioSource,CCAnimatorConfig config, Transform transform)
    {
        trackRunners = new List<TrackRunner>();
        AudioSource = audioSource;
        this.anim = anim;
        this.transform = transform;
        animatorConfig = config;
    }
    public void Update()
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
    public void LoadConfig(TimelineAsset asset, SkillTrigger trigger)
    {
        TimelineAsset playable = asset;
        var tracks = playable.GetOutputTracks();
        foreach (var track in tracks)
        {
            var trackRunner = new TrackRunner();
            var events = track.GetClips();
            foreach (var e in events)
            {
                EventClip clip;
                if (track is AnimationTrack)
                {
                    clip = new AnimEventClip(animatorConfig.clipAnimators[e.displayName], anim);
                }
                else if (track is AudioTrack)
                {
                    clip = new AudioEventClip(AudioSource, e.displayName);
                }
                else if (track is ControlTrack)
                {
                    clip = new FxEventClip (e.displayName);
                }
                //else if (track is TriggerTrack)
                //{
                //    clip = new TriggerEventClip(trigger);
                //}
                else
                {
                    clip = new();
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
        if (isSubSkill)
        {
            //删除自己
        }
    }

    public void Reset()
    {
        trackRunners.Clear();
        isFinish = false;
    }
}
