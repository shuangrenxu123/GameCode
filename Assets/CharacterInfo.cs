using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public TMP_Text rigiboydSpeed;
    public TMP_Text localSpeed;
    public TMP_Text PlanerSpeed;
    public CharacterActor actor;

    public void Update()
    {
        rigiboydSpeed.text = actor.Velocity.ToString();
        localSpeed.text = actor.LocalVelocity.ToString();
        PlanerSpeed.text = actor.PlanarVelocity.ToString();
    }
}
