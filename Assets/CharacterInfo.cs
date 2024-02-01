using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public TMP_Text rigiboydSpeed;
    public TMP_Text localSpeed;
    public TMP_Text PlanerSpeed;
    public TMP_Text state;
    public CharacterActor actor;
    public StateManger stateManger;
    public void Update()
    {
        rigiboydSpeed.text = actor.Velocity.ToString();
        localSpeed.text = actor.LocalVelocity.ToString();
        PlanerSpeed.text = actor.PlanarVelocity.ToString();
        state.text = stateManger.controller.CurrentState.name;
    }
}
