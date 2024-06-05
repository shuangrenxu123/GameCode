using UnityEngine;

/// <summary>
/// 此类负责检测输入和管理角色操作。
/// </summary>
public class CharacterBrain : MonoBehaviour
{
    public UpdateModeType UpdateMode = UpdateModeType.FixedUpdate;
    bool isAI = false;
    [SerializeField]
    InputHandlerSettings inputHandlerSettings = new InputHandlerSettings();
    [SerializeField]
    InputHandlerSettings UIinputHandlerSettings = new InputHandlerSettings();
    public InputHandlerSettings CameraInputHandlerSettings = new InputHandlerSettings();
    //CharacterAIBehaviour aiBehaviour = null;

    CharacterActions characterActions = new CharacterActions();
    CharacrerUIActions characterUIActions = new CharacrerUIActions();

    private bool IsUIInput = false;

    bool firstUpdateFlag = false;

    public bool IsAI => isAI;

    public CharacterActions CharacterActions => characterActions;
    public CharacrerUIActions CharacterUIActions => characterUIActions;

    public void SetAction(CharacterActions characterActions) => this.characterActions = characterActions;

    public void SetBrainType(bool IsAI)
    {
        characterActions.Reset();
        if (IsAI)
        {
            // todo SetAIBehaviour();
        }
        else
        {
            this.isAI = IsAI;
        }
    }

    // public void SetAIBehaviour(CharacterAIBehaviour )

    public void UpdateBrainValues(float dt)
    {
        if (Time.timeScale == 0)
            return;
        if (IsAI)
        {
            //todo UpdateAIBrainValues(dt);
        }
        else
        {
            UpdateHumanBrainValues(dt);
        }
    }
    void UpdateHumanBrainValues(float dt)
    {
        if (IsUIInput)
        {
            characterUIActions.SetValues(UIinputHandlerSettings.InputHandler);
            characterUIActions.Update(dt);
        }
        else
        {
            characterActions.SetValues(inputHandlerSettings.InputHandler);
            characterActions.Update(dt);
        }

    }

    public void EnableUIIpnut()
    {
        IsUIInput = true;
        characterActions.Reset();
        characterUIActions.Reset();
        inputHandlerSettings.InputHandler.Disable();
        UIinputHandlerSettings.InputHandler.Enable();
        CameraInputHandlerSettings.InputHandler.Disable();


    }
    public void DisableUIInput()
    {
        characterActions.Reset();
        characterUIActions.Reset();
        inputHandlerSettings.InputHandler.Enable();
        UIinputHandlerSettings.InputHandler.Disable();
        CameraInputHandlerSettings.InputHandler.Enable();
        IsUIInput = false;
    }
    protected virtual void Awake()
    {
        characterActions.InitializeActions();
        characterUIActions.InitalizeAcionts();

    }
    protected virtual void OnEnable()
    {
        characterActions.InitializeActions();
        characterUIActions.InitalizeAcionts();
        characterUIActions.Reset();
        characterActions.Reset();
    }
    protected virtual void OnDisable()
    {
        characterActions.Reset();
        characterUIActions.Reset();
    }
    private void Start()
    {
        SetBrainType(isAI);
    }
    protected virtual void Fixupdate()
    {
        firstUpdateFlag = true;
        if (UpdateMode == UpdateModeType.FixedUpdate)
        {
            UpdateBrainValues(0f);
        }
    }
    protected virtual void Update()
    {
        float dt = Time.deltaTime;
        if (UpdateMode == UpdateModeType.FixedUpdate)
        {
            if (firstUpdateFlag)
            {
                firstUpdateFlag = false;
                characterActions.Reset();
                characterUIActions.Reset();
            }
        }
        else
        {
            characterActions.Reset();
            characterUIActions.Reset();
        }

        UpdateBrainValues(dt);
    }
    public enum UpdateModeType { FixedUpdate, Update }
}