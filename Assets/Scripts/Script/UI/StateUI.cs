using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateUI : WindowRoot
{
    public GameObject hp;
    public GameObject buffGameObejct;
    private Dictionary<string, GameObject> buffs;
    private void Awake()
    {
        buffs = new Dictionary<string, GameObject>();
        hp = GetUIGameObject("hp");
        buffGameObejct = GetUIGameObject("buff");    
    }
    public override void Start()
    {
    }

    public override void Update()
    {
        
    }
    public void SetHPPercent(float v)
    {
        hp.transform.GetChild(0).GetComponent<Image>().fillAmount = v;
    }
    public void AddBuff(BuffBase buff)
    {
        var go = new GameObject(buff.data.name);
        var image = go.AddComponent<Image>();
        go.transform.SetParent(buffGameObejct.transform, false);
        image.sprite = buff.data.icon;
        buffs.Add(buff.data.name,go);
    }
    public void RemoveBuff(BuffBase buff ) {
        var go = buffs[buff.data.name];
        buffs.Remove(buff.data.name);
        Destroy(go);
    }
}
