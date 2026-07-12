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

    [DefaultExecutionOrder(-100)]
    public class CharacterBrain : MonoBehaviour
    {
        enum InputSourceType
        {
            Player,
            AI,
            Network,
        }

        [EnumToggleButtons, HideLabel]
        public BrainType brainType = BrainType.Player;

        [FormerlySerializedAs("UpdateMode")]
        [SerializeField, HideInInspector]
        UpdateModeType updateModeCompatibility = UpdateModeType.Update;

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
        CharacterActions sampledCharacterActions = new CharacterActions();
        CharacterUIActions characterUIActions = new CharacterUIActions();
        CharacterUIActions sampledCharacterUIActions = new CharacterUIActions();

        [SerializeField, Min(8)]
        int inputQueueCapacity = 64;

        [SerializeField, Min(0.02f)]
        float inputCommandLifeTime = 0.3f;

        CharacterInputCommandQueue inputCommandQueue;
        CharacterInputMask enabledInputMask = CharacterInputMask.All;
        Vector2 lastQueuedMovement;
        uint inputSequence;

        bool isUIInputEnabled;
        bool actionsInitialized;
        bool aiReferenceWarningShown;
        bool networkReferenceWarningShown;

        public bool IsAI => isAI;

        public CharacterActions CharacterActions => characterActions;
        public CharacterUIActions CharacterUIActions => characterUIActions;
        public int PendingInputCount => inputCommandQueue?.Count ?? 0;

        public void UpdateBrainValues(float dt)
        {
            AdvanceActions(dt);
            EnqueueCurrentActions();
        }

        public bool TryGetInputCommand(CharacterInputType type, out CharacterInputCommand command)
        {
            if (inputCommandQueue == null)
            {
                command = default;
                return false;
            }

            return inputCommandQueue.TryConsume(type, Time.time, out command);
        }

        public bool TryGetLatestInputCommand(CharacterInputType type, out CharacterInputCommand command)
        {
            if (inputCommandQueue == null)
            {
                command = default;
                return false;
            }

            return inputCommandQueue.TryConsumeLatest(type, Time.time, out command);
        }

        public bool IsInputEnabled(CharacterInputType type)
        {
            return (enabledInputMask & type.ToMask()) != 0;
        }

        public void EnableInput(CharacterInputMask mask)
        {
            enabledInputMask |= mask;
        }

        public void DisableInput(CharacterInputMask mask, bool clearQueuedCommands = true)
        {
            enabledInputMask &= ~mask;

            if (!clearQueuedCommands || inputCommandQueue == null)
            {
                return;
            }

            for (int i = 0; i <= (int)CharacterInputType.UICancel; i++)
            {
                CharacterInputType type = (CharacterInputType)i;
                if ((mask & type.ToMask()) != 0)
                {
                    inputCommandQueue.Remove(type);
                }
            }
        }

        public void SetEnabledInputs(CharacterInputMask mask, bool clearDisabledCommands = true)
        {
            CharacterInputMask disabledMask = CharacterInputMask.All & ~mask;
            enabledInputMask = mask;

            if (clearDisabledCommands)
            {
                DisableInput(disabledMask, true);
                enabledInputMask = mask;
            }
        }

        public void ClearInputCommands()
        {
            inputCommandQueue?.Clear();
        }

        void AdvanceActions(float dt)
        {
            SampleActions();
            characterActions.ClearFrameFlags();
            characterUIActions.ClearFrameFlags();
            characterActions.SetValues(sampledCharacterActions);
            characterUIActions.SetValues(sampledCharacterUIActions);
            characterActions.Update(dt);
            characterUIActions.Update(dt);
        }

        void SampleActions()
        {
            sampledCharacterActions.Reset();
            sampledCharacterUIActions.Reset();

            switch (ResolveInputSource())
            {
                case InputSourceType.Player:
                    SamplePlayerActions();
                    break;
                case InputSourceType.AI:
                    SampleAIActions();
                    break;
                case InputSourceType.Network:
                    SampleNetworkActions();
                    break;
            }
        }

        void SamplePlayerActions()
        {
            if (isUIInputEnabled)
            {
                if (UIinputHandlerSettings.InputHandler != null)
                {
                    sampledCharacterUIActions.SetValues(UIinputHandlerSettings.InputHandler);
                }

                if (inputHandlerSettings.InputHandler != null)
                {
                    // UI模式下仍然允许控制台快捷键通过角色输入触发。
                    sampledCharacterActions.OpenConsoleUI.value = inputHandlerSettings.InputHandler.GetBool("OpenConsole");
                }
            }
            else
            {
                if (inputHandlerSettings.InputHandler != null)
                {
                    sampledCharacterActions.SetValues(inputHandlerSettings.InputHandler);
                }
            }
        }

        void SampleAIActions()
        {
            if (aiBehaviour == null)
            {
                ResolveExternalReferences();
            }

            if (aiBehaviour == null)
            {
                LogMissingAIReference();
                return;
            }

            aiReferenceWarningShown = false;
            sampledCharacterActions.SetValues(aiBehaviour.characterActions);
        }

        void SampleNetworkActions()
        {
            if (netCharacterInput == null)
            {
                ResolveExternalReferences();
            }

            if (netCharacterInput == null)
            {
                LogMissingNetworkReference();
                return;
            }

            networkReferenceWarningShown = false;
            sampledCharacterActions.SetValues(netCharacterInput.CharacterActions);
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
            if (!isPlayer || isUIInputEnabled == enabled)
            {
                return;
            }

            isUIInputEnabled = enabled;
            ResetLocalActions();
            ApplyPlayerInputMode();
        }

        protected virtual void Awake()
        {
            inputCommandQueue = new CharacterInputCommandQueue(inputQueueCapacity);
            InitializeActions();
            ResolveExternalReferences();
        }

        protected virtual void OnEnable()
        {
            InitializeActions();
            ResolveExternalReferences();
            ResetLocalActions();
            ApplyPlayerInputMode();
        }

        protected virtual void OnDisable()
        {
            ResetLocalActions();
        }

        protected virtual void FixedUpdate()
        {
            AdvanceActions(Time.fixedDeltaTime);
            EnqueueCurrentActions();
        }

        void EnqueueCurrentActions()
        {
            if (inputCommandQueue == null)
            {
                return;
            }

            float currentTime = Time.time;
            inputCommandQueue.RemoveExpired(currentTime);

            EnqueueBool(CharacterInputType.Jump, characterActions.jump, currentTime);
            EnqueueBool(CharacterInputType.Run, characterActions.run, currentTime);
            EnqueueBool(CharacterInputType.Interact, characterActions.interact, currentTime);
            EnqueueBool(CharacterInputType.Roll, characterActions.roll, currentTime);
            EnqueueBool(CharacterInputType.Lock, characterActions.@lock, currentTime);
            EnqueueBool(CharacterInputType.Attack, characterActions.attack, currentTime);
            EnqueueBool(CharacterInputType.HeavyAttack, characterActions.heavyAttack, currentTime);
            EnqueueBool(CharacterInputType.Crouch, characterActions.crouch, currentTime);
            EnqueueBool(CharacterInputType.OpenUI, characterActions.OpenUI, currentTime);
            EnqueueBool(CharacterInputType.OpenConsole, characterActions.OpenConsoleUI, currentTime);
            EnqueueBool(CharacterInputType.UIConfirm, characterUIActions.confirm, currentTime);
            EnqueueBool(CharacterInputType.UICancel, characterUIActions.cancel, currentTime);
            EnqueueMovement(characterActions.movement.value, currentTime);
        }

        void EnqueueBool(CharacterInputType type, BoolAction action, float currentTime)
        {
            if (!action.value && !action.Canceled)
            {
                return;
            }

            CharacterInputPhase phase = action.Started
                ? CharacterInputPhase.Started
                : action.Canceled
                    ? CharacterInputPhase.Canceled
                    : CharacterInputPhase.Performed;

            EnqueueCommand(type, phase, action.value, Vector2.zero, currentTime);
        }

        void EnqueueMovement(Vector2 movement, float currentTime)
        {
            bool hasMovement = movement != Vector2.zero;
            bool hadMovement = lastQueuedMovement != Vector2.zero;

            if (!hasMovement && !hadMovement)
            {
                return;
            }

            CharacterInputPhase phase = !hadMovement && hasMovement
                ? CharacterInputPhase.Started
                : hadMovement && !hasMovement
                    ? CharacterInputPhase.Canceled
                    : CharacterInputPhase.Performed;

            EnqueueCommand(CharacterInputType.Movement, phase, hasMovement, movement, currentTime);
            lastQueuedMovement = movement;
        }

        void EnqueueCommand(
            CharacterInputType type,
            CharacterInputPhase phase,
            bool boolValue,
            Vector2 vector2Value,
            float currentTime)
        {
            if (!IsInputEnabled(type))
            {
                return;
            }

            inputSequence++;
            inputCommandQueue.Enqueue(new CharacterInputCommand(
                type,
                phase,
                boolValue,
                vector2Value,
                currentTime,
                currentTime + inputCommandLifeTime,
                inputSequence));
        }

        InputSourceType ResolveInputSource()
        {
            if (isAI)
            {
                return InputSourceType.AI;
            }

            if (isNetwork)
            {
                return InputSourceType.Network;
            }

            return InputSourceType.Player;
        }

        void InitializeActions()
        {
            if (actionsInitialized)
            {
                return;
            }

            characterActions.InitializeActions();
            sampledCharacterActions.InitializeActions();
            characterUIActions.InitializeActions();
            sampledCharacterUIActions.InitializeActions();
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
            sampledCharacterActions.Reset();
            characterUIActions.Reset();
            sampledCharacterUIActions.Reset();
            lastQueuedMovement = Vector2.zero;
            inputCommandQueue?.Clear();
        }

        void ApplyPlayerInputMode()
        {
            if (!isPlayer)
            {
                return;
            }

            SetHandlerEnabled(inputHandlerSettings, !isUIInputEnabled);
            SetHandlerEnabled(UIinputHandlerSettings, isUIInputEnabled);
            SetHandlerEnabled(CameraInputHandlerSettings, !isUIInputEnabled);
        }

        void LogMissingAIReference()
        {
            if (aiReferenceWarningShown)
            {
                return;
            }

            aiReferenceWarningShown = true;
            Debug.LogWarning($"{name} 的 CharacterBrain 未找到 IEnemyBrain，已清空 AI 输入。", this);
        }

        void LogMissingNetworkReference()
        {
            if (networkReferenceWarningShown)
            {
                return;
            }

            networkReferenceWarningShown = true;
            Debug.LogWarning($"{name} 的 CharacterBrain 未找到 NetCharacterInput，已清空网络输入。", this);
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
