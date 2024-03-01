using UnityEngine;

public class EnemyStateUI : StateUI
{
    new Camera camera;
    private void Awake()
    {
        camera = Camera.main;
        hp = GetUIGameObject("hp");
        buffGameObejct = GetUIGameObject("buff");
    }
    public  void Update()
    {
        transform.rotation = Quaternion.LookRotation(camera.transform.forward);
    }
}