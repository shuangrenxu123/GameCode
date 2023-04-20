using Fight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillExecuter : MonoBehaviour
{
    public CombatEntity InputEntity;
    public Vector3 InputPoint;
    public abstract void BeginExecute();
    public abstract void EndExecute();

}
