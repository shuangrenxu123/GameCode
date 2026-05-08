using UnityEngine;
using System.Collections.Generic;

public class PackageLocalData
{
    private static PackageLocalData _instance;

    public static PackageLocalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
            }
            return _instance;
        }
    }

    //List,用于缓存当前所有物品的动态信息
    public List<PackageLocalItem> items;

    //储存方法和读取方法，作用是将文件转换为字符串，再将字符串转换为类
    public void SavePackage()
    {
        string inventoryJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageLocalData", inventoryJson);
        PlayerPrefs.Save();
    }

    public List<PackageLocalItem> LoadPackage()
    {
        //先判断一下缓存的数据是否存在
        //如果存在，说明之前已经读取过文本信息了，返回items
        //否则就要去本地的文件中读取
        if (items != null)
        {
            return items;
        }
        if (PlayerPrefs.HasKey("PackageLocalData"))
        {
            //PlayerPrefs把把本地文件读取到内存中，使之成为字符串
            //再使用JsonUtility来反序列化PackageLocalData
            string inventoryJson = PlayerPrefs.GetString("PackageLocalData");
            PackageLocalData packageLocalData = JsonUtility.FromJson<PackageLocalData>(inventoryJson);
            items = packageLocalData.items;
            return items;
        }
        else
        {
            items = new List<PackageLocalItem>();
            return items;
        }
    }
}


[System.Serializable]
//动态数据的参数
public class PackageLocalItem
{
    public int id;
    public int num;

    //重写ToString，方便后续打印和调试
    public override string ToString()
    {
        return string.Format("[id]:{0} [num]:{1}", id, num);
    }
}