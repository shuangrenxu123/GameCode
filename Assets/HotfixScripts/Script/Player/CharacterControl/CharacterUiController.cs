using Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(100)]
public class CharacterUiController : MonoBehaviour
{
    [SerializeField]
    private CharacterBrain CharacterBrain;

    bool showingUI;
    public CharacterActions InputActions 
    {
        get
        {
            return CharacterBrain == null ?
                new CharacterActions() : CharacterBrain.CharacterActions;
        }
    }
    void Update()
    {
        if (InputActions.OpenUI.Started)
        {
            CharacterBrain.EnableUIIpnut();
            UIManager.Instance.OpenUI<GameUIMgr>(Resources.Load<GameUIMgr>("GameUI"));
            Debug.Log(CharacterBrain.CharacterActions.OpenUI.Started);
            Debug.Log(CharacterBrain.CharacterUIActions.cancel.Started);
           
        }
    }
}
