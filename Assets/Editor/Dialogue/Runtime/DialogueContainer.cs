using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLInkData> nodeLinks = new List<NodeLInkData>();
    public List<NodeData> NodeDatas = new List<NodeData>();
}