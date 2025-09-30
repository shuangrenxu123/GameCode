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
        Climb,
        LockOnMove
    }
    public enum ECharacterLoginState
    {
        Empty,
        Interaction,
        Attack,

    }
}
