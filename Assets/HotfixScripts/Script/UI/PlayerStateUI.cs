using System.Collections.Generic;
using UIWindow;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : UIWindowBase
{
    private Player player;
    [SerializeField]
    private Image hp;
    [SerializeField]
    private GameObject buffGameObejct;

    [SerializeField] private Image leftSlot;
    [SerializeField] private Image rightSlot;
    [SerializeField] private Image magicSlot;
    [SerializeField] private Image itemSlot;

    private Dictionary<string, GameObject> buffs;
    #region interface
    public override void OnCreate()
    {
        base.OnCreate();
        leftSlot.gameObject.SetActive(false);
        rightSlot.gameObject.SetActive(false);
        magicSlot.gameObject.SetActive(false);
        itemSlot.gameObject.SetActive(false);

        player = FindObjectOfType<Player>();
        player.Inventory.OnRightWeaponLoad += OnLoadRightWeapon;
        player.Inventory.OnLeftWeaponLoad += OnLoadLeftWeapon;
        player.CombatEntity.hp.OnHPChange += OnHPChange;
    }


    #endregion
    #region buff
    public void AddBuff(BuffBase buff)
    {
        var go = new GameObject(buff.data.name);
        var image = go.AddComponent<Image>();
        go.transform.SetParent(buffGameObejct.transform, false);
        image.sprite = buff.data.icon;
        buffs.Add(buff.data.name, go);
    }
    public void RemoveBuff(BuffBase buff)
    {
        var go = buffs[buff.data.name];
        buffs.Remove(buff.data.name);
        GameObject.Destroy(go);
    }
    #endregion
    #region Item
    private void OnLoadLeftWeapon(WeaponItemData data)
    {
        leftSlot.gameObject.SetActive(true);
        leftSlot.sprite = data.Icon;
    }

    private void OnLoadRightWeapon(WeaponItemData data)
    {
        rightSlot.gameObject.SetActive(true);
        rightSlot.sprite = data.Icon;
    }
    #endregion
    #region HP

    private void OnHPChange(int arg1, int arg2)
    {
        hp.fillAmount = (float)arg1 / arg2;
    }
    #endregion
}
