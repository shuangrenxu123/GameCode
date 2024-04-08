using Animancer;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="animations", menuName = "Animator")]
public class CCAnimatorConfig : ScriptableObject
{
    [SerializedDictionary("ClipName","Clip")]
    public SerializedDictionary<string,LinearMixerTransition> linearMixerAnimators;
    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string,ClipTransition> clipAnimators;

    [SerializeReference]
    public ITransition LockMovement;
}
