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
        NormalMove = 1,
        CrouchMove = 2,
        Jump = 3,
        Climb = 4,
        LockOnMove = 5,
        CrouchLockOnMove = 6
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
