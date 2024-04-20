using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class RotationClip : PlayableAsset
{
    private RotationData template = new ();
    public float RotationSpeed;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RotationData>.Create(graph, template);
        //var clone = playable.GetBehaviour();
        //clone.RotationSpeed = RotationSpeed;
        return playable;
    }

}
