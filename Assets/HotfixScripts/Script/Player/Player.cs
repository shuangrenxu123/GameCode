using Fight;
using UnityEngine;

public class Player : CharacterManager
{
    [Header("InputHandle")]
    public PlayerInputHandle inputHandle;
    public CameraHandler cameraHandler;
    public PlayerController controller;
    public PlayerCombatInputHandle combatInputHandle;
    public CombatEntity combatEntity;
    public NetTranform net;
    public PlayerInventory inventory;
    public Interactable interactable;
    [Header("Player Status")]
    public bool isInteracting;
    public bool isDefense;
    public bool isInAir = false;
    public bool canDoCombo;
    public bool isStep;
    [Header("Net")]
    public string id;
    private void Awake()
    {
        inputHandle = GetComponent<PlayerInputHandle>();
        combatInputHandle = new PlayerCombatInputHandle(this);
        combatEntity = GetComponent<CombatEntity>();
        animatorHandle = GetComponent<AnimatorHandle>();
        net = GetComponent<NetTranform>();
        characterController = GetComponent<CharacterController>();
        controller = GetComponent<PlayerController>();
        inventory = GetComponent<PlayerInventory>();

        backStep = GetComponentInChildren<BackStepCollider>();
    }
    void Start()
    {
        cameraHandler = CameraHandler.singleton;
        combatEntity.Init(1000);
        combatInputHandle.Init();
        inputHandle.PlayerAttacker = combatInputHandle;
    }
    void Update()
    {
        float delta = Time.deltaTime;
        isInteracting = animatorHandle.GetBool("isInteracting");
        canDoCombo = animatorHandle.GetBool("canDoCombo");
        isDefense = animatorHandle.GetBool("isDefense");
        animatorHandle.canRotate = animatorHandle.GetBool("canRotate");
        isStep = animatorHandle.GetBool("isStep");
        climbLabber = animatorHandle.GetBool("ClimbLadder");
        inputHandle.TickInput(delta);
        if (climbLabber == false)
        {
            controller.HandleGroundMovement(delta);
        }
        else
        {
            controller.HandleLadderMovement(delta);
        }
        controller.HandleRollingAndSprinting(delta);
        CheckForInteractableObject();
        controller.HandleRotation(delta);
        if (cameraHandler != null)
        {
            cameraHandler.FollowTarget(delta);
            cameraHandler.HandleCamerRotation(delta, inputHandle.mousex, inputHandle.mousey);
        }
    }
    private void LateUpdate()
    {
        inputHandle.rollFlag = false;
    }
    private void CheckForInteractableObject()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.3f, transform.forward, out hit, 1f, cameraHandler.ignoreLayers))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    WindowsManager.Instance.EnableWindow<InteractPanel>();
                }
            }
        }
        else
        {
            WindowsManager.Instance.DisableWindow<InteractPanel>();
            interactable = null;
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
