using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIElement
{
    public IUIElement Right { get; set; }
    public IUIElement Left { get; set; }
    public IUIElement Up { get; set; }
    public IUIElement Down { get; set; }
}
