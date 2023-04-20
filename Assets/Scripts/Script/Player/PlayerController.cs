using PlayerInfo;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    CameraHandler cameraHandler;
    Transform cameraObject;
    PlayerInputHandle inputHandle;
    Player PlayerManager;
    public Transform myTransform;
    public AnimatorHandle animatorHandle;
    public NetTranform netController;

    public Vector3 moveDirection;
    public Rigidbody rb;
    public GameObject normalCamera;
    [SerializeField]
    float groundDetectionRayStartPoint = 0.5f;
    [SerializeField]
    float minimumDistanceNeededToBeginFall = 1f;
    [SerializeField]
    float groundDirectionRayDistance = 0.2f;
    LayerMask ignoreForGroundCheck;
    public float inAirTimer;

    [Header("state")]
    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float rotationSpeed = 10;
    [SerializeField]
    float sprintSpeed = 8;
    [SerializeField]
    float fallingSpeed =30;

    void Start()
    {
        animatorHandle = GetComponentInChildren<AnimatorHandle>();
        cameraHandler = CameraHandler.singleton;
        rb = GetComponent<Rigidbody>();
        PlayerManager = GetComponent<Player>();
        inputHandle = GetComponent<PlayerInputHandle>();
        cameraObject = Camera.main.transform; 
        myTransform = transform;
        animatorHandle.Initialize(); 

        PlayerManager.isGrounded= true;
        ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
    }
    #region 移动代码
    Vector3 noramalVector;
    Vector3 targetPosition;
    
    private void HandleRotation(float delta)
    {
        if (inputHandle.LockFlag)
        {
            if (inputHandle.sprintFlag || inputHandle.rollFlag)
            {
                Vector3 targetDirection = Vector3.zero;
                targetDirection = cameraHandler.cameraTransform.forward * inputHandle.vertical;
                targetDirection += cameraHandler.cameraTransform.right * inputHandle.horizontal;
                targetDirection.Normalize();
                targetDirection.y = 0;
                if (targetDirection == Vector3.zero)
                {
                    targetDirection = transform.forward;
                }
                Quaternion tr = Quaternion.LookRotation(targetDirection);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);
                transform.rotation = targetRotation;
            }
            else
            {
                Vector3 rotationDirection = moveDirection;
                rotationDirection = cameraHandler.currentLockOnTarget.position - transform.position;
                rotationDirection.y = 0;
                rotationDirection.Normalize();
                Quaternion tr = Quaternion.LookRotation(rotationDirection);
                Quaternion targetRotation =Quaternion.Slerp(transform.rotation,tr,rotationSpeed * delta);
                transform.rotation = targetRotation;    
            }
        }
        else
        {
            if (PlayerManager.isInteracting || PlayerManager.isInAir)
            {
                return;
            }
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandle.moveAmount;
            targetDir = cameraObject.forward * inputHandle.vertical;
            targetDir += cameraObject.right * inputHandle.horizontal;
            targetDir.Normalize();
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
            {
                targetDir = myTransform.forward;
            }
            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targerRotition = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
            myTransform.rotation = targerRotition;
        }
    }
    /// <summary>
    /// 具体的移动
    /// </summary>
    /// <param name="delta"></param>
    public void HandleMovement(float delta)
    {
        if (inputHandle.rollFlag || PlayerManager.isInAir || PlayerManager.isInteracting)
        {
            return;
        }
        moveDirection = cameraObject.forward * inputHandle.vertical;
        moveDirection += cameraObject.right * inputHandle.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;
        float speed = movementSpeed;
        if(inputHandle.sprintFlag && moveDirection != Vector3.zero)
        {
            speed = sprintSpeed;
            PlayerManager.isSprinting = true;
            moveDirection *= speed;
        }
        else
        {
            PlayerManager.isSprinting = false;
            moveDirection *= speed;
        }
        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, noramalVector);
        rb.velocity = projectedVelocity;
        if (inputHandle.LockFlag  && inputHandle.sprintFlag == false)
        {
            animatorHandle.UpdateAnimatorValues(inputHandle.vertical, inputHandle.horizontal, PlayerManager.isSprinting);
        }
        else
        {
            animatorHandle.UpdateAnimatorValues(inputHandle.moveAmount, 0, PlayerManager.isSprinting);
        }
        if (animatorHandle.canRotate)
        {
            HandleRotation(delta);
        }
    }
    /// <summary>
    /// 翻滚与后撤
    /// </summary>
    /// <param name="delta"></param>
    public void HandleRollingAndSprinting(float delta)
    {
        if(animatorHandle.anim.GetBool("isInteracting")) 
        {
            return;
        }
        if (inputHandle.rollFlag && !PlayerManager.isInAir)
        {
            moveDirection = cameraObject.forward * inputHandle.vertical;
            moveDirection += cameraObject.right * inputHandle.horizontal;

            if(inputHandle.moveAmount> 0)
            {
                moveDirection.y = 0;
                animatorHandle.PlayTargetAnimation("Rolling", true);
                netController.SendAction("Rolling");
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                myTransform.rotation = rollRotation;
            }
            else
            {
                animatorHandle.PlayTargetAnimation("Backstep", true);
                netController.SendAction("Backstep");
                return;
            }
        }
    }
    /// <summary>
    /// 坠落与地面检测
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="moveDirction"></param>
    public void HandleFalling(float delta)
    {
        PlayerManager.isGrounded = false;
        RaycastHit hit;
        Vector3 origin = myTransform.position;
        origin.y += groundDetectionRayStartPoint;
        //由于碰撞体已经不包含脚，这里来判断前方的障碍物是否可以通行
        if(Physics.Raycast(origin,myTransform.forward,out hit, 0.2f))
        {
            moveDirection = Vector3.zero;
        }
        //添加下落的速度
        if(PlayerManager.isInAir)
        {
            rb.AddForce(-Vector3.up * fallingSpeed);
            rb.AddForce(moveDirection * fallingSpeed / 5f); 
        }
        Vector3 dir = moveDirection;
        dir.Normalize();
        origin = origin + dir * groundDirectionRayDistance;
        targetPosition = myTransform.position;
        Debug.DrawRay(transform.position,-Vector3.up * minimumDistanceNeededToBeginFall,Color.red,0.1f,false);

        if(Physics.Raycast(origin,-Vector3.up,out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
        {
            noramalVector = hit.normal;
            Vector3 tp = hit.point;
            PlayerManager.isGrounded = true;
            targetPosition.y = tp.y;
            if(PlayerManager.isInAir)
            {
                moveDirection = Vector3.zero;
                if (inAirTimer > 0.5f)
                {
                    animatorHandle.PlayTargetAnimation("Landing", true);
                    inAirTimer = 0;
                }
                else
                {
                    animatorHandle.PlayTargetAnimation("Empty",false);
                    inAirTimer= 0;
                }
                PlayerManager.isInAir= false;
            }
        }
        else
        {
            if(PlayerManager.isGrounded)
            {
                PlayerManager.isGrounded = false;
            }

            if(PlayerManager.isInAir == false) 
            {
                if(PlayerManager.isInteracting == false)
                {
                    animatorHandle.PlayTargetAnimation("Falling",true);
                }

                Vector3 vel = rb.velocity;
                vel.Normalize();
                rb.velocity = vel * (movementSpeed / 2);
                PlayerManager.isInAir = true;
            }
        }
        if (PlayerManager.isGrounded)
        {
            if(PlayerManager.isInteracting || inputHandle.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position,targetPosition,Time.deltaTime);
            }
            else
            {
                myTransform.position = targetPosition;
            }
        }
    }
    #endregion
}
