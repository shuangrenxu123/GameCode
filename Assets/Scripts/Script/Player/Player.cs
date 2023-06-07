using Fight;
using NetWork;
using PlayerInfo;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


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
        inputHandle =       GetComponent<PlayerInputHandle>();
        combatInputHandle = new PlayerCombatInputHandle(this);
        combatEntity =            GetComponent<CombatEntity>();
        animatorHandle =    GetComponent<AnimatorHandle>();
        net =               GetComponent<NetTranform>();
        characterController = GetComponent<CharacterController>();
        controller          = GetComponent<PlayerController>();   
        inventory           = GetComponent<PlayerInventory>();
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
        inputHandle.TickInput(delta);
        controller.HandleGroundMovement(delta);
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
        if (Physics.SphereCast(transform.position,0.3f,transform.forward,out hit,1f,cameraHandler.ignoreLayers))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    WindowsManager.Instance.EnableWindow<InteractPanel>();
                    if (inputHandle.e_Input && isInteracting == false)
                    {
                        interactable.Interact(this);
                    }
                    else if(isInteracting == true)
                    {
                        interactable.InteractUpdate(this);
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
