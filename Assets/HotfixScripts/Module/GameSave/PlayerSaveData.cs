using System;
using System.Collections.Generic;
using Utf8Json;

namespace GameSave
{
    public enum EquipmentSlot
    {
        MainHand,
        OffHand,
        Head,
        Torso,
        Arms,
        Hands,
        Hips,
        Legs
    }

    [Serializable]
    public struct InventoryItemSaveData
    {
        public string instanceId;
        public int itemId;
        public int count;
    }

    [Serializable]
    public struct EquippedItemSaveData
    {
        public EquipmentSlot slot;
        public string itemInstanceId;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public string playerId;
        public List<InventoryItemSaveData> inventory = new();
        public List<EquippedItemSaveData> equipment = new();

        public void EnsureInitialized()
        {
            inventory ??= new List<InventoryItemSaveData>();
            equipment ??= new List<EquippedItemSaveData>();
        }

        public void CopyFrom(PlayerSaveData source)
        {
            if (source == null)
            {
                playerId = string.Empty;
                inventory = new List<InventoryItemSaveData>();
                equipment = new List<EquippedItemSaveData>();
                return;
            }

            source.EnsureInitialized();
            playerId = source.playerId;
            inventory = new List<InventoryItemSaveData>(source.inventory);
            equipment = new List<EquippedItemSaveData>(source.equipment);
        }
    }

    public sealed class PlayerSaveEntity : IGameSave
    {
        public DataType dataType => DataType.Character;
        public PlayerSaveData Data { get; } = new();

        public PlayerSaveEntity()
        {
            GameSaveManager.Instance.RegisterSaver(this);
        }

        public string SaveData()
        {
            Data.EnsureInitialized();
            return JsonSerializer.ToJsonString(Data);
        }

        public void LoadData(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Data.CopyFrom(null);
                return;
            }

            PlayerSaveData loadedData = JsonSerializer.Deserialize<PlayerSaveData>(json);
            Data.CopyFrom(loadedData);
            GameSaveManager.Instance.NotifyPlayerDataLoaded();
        }
    }
}
