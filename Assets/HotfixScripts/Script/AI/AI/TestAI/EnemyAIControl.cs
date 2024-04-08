using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIControl : MonoBehaviour
{
    EnemyAI Ai;
    [SerializeField]
    CharacterActor actor;
    DataBase dataBase;
    // Start is called before the first frame update
    void Start()
    {
        Ai = new EnemyAI();
        dataBase = new();
        Ai.actor = actor;
        Ai.Init(null, dataBase);
    }
    private void Update()
    {
        Ai.Update();
    }
}
