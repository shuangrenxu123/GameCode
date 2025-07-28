using UnityEngine;
using UnityEngine.Playables;

public class RootMotionClip : PlayableAsset
{
    private RootMotionData template = new();
    public bool usePositionRootMotion;
    public bool useRotationRootMotion;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RootMotionData>.Create(graph, template);
        var clone = playable.GetBehaviour();
        clone.usePositionRootMotion = usePositionRootMotion;
        clone.useRotationRootMotion = useRotationRootMotion;
        return playable;
    }

}
