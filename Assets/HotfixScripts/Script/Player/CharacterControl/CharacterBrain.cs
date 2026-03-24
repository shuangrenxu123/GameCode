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

        bool isUIInputEnabled;
        bool actionsInitialized;
        bool aiReferenceWarningShown;
        bool networkReferenceWarningShown;

        public bool IsAI => isAI;

        public CharacterActions CharacterActions => characterActions;
        public CharacterUIActions CharacterUIActions => characterUIActions;

        public void UpdateBrainValues(float dt)
        {
            AdvanceActions(dt);
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

        protected virtual void Update()
        {
            AdvanceActions(Time.deltaTime);
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
