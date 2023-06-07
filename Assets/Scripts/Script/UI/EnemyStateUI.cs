using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStateUI : StateUI
{
    new Camera camera;
    private void Awake()
    {
        camera = Camera.main;
        hp = GetUIGameObject("hp");
        buffGameObejct = GetUIGameObject("buff");
    }
    public override void Start()
    {
    }

    public override void Update()
    {
        transform.rotation = Quaternion.LookRotation(camera.transform.forward);
    }
}