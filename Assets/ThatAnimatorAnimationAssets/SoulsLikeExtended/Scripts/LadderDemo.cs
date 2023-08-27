using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderDemo : MonoBehaviour
{
    Animator _character;
    bool _climbladder;
    bool _climbup;
    private void Start()
    {
        _character = GetComponent<Animator>();
    }

    public void ClimbLadder()
    {
        _climbladder = !_climbladder;
        _character.SetBool("Climb Ladder", _climbladder);
    }

    public void ClimbUp()
    {
        _climbup = !_climbup;
        _character.SetBool("Climb Ladder Up", _climbup);
    }
}
