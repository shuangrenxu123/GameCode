using System;
using Character.Player;
using CharacterController;
using UnityEngine;

namespace Game.Net.Sync
{
    public sealed class TransformSyncSender : MonoBehaviour
    {
        [SerializeField]
        private StateSyncMgr stateSyncMgr;

        [SerializeField]
        private Player player;

        [SerializeField]
        private CharacterBrain characterBrain;

        [SerializeField, Min(0.02f)]
        private float sendInterval = 0.05f;

        [SerializeField]
        private bool sendImmediateOnAttackStarted = true;

        [SerializeField]
        private bool sendImmediateOnHeavyAttackStarted = true;

        private float nextSendTime;
        private int lastSendFrame = -1;

        private void OnEnable()
        {
            ResolveReferences();
            nextSendTime = Time.time;
            lastSendFrame = -1;
        }

        private void Update()
        {
            if (stateSyncMgr == null)
            {
                ResolveReferences();
                if (stateSyncMgr == null)
                {
                    return;
                }
            }

            var actions = characterBrain != null ? characterBrain.CharacterActions : null;
            bool immediateSend = ShouldSendImmediately(actions);
            bool intervalDue = Time.time >= nextSendTime;
            if (!immediateSend && !intervalDue)
            {
                return;
            }

            // 同一帧只发一次，避免“定时发送+立即发送”重入导致重复发包。
            if (lastSendFrame == Time.frameCount)
            {
                return;
            }

            SendTransform(actions);
            lastSendFrame = Time.frameCount;
            float interval = sendInterval > 0f ? sendInterval : 0.05f;
            nextSendTime = Time.time + interval;
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                player = Player.Instance;
            }

            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            if (player != null && string.IsNullOrEmpty(player.id))
            {
                player.id = Guid.NewGuid().ToString("N");
            }

            if (characterBrain == null)
            {
                if (player != null)
                {
                    characterBrain = player.brain;
                }
                if (characterBrain == null)
                {
                    characterBrain = GetComponentInChildren<CharacterBrain>();
                }
            }

            if (stateSyncMgr == null)
            {
                stateSyncMgr = FindObjectOfType<StateSyncMgr>();
            }
        }

        private bool ShouldSendImmediately(CharacterActions actions)
        {
            if (actions == null)
            {
                return false;
            }

            if (sendImmediateOnAttackStarted && actions.attack.Started)
            {
                return true;
            }

            if (sendImmediateOnHeavyAttackStarted && actions.heavyAttack.Started)
            {
                return true;
            }

            return false;
        }

        private void SendTransform(CharacterActions actions)
        {
            var target = transform;
            stateSyncMgr.SendCharacterState(target.position, actions);
        }
    }
}
