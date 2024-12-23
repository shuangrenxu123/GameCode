using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField]
    Transform start;
    [SerializeField]
    Transform end;
    [SerializeField]
    int limit = 4;
    void Start()
    {
        Vector3 pos = end.position - start.position;
        for (int i = 0; i < limit; i++)
        {
            var go = Instantiate(start.gameObject);

            go.transform.position = start.position + pos / 4 * i;

            go.transform.rotation = Quaternion.Lerp
                (start.rotation, end.rotation, (float)(i + 1) / (limit + 1));
        }
    }
}
