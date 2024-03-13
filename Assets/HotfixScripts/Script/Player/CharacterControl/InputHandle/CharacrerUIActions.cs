using UnityEngine;

public struct CharacrerUIActions
{

    public BoolAction confirm;
    public BoolAction cancel;
    public void Reset()
    {
        confirm.Reset();
        cancel.Reset();
    }
    public void InitalizeAcionts()
    {
        confirm = new BoolAction();
        confirm.Initialize();

        cancel = new BoolAction();
        cancel.Initialize();
    }
    public void SetValues(CharacrerUIActions UiActions)
    {
        confirm.value = UiActions.confirm.value;
        cancel.value = UiActions.cancel.value;
    }
    public void SetValues(InputHandler input)
    {
        if (input == null)
        {
            Debug.Log("inputAsset is null");
            return;
        }
        confirm.value = input.GetBool("confirm");
        cancel.value = input.GetBool("cancel");
    }
    public void Update(float dt)
    {
        confirm.Update(dt);
        cancel.Update(dt);
    }
}
