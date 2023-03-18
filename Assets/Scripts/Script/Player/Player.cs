using Fight;
using NetWork;
using PlayerInfo;
using UnityEngine;


public class Player : CharacterManager
{
    CombatEntity entity;
    PlayerInputHandle inputHandle;
    CameraHandler cameraHandler;
    PlayerController controller;
    Animator animator;
    [Header("Player Flag")]
    public bool isInteracting;
    public bool isSprinting;
    public bool isDefense;
    public bool isInAir = false;
    public bool isGrounded;
    public bool canDoCombo;
    #region 网络同步部分
    MotionState lastMotionState;
    public string id;
    /// <summary>
    /// 位置差。当大于该值时才会发送坐标
    /// </summary>
    public float deadReckoningThreshold = 1f;
    /// <summary>
    /// 预测位置，即在其他客户端中的位置
    /// </summary>
    private Vector3 drPosition = Vector3.zero;
#endregion
    void Start()
    {
        inputHandle = GetComponent<PlayerInputHandle>();
        cameraHandler = CameraHandler.singleton;
        animator = GetComponentInChildren<Animator>();
        controller= GetComponent<PlayerController>();   
    }
    public void SyncPlayerLastMotionState(DefaultNetWorkPackage package)
    {
        var state = (move)package.Msgobj;
        if (state != null)
        {
            lastMotionState.lastMotionTime = Time.time;
            lastMotionState.Position = NetWorkUtility.ToUnityV3(state.Position);
            lastMotionState.velocity = NetWorkUtility.ToUnityV3(state.Velocity);
        }
    }
    void Update()
    {
        float delta = Time.deltaTime;
        canDoCombo = animator.GetBool("canDoCombo");
        isInteracting = animator.GetBool("isInteracting");
        isDefense = animator.GetBool("isDefense");

        inputHandle.TickInput(delta);
        controller.HandleMovement(delta);
        controller.HandleRollingAndSprinting(delta);
        //controller.HandleFalling(delta,controller.moveDirection);
        CheckForInteractableObject();
    }
    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        if (cameraHandler != null)
        {
            cameraHandler.FollowTarget(delta);
            cameraHandler.HandleCamerRotation(delta, inputHandle.mousex, inputHandle.mousey);
        }
        controller.HandleFalling(delta, controller.moveDirection);
    }
    private void LateUpdate()
    {
        inputHandle.rollFlag = false;
        inputHandle.LightAttackFlag = false;
        if(isInAir)
        {
            controller.inAirTimer = controller.inAirTimer + Time.deltaTime;
        }
    } 

    private void CheckForInteractableObject()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position,0.3f,transform.forward,out hit,1f,cameraHandler.ignoreLayers))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    WindowsManager.Instance.EnableWindow<InteractPanel>();
                    if (inputHandle.e_Input)
                    {
                        interactable.Interact(this);
                    }
                }
            }
        }
        else
        {
            WindowsManager.Instance.DisableWindow<InteractPanel>();
        }
    }
}
public struct MotionState
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 velocity;
    public float lastMotionTime;
}
