using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public Transform sourceSkin;
    public Transform targetSkin;
    public Mesh targetmesh;
    SkinnedMeshRenderer targetSkinMeshRender;

    void Start()
    {
        targetSkinMeshRender = targetSkin.GetComponent<SkinnedMeshRenderer>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            changeSkinnedMesh();
        }
    }

    public void changeSkinnedMesh()
    {
        SkinnedMeshRenderer sourceSkinMeshRender = sourceSkin.GetComponentInChildren<SkinnedMeshRenderer>();    //获取当前要替换的皮肤，这是源
        Transform[] bones = new Transform[sourceSkinMeshRender.bones.Length];

        for (int i = 0; i < sourceSkinMeshRender.bones.Length; i++)
        {
            bones[i] = FindChildByName(sourceSkinMeshRender.bones[i].name, transform);
        }
        //sourceSkinMeshRender.sharedMesh = targetSkinMeshRender.sharedMesh;
        //如果模型相同，可以不替换mesh
        //targetSkinMeshRender.sharedMesh = sourceSkinMeshRender.sharedMesh;    //替换皮肤
        //targetSkinMeshRender.bones = bones;    //替换骨骼
        //targetSkinMeshRender.bounds = sourceSkinMeshRender.bounds;
        //targetSkinMeshRender.rootBone = sourceSkinMeshRender.rootBone;
        //targetSkinMeshRender.sharedMaterials = sourceSkinMeshRender.sharedMaterials;  //替换材质
        sourceSkinMeshRender.sharedMesh = targetmesh;
        sourceSkinMeshRender.bounds = targetmesh.bounds;
        //sourceSkin.gameObject.SetActive(false);
    }


    public static Transform FindChildByName(string ThisName, Transform ThisGObj)
    {
        Transform ReturnObj;
        if (ThisGObj.name == ThisName)
            return ThisGObj.transform;
        foreach (Transform child in ThisGObj)
        {
            ReturnObj = FindChildByName(ThisName, child);

            if (ReturnObj != null)
                return ReturnObj;
        }

        return null;
    }
}
