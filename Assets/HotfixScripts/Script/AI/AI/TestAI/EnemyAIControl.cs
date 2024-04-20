using Animancer;
using SKUnityToolkit.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIControl : MonoBehaviour
{
    EnemyAI Ai;
    [SerializeField]
    CharacterActor actor;
    public DataBase dataBase { get; private set; }
    public CCAnimatorConfig config;
    public SkillRunner skillRunner;
    public AnimancerComponent anim;

    // Start is called before the first frame update
    void Start()
    {
        Ai = new EnemyAI(this);
        dataBase = new();
        Ai.actor = actor;
        Ai.Init(null, dataBase);
    }
    private void Update()
    {
        Ai.Update();
    }
}
