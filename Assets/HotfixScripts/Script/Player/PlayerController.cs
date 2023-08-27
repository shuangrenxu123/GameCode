using UnityEngine;

public class PlayerController : CharacterLocomotionManager
{
    public CameraHandler cameraHandler;
    public Transform cameraObject;
    public PlayerInputHandle inputHandle;
    public Transform myTransform;
    public AnimatorHandle animatorHandle;
    public NetTranform netController;
    public Player player;
    public GameObject normalCamera;

    [Header("state")]
    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float rotationSpeed = 10;
    [SerializeField]
    float sprintSpeed = 8;
    [SerializeField]
    float climbSpeed = 0.001f;
    private void Awake()
    {
        player = GetComponent<Player>();
        entity = player;
    }
    private void Start()
    {
        cameraObject = Camera.main.transform;
        myTransform = player.transform;
        player.isGrounded = true;
        groundLayer = ~(1 << 13 | 1 << 11 | 1 << 10 | 1 << 12);
        animatorHandle = player.animatorHandle as AnimatorHandle;
        cameraHandler = CameraHandler.singleton;

        inputHandle = player.inputHandle;
        netController = player.net;
        animatorHandle.Initialize();
    }
    #region 移动代码
    /// <summary>
    /// 旋转相关
    /// </summary>
    /// <param name="delta"></param>
    public void HandleRotation(float delta)
    {
        if (player.climbLabber)
        {
            return;
        }
        if (animatorHandle.canRotate)
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
                        targetDirection = myTransform.forward;
                    }
                    Quaternion tr = Quaternion.LookRotation(targetDirection);
                    Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rotationSpeed * delta);
                    myTransform.rotation = targetRotation;
                }
                else
                {
                    Vector3 rotationDirection = moveDirection;
                    rotationDirection = cameraHandler.currentLockOnTarget.position - myTransform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rotationSpeed * delta);
                    myTransform.rotation = targetRotation;
                }
            }
            else
            {
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
    }
    /// <summary>
    /// 在地面上移动
    /// </summary>
    /// <param name="delta"></param>
    public void HandleGroundMovement(float delta)
    {
        if (inputHandle.rollFlag || player.isInAir || player.isInteracting)
        {
            return;
        }
        moveDirection = player.cameraHandler.transform.forward * inputHandle.vertical;
        moveDirection = moveDirection + player.cameraHandler.transform.right * player.inputHandle.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (inputHandle.sprintFlag)
        {
            player.characterController.Move(moveDirection * sprintSpeed * Time.deltaTime);
        }
        else
        {
            player.characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
        }

        if (inputHandle.LockFlag && inputHandle.sprintFlag == false)
        {
            animatorHandle.UpdateAnimatorValues(inputHandle.vertical, inputHandle.horizontal, inputHandle.sprintFlag);
        }
        else
        {
            if (inputHandle.sprintFlag && moveDirection.sqrMagnitude >= 0.01)
            {
                animatorHandle.UpdateAnimatorValues(inputHandle.moveAmount, 0, true);
            }
            else
            {
                animatorHandle.UpdateAnimatorValues(inputHandle.moveAmount, 0, false);
            }
        }
    }
    /// <summary>
    /// 攀爬梯子
    /// </summary>
    /// <param name="delta"></param>
    public void HandleLadderMovement(float delta)
    {
        if (inputHandle.rollFlag || player.isInAir || player.isInteracting)
        {
            return;
        }
        animatorHandle.UpdateAnimatorValues(inputHandle.vertical, 0, false);
        if (inputHandle.vertical < -0.01 || inputHandle.vertical > 0.01)
        {
            animatorHandle.anim.SetBool("ClimbLaddering",true);

            if (inputHandle.vertical < 0)
            {
                transform.position -= new Vector3(0, climbSpeed * Time.deltaTime, 0);
            }
            else if (inputHandle.vertical > 0)
            {
                transform.position += new Vector3(0, climbSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            animatorHandle.anim.SetBool("ClimbLaddering", false);
        }
    }
    /// <summary>
    /// 翻滚与后撤
    /// </summary>
    /// <param name="delta"></param>
    public void HandleRollingAndSprinting(float delta)
    {
        if (animatorHandle.anim.GetBool("isInteracting"))
        {
            return;
        }
        if (inputHandle.rollFlag && !player.isInAir)
        {
            moveDirection = cameraObject.forward * inputHandle.vertical;
            moveDirection += cameraObject.right * inputHandle.horizontal;

            if (inputHandle.moveAmount > 0)
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
                animatorHandle.anim.SetBool("isStep", true);
                netController.SendAction("Backstep");
                return;
            }
        }
    }
    #endregion
}
