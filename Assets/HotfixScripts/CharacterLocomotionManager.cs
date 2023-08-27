using UnityEngine;

public class CharacterLocomotionManager : MonoBehaviour
{
    public CharacterManager entity;
    public Vector3 moveDirection;
    public LayerMask groundLayer;
    public float inAirTimer;
    [SerializeField]
    protected Vector3 Yvelcoity;
    [SerializeField]
    protected float groundedYVelcoity = -20;
    [SerializeField]
    protected float fallStartYVelcoity = -7;
    [SerializeField]
    protected float gravityForce = -25;
    [SerializeField]
    protected float groundCheckSphereRadius = 1f;
    [SerializeField]
    protected bool fallingVelocitySet = false;
    private void Update()
    {
        entity.isGrounded =Physics.CheckSphere(entity.transform.position, groundCheckSphereRadius, groundLayer);
        
        entity.animatorHandle.anim.SetBool("isGround", entity.isGrounded);
        if (!entity.climbLabber)
        {
            HandleGroundCheck();
        }
    }
    public virtual void HandleGroundCheck()
    {
        if (entity.isGrounded)
        {
            if (Yvelcoity.y < 0)
            {
                inAirTimer = 0;
                fallingVelocitySet = false;
                Yvelcoity.y = groundedYVelcoity;
            }
        }
        else
        {
            if (!fallingVelocitySet)
            {
                fallingVelocitySet = true;
                Yvelcoity.y = fallStartYVelcoity;
            }
            inAirTimer += Time.deltaTime;
            Yvelcoity.y += gravityForce * Time.deltaTime;
        }
        entity.animatorHandle.anim.SetFloat("inAirTime", inAirTimer);
        entity.characterController.Move(Yvelcoity * Time.deltaTime);
    }
}
