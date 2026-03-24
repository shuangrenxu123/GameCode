using UnityEngine;

public struct CharacterUIActions
{

    public BoolAction confirm;
    public BoolAction cancel;
    public void ClearFrameFlags()
    {
        confirm.ClearFrameFlags();
        cancel.ClearFrameFlags();
    }
    public void Reset()
    {
        confirm.Reset();
        cancel.Reset();
    }
    public void InitializeActions()
    {
        confirm = new BoolAction();
        confirm.Initialize();

        cancel = new BoolAction();
        cancel.Initialize();
    }
    public void SetValues(CharacterUIActions UiActions)
    {
        confirm.value = UiActions.confirm.value;
        cancel.value = UiActions.cancel.value;
    }
    public void SetValues(InputHandler input)
    {
        if (input == null)
        {
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
