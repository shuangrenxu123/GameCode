using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Interactable
{
    Transform targetPosition;
    Transform dir;
    private void Start()
    {
        targetPosition = transform.GetChild(0);
        dir = transform.GetChild(1);
    }
    public override void Interact(Player playerManager)
    {

        Vector3 rotationDir = dir.transform.position - playerManager.transform.position;
        rotationDir.y = 0;
        rotationDir.Normalize();
        Quaternion tr = Quaternion.LookRotation(rotationDir);
        Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 300 * Time.deltaTime);
        playerManager.inputHandle.transform.rotation = targetRotation;
        playerManager.transform.position = targetPosition.position;
        playerManager.animatorHandle.anim.SetBool("ClimbLadder",true);
    }
}

