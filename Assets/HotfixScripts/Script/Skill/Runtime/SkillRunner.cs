using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class SkillRunner
{
    public bool isSubSkill;
    private GameObject SkillOwner;
    private List<TrackRunner> trackRunners;
    public bool isFinish = false;
    public Animator anim;
    private AudioSource AudioSource;
    private Transform transform;
    public SkillRunner(Animator anim, AudioSource audioSource, Transform transform)
    {
        trackRunners = new List<TrackRunner>();
        AudioSource = audioSource;
        this.anim = anim;
        this.transform = transform;
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
    public void LoadConfig(TimelineAsset playable, SkillTrigger trigger)
    {
        //TimelineAsset playable = Resources.Load<TimelineAsset>(name);
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
                    clip = new AnimEventClip(e.displayName, transform, anim);
                }
                else if (track is AudioTrack)
                {
                    clip = new AudioEventClip(transform, AudioSource, e.displayName);
                }
                else if (track is ControlTrack)
                {
                    clip = new FxEventClip(transform, e.displayName);
                }
                else if (track is TriggerTrack)
                {
                    clip = new TriggerEventClip(transform, trigger);
                }
                else
                {
                    clip = new(transform);
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
        if (isSubSkill)
        {
            //删除自己
        }
    }
}
