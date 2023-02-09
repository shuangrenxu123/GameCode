using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLInkData> nodeLinks = new List<NodeLInkData>();
    public List<DialogueNodeData> NodeDatas = new List<DialogueNodeData>();
}
