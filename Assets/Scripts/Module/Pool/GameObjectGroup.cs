using System.Collections.Generic;
using UnityEngine;

public class GameObjectGroup : MonoBehaviour
{
    /// <summary>
    ///未使用的资源队列 
    /// </summary>
    public Queue<PoolObject> pool;
    /// <summary>
    /// 使用了的队列
    /// </summary>
    public Queue<PoolObject> usePool;
    private GameObject Go;
    /// <summary>
    /// 用完后不销毁
    /// </summary>
    //public bool DontDestory;
    /// <summary>
    /// 总数
    /// </summary>
    private int capcity;
    public GameObjectGroup(string name, GameObject prefab, int size)
    {
        capcity = size;
        pool = new Queue<PoolObject>();
        Go = prefab;
        usePool = new Queue<PoolObject>();
        for (int i = 0; i < capcity; i++)
        {
            GameObject a = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            a.SetActive(false);
            PoolObject b = a.GetComponent<PoolObject>();
            pool.Enqueue(b);
        }
    }
    /// <summary>
    /// 取一个物体
    /// </summary>
    public GameObject GetGameobject(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            AddObject();
        }
        var temp = pool.Dequeue();
        usePool.Enqueue(temp);
        Debug.Log(pool.Count);
        temp.gameObject.transform.position = position;
        temp.gameObject.transform.rotation = rotation;
        temp.gameObject.SetActive(true);
        return temp.gameObject;
    }
    /// <summary>
    /// 回收物体
    /// </summary>
    public void Restore()
    {
        PoolObject c = usePool.Dequeue();
        Debug.Log("物体回收");
        c.gameObject.SetActive(false);
        pool.Enqueue(c);
    }
    /// <summary>
    /// 销毁对象池
    /// </summary>
    public void Destory()
    {

    }
    private void AddObject()
    {
        Debug.Log("添加总数量");
        for (int i = 0; i < 5; i++)
        {
            GameObject a = Instantiate(Go, Vector3.zero, Quaternion.identity);
            a.SetActive(false);
            PoolObject b = a.GetComponent<PoolObject>();
            pool.Enqueue(b);
        }
        capcity += 5;
    }
}
