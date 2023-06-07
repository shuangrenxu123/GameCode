using Fight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumableItem 
{
    public ConsumableItemData data;
    public GameObject go;
    public int currentItemAmount = 10;
    public ConsumableItem(string name)
    {
        data = Resources.Load<ConsumableItemData>(name);
    }
    public virtual void AttemptToConsumeItem(AnimatorHandle animator,Equipmanager equipmanager)
    {
        if (currentItemAmount > 0)
        {
            animator.PlayTargetAnimation(data.consumeAnimation, data.isInteracting);
            go = Object.Instantiate(data.modelPrefab, equipmanager.rightSlot.transform);
            equipmanager.rightSlot.UnloadWeapon();
        }
        else
        {
            animator.PlayTargetAnimation(data.UsageFailedAnimation, true);
        }
    }
    public abstract void Effect(CombatEntity me,Equipmanager equipmanager);

 
}
