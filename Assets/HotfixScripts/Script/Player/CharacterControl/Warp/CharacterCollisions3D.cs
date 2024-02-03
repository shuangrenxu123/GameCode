using UnityEngine;

public class CharacterCollisions3D : CharacterCollisions
{
    public override float ContactOffset => Physics.defaultContactOffset;

    public override float CollisionRadius => CharacterActor.BodySize.x / 2f;

    public override void Awake()
    {
        base.Awake();
        PhysicsComponent = gameObject.AddComponent<PhysicsComponent3D>();
    }
}

