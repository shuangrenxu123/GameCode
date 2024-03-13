using Fight;
using UnityEngine;
public class FlaskItem : ConsumableItem
{
    public FlaskItem(ConsumableItemData data) : base(data)
    {
    }

    public  void Effect(CombatEntity me, Equipmanager equipmanager)
    {
        //new RegenerationAction(me, new CombatEntity[] { me }).Apply(30);
        //Debug.Log(111);
        //equipmanager.LoadBothWeaponOnSlots();
        //Object.Destroy(go);
    }
}
