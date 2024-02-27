using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUiController : MonoBehaviour
{
    [SerializeField]
    private CharacterBrain CharacterBrain;

    bool showingUI;
    public CharacrerUIActions UIInputActions 
    {
        get
        {
            return CharacterBrain == null ?
                new CharacrerUIActions() : CharacterBrain.CharacterUIActions;
        }
    }
    void Update()
    {
        if (UIInputActions.open.Started)
        {
            if(showingUI == false)
            {
                showingUI = true;
                WindowsManager.Instance.EnableWindow<GameUIMgr>();
            }
            else
            {
                showingUI =false;
                WindowsManager.Instance.DisableWindow<GameUIMgr>();
            }
        }
    }
}
