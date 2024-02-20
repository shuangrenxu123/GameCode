using Fight;
using UnityEngine;
public class FlaskItem : ConsumableItem
{
    public FlaskItem(string name) : base(name)
    {
    }

    public override void Effect(CombatEntity me, Equipmanager equipmanager)
    {
        new RegenerationAction(me, new CombatEntity[] { me }).Apply(30);
        //equipmanager.LoadBothWeaponOnSlots();
        Object.Destroy(go);
    }
}
