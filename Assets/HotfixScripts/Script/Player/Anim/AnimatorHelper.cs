using System.Collections.Generic;
using Animancer;

public class AnimatorHelper
{
    public AnimancerComponent Animancer { get; private set; }
    public List<AnimancerLayer> Layers { get; private set; }
    public AnimatorHelper(AnimancerComponent animancer)
    {
        Animancer = animancer;
        Layers = new List<AnimancerLayer>();
    }

    public AnimancerState Play(ClipTransition clip, int layer = 0)
    {
        return Animancer.Play(clip);
    }
    public AnimancerState Play(ITransition transition, int layer = 0)
    {
        return Animancer.Play(transition);
    }
    public AnimancerState Play(AnimancerState state, int layer = 0)
    {
        return Animancer.Play(state);
    }
}
