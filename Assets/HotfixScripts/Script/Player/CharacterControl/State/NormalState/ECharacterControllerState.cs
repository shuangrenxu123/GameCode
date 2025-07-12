using UnityEngine;

namespace Character.Controller.State
{

    public enum ECharacterControllerState
    {
        Move,
        Login,
    }
    public enum ECharacterMoveState
    {

        NormalMove,
        CrouchMove,
        Jump,
        Climb
    }
    public enum ECharacterLoginState
    {
        Empty,
        Interaction,
        Attack,

    }
}
