using UnityEngine;

public class WeaponHolderSlot : HolderSlot
{
    public bool isLeftHandSlot;
    public bool isRightHandSlot;

    public void UnloadWeapon()
    {
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }
    }
    public void UnloadWeaponAndDestory()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
    }
    public void LoadModel(ItemData Item)
    {
        var weaponItem = Item;
        if (weaponItem == null)
        {
            UnloadWeapon();
            return;
        }
        if (currentData != null && currentData.id == Item.id)
        {
            currentModel.SetActive(true);
            return;
        }
        GameObject model = Instantiate(weaponItem.modelPrefab);
        if (model != null)
        {
            if (parentOverride != null)
            {
                model.transform.parent = parentOverride;
            }
            else
            {
                model.transform.parent = transform;
            }
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
        }
        currentModel = model;
        currentData = weaponItem;

    }
}
