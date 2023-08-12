using UnityEngine;

[CreateAssetMenu(menuName = ("buffData"))]
public class BuffDataBase : ScriptableObject
{
    public int id;
    public int Maxtime = 0;
    public string buffName;
    public string buffDescription;
    public BuffTag[] Tag;
    public BuffTag[] mutexTag;
    public Sprite icon;
    /// <summary>
    /// buff展现的特效
    /// </summary>
    public GameObject prefab;
}
