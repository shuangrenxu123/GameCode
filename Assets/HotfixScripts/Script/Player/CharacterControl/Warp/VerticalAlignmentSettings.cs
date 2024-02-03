using UnityEngine;

public class VerticalAlignmentSettings
{
    public Transform alignmentReference = null;

    public VerticalReferenceMode referenceMode = VerticalReferenceMode.Away;
    public Vector3 aligmentDIrection = Vector3.up;
    public enum VerticalReferenceMode
    {
        Towards,
        Away
    }
}