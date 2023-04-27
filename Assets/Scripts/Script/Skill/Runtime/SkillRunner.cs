using PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class SkillRunner : MonoBehaviour
{
    private GameObject SkillOwner;
    private List<TrackRunner> trackRunners;
    private bool isFinish = false;
    private Animator anim;
    private AudioSource AudioSource;
    private void Awake()
    {
        trackRunners = new List<TrackRunner>();
        anim = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        LoadConfig();
    }
    void Update()
    {
        int trackCount = trackRunners.Count;
        if(trackCount == 0)
        {
            return;
        }
        for(int i = trackCount - 1;i >= 0;i--)
        {
            trackRunners[i].UpdateEvent();
            if (trackRunners[i].hasFinished)
            {
                trackRunners.RemoveAt(i);
            }
        }
    }
    void LoadConfig()
    {
        TimelineAsset playable = Resources.Load<TimelineAsset>("skill");
        var tracks = playable.GetOutputTracks();
        foreach (var track in tracks)
        {
            var trackRunner = new TrackRunner();
            var events = track.GetClips();
            foreach (var e in events)
            {
                EventClip clip;
                if(track is AnimationTrack)
                {
                    clip = new AnimEventClip(e.displayName,transform,anim);
                }
                else if(track is AudioTrack)
                {
                    clip = new AudioEventClip(transform,AudioSource,e.displayName);
                }
                else if(track is ControlTrack)
                {
                    clip = new FxEventClip(transform,e.displayName);
                }
                else if(track is TriggerTrack)
                {
                    clip = new TriggerEventClip(transform);
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
}
