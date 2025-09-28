using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{

    public class CameraFixPositionEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.FixPosition;

        private float priority = 150f;
        public float Priority { get => priority; set => priority = value; }

        [SerializeField, ReadOnly]
        private Vector3 fixedPosition = Vector3.zero;

        private bool isActive = false;
        public bool IsActive => isActive;

        public bool isDefaultActive => false;

        public void SetFixedPosition(Vector3 position)
        {
            fixedPosition = position;
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive)
            {
                return context;
            }

            // 创建修改后的上下文
            CameraEffectContext modifiedContext = context;

            // 固定位置，但保持原有的旋转和视野角度
            modifiedContext.currentPosition = fixedPosition;

            // 不修改旋转
            modifiedContext.currentRotation = context.currentRotation;

            // 不修改视野角度
            modifiedContext.currentFieldOfView = context.currentFieldOfView;

            return modifiedContext;
        }
    }
}
