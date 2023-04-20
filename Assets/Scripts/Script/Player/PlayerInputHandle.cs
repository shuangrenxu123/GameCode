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
    public bool LockOnInput;

    [SerializeField]
    public bool LockFlag;
    public bool rollFlag;
    public bool comboFlag;
    public bool DefenseFlag;
    public bool UIFlag;

    PlayerInventory PlayerInventory;
    PlayerAttack PlayerAttacker;
    PlayerControls inputAction;
    Player playerManager;
    public CameraHandler cameraHandler;
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
        inputAction.PlayerMovement.Movement.performed   += i => movementInput = i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Movement.canceled    += i => movementInput = i.ReadValue<Vector2>();

        inputAction.PlayerMovement.Camera.performed     += i => cameraInput = i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Camera.canceled      += i => cameraInput = i.ReadValue<Vector2>();

        inputAction.PlayerMovement.Sprint.performed     += i => sprintFlag = true;
        inputAction.PlayerMovement.Sprint.canceled      += i => sprintFlag = false;

        inputAction.PlayerAction.defense.performed      += i => DefenseFlag = true;
        inputAction.PlayerAction.defense.canceled       += i => DefenseFlag = false;

        inputAction.PlayerAction.Interactable.performed += i => e_Input = true;
        inputAction.PlayerAction.Interactable.canceled  += i => e_Input = false;

        inputAction.PlayerAction.Roll.performed         += i => b_Input = true;
        inputAction.PlayerAction.Roll.canceled          += i => b_Input = false;

        inputAction.PlayerAction.Lock.performed         += i => LockOnInput = true;
        inputAction.PlayerAction.Lock.canceled          += i => LockOnInput = false;

        inputAction.PlayerUI.ESC.performed              += i => HandleEscInput();
        inputAction.PlayerAction.ESC.performed          += i => HandleEscInput();
        inputAction.Enable();
        EnableGameTable();

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
        HandleMoveInput(delta);
        HanldeRollInput(delta);
        HandleAttackInput(delta);
        HandleDefenseInput(delta);
        HandleLockInput();
    }
    private void HandleMoveInput(float delta)
    {
        horizontal = movementInput.x;
        vertical = movementInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mousex = cameraInput.x;
        mousey = cameraInput.y;
    }
    private void HanldeRollInput(float delta)
    {
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
                if(playerManager.isInteracting|| playerManager.canDoCombo || playerManager.isDefense)
                {
                    return;
                }
                PlayerAttacker.HandleLightAttack(PlayerInventory.rightWeapon as WeaponItem);
            }
        }
    }
    private void HandleLockInput()
    {
        if (LockOnInput && LockFlag == false)
        {
            cameraHandler.ClearLockTargets();
            LockOnInput= false;
            LockFlag = cameraHandler.HandleLockOn();
        }
        else if(LockOnInput && LockFlag) 
        {
            LockFlag = false;
            LockOnInput = false;
            cameraHandler.ClearLockTargets();
        }
        cameraHandler.SetCameraHeight();
    }
    private void HandleDefenseInput(float delta)
    {
        if (LightAttackFlag)
            return;
        
        PlayerAttacker.HandleDefense(PlayerInventory.leftWeapon as ArmorItem);

    }

    private void HandleEscInput()
    {
        UIFlag = !UIFlag;
        if (UIFlag)
        {
            WindowsManager.Instance.EnableWindow<GameUIMgr>();
            DisableGameTable();
        }
        else
        {
            WindowsManager.Instance.DisableWindow<GameUIMgr>();
            EnableGameTable();
        }
    }

    private void EnableGameTable()
    {
        Cursor.visible = false;
        inputAction.PlayerAction.Enable();
        inputAction.PlayerMovement.Enable();
        inputAction.PlayerUI.Disable();
    }
    private void DisableGameTable()
    {
        Cursor.visible = true;
        inputAction.PlayerAction.Disable();
        inputAction.PlayerMovement.Disable();
        inputAction.PlayerUI.Enable();
    }
}