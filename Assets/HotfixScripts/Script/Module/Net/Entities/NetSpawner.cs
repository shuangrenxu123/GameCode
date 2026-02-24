using UnityEngine;

namespace Game.Net.Entities
{
    public sealed class NetSpawner
    {
        private readonly GameObject prefab;

        public NetSpawner(GameObject prefab)
        {
            this.prefab = prefab;
        }

        public NetEntity Spawn(string netId)
        {
            if (prefab == null || string.IsNullOrEmpty(netId))
            {
                return null;
            }

            var go = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            var entity = go.GetComponent<NetEntity>();
            if (entity == null)
            {
                entity = go.AddComponent<NetEntity>();
            }

            entity.Initialize(netId, false);

            var netObj = go.GetComponent<NetObj>();
            if (netObj != null)
            {
                netObj.id = netId;
            }

            return entity;
        }
    }
}
