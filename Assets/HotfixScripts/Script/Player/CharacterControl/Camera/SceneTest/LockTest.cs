using Character.Controller.MoveState;
using Character.Controller.State;
using CharacterController.Camera;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Camera.Test
{
    /// <summary>
    /// 相机锁定效果测试脚本
    /// 用于测试瞄准敌人功能的最简单实现
    /// </summary>
    public class LockTest : MonoBehaviour
    {
        [SerializeField, LabelText("锁定检测距离")]
        private float _lockDistance = 20f;

        [SerializeField, LabelText("最大锁定距离")]
        private float _maxLockDistance = 30f;

        [SerializeField, LabelText("相机转向速度")]
        private float _cameraMoveSpeed = 10f;

        [SerializeField, LabelText("可视角度范围")]
        private float _viewableAngle = 60f;

        [SerializeField, LabelText("敌人标签")]
        private string _enemyTag = "Enemy";

        [SerializeField, LabelText("延迟触发时间")]
        private float _delayTime = 0f;

        [SerializeField, LabelText("测试用敌人对象")]
        private Transform _testEnemyTarget;

        private void OnTriggerEnter(Collider other)
        {
            var Player = other.gameObject.GetComponent<Player>();
            if (Player != null)
            {
                StartLockTest(Player);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var Player = other.gameObject.GetComponent<Player>();
            if (Player != null)
            {
                var lockOnEffect = Player.camera3D.effectManager.GetEffect<CameraLockOnEffect>();
                if (lockOnEffect != null)
                {
                    lockOnEffect.Deactivate();
                }
                Player.StateManager.moveStateMachine.ChangeState(ECharacterMoveState.NormalMove);
            }
        }

        /// <summary>
        /// 开始锁定测试
        /// </summary>
        private void StartLockTest(Player player)
        {
            // 获取锁定效果
            var lockOnEffect = player.camera3D.effectManager.GetEffect<CameraLockOnEffect>();
            if (lockOnEffect != null)
            {
                // 配置锁定参数
                lockOnEffect.SetParameters(
                    lockDistance: _lockDistance,
                    lockEnemyMaxDistance: _maxLockDistance,
                    lockCameraMoveSpeed: _cameraMoveSpeed,
                    viewableAngle: _viewableAngle
                );

                // 如果指定了测试目标，直接锁定该目标
                if (_testEnemyTarget != null)
                {
                    lockOnEffect.SetLockTarget(_testEnemyTarget);
                }

                // 激活锁定效果
                lockOnEffect.Activate();

                player.StateManager.moveStateMachine
                    .database.SetData(CharacterLockOnMovementState.targetKey, _testEnemyTarget);

                player.StateManager.moveStateMachine
                    .ChangeState(ECharacterMoveState.LockOnMove);

                Debug.Log($"相机锁定效果已激活，锁定目标: {_testEnemyTarget?.name ?? "自动寻找敌人"}");
            }
            else
            {
                Debug.LogError("未找到CameraLockOnEffect，请确保相机系统已正确配置");
            }
        }


    }
}