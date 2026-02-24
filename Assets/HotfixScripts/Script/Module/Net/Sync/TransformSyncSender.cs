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

        private float nextSendTime;

        private void OnEnable()
        {
            ResolveReferences();
            nextSendTime = Time.time;
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

            if (Time.time < nextSendTime)
            {
                return;
            }

            SendTransform();
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

        private void SendTransform()
        {
            var target = transform;
            var actions = characterBrain != null ? characterBrain.CharacterActions : null;
            stateSyncMgr.SendCharacterState(target.position, actions);
        }
    }
}
