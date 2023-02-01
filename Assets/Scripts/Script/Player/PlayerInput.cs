using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class PlayerInput : PlayerAction.IGamePlayActions
{
    PlayerAction InputActions;

    public event UnityAction<Vector2> Onmove;
    public event UnityAction<Vector2> Onstop;

    void OnEnable()
    {
        InputActions = new PlayerAction();
        InputActions.GamePlay.SetCallbacks(this);
    }

    public void EnableTable()
    {
        OnEnable();
        InputActions.GamePlay.Enable();
    }
    void DisableAllTable()
    {
        InputActions.GamePlay.Disable();
    }
    void OnDisable()
    {
        DisableAllTable();
    }
    public void OnMovement(InputAction.CallbackContext context)
    {

    }

    public void OnCamera(InputAction.CallbackContext context)
    {

    }
}
