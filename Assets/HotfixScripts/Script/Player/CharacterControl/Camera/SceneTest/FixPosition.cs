using CharacterController.Camera;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Camera.Test
{
    public class FixPosition : MonoBehaviour
    {
        [SerializeField, LabelText("摄像机固定位置")]
        Transform fixPosition;

        [SerializeField, LabelText("是否平滑")]
        bool isSmooth = true;

        [SerializeField, LabelText("平滑时间")]
        float smoothTime = 0.3f;

        void OnTriggerEnter(Collider other)
        {
            var Player = other.gameObject.GetComponent<Player>();
            var effect = Player.camera3D.effectManager.GetEffect<CameraFixPositionEffect>();

            effect.ActivateWithSmoothMove(fixPosition.position, isSmooth ? smoothTime : 0);
            effect.Activate();

            var rotationEffect = Player.camera3D.effectManager.GetEffect<CameraRotationEffect>();
            rotationEffect.SetInputEnabled(false);
        }
        void OnTriggerExit(Collider other)
        {
            var Player = other.gameObject.GetComponent<Player>();
            var effect = Player.camera3D.effectManager.GetEffect<CameraFixPositionEffect>();
            effect.Deactivate();


            var rotationEffect = Player.camera3D.effectManager.GetEffect<CameraRotationEffect>();
            rotationEffect.SetInputEnabled(true);
        }
    }
}
