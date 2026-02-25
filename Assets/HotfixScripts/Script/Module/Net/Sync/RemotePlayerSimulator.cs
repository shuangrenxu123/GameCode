using System;
using Network;
using Network.Tcp;
using PlayerInfo;
using UnityEngine;

namespace Game.Net.Sync
{
    public sealed class RemotePlayerSimulator : MonoBehaviour
    {
        [SerializeField]
        private StateSyncMgr stateSyncMgr;

        [SerializeField]
        private string simulatedId = "SimB";

        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private Vector3 spawnOffset = new Vector3(2f, 0f, 0f);

        [SerializeField]
        private bool applyLocal = true;

        [SerializeField]
        private float sendInterval = 0.1f;

        private bool active;
        private Vector3 simulatedPosition;
        private float nextSendTime;

        private void Update()
        {
            if (stateSyncMgr == null)
            {
                stateSyncMgr = FindObjectOfType<StateSyncMgr>();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ToggleSimulation();
            }

            if (!active || stateSyncMgr == null)
            {
                return;
            }

            Vector2 movement = ReadMovementInput();
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            simulatedPosition += new Vector3(movement.x, 0f, movement.y) * moveSpeed * Time.deltaTime;
            SendSimulatedState(movement);
        }

        private void ToggleSimulation()
        {
            active = !active;
            if (active)
            {
                simulatedPosition = transform.position + spawnOffset;
                nextSendTime = Time.unscaledTime;
                SendSimulatedState(Vector2.zero);
            }
        }

        private Vector2 ReadMovementInput()
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.Keypad6))
            {
                x += 1f;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Keypad4))
            {
                x -= 1f;
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Keypad8))
            {
                y += 1f;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Keypad2))
            {
                y -= 1f;
            }

            return new Vector2(x, y);
        }

        private void SendSimulatedState(Vector2 movement)
        {
            var state = new CharacterState
            {
                Position = NetWorkUtility.ToProtoBufV3(simulatedPosition),
                MovementX = movement.x,
                MovementY = movement.y,
                Jump = false,
                Run = false,
                Interact = false,
                Roll = false,
                Lock = false,
                Attack = false,
                HeavyAttack = false,
                Crouch = false,
                OpenUI = false,
                OpenConsoleUI = false,
            };

            if (applyLocal)
            {
                stateSyncMgr.SimulateCharacterState(simulatedId, state);
            }

            if (sendInterval > 0f && Time.unscaledTime < nextSendTime)
            {
                return;
            }

            nextSendTime = Time.unscaledTime + sendInterval;
            TcpClient client = stateSyncMgr.Client;
            if (client != null && client.state == ENetWorkState.Connected)
            {
                client.SendMessage(simulatedId, 1, state);
            }
        }
    }
}
