using UnityEngine;

public class OpenTwoDoor : Interactable
{
    public Transform targetPosition;
    public Transform Lookdir;
    private void Start()
    {
        targetPosition = transform.GetChild(0).transform;
        Lookdir = transform.GetChild(1).transform;
    }
    public override void Interact(Player playerManager)
    {
        playerManager.transform.position = targetPosition.position;

        Vector3 rotationDir = Lookdir.transform.position - playerManager.transform.position;
        rotationDir.y = 0;
        rotationDir.Normalize();
        Quaternion tr = Quaternion.LookRotation(rotationDir);
        Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 300 * Time.deltaTime);
        //playerManager.inputHandle.transform.rotation = targetRotation;
        GetComponent<Animator>().SetBool("open", true);
        playerManager.animatorHandle.PlayTargetAnimation("OpenDoor_DoubleDoors", true);
    }
}
