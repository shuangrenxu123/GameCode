using Enemy;
using Sirenix.OdinInspector;
using UnityEngine;
namespace CharacterController
{
    public enum BrainType
    {
        Player,
        AI
    }

    public class CharacterBrain : MonoBehaviour
    {
        [EnumToggleButtons, HideLabel]
        public BrainType brainType = BrainType.Player;

        public UpdateModeType UpdateMode = UpdateModeType.FixedUpdate;

        bool isAI => brainType == BrainType.AI;
        [SerializeField, ShowIf("@isAI==false")]
        InputHandlerSettings inputHandlerSettings = new InputHandlerSettings();

        [SerializeField, ShowIf("@isAI==false")]
        InputHandlerSettings UIinputHandlerSettings = new InputHandlerSettings();

        [ShowIf("@isAI==false")]

        public InputHandlerSettings CameraInputHandlerSettings = new InputHandlerSettings();

        [SerializeField, ShowIf("isAI")]
        GameObject entityBraidGo;

        IEnemyBrain aiBehaviour = null;

        CharacterActions characterActions = new CharacterActions();
        CharacterUIActions characterUIActions = new CharacterUIActions();

        private bool IsUIInput = false;

        bool firstUpdateFlag = false;

        public bool IsAI => isAI;

        public CharacterActions CharacterActions => characterActions;
        public CharacterUIActions CharacterUIActions => characterUIActions;

        public void SetAction(CharacterActions characterActions) => this.characterActions = characterActions;


        public void UpdateBrainValues(float dt)
        {
            if (Time.timeScale == 0)
                return;

            UpdateHumanBrainValues(dt);
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
                if (isAI && aiBehaviour != null)
                {
                    characterActions.SetValues(aiBehaviour.characterActions);
                }
                else
                {
                    characterActions.SetValues(inputHandlerSettings.InputHandler);
                }

                characterActions.Update(dt);
            }

        }

        public void EnableUIInput()
        {
            if (IsUIInput)
            {
                return;
            }

            IsUIInput = true;
            characterActions.Reset();
            characterUIActions.Reset();
            inputHandlerSettings.InputHandler.Disable();
            UIinputHandlerSettings.InputHandler.Enable();
            CameraInputHandlerSettings.InputHandler.Disable();
        }
        public void DisableUIInput()
        {
            if (!IsUIInput)
            {
                return;
            }

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
            characterUIActions.InitializeActions();

            if (isAI)
            {
                aiBehaviour = entityBraidGo.GetComponent<IEnemyBrain>();
            }
        }
        protected virtual void OnEnable()
        {
            characterActions.InitializeActions();
            characterUIActions.InitializeActions();
            characterUIActions.Reset();
            characterActions.Reset();
        }
        protected virtual void OnDisable()
        {
            characterActions.Reset();
            characterUIActions.Reset();
        }

        protected virtual void FixedUpdate()
        {
            firstUpdateFlag = true;
            if (UpdateMode == UpdateModeType.FixedUpdate)
            {
                UpdateBrainValues(0f);
            }
            if (isAI && aiBehaviour != null)
            {
                // 对于AI而言他会清空输入，这样就不需要我们手动去处理输入值了
                aiBehaviour.characterActions.ForceReset();
            }
        }
        protected virtual void Update()
        {
            float dt = Time.deltaTime;
            if (!isAI)
            {
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
            }

            UpdateBrainValues(dt);

        }
        public enum UpdateModeType { FixedUpdate, Update }
    }
}