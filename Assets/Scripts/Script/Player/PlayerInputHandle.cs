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

    public bool LockFlag;
    public bool rollFlag;
    public bool comboFlag;
    public bool DefenseFlag;
    public bool UIFlag;
    public bool TwoHandFlag;
    public bool UseItem;
    public bool BagPanleFlag = false;

    public Equipmanager equipmanager;
    public PlayerInventory PlayerInventory;
    public PlayerCombatInputHandle PlayerAttacker;
    public PlayerControls inputAction;
    public Player playerManager;
    public CameraHandler cameraHandler;
    Vector2 movementInput;
    Vector2 cameraInput;

    public bool sprintFlag;
    public bool LightAttackFlag;
    private void Awake()
    {
        PlayerInventory = GetComponent<PlayerInventory>();
        playerManager = GetComponent<Player>();
        equipmanager = GetComponent<Equipmanager>();
    }
    private void Start()
    {
        cameraHandler = playerManager.cameraHandler;
        PlayerAttacker = playerManager.combatInputHandle;
    }
    private void OnEnable()
    {
        if (inputAction == null)
        {
            inputAction = new PlayerControls();
        }
        inputAction.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Movement.canceled += i => movementInput = i.ReadValue<Vector2>();

        inputAction.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
        inputAction.PlayerMovement.Camera.canceled += i => cameraInput = i.ReadValue<Vector2>();
        //奔跑
        inputAction.PlayerMovement.Sprint.performed += i => sprintFlag = true;
        inputAction.PlayerMovement.Sprint.canceled += i => sprintFlag = false;
        //防御
        inputAction.PlayerAction.defense.performed += i => DefenseFlag = true;
        inputAction.PlayerAction.defense.canceled += i => DefenseFlag = false;
        //交互
        inputAction.PlayerAction.Interactable.performed += i => e_Input = true;
        inputAction.PlayerAction.Interactable.canceled += i => e_Input = false;
        //翻滚
        inputAction.PlayerAction.Roll.performed += i => b_Input = true;
        inputAction.PlayerAction.Roll.canceled += i => b_Input = false;
        //锁定敌人
        inputAction.PlayerAction.LightAttack.performed += i => LightAttackFlag = true;
        inputAction.PlayerAction.LightAttack.canceled += i => LightAttackFlag = false;

        inputAction.PlayerAction.Lock.performed += i => HandleLockInput();

        inputAction.PlayerAction.DualWielding.performed += i => HandleTwoHandInput();

        inputAction.PlayerAction.useItem.performed += i => HandleUseItemInput();

        inputAction.PlayerUI.ESC.performed += i => HandleEscInput();
        inputAction.PlayerAction.ESC.performed += i => HandleEscInput();

        inputAction.PlayerAction.Bag.performed += i => HandleBaginput();

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
        if (LightAttackFlag)
        {
            PlayerAttacker.HandleLightAttack();
            LightAttackFlag = false;
        }
    }
    private void HandleLockInput()
    {
        if (LockFlag == false)
        {
            cameraHandler.ClearLockTargets();
            LockFlag = cameraHandler.HandleLockOn();
        }
        else if (LockFlag)
        {
            LockFlag = false;
            cameraHandler.ClearLockTargets();
        }
        cameraHandler.SetCameraHeight();
    }
    private void HandleDefenseInput(float delta)
    {
        if (LightAttackFlag)
            return;

        PlayerAttacker.HandleDefense(PlayerInventory.leftWeapon as ArmorItemData);

    }
    private void HandleEscInput()
    {
        UIFlag = !UIFlag;
        if (UIFlag)
        {
            WindowsManager.Instance.EnableWindow<GameUIMgr>();
            WindowsManager.Instance.DisableWindow<StateUI>();
            DisableGameTable();
        }
        else
        {
            WindowsManager.Instance.DisableWindow<GameUIMgr>();
            WindowsManager.Instance.EnableWindow<StateUI>();
            EnableGameTable();
        }
    }
    private void HandleUseItemInput()
    {
        PlayerInventory.UseProps();
    }
    private void EnableGameTable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        inputAction.PlayerAction.Enable();
        inputAction.PlayerMovement.Enable();
        inputAction.PlayerUI.Disable();
    }
    private void DisableGameTable()
    {
        Cursor.lockState = CursorLockMode.None;
        inputAction.PlayerAction.Disable();
        inputAction.PlayerMovement.Disable();
        inputAction.PlayerUI.Enable();
    } 
    private void HandleTwoHandInput()
    {
        TwoHandFlag = !TwoHandFlag;
        if(TwoHandFlag == false)
        {
            equipmanager.LoadWeaponOnSlot(PlayerInventory.rightWeapon,false);
            equipmanager.LoadRightHandIK(false);
        }
        else
        {
            equipmanager.LoadWeaponOnSlot(PlayerInventory.rightWeapon, false);
            equipmanager.LoadWeaponOnSlot(PlayerInventory.leftWeapon, true);
            equipmanager.LoadRightHandIK(true);
        }
    }
    private void HandleBaginput()
    {
        if (BagPanleFlag == false)
        {
            WindowsManager.Instance.EnableWindow<BagPanel>();
            
        }
        else
        {
            WindowsManager.Instance.DisableWindow<BagPanel>();
        }
        BagPanleFlag = !BagPanleFlag;
    }
}
