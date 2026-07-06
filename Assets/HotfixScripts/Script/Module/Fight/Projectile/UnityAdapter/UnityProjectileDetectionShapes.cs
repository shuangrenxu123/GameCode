using Fight.Projectile;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class SphereCastProjectileDetectionShape : IProjectileDetectionShape
    {
        public readonly float Radius;

        public SphereCastProjectileDetectionShape(float radius)
        {
            Radius = ProjectileMath.Max(0f, radius);
        }
    }

    public sealed class OverlapSphereProjectileDetectionShape : IProjectileDetectionShape
    {
        public readonly float Radius;

        public OverlapSphereProjectileDetectionShape(float radius)
        {
            Radius = ProjectileMath.Max(0f, radius);
        }
    }

    public sealed class OverlapBoxProjectileDetectionShape : IProjectileDetectionShape
    {
        public readonly float Width;
        public readonly float Height;
        public readonly float Length;

        public OverlapBoxProjectileDetectionShape(float width, float height, float length)
        {
            Width = ProjectileMath.Max(0f, width);
            Height = ProjectileMath.Max(0f, height);
            Length = ProjectileMath.Max(0f, length);
        }
    }

    public sealed class ConeOverlapProjectileDetectionShape : IProjectileDetectionShape
    {
        public readonly float Radius;
        public readonly float Angle;

        public ConeOverlapProjectileDetectionShape(float radius, float angle)
        {
            Radius = ProjectileMath.Max(0f, radius);
            Angle = ProjectileMath.Clamp(angle, 0f, 360f);
        }
    }

    public sealed class RayFanProjectileDetectionShape : IProjectileDetectionShape
    {
        public readonly float Range;
        public readonly float Angle;
        public readonly int RayCount;

        public RayFanProjectileDetectionShape(float range, float angle, int rayCount)
        {
            Range = ProjectileMath.Max(0f, range);
            Angle = ProjectileMath.Clamp(angle, 0f, 360f);
            RayCount = ProjectileMath.Max(1, rayCount);
        }
    }
}
