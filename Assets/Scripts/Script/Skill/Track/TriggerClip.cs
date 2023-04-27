using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerClip : PlayableAsset
{
    private TriggerData template = new TriggerData();
    public float radius = 0;
    public Vector3 point = new Vector3();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TriggerData>.Create(graph,template);
        var clone = playable.GetBehaviour();
        clone.radius = radius;
        clone.point = point;
        return playable;
    }
}
