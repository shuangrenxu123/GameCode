using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TrackRunner 
{
    public bool hasFinished = false;
    private float currentTime = 0;
    private int currentClip = 0;
    public List<EventClip> events = new List<EventClip>();
    public void AddEvent(EventClip clip)
    {
        clip.Init();
        events.Add(clip);
    }
    public void UpdateEvent()
    {
        int count = events.Count;
        if(count == 0)
        {
            hasFinished = true;
            return;
        }
        currentTime += Time.deltaTime;
        int i = currentClip;
        if(currentTime < events[i].StartTime)
        {
            return;
        }
        if (currentTime >= events[i].StartTime && currentTime <= events[i].EndTime)
        {
            if (events[i].isStart)
            {
                events[i].OnStart();
            }
            if (!events[i].isFinished)
            {
                events[i].OnUpdate(currentTime);
            }
        }
        else if (currentTime >= events[i].EndTime)
        {
            events[i].OnFinish();

            if (i == count - 1)
            {
                hasFinished = true;
            }
            else
            {
                currentClip += 1;
            }
        }
    }
    public void Reset()
    {
        currentClip = 0;
        currentTime = 0;
        foreach (var e in events)
        {
            e.OnReset();
        }
    }
}
