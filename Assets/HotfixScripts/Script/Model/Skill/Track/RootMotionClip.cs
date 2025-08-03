using UnityEngine;
using UnityEngine.Playables;

public class RootMotionClip : PlayableAsset
{
    private RootMotionData template = new();
    public bool usePositionRootMotion;
    public bool useRotationRootMotion;
    public int rotationMultiplier = 100;
    public int positionMultiplier = 100;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RootMotionData>.Create(graph, template);
        var clone = playable.GetBehaviour();
        clone.usePositionRootMotion = usePositionRootMotion;
        clone.useRotationRootMotion = useRotationRootMotion;
        clone.rotationMultiplier = rotationMultiplier;
        clone.positionMultiplier = positionMultiplier;
        return playable;
    }

}
