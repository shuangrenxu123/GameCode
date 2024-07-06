using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ColliderClip : PlayableAsset
{
    private ColliderData template = new ColliderData();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ColliderData>.Create(graph, template);
        var clone = playable.GetBehaviour();
        //clone.collider = collider;
        return playable;
    }
}
