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
        NormalMove = 0,
        CrouchMove = 1,
        Jump = 2,
        Climb = 3,
        LockOnMove = 4,
        CrouchLockOnMove = 5
    }
    public enum ECharacterLoginState
    {
        Empty = 0,
        Interaction = 1,
        Attack = 2,
        UseProp = 3,
        InjIry = 4,

    }
}
