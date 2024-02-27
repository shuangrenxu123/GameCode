using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacrerUIActions 
{
    public BoolAction open;
    public void Reset()
    {
        open.Reset();
    }
    public void InitalizeAcionts()
    {
        open =new BoolAction();
        open.Initialize();
    }
    public void SetValues(CharacrerUIActions UiActions)
    {
        open.value = UiActions.open.value;
    }
    public void SetValues(InputHandler input)
    {
        if(input == null)
        {
            Debug.Log("inputAsset is null");
            return;
        }
        open.value = input.GetBool("Open");
    }
    public void Update(float dt)
    {
        open.Update(dt);
    }
}
