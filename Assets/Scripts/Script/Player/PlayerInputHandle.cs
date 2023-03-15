using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputHandle : MonoBehaviour
{
    public float horizontal;
    public float vertical;
    public float moveAmount;

    public float mousex;
    public float mousey;

    public bool b_Input;
    public bool e_Input;
    public bool rollFlag;
    public bool comboFlag;
    public bool DefenseFlag;

    PlayerInventory PlayerInventory;
    PlayerAttack PlayerAttacker;
    PlayerControls inputAction;
    Player playerManager;

    Vector2 movementInput;
    Vector2 cameraInput;

    public bool sprintFlag;
    public bool LightAttackFlag;
    private void Awake()
    {
        PlayerInventory = GetComponent<PlayerInventory>();
        PlayerAttacker = GetComponent<PlayerAttack>();
        playerManager = GetComponent<Player>();
    }
    private void OnEnable()
    {
        if (inputAction == null)
        {
            inputAction = new PlayerControls();
        }
        inputAction.PlayerMovement.Movement.performed   += i =>movementInput=i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Movement.canceled    += i => movementInput = i.ReadValue<Vector2>();

        inputAction.PlayerMovement.Camera.performed     += i => cameraInput = i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Camera.canceled      += i => cameraInput = i.ReadValue<Vector2>();

        inputAction.PlayerMovement.Sprint.performed += i => sprintFlag = true;
        inputAction.PlayerMovement.Sprint.canceled += i => sprintFlag = false;

        inputAction.PlayerAction.defense.performed += i => DefenseFlag = true;
        inputAction.PlayerAction.defense.canceled += i => DefenseFlag = false;

        inputAction.Enable();
    }
    private void Start()
    {
        Cursor.visible = false;
    }
    private void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.Disable();
        }
    }

    public void TickInput(float delta)
    {
        MoveInput(delta);
        HanldeRollInput(delta);
        HandleAttackInput(delta);
        HandleInteractingInput(delta);
        HandleDefenseInput(delta);
    }
    private void MoveInput(float delta)
    {
        horizontal = movementInput.x;
        vertical = movementInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mousex = cameraInput.x;
        mousey = cameraInput.y;
    }

    private void HanldeRollInput(float delta)
    {
        b_Input = inputAction.PlayerAction.Roll.phase == InputActionPhase.Performed;
        if (b_Input)
        {
            rollFlag = true;
        }
    }
    private void HandleAttackInput(float delta)
    {
        inputAction.PlayerAction.LightAttack.performed += i => LightAttackFlag = true;

        if(LightAttackFlag)
        {
            if (playerManager.canDoCombo)
            {
                comboFlag = true;
                PlayerAttacker.HandleWeaponCombo(PlayerInventory.rightWeapon as WeaponItem);
                comboFlag = false;
            }
            else
            {
                if(playerManager.isInteracting)
                {
                    return;
                }
                if (playerManager.canDoCombo)
                    return;
                PlayerAttacker.HandleLightAttack(PlayerInventory.rightWeapon as WeaponItem);
            }
        }
    }
    private void HandleInteractingInput(float delta)
    {
        inputAction.PlayerAction.Interactable.performed += i => e_Input = true;
    }

    private void HandleDefenseInput(float delta)
    {
        if (LightAttackFlag)
            return;
        
        PlayerAttacker.HandleDefense(PlayerInventory.leftWeapon as ArmorItem);

    }

}
