using CharacterController.Camera;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera.SceneTest
{
    /// <summary>
    /// CameraSmoothMoveEffect使用示例
    /// </summary>
    public class SmoothMoveTest : MonoBehaviour
    {
        [SerializeField, LabelText("起始位置")]
        private Transform _startPosition;

        [SerializeField, LabelText("结束位置")]
        private Transform _endPosition;

        [SerializeField, LabelText("移动时间")]
        private float _moveDuration = 2f;

        void OnTriggerEnter(Collider other)
        {
            var player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                var smoothMoveEffect = player.camera3D.effectManager.GetEffect<CameraSmoothMoveEffect>();
                smoothMoveEffect.Activate();
                smoothMoveEffect.StartSmoothMove(_startPosition.position, _endPosition.position, _moveDuration);
            }
        }
        void OnTriggerExit(Collider other)
        {
            var player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                var smoothMoveEffect = player.camera3D.effectManager.GetEffect<CameraSmoothMoveEffect>();
                smoothMoveEffect.Deactivate();
            }
        }
    }
}