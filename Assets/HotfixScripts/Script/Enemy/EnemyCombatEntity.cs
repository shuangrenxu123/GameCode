using Fight;

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
    }
    public override void TakeDamage(int damage, string animatorName)
    {
        hp.Minus(damage);
        if (animatorName == null || animatorName == "")
        {
        }
        else
        {
            //animator.PlayTargetAnimation(animatorName, false);
        }
        if (enemy.isDead)
        {
            return;
        }
        if (hp.Value <= 0)
        {
            //animator.PlayTargetAnimation("dead", false);
            enemy.isDead = true;
            return;
        }

    }
}

