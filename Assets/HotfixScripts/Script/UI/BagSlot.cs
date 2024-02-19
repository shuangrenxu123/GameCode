using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour, IUIElement
{
    [SerializeField]
    public TMP_Text num;
    [SerializeField]
    public Image icon;

    public IUIElement Right { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Left { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Up { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Down { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    
}
