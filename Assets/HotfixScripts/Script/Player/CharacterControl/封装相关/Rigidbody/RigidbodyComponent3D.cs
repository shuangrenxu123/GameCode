using UnityEngine;

public class RigidbodyComponent3D : RigidbodyComponent
{
    new Rigidbody rigidbody = null;
    public override float Mass { get => rigidbody.mass; set => rigidbody.mass = value; }
    public override float LinerDrag { get => rigidbody.drag; set => rigidbody.drag = value; }
    public override float AngularDrag { get => rigidbody.angularDrag; set => rigidbody.angularDrag = value; }
    public override bool IsKinematic
    {
        get => rigidbody.isKinematic;
        set
        {
            bool previousIsKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = value;
            if (value)
            {
                ContinuousCollisionDetection = false;
            }
            else
            {
                ContinuousCollisionDetection = previousContinuousCollisionDetection;
            }
            if (!(previousIsKinematic & rigidbody.isKinematic))
            {
                OnBodyTypeChangeInternal();
            }
        }
    }
    public override bool UseGravity { get => rigidbody.useGravity; set => rigidbody.useGravity = value; }
    public override bool UseInterpolation { get => rigidbody.interpolation == RigidbodyInterpolation.Interpolate; set => rigidbody.interpolation = value ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None; }
    public override bool ContinuousCollisionDetection { get => rigidbody.collisionDetectionMode == CollisionDetectionMode.Continuous; set => rigidbody.collisionDetectionMode = value ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete; }
    public override RigidbodyConstraints Constraints { get => rigidbody.constraints; set => rigidbody.constraints = value; }
    public override Vector3 Position { get => rigidbody.position; set => rigidbody.position = value; }
    public override Quaternion Rotation { get => rigidbody.rotation; set => rigidbody.rotation = value; }
    public override Vector3 Velocity { get => rigidbody.velocity; set => rigidbody.velocity = value; }
    public override Vector3 AngularVelocity { get => rigidbody.angularVelocity; set => rigidbody.angularVelocity = value; }
    protected override bool IsUsingContinuousCollisionDetection => rigidbody.collisionDetectionMode > 0;
    public override void AddExplosionForceToRigidbody(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModeifier = 0)
    {
        rigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
    }

    public override void AddForceToRigidbody(Vector3 force, ForceMode forcemode = ForceMode.Force)
    {
        rigidbody.AddForce(force, forcemode);
    }
    public override Vector3 GetPointVelocity(Vector3 point)
    {
        return rigidbody.GetPointVelocity(point);
    }

    public override void Interpolate(Vector3 position)
    {
        rigidbody.MovePosition(position);
    }

    public override void Interpolate(Quaternion rotation)
    {
        rigidbody.MoveRotation(rotation);
    }

    public override HitInfo Sweep(Vector3 position, Vector3 direction, float distance)
    {
        Vector3 pos = position;
        Position = position;
        rigidbody.SweepTest(direction, out RaycastHit raycasthit, distance);
        Position = pos;
        return new HitInfo(ref raycasthit, direction);
    }
    protected override void Awake()
    {
        base.Awake();
        rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
        rigidbody.hideFlags = HideFlags.NotEditable;

        previousContinuousCollisionDetection = IsUsingContinuousCollisionDetection;
    }
}
