using Enemy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterController
{
    public enum BrainType
    {
        Player,
        AI,
        Network,
    }

    public class CharacterBrain : MonoBehaviour
    {
        enum InputSourceType
        {
            None,
            Player,
            AI,
            Network,
            UI,
        }

        [EnumToggleButtons, HideLabel]
        public BrainType brainType = BrainType.Player;

        public UpdateModeType UpdateMode = UpdateModeType.FixedUpdate;

        bool isAI => brainType == BrainType.AI;
        bool isNetwork => brainType == BrainType.Network;
        bool isPlayer => brainType == BrainType.Player;

        [SerializeField, ShowIf("@isPlayer")]
        InputHandlerSettings inputHandlerSettings = new InputHandlerSettings();

        [SerializeField, ShowIf("@isPlayer")]
        InputHandlerSettings UIinputHandlerSettings = new InputHandlerSettings();

        [ShowIf("@isPlayer")]
        public InputHandlerSettings CameraInputHandlerSettings = new InputHandlerSettings();

        [SerializeField, ShowIf("isAI"), FormerlySerializedAs("entityBraidGo")]
        GameObject entityBrainGo;

        [SerializeField, ShowIf("isNetwork")]
        NetCharacterInput netCharacterInput;

        IEnemyBrain aiBehaviour;

        CharacterActions characterActions = new CharacterActions();
        CharacterUIActions characterUIActions = new CharacterUIActions();

        bool isUIInputEnabled;
        bool shouldResetActionsOnNextUpdate;
        bool actionsInitialized;

        public bool IsAI => isAI;

        public CharacterActions CharacterActions => characterActions;
        public CharacterUIActions CharacterUIActions => characterUIActions;

        public void SetAction(CharacterActions characterActions) => this.characterActions = characterActions;

        public void UpdateBrainValues(float dt)
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            UpdateActions(dt, ResolveInputSource());
        }

        void UpdateActions(float dt, InputSourceType inputSource)
        {
            if (inputSource == InputSourceType.UI)
            {
                UpdateUIActions(dt);
                return;
            }

            UpdateGameplayActions(dt, inputSource);
        }

        void UpdateGameplayActions(float dt, InputSourceType inputSource)
        {
            switch (inputSource)
            {
                case InputSourceType.Player:
                    ApplyPlayerInput();
                    break;
                case InputSourceType.AI:
                    characterActions.SetValues(aiBehaviour.characterActions);
                    break;
                case InputSourceType.Network:
                    characterActions.SetValues(netCharacterInput.CharacterActions);
                    break;
                default:
                    characterActions.ForceReset();
                    break;
            }

            characterActions.Update(dt);
        }

        void UpdateUIActions(float dt)
        {
            if (UIinputHandlerSettings.InputHandler == null)
            {
                characterUIActions.SetValues(default(CharacterUIActions));
            }
            else
            {
                characterUIActions.SetValues(UIinputHandlerSettings.InputHandler);
            }

            characterUIActions.Update(dt);
        }

        void ApplyPlayerInput()
        {
            if (inputHandlerSettings.InputHandler == null)
            {
                characterActions.ForceReset();
                return;
            }

            characterActions.SetValues(inputHandlerSettings.InputHandler);
        }

        public void EnableUIInput()
        {
            SetUIInputEnabled(true);
        }

        public void DisableUIInput()
        {
            SetUIInputEnabled(false);
        }

        public void SetUIInputEnabled(bool enabled)
        {
            if (isNetwork || isUIInputEnabled == enabled)
            {
                return;
            }

            isUIInputEnabled = enabled;
            ResetLocalActions();

            SetHandlerEnabled(inputHandlerSettings, !enabled);
            SetHandlerEnabled(UIinputHandlerSettings, enabled);
            SetHandlerEnabled(CameraInputHandlerSettings, !enabled);
        }

        protected virtual void Awake()
        {
            InitializeActions();
            ResolveExternalReferences();
        }

        protected virtual void OnEnable()
        {
            InitializeActions();
            ResetLocalActions();
        }

        protected virtual void OnDisable()
        {
            ResetLocalActions();
        }

        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModeType.FixedUpdate)
            {
                shouldResetActionsOnNextUpdate = !isAI;
                UpdateBrainValues(0f);
                ResetExternalInputSourceIfNeeded();
            }
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;
            if (!isAI)
            {
                if (UpdateMode == UpdateModeType.FixedUpdate)
                {
                    if (shouldResetActionsOnNextUpdate)
                    {
                        shouldResetActionsOnNextUpdate = false;
                        ResetLocalActions();
                    }
                }
                else
                {
                    ResetLocalActions();
                }
            }

            UpdateBrainValues(dt);
        }

        InputSourceType ResolveInputSource()
        {
            if (isUIInputEnabled && !isNetwork)
            {
                return InputSourceType.UI;
            }

            if (isAI && aiBehaviour != null)
            {
                return InputSourceType.AI;
            }

            if (isNetwork && netCharacterInput != null)
            {
                return InputSourceType.Network;
            }

            if (isPlayer)
            {
                return InputSourceType.Player;
            }

            return InputSourceType.None;
        }

        void InitializeActions()
        {
            if (actionsInitialized)
            {
                return;
            }

            characterActions.InitializeActions();
            characterUIActions.InitializeActions();
            actionsInitialized = true;
        }

        void ResolveExternalReferences()
        {
            if (isAI)
            {
                aiBehaviour = entityBrainGo != null
                    ? entityBrainGo.GetComponent<IEnemyBrain>()
                    : GetComponentInChildren<IEnemyBrain>();
            }

            if (isNetwork && netCharacterInput == null)
            {
                netCharacterInput = GetComponentInChildren<NetCharacterInput>();
            }
        }

        void ResetLocalActions()
        {
            characterActions.Reset();
            characterUIActions.Reset();
        }

        void ResetExternalInputSourceIfNeeded()
        {
            if (isAI && aiBehaviour != null)
            {
                aiBehaviour.characterActions.ForceReset();
            }
        }

        static void SetHandlerEnabled(InputHandlerSettings settings, bool enabled)
        {
            if (settings == null || settings.InputHandler == null)
            {
                return;
            }

            if (enabled)
            {
                settings.InputHandler.Enable();
            }
            else
            {
                settings.InputHandler.Disable();
            }
        }

        public enum UpdateModeType
        {
            FixedUpdate,
            Update
        }
    }
}
