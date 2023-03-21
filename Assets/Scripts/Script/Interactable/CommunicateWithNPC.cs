using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicateWithNPC : Interactable
{
    public override void Interact(Player playerManager)
    {
        transform.LookAt(playerManager.transform.position);
        Debug.Log("与NPC产生交互");
    }
    public override void InteractUpdate(Player playerManager)
    {
        base.InteractUpdate(playerManager);
    }
    public override void InteractEnd(Player playerManager)
    {
        base.InteractEnd(playerManager);
    }
}