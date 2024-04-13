using Animancer;
using Audio;
using Fight;
using UnityEngine;

public class EnemyCombatEntity : CombatEntity
{
    [SerializeField]
    AudioData hit;
    [SerializeField]
    private GameObject bloodFx;
    [SerializeField]
    AnimancerComponent animator;
    [SerializeField]
    CCAnimatorConfig animationConfig;
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
        if (arg1 <= 0)
        {

            Debug.Log(name + "is Dead");
        }
    }
    protected override void ChooseWhichDirectionDamageCameFrom(float direction)
    {

        string currentDamageAnimation = null;
        if (direction >= 145 && direction <= 180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }
        else if (direction <= -145 && direction >= -180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }
        else if (direction >= -45 && direction <= 45)
        {
            currentDamageAnimation = "Damage_Back_01";
        }
        else if (direction >= -144 && direction <= -45)
        {
            currentDamageAnimation = "Damage_Left_01";
        }
        else if (direction >= 45 && direction <= 144)
        {
            currentDamageAnimation = "Damage_Right_01";
        }
        animator.Play(animationConfig.clipAnimators[currentDamageAnimation]);
        Instantiate(bloodFx,transform.position +new Vector3(0,1,0),Quaternion.Euler(0,direction,0));
        AudioManager.Instance.PlayAudio(hit);
    }
}

