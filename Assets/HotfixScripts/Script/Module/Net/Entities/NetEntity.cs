using UnityEngine;

namespace Game.Net.Entities
{
    public sealed class NetEntity : MonoBehaviour
    {
        [SerializeField]
        private string netId;

        [SerializeField]
        private bool isLocal;

        public string NetId => netId;
        public bool IsLocal => isLocal;

        public void Initialize(string id, bool local)
        {
            netId = id;
            isLocal = local;
        }
    }
}
