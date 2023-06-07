using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CharacterLocomotionManager
{
    private void Awake()
    {
        entity = GetComponent<Enemy>();
        
    }
}
