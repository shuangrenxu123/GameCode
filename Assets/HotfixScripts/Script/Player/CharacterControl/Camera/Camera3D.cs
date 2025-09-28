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

        [SerializeField]
        [LabelText("相机输入轴")]
        string axes = "Camera";

        [SerializeField]
        [LabelText("缩放输入轴")]
        string zoomAxis = "Camera Zoom";

        [SerializeField]
        [LabelText("相机效果管理器")]
        public CameraEffectManager effectManager;

        [SerializeField]
        [LabelText("目标变换")]
        Transform targetTransform = null;

        [SerializeField]
        [LabelText("状态管理器")]
        StateManger stateManager;

        [SerializeField]
        [LabelText("头部偏移")]
        Vector3 offsetFromHead = Vector3.zero;

        [SerializeField]
        [LabelText("高度插值速度")]
        float heightLerpSpeed = 10f;

        [LabelText("Yaw旋转速度")]
        public float yawSpeed = 180f;

        [SerializeField]
        [LabelText("初始俯仰角度")]
        float initialPitch = 45f;

        [LabelText("Pitch旋转速度")]
        public float pitchSpeed = 180f;

        CharacterActor characterActor = null;
        UnityEngine.Camera camera;


        void Awake()
        {
            Initialize(targetTransform);
            camera = GetComponent<UnityEngine.Camera>();
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
        private void SetupDefaultEffects()
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

        void Start()
        {
            // 设置初始旋转，确保相机看向玩家
            if (effectManager != null)
            {
                // 使用CameraEffect系统设置初始旋转
                var rotationEffect = effectManager.GetEffect<CameraRotationEffect>();
                if (rotationEffect != null)
                {
                    // 设置初始朝向
                    Vector3 initialLookAtPoint = targetTransform.position + targetTransform.up * characterActor.BodySize.y * 0.5f;
                    Vector3 initialDirection = initialLookAtPoint - transform.position;
                    if (initialDirection.magnitude > 0.1f)
                    {
                        Quaternion initialRotation = Quaternion.LookRotation(initialDirection);
                        // 应用初始俯仰角
                        initialRotation = initialRotation * Quaternion.Euler(initialPitch, 0f, 0f);
                        transform.rotation = initialRotation;
                    }
                }
            }
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
                targetCamera = camera,
                targetTransform = targetTransform,
                basePosition = targetTransform.position,
                baseRotation = transform.rotation,
                baseFieldOfView = camera.fieldOfView,
                deltaTime = dt,
                currentPosition = transform.position,
                currentRotation = transform.rotation,
                currentFieldOfView = camera.fieldOfView,
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