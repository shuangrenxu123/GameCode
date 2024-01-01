using UnityEngine;

/// <summary>
/// 该类包含了角色一系列的碰撞状态（如在边缘上之类），且内部字段都是会逐帧更新，通过Actor来间接访问
/// </summary>
public struct CharacterCollisionInfo
{
    /// <summary>
    /// 与地面接触点
    /// </summary>
    public Vector3 groundContactPoint;
    /// <summary>
    /// 接触点法线
    /// </summary>
    public Vector3 groundContactNormal;
    /// <summary>
    /// 地面平均法线
    /// </summary>
    public Vector3 groundStableNormal;
    /// <summary>
    /// 地面坡度
    /// </summary>
    public float groundSlopeAngle;

    public bool headCollision;
    public Contact headContact;
    public float headAngle;

    //wall
    public bool wallCollision;
    public Contact wallContact;
    public float wallAngle;
    //Edge
    public bool isOnEdge;
    public float edgeAngle;

    public GameObject groundObject;
    public int groundLayer;
    public Collider groundCollider3D;
    public Collider2D groundCollider2D;

    public Rigidbody groundRigidbody3D;
    public Rigidbody2D groundRigidbody2D;

    public void Reset()
    {
        ResetGroundInfo();
        ResetWallInfo();
        ResetHeadInfo();
    }
    public void ResetGroundInfo()
    {
        groundCollider2D = null;
        groundCollider3D = null;
        groundLayer = 0;
        groundObject = null;
        groundContactNormal = Vector3.up;
        groundStableNormal = Vector3.up;
        groundContactPoint = Vector3.zero;
        groundSlopeAngle = 0;
        isOnEdge = false;
        edgeAngle = 0f;
    }
    public void ResetWallInfo()
    {
        wallAngle = 0f;
        wallCollision = false;
        wallContact = new Contact();
    }
    public void ResetHeadInfo()
    {
        headAngle = 0f;
        headCollision = false;
        headContact = new Contact();
    }

    public void SetWallInfo(in Contact contact, CharacterActor characterActor)
    {
        wallCollision = true;
        wallAngle = Vector3.Angle(characterActor.Up, contact.normal);
        wallContact = contact;
    }
    public void SetGroundInfo(in CollisionInfo collisionInfo, CharacterActor characterActor)
    {
        //如果碰撞到了就会设置相关的碰撞信息
        if (collisionInfo.hitInfo.hit)
        {
            isOnEdge = collisionInfo.isAnEdge;
            edgeAngle = collisionInfo.edgeAngle;
            groundContactNormal = collisionInfo.contactSlopeAngle < 90f ? collisionInfo.hitInfo.normal : characterActor.Up;
            groundContactPoint = collisionInfo.hitInfo.point;
            groundSlopeAngle = Vector3.Angle(characterActor.Up, groundStableNormal);

            groundObject = collisionInfo.hitInfo.transform.gameObject;
            groundLayer = groundObject.layer;
            groundCollider2D = collisionInfo.hitInfo.collider2D;
            groundCollider3D = collisionInfo.hitInfo.collider3D;

            groundRigidbody2D = collisionInfo.hitInfo.rigidbody2D;
            groundRigidbody3D = collisionInfo.hitInfo.rigidbody3D;

            Vector3 pointVelocity = Vector3.zero;
            if (collisionInfo.hitInfo.rigidbody2D != null)
                pointVelocity = collisionInfo.hitInfo.rigidbody2D.GetPointVelocity(groundContactPoint);
            else if (collisionInfo.hitInfo.rigidbody3D != null)
            {
                pointVelocity = collisionInfo.hitInfo.rigidbody3D.GetPointVelocity(groundContactPoint);
            }
            Vector3 relativeVelocity = characterActor.Velocity - pointVelocity;

            Contact groundContact = new Contact(groundContactPoint, groundContactNormal, pointVelocity, relativeVelocity);
            characterActor.GroundContacts.Add(groundContact);
        }
        else
        {
            ResetGroundInfo();
        }
    }
    public void SetHeadInfo(in Contact contact, CharacterActor characterActor)
    {
        headCollision = true;
        headAngle = Vector3.Angle(characterActor.Up, headContact.normal);
        headContact = contact;
    }
}
