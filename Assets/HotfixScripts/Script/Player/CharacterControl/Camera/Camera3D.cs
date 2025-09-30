using System.Collections;
using System.Collections.Generic;
using CharacterController.Camera;
using CharacterControllerStateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    [DefaultExecutionOrder(110)]
    public class Camera3D : MonoBehaviour
    {
        [SerializeField]
        [LabelText("角色大脑组件")]
        CharacterBrain characterBrain;

        [SerializeField]
        InputHandlerSettings inputHandlerSettings => characterBrain.CameraInputHandlerSettings;

        [SerializeField, ReadOnly]
        [LabelText("相机输入轴")]
        string axes = "Camera";

        [SerializeField, ReadOnly]
        [LabelText("缩放输入轴")]
        string zoomAxis = "Camera Zoom";

        [SerializeField]
        [LabelText("相机效果管理器")]
        public CameraEffectManager effectManager;

        [SerializeField]
        [LabelText("目标变换")]
        Transform targetTransform = null;

        [SerializeField]
        [LabelText("初始俯仰角度")]
        float initialPitch = 45f;

        CharacterActor characterActor = null;
        UnityEngine.Camera _camera;


        void Awake()
        {
            Initialize(targetTransform);
            _camera = GetComponent<UnityEngine.Camera>();
            InitializeCameraEffectManager();
        }

        /// <summary>
        /// 初始化相机效果管理器
        /// </summary>
        void InitializeCameraEffectManager()
        {
            if (effectManager == null)
            {
                effectManager = new CameraEffectManager();
            }

            SetupDefaultEffects();
        }

        /// <summary>
        /// 设置默认效果
        /// </summary>
        void SetupDefaultEffects()
        {
            if (effectManager == null)
            {
                return;
            }

            var effects = GetComponentsInChildren<ICameraEffect>();
            foreach (var effect in effects)
            {
                effectManager.AddEffect(effect);
            }
        }


        public bool Initialize(Transform targetTransform)
        {
            if (targetTransform == null)
                return false;

            characterActor = targetTransform.GetComponentInBranch<CharacterActor>();

            if (characterActor == null || !characterActor.isActiveAndEnabled)
            {
                Debug.Log("character actor is null");
                return false;
            }

            return true;
        }

        void OnEnable()
        {
            if (characterActor == null)
                return;

            characterActor.OnTeleport += OnTeleport;
        }

        void OnDisable()
        {
            if (characterActor == null)
                return;

            characterActor.OnTeleport -= OnTeleport;
        }

        void Update()
        {
            if (targetTransform == null)
            {
                this.enabled = false;
                return;
            }
            if (effectManager == null)
            {
                Debug.LogError("相机效果管理器为空");
                return;
            }

            UpdateInputValue();
            float dt = Time.deltaTime;
            UpdateCameraWithEffects(dt);

        }

        /// <summary>
        /// 使用CameraEffect系统更新相机
        /// </summary>
        private void UpdateCameraWithEffects(float dt)
        {
            // 创建初始上下文
            CameraEffectContext initialContext = new CameraEffectContext
            {
                targetCamera = _camera,
                targetTransform = targetTransform,
                basePosition = targetTransform.position,
                baseRotation = transform.rotation,
                baseFieldOfView = _camera.fieldOfView,
                deltaTime = dt,
                currentPosition = transform.position,
                currentRotation = transform.rotation,
                currentFieldOfView = _camera.fieldOfView,
                currentDistance = 5 // 设置初始距离
            };

            // 处理效果链（现在包含了所有Update逻辑）
            CameraEffectContext finalContext = effectManager.ProcessEffectChain(initialContext);

            // 应用最终结果 - 直接使用处理后的中间值
            transform.position = finalContext.currentPosition;
            transform.rotation = finalContext.currentRotation;
        }

        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        /// <summary>
        /// 更新输入值
        /// </summary>
        void UpdateInputValue()
        {
            Vector2 cameraAxes = inputHandlerSettings.InputHandler.GetVector2(axes);

            if (effectManager?.GetEffect<CameraRotationEffect>() != null)
            {
                effectManager.SetRotationInput(cameraAxes.x, -cameraAxes.y);
            }

            if (effectManager?.GetEffect<CameraZoomEffect>() != null)
            {
                effectManager.SetZoomInput(-inputHandlerSettings.InputHandler.GetFloat(zoomAxis));
            }
        }
    }
}