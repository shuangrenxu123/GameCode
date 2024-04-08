using Fight;
using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyCombatEntity : CombatEntity
{
    [SerializeField]
    private GameObject bloodFx;
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
    protected override void ChooseWhichDirectionDamageCameFrom(float direction)
    {
        base.ChooseWhichDirectionDamageCameFrom(direction);
        Instantiate(bloodFx,transform.position +new Vector3(0,1,0),Quaternion.Euler(0,direction,0));
    }
}

