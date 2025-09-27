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
        [LabelText("锁定检测距离")]
        float lockDistance = 20f;
        [SerializeField]
        [LabelText("最大锁定敌人距离")]
        float lockEnemyMaxDistance = 30f;
        [SerializeField]
        [LabelText("锁定相机移动速度")]
        float lockEnemyCameraMoveSpeed = 10f;
        [SerializeField]
        [LabelText("锁定敌人标签")]
        string lockEnemyTag = "Enemy";

        [SerializeField]
        [LabelText("初始俯仰角度")]
        float initialPitch = 45f;

        [LabelText("Pitch旋转速度")]
        public float pitchSpeed = 180f;

        [LabelText("最大俯仰角度")]
        public float maxPitchAngle = 80f;

        [LabelText("最小俯仰角度")]
        public float minPitchAngle = 80f;

        [Min(0f)]
        [SerializeField]
        [LabelText("目标距离")]
        float distanceToTarget = 5f;

        [Min(0f)]
        [LabelText("缩放输入输出速度")]
        public float zoomInOutSpeed = 40f;

        [Min(0f)]
        [LabelText("缩放插值速度")]
        public float zoomInOutLerpSpeed = 5f;

        [Min(0f)]
        [LabelText("最小缩放距离")]
        public float minZoom = 2f;

        [Min(0.001f)]
        [LabelText("最大缩放距离")]
        public float maxZoom = 12f;

        [LabelText("碰撞影响缩放")]
        public bool collisionAffectsZoom = true; // 启用碰撞影响缩放
        [LabelText("碰撞检测半径")]
        public float detectionRadius = 0.3f; // 与CameraCollisionEffect保持一致
        [LabelText("碰撞检测层级遮罩")]
        public LayerMask layerMask = -1; // 检测所有层级

        CharacterActor characterActor = null;

        void OnValidate()
        {
            initialPitch = Mathf.Clamp(initialPitch, -minPitchAngle, maxPitchAngle);
        }

        void Awake()
        {
            Initialize(targetTransform);
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

            var followEffect = new CameraFollowEffect();
            followEffect.Priority = 25f;
            // 激活并设置参数
            followEffect.Activate();
            followEffect.SetParameters(heightLerpSpeed, characterActor, offsetFromHead);
            effectManager.AddEffect(followEffect);

            // 配置旋转效果 (高优先级，控制最终朝向和位置)

            var rotationEffect = new CameraRotationEffect();
            rotationEffect.Priority = 100f; // 提高优先级
            effectManager.AddEffect(rotationEffect);
            // 激活并设置参数
            rotationEffect.Activate();
            rotationEffect.SetParameters(characterActor, yawSpeed, pitchSpeed, maxPitchAngle, minPitchAngle);

            var zoomEffect = new CameraZoomEffect();

            zoomEffect.Priority = 60f;
            effectManager.AddEffect(zoomEffect);
            zoomEffect.Activate();
            zoomEffect.SetParameters(zoomInOutSpeed, zoomInOutLerpSpeed, minZoom, maxZoom, distanceToTarget);

            var collisionEffect = new CameraCollisionEffect();
            collisionEffect.Priority = 30f;
            effectManager.AddEffect(collisionEffect);
            collisionEffect.Activate();
            collisionEffect.SetParameters(detectionRadius, layerMask, collisionAffectsZoom);

            // 设置碰撞检测与缩放效果的联动
            if (collisionAffectsZoom)
            {
                var collisionEffectRef = effectManager.GetEffect<CameraCollisionEffect>();
                var zoomEffectRef = effectManager.GetEffect<CameraZoomEffect>();
                if (collisionEffectRef != null && zoomEffectRef != null)
                {
                    collisionEffectRef.SetZoomEffect(zoomEffectRef);
                }
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
            float dt = Time.fixedDeltaTime;
            UpdateCameraWithEffects(dt);

        }

        /// <summary>
        /// 使用CameraEffect系统更新相机
        /// </summary>
        private void UpdateCameraWithEffects(float dt)
        {
            // 更新效果状态
            effectManager.UpdateEffects(dt);

            // 获取所有活跃效果
            var activeEffects = new List<ICameraEffect>(effectManager.ActiveEffects);

            // 创建基础输入（使用角色位置作为基准）
            var baseInput = new CameraEffectInput
            {
                basePosition = targetTransform.position,
                baseRotation = transform.rotation,
                baseFieldOfView = GetComponent<UnityEngine.Camera>()?.fieldOfView ?? 60f,
                targetTransform = targetTransform,
                cameraTransform = transform,
                activeEffects = activeEffects
            };

            // 第一步：计算朝向（基于角色位置）
            Quaternion finalRotation = CalculateRotation(baseInput);

            // 第二步：计算位置（基于朝向）
            Vector3 finalPosition = CalculatePosition(baseInput, finalRotation);

            // 一次性应用结果
            transform.position = finalPosition;
            transform.rotation = finalRotation;
        }

        /// <summary>
        /// 计算朝向（基于角色位置，不依赖相机当前位置）
        /// </summary>
        private Quaternion CalculateRotation(CameraEffectInput baseInput)
        {
            var rotationEffect = effectManager.GetEffect<CameraRotationEffect>();
            if (rotationEffect == null || !rotationEffect.IsActive)
            {
                return baseInput.baseRotation;
            }

            // 使用角色位置计算朝向，避免循环依赖
            var rotationInput = new CameraEffectInput
            {
                basePosition = baseInput.targetTransform.position, // 使用角色位置
                baseRotation = baseInput.baseRotation,
                baseFieldOfView = baseInput.baseFieldOfView,
                targetTransform = baseInput.targetTransform,
                cameraTransform = baseInput.cameraTransform,
                activeEffects = baseInput.activeEffects
            };

            var rotationResult = rotationEffect.ModifyCamera(rotationInput);
            return rotationResult.overrideRotation ? rotationResult.modifiedRotation : baseInput.baseRotation;
        }

        /// <summary>
        /// 计算位置（基于朝向）
        /// </summary>
        private Vector3 CalculatePosition(CameraEffectInput baseInput, Quaternion rotation)
        {
            var followEffect = effectManager.GetEffect<CameraFollowEffect>();
            if (followEffect == null || !followEffect.IsActive)
            {
                return baseInput.basePosition;
            }

            // 计算跟随位置
            var followInput = new CameraEffectInput
            {
                basePosition = baseInput.targetTransform.position, // 使用角色位置
                baseRotation = rotation, // 使用计算出的朝向
                baseFieldOfView = baseInput.baseFieldOfView,
                targetTransform = baseInput.targetTransform,
                cameraTransform = baseInput.cameraTransform,
                activeEffects = baseInput.activeEffects
            };

            var followResult = followEffect.ModifyCamera(followInput);
            Vector3 position = followResult.overridePosition ? followResult.modifiedPosition : baseInput.targetTransform.position;

            // 应用碰撞检测
            var collisionEffect = effectManager.GetEffect<CameraCollisionEffect>();
            if (collisionEffect != null && collisionEffect.IsActive)
            {
                var collisionInput = new CameraEffectInput
                {
                    basePosition = position,
                    baseRotation = rotation,
                    baseFieldOfView = baseInput.baseFieldOfView,
                    targetTransform = baseInput.targetTransform,
                    cameraTransform = baseInput.cameraTransform,
                    activeEffects = baseInput.activeEffects
                };

                var collisionResult = collisionEffect.ModifyCamera(collisionInput);
                if (collisionResult.overridePosition)
                {
                    position = collisionResult.modifiedPosition;
                }
            }

            return position;
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