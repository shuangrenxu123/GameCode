using UnityEngine;
using UnityEngine.Playables;

public class SkillClip : PlayableAsset
{
    private SkillTrackData template = new SkillTrackData();
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SkillTrackData>.Create(graph, template);
        var clone = playable.GetBehaviour();
        return playable;
    }

}
