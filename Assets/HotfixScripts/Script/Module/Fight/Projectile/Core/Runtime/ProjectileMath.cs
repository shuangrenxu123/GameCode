using System;

namespace Fight.Projectile
{
    public struct ProjectileVector3 : IEquatable<ProjectileVector3>
    {
        public float X;
        public float Y;
        public float Z;

        public ProjectileVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static ProjectileVector3 Zero => new ProjectileVector3(0f, 0f, 0f);
        public static ProjectileVector3 One => new ProjectileVector3(1f, 1f, 1f);
        public static ProjectileVector3 Forward => new ProjectileVector3(0f, 0f, 1f);
        public static ProjectileVector3 Up => new ProjectileVector3(0f, 1f, 0f);
        public static ProjectileVector3 Right => new ProjectileVector3(1f, 0f, 0f);

        public float SqrMagnitude => X * X + Y * Y + Z * Z;
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public ProjectileVector3 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude > 0.0001f ? this / magnitude : Zero;
            }
        }

        public static ProjectileVector3 NormalizeOrForward(ProjectileVector3 value)
        {
            return value.SqrMagnitude > 0.0001f ? value.Normalized : Forward;
        }

        public static float Dot(ProjectileVector3 a, ProjectileVector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static ProjectileVector3 Cross(ProjectileVector3 a, ProjectileVector3 b)
        {
            return new ProjectileVector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static float Distance(ProjectileVector3 a, ProjectileVector3 b)
        {
            return (a - b).Magnitude;
        }

        public static ProjectileVector3 Lerp(ProjectileVector3 a, ProjectileVector3 b, float t)
        {
            t = ProjectileMath.Clamp01(t);
            return a + (b - a) * t;
        }

        public static ProjectileVector3 RotateYaw(ProjectileVector3 direction, float degrees)
        {
            ProjectileVector3 normalized = NormalizeOrForward(direction);
            float radians = degrees * ProjectileMath.Deg2Rad;
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);
            return new ProjectileVector3(
                normalized.X * cos + normalized.Z * sin,
                normalized.Y,
                -normalized.X * sin + normalized.Z * cos).Normalized;
        }

        public static ProjectileVector3 operator +(ProjectileVector3 a, ProjectileVector3 b)
        {
            return new ProjectileVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static ProjectileVector3 operator -(ProjectileVector3 a, ProjectileVector3 b)
        {
            return new ProjectileVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static ProjectileVector3 operator -(ProjectileVector3 value)
        {
            return new ProjectileVector3(-value.X, -value.Y, -value.Z);
        }

        public static ProjectileVector3 operator *(ProjectileVector3 value, float multiplier)
        {
            return new ProjectileVector3(value.X * multiplier, value.Y * multiplier, value.Z * multiplier);
        }

        public static ProjectileVector3 operator *(float multiplier, ProjectileVector3 value)
        {
            return value * multiplier;
        }

        public static ProjectileVector3 operator /(ProjectileVector3 value, float divisor)
        {
            return divisor > 0.0001f
                ? new ProjectileVector3(value.X / divisor, value.Y / divisor, value.Z / divisor)
                : Zero;
        }

        public bool Equals(ProjectileVector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectileVector3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
    }

    public static class ProjectileMath
    {
        public const float Deg2Rad = (float)(Math.PI / 180.0);

        public static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return value >= 1f ? 1f : value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        public static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }
    }
}
