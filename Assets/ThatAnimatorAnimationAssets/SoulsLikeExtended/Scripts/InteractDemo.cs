using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDemo : MonoBehaviour
{
    Animator _character;
    public Animator _object;
    Vector3 _intpos;

    private void Start()
    {
        _character = GetComponent<Animator>();
        _intpos = transform.position;
    }
    public void Interact()
    {
        transform.position = _intpos;
        _character.SetTrigger("interact");
        _object.SetTrigger("interact");
    }
}
