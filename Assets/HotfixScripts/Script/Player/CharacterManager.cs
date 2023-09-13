using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Combat")]
    public Transform LockOnTransform;
    public BoxCollider backStepCollider;
    public BackStepCollider backStep;
    [Header("Otheer")]
    public AnimatorManager animatorHandle;
    public bool isGrounded;
    public bool climbLabber;
    public CharacterController characterController;

}
