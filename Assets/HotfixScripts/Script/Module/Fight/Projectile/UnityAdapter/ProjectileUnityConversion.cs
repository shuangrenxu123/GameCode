using Fight.Projectile;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public static class ProjectileUnityConversion
    {
        public static ProjectileVector3 ToProjectile(this Vector3 value)
        {
            return new ProjectileVector3(value.x, value.y, value.z);
        }

        public static Vector3 ToUnity(this ProjectileVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Quaternion ToUnityRotation(this ProjectileVector3 forward)
        {
            Vector3 unityForward = forward.ToUnity();
            if (unityForward.sqrMagnitude <= 0.0001f)
            {
                unityForward = Vector3.forward;
            }

            return Quaternion.LookRotation(unityForward.normalized, Vector3.up);
        }
    }
}
