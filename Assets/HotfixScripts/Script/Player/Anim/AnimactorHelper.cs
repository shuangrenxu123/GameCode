using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimactorHelper
{
    public AnimancerComponent Animancer { get; private set; }
    public List<AnimancerLayer> Layers { get; private set; }
    public AnimactorHelper(AnimancerComponent animancer)
    {
        Animancer = animancer;
        Layers = new List<AnimancerLayer>
        {
            //Animancer.Layers[0]
        };
    }

    public AnimancerState Play(ClipTransition clip, int layer = 0)
    {
        return Animancer.Play(clip);
    }
    public AnimancerState Play(ITransition transition,int layer = 0)
    {
        return Animancer.Play(transition);
    }
    public AnimancerState Play(AnimancerState state,int layer = 0)
    {
        return Animancer.Play(state);
    }
}
