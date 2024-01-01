using UnityEngine;

public class OpenChest : Interactable
{
    public Transform targetPosition;
    public GameObject Soul;
    private void Start()
    {
        targetPosition = transform.GetChild(1).transform;
    }
    public override void Interact(Player playerManager)
    {
        playerManager.transform.position = targetPosition.position;
        Vector3 rotationDir = transform.position - playerManager.transform.position;
        rotationDir.y = 0;
        rotationDir.Normalize();
        Quaternion tr = Quaternion.LookRotation(rotationDir);
        Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 300 * Time.deltaTime);
        playerManager.inputHandle.transform.rotation = targetRotation;
    }

    public void GenerateItem()
    {
        //Instantiate(Soul,transform.position,Quaternion.identity);

    }
}
