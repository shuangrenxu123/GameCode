using CharacterControllerStateMachine;
using UnityEngine;
using UnityEngine.Events;

namespace GameLogin.Interact
{
    public enum IntractableType
    {
        Door_Double = 1,
        Door_Up = 2,
        Door_Outward = 3,
        Chest = 4,
        Lever = 5,
        Item = 6
    }

    public class Intractable : MonoBehaviour
    {
        public IntractableType intractableType;
        public Transform reference;
        [SerializeField]
        bool useRootMotion = true;
        public bool UseRootMotion => useRootMotion;

        public UnityEvent onIntractableStart = new();

        public void Interactive()
        {
            onIntractableStart.Invoke();
        }

    }
}
