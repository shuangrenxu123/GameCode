using System;
using System.Collections.Generic;

namespace Game.Net.Entities
{
    public sealed class NetEntityRegistry
    {
        private readonly Dictionary<string, NetEntity> entities = new Dictionary<string, NetEntity>(StringComparer.Ordinal);

        public bool TryGet(string netId, out NetEntity entity)
        {
            return entities.TryGetValue(netId, out entity);
        }

        public bool Contains(string netId)
        {
            return entities.ContainsKey(netId);
        }

        public void Register(NetEntity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.NetId))
            {
                return;
            }

            entities[entity.NetId] = entity;
        }

        public void Unregister(string netId)
        {
            if (string.IsNullOrEmpty(netId))
            {
                return;
            }

            entities.Remove(netId);
        }
    }
}
