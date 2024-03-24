using Fight;
using System;
using UnityEngine;

public class EnemyCombatEntity : CombatEntity
{
    Enemy enemy;
    public override void Awake()
    {
        base.Awake();
        enemy = GetComponent<Enemy>();
    }
    public override void Init(int h)
    {
        hp.Init(false);
        numberBox.Init();
        hp.SetMaxValue(h);
        hp.OnHPChange += HPChange;
    }

    private void HPChange(int arg1, int arg2)
    {
        Debug.Log(arg1);
        if (arg1 <= 0)
        {
            Debug.Log(name + "is Dead");
        }
    }
}

