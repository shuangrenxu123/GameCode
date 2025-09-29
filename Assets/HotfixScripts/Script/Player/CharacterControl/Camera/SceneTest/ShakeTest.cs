using System.Threading.Tasks;
using CharacterController.Camera;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Camera.Test
{
    public class ShakeTest : MonoBehaviour
    {
        [SerializeField, LabelText("震动时长")]
        private float _shakeDuration = 1f;
        [SerializeField, LabelText("震动强度")]
        private float _shakeIntensity = 0.5f;

        [SerializeField, LabelText("延迟时间")]
        float _delayTime = 0;

        async void OnTriggerEnter(Collider other)
        {
            await UniTask.WaitForSeconds(_delayTime);

            var Player = other.gameObject.GetComponent<Player>();
            var shake = Player.camera3D.effectManager.GetEffect<CameraShakeEffect>();
            shake.Activate();
            shake.StartShake(_shakeDuration, _shakeIntensity);
        }

    }
}
