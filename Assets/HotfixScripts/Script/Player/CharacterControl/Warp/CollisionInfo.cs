using UnityEngine;

// <summary>
/// 此类包含从所有不同的物理查询（raycast、capsuleCast 等）收集的碰撞信息。这些信息非常重要,用于运动预测。
/// </summary>
public class CollisionInfo
{
    /// <summary>
    /// The physics query data.
    /// </summary>
    public HitInfo hitInfo;

    /// <summary>
    /// The available displacement obtained as the result of the collision test. By adding this vector to the character position, the result will represent the closest possible position to the hit surface.
    /// </summary>
    public Vector3 displacement;

    /// <summary>
    /// The angle between the contact normal and the character up vector.
    /// </summary>
    public float contactSlopeAngle;

    /// <summary>
    /// Flag that indicates if the character is standing on an edge or not.
    /// </summary>
    public bool isAnEdge;

    /// <summary>
    /// Flag that indicates if the character is standing on an step or not.
    /// </summary>
    public bool isAStep;

    /// <summary>
    /// Normal vector obtained from the edge detector upper ray.
    /// </summary>
    public Vector3 edgeUpperNormal;

    /// <summary>
    /// Normal vector obtained from the edge detector lower ray.
    /// </summary>
    public Vector3 edgeLowerNormal;

    /// <summary>
    /// Angle between the character up vector and the edge detector upper normal.
    /// </summary>
    public float edgeUpperSlopeAngle;

    /// <summary>
    /// Angle between the character up vector and the edge detector lower normal.
    /// </summary>
    public float edgeLowerSlopeAngle;

    /// <summary>
    /// Angle between the edge detector upper normal and the edge detector lower normal.
    /// </summary>
    public float edgeAngle;

    /// <summary>
    /// Resets all the fields to their default values.
    /// </summary>
    public void Reset()
    {
        this.hitInfo = default;
        this.displacement = default;
        this.contactSlopeAngle = default;
        this.edgeUpperNormal = default;
        this.edgeLowerNormal = default;
        this.edgeUpperSlopeAngle = default;
        this.edgeLowerSlopeAngle = default;
        this.edgeAngle = default;
        this.isAnEdge = default;
        this.isAStep = default;
    }

    /// <summary>
    /// Sets the fields values based on the character movement.
    /// </summary>
    public void SetData(in HitInfo hitInfo, Vector3 upDirection, Vector3 displacement)
    {
        this.hitInfo = hitInfo;
        this.displacement = displacement;
        this.contactSlopeAngle = Vector3.Angle(upDirection, hitInfo.normal);
    }

    /// <summary>
    /// Sets the fields values based on the character movement.
    /// </summary>
    public void SetData(
            in HitInfo hitInfo,
            Vector3 upDirection,
    Vector3 displacement,
    in HitInfo upperHitInfo,
            in HitInfo lowerHitInfo
    )
    {
        SetData(in hitInfo, upDirection, displacement);

        this.edgeUpperNormal = upperHitInfo.normal;
        this.edgeLowerNormal = lowerHitInfo.normal;

        this.edgeUpperSlopeAngle = Vector3.Angle(edgeUpperNormal, upDirection);
        this.edgeLowerSlopeAngle = Vector3.Angle(edgeLowerNormal, upDirection);

        this.edgeAngle = Vector3.Angle(edgeUpperNormal, edgeLowerNormal);

        this.isAnEdge = CustomUtilities.isBetween(edgeAngle, CharacterConstants.MinEdgeAngle, CharacterConstants.MaxEdgeAngle, true);
        this.isAStep = CustomUtilities.isBetween(edgeAngle, CharacterConstants.MinStepAngle, CharacterConstants.MaxStepAngle, true);
    }

}
