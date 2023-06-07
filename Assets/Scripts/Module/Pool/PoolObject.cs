using UnityEngine;

public class PoolObject : MonoBehaviour
{
    float time = 0;
    int speed = 2;
    private void OnEnable()
    {
        time = 0;
    }
    void Update()
    {
        if (time < 2f)
        {
            transform.position += new Vector3(0, 1, 0) * speed * Time.deltaTime;
            time += Time.deltaTime;
        }
        else
        {
            PoolManager.Instance.ReturnObjectToPool("hppanel");
        }
    }
}
