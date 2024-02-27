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

    //CharacterAIBehaviour aiBehaviour = null;

    CharacterActions characterActions = new CharacterActions();
    CharacrerUIActions characterUIActions = new CharacrerUIActions();


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
    public void SetInputHandler(InputHandler inputHandler)
    {
        if (inputHandler == null)
        {
            return;
        }
        inputHandlerSettings.InputHandler = inputHandler;
        characterActions.Reset();
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
        characterActions.SetValues(inputHandlerSettings.InputHandler);
        characterActions.Update(dt);

        characterUIActions.SetValues(UIinputHandlerSettings.InputHandler);
        characterUIActions.Update(dt);
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