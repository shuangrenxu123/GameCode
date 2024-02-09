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
        SkinnedMeshRenderer sourceSkinMeshRender = sourceSkin.GetComponentInChildren<SkinnedMeshRenderer>();    //��ȡ��ǰҪ�滻��Ƥ��������Դ
        Transform[] bones = new Transform[sourceSkinMeshRender.bones.Length];

        for (int i = 0; i < sourceSkinMeshRender.bones.Length; i++)
        {
            bones[i] = FindChildByName(sourceSkinMeshRender.bones[i].name, transform);
        }
        //sourceSkinMeshRender.sharedMesh = targetSkinMeshRender.sharedMesh;
        //���ģ����ͬ�����Բ��滻mesh
        //targetSkinMeshRender.sharedMesh = sourceSkinMeshRender.sharedMesh;    //�滻Ƥ��
        //targetSkinMeshRender.bones = bones;    //�滻����
        //targetSkinMeshRender.bounds = sourceSkinMeshRender.bounds;
        //targetSkinMeshRender.rootBone = sourceSkinMeshRender.rootBone;
        //targetSkinMeshRender.sharedMaterials = sourceSkinMeshRender.sharedMaterials;  //�滻����
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
