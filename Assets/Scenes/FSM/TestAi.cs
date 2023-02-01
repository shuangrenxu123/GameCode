using UnityEngine;

public class TestAi : MonoBehaviour
{
    public HealthPoint hp;
    public bool isDead = false;
    private void Start()
    {
        hp = new HealthPoint();
        hp.SetMaxValue(100);
    }
    public void kouxue(int num, out bool isdead)
    {
        hp.Minus(num);
        if (hp.Value <= 0)
        {
            isdead = true;
            isDead = true;
            Destroy(gameObject);
        }
        else
        {
            isdead = false;
        }
    }
}
