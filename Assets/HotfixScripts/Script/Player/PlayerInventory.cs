using System;
using System.Collections.Generic;
using GameSave;

/// <summary>
/// 管理玩家背包物品与当前装备。
/// </summary>
public sealed class PlayerInventory
{
    readonly PlayerSaveData playerData;
    readonly Dictionary<Guid, ItemData> itemDefinitions = new();

    public event Action<ItemData, int> OnItemAdd;
    public event Action<ItemData, int> OnItemRemove;
    public event Action<EquipmentSlot, ItemData> OnEquipmentChanged;
    public event Action OnInventoryChanged;

    public IReadOnlyList<InventoryItemSaveData> Items => playerData.inventory;
    public IReadOnlyList<EquippedItemSaveData> Equipment => playerData.equipment;

    public PlayerInventory(PlayerSaveData playerData)
    {
        this.playerData = playerData ?? throw new ArgumentNullException(nameof(playerData));
        this.playerData.EnsureInitialized();
        RemoveInvalidEquipmentRecords();
    }

    public Guid AddItem(ItemData item, int count = 1)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "添加数量必须大于 0");
        }

        Guid firstInstanceId = AddItem(item.id, count, IsStackable(item));
        RegisterDefinitions(item);
        OnItemAdd?.Invoke(item, count);
        return firstInstanceId;
    }

    public Guid AddItem(int itemId, int count = 1, bool stackable = true)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "添加数量必须大于 0");
        }

        Guid firstInstanceId = Guid.Empty;
        if (stackable)
        {
            int itemIndex = FindItemIndex(itemId);
            if (itemIndex >= 0)
            {
                InventoryItemSaveData savedItem = playerData.inventory[itemIndex];
                savedItem.count += count;
                playerData.inventory[itemIndex] = savedItem;
                firstInstanceId = ParseInstanceId(savedItem.instanceId);
            }
            else
            {
                firstInstanceId = AddItemInstance(itemId, count);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Guid instanceId = AddItemInstance(itemId, 1);
                if (firstInstanceId == Guid.Empty)
                {
                    firstInstanceId = instanceId;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return firstInstanceId;
    }

    public bool DropItem(Guid instanceId, int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "丢弃数量必须大于 0");
        }

        int itemIndex = FindItemIndex(instanceId);
        if (itemIndex < 0)
        {
            return false;
        }

        InventoryItemSaveData savedItem = playerData.inventory[itemIndex];
        ItemData itemDefinition = GetItemDefinition(instanceId);
        int removedCount = Math.Min(count, savedItem.count);
        if (removedCount < savedItem.count)
        {
            savedItem.count -= removedCount;
            playerData.inventory[itemIndex] = savedItem;
        }
        else
        {
            UnequipItemInstance(instanceId);
            playerData.inventory.RemoveAt(itemIndex);
            itemDefinitions.Remove(instanceId);
        }

        OnItemRemove?.Invoke(itemDefinition, removedCount);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int DropItem(int itemId, int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "丢弃数量必须大于 0");
        }

        int remaining = count;
        for (int i = playerData.inventory.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventoryItemSaveData savedItem = playerData.inventory[i];
            if (savedItem.itemId != itemId)
            {
                continue;
            }

            Guid instanceId = ParseInstanceId(savedItem.instanceId);
            int removedCount = Math.Min(remaining, savedItem.count);
            DropItem(instanceId, removedCount);
            remaining -= removedCount;
        }

        return count - remaining;
    }

    public bool EquipItem(Guid instanceId, EquipmentSlot slot)
    {
        int itemIndex = FindItemIndex(instanceId);
        if (itemIndex < 0)
        {
            return false;
        }

        ItemData itemDefinition = GetItemDefinition(instanceId);
        if (itemDefinition != null && itemDefinition.Type != ItemType.Equip && itemDefinition.Type != ItemType.Weapon)
        {
            return false;
        }

        UnequipItemInstance(instanceId);
        int equipmentIndex = FindEquipmentIndex(slot);
        EquippedItemSaveData equippedItem = new()
        {
            slot = slot,
            itemInstanceId = instanceId.ToString("N")
        };

        if (equipmentIndex >= 0)
        {
            playerData.equipment[equipmentIndex] = equippedItem;
        }
        else
        {
            playerData.equipment.Add(equippedItem);
        }

        OnEquipmentChanged?.Invoke(slot, itemDefinition);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool UnequipItem(EquipmentSlot slot)
    {
        int equipmentIndex = FindEquipmentIndex(slot);
        if (equipmentIndex < 0)
        {
            return false;
        }

        playerData.equipment.RemoveAt(equipmentIndex);
        OnEquipmentChanged?.Invoke(slot, null);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool TryGetItem(Guid instanceId, out InventoryItemSaveData item, out ItemData itemDefinition)
    {
        int itemIndex = FindItemIndex(instanceId);
        if (itemIndex < 0)
        {
            item = default;
            itemDefinition = null;
            return false;
        }

        item = playerData.inventory[itemIndex];
        itemDefinition = GetItemDefinition(instanceId);
        return true;
    }

    public bool TryGetEquippedItem(
        EquipmentSlot slot,
        out InventoryItemSaveData item,
        out ItemData itemDefinition)
    {
        int equipmentIndex = FindEquipmentIndex(slot);
        if (equipmentIndex < 0 || !TryParseInstanceId(playerData.equipment[equipmentIndex].itemInstanceId, out Guid instanceId))
        {
            item = default;
            itemDefinition = null;
            return false;
        }

        return TryGetItem(instanceId, out item, out itemDefinition);
    }

    public void RegisterItemDefinition(Guid instanceId, ItemData item)
    {
        if (item == null || FindItemIndex(instanceId) < 0)
        {
            return;
        }

        itemDefinitions[instanceId] = item;
    }

    public void Clear()
    {
        if (playerData.inventory.Count == 0 && playerData.equipment.Count == 0)
        {
            return;
        }

        playerData.inventory.Clear();
        playerData.equipment.Clear();
        itemDefinitions.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void RefreshLoadedData()
    {
        itemDefinitions.Clear();
        playerData.EnsureInitialized();
        RemoveInvalidEquipmentRecords();
        OnInventoryChanged?.Invoke();
    }

    Guid AddItemInstance(int itemId, int count)
    {
        Guid instanceId = Guid.NewGuid();
        playerData.inventory.Add(new InventoryItemSaveData
        {
            instanceId = instanceId.ToString("N"),
            itemId = itemId,
            count = count
        });
        return instanceId;
    }

    void RegisterDefinitions(ItemData item)
    {
        for (int i = 0; i < playerData.inventory.Count; i++)
        {
            InventoryItemSaveData savedItem = playerData.inventory[i];
            if (savedItem.itemId == item.id && TryParseInstanceId(savedItem.instanceId, out Guid instanceId))
            {
                itemDefinitions[instanceId] = item;
            }
        }
    }

    void UnequipItemInstance(Guid instanceId)
    {
        string savedInstanceId = instanceId.ToString("N");
        for (int i = playerData.equipment.Count - 1; i >= 0; i--)
        {
            EquippedItemSaveData equippedItem = playerData.equipment[i];
            if (!string.Equals(equippedItem.itemInstanceId, savedInstanceId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            playerData.equipment.RemoveAt(i);
            OnEquipmentChanged?.Invoke(equippedItem.slot, null);
        }
    }

    void RemoveInvalidEquipmentRecords()
    {
        HashSet<string> inventoryIds = new(StringComparer.OrdinalIgnoreCase);
        for (int i = playerData.inventory.Count - 1; i >= 0; i--)
        {
            InventoryItemSaveData item = playerData.inventory[i];
            if (!TryParseInstanceId(item.instanceId, out Guid instanceId) || item.count <= 0)
            {
                playerData.inventory.RemoveAt(i);
                continue;
            }

            item.instanceId = instanceId.ToString("N");
            playerData.inventory[i] = item;
            inventoryIds.Add(item.instanceId);
        }

        HashSet<EquipmentSlot> occupiedSlots = new();
        for (int i = playerData.equipment.Count - 1; i >= 0; i--)
        {
            EquippedItemSaveData equippedItem = playerData.equipment[i];
            if (!TryParseInstanceId(equippedItem.itemInstanceId, out Guid instanceId)
                || !inventoryIds.Contains(instanceId.ToString("N"))
                || !occupiedSlots.Add(equippedItem.slot))
            {
                playerData.equipment.RemoveAt(i);
                continue;
            }

            equippedItem.itemInstanceId = instanceId.ToString("N");
            playerData.equipment[i] = equippedItem;
        }
    }

    int FindItemIndex(Guid instanceId)
    {
        string savedInstanceId = instanceId.ToString("N");
        for (int i = 0; i < playerData.inventory.Count; i++)
        {
            if (string.Equals(playerData.inventory[i].instanceId, savedInstanceId, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    int FindItemIndex(int itemId)
    {
        for (int i = 0; i < playerData.inventory.Count; i++)
        {
            if (playerData.inventory[i].itemId == itemId)
            {
                return i;
            }
        }

        return -1;
    }

    int FindEquipmentIndex(EquipmentSlot slot)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].slot == slot)
            {
                return i;
            }
        }

        return -1;
    }

    ItemData GetItemDefinition(Guid instanceId)
    {
        itemDefinitions.TryGetValue(instanceId, out ItemData itemDefinition);
        return itemDefinition;
    }

    static bool IsStackable(ItemData item)
    {
        return item.Type != ItemType.Equip && item.Type != ItemType.Weapon;
    }

    static Guid ParseInstanceId(string instanceId)
    {
        return TryParseInstanceId(instanceId, out Guid parsedId) ? parsedId : Guid.Empty;
    }

    static bool TryParseInstanceId(string instanceId, out Guid parsedId)
    {
        return Guid.TryParseExact(instanceId, "N", out parsedId) || Guid.TryParse(instanceId, out parsedId);
    }
}
