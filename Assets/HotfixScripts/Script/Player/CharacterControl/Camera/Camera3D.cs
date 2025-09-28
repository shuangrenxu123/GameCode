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
        [Header("Camera Effect System")]
        [SerializeField]
        [LabelText("是否使用相机效果系统")]
        private bool useCameraEffectSystem = true;

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
        private CameraEffectManager effectManager;

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
            if (!useCameraEffectSystem) return;

            if (effectManager == null)
            {
                effectManager = new CameraEffectManager();
            }

            effectManager.Initialize(transform, targetTransform, GetComponent<UnityEngine.Camera>());

            if (useCameraEffectSystem)
            {
                SetupDefaultEffects();
            }
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

            var effects = GetComponents<ICameraEffect>();
            foreach (var effect in effects)
            {
                effectManager.AddEffect(effect);
                effect.Activate();
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
            if (useCameraEffectSystem && effectManager != null)
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


            // 按键触发震动
            if (Input.GetKeyDown(KeyCode.Y))
            {
                StartShake(2f, .5f, 10);
                Debug.Log($"启动相机震动: 持续时间={0.5f}s, 强度={.1f}, 频率={10}");
            }

            // 按键停止震动
            if (Input.GetKeyDown(KeyCode.U))
            {
                StopShake();
                Debug.Log("停止相机震动");
            }

            // 显示震动状态
            if (IsShaking())
            {
                float remainingTime = GetRemainingShakeTime();
                Debug.Log($"震动中... 剩余时间: {remainingTime:F2}秒");
            }
        }

        /// <summary>
        /// 启动相机震动效果
        /// </summary>
        /// <param name="duration">震动持续时间</param>
        /// <param name="intensity">震动强度</param>
        /// <param name="frequency">震动频率</param>
        public void StartShake(float duration, float intensity, float frequency = 10f)
        {
            var shakeEffect = effectManager?.GetEffect<CameraShakeEffect>();
            if (shakeEffect != null)
            {
                shakeEffect.StartShake(duration, intensity, frequency);
            }
        }

        /// <summary>
        /// 停止相机震动效果
        /// </summary>
        public void StopShake()
        {
            var shakeEffect = effectManager?.GetEffect<CameraShakeEffect>();
            if (shakeEffect != null)
            {
                shakeEffect.StopShake();
            }
        }

        /// <summary>
        /// 获取震动效果状态
        /// </summary>
        public bool IsShaking()
        {
            var shakeEffect = effectManager?.GetEffect<CameraShakeEffect>();
            return shakeEffect?.IsShaking() ?? false;
        }

        /// <summary>
        /// 获取剩余震动时间
        /// </summary>
        public float GetRemainingShakeTime()
        {
            var shakeEffect = effectManager?.GetEffect<CameraShakeEffect>();
            return shakeEffect?.GetRemainingShakeTime() ?? 0f;
        }
    }
}