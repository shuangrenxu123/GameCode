using UnityEngine;
public delegate bool HitFilterDelegate(Transform hitTransform);
public delegate bool Hit2DFilterDelegate(ref RaycastHit2D hitInfo);
public delegate bool Hit3DFilterDelegate(ref RaycastHit hitInfo);
public delegate bool OverlapFilterDelegate2D(Collider2D collider);
public delegate bool OverlapFilterDelegate3D(Collider collider);
public static class PhysicsExtensions
{
    public enum ClosestHitResult
    {
        NoHit,
        Hit,
        Overlap
    }

    public static ClosestHitResult GetFurthestHit(this RaycastHit[] array, out RaycastHit hitInfo, int length, Hit3DFilterDelegate filter)
    {
        float closestDistance = 0f;
        int index = -1;
        hitInfo = new RaycastHit();

        if (length == 0)
            return ClosestHitResult.NoHit;

        for (int i = 0; i < length; i++)
        {
            var h = array[i];

            if (filter != null)
                if (!filter.Invoke(ref h))
                    continue;

            if (h.distance > closestDistance)
            {
                closestDistance = h.distance;
                index = i;
            }
        }

        bool hit = index != -1;
        if (hit)
        {
            hitInfo = array[index];
            return ClosestHitResult.Hit;
        }

        return ClosestHitResult.NoHit;
    }

    public static ClosestHitResult GetFurthestHit(this RaycastHit2D[] array, out RaycastHit2D hitInfo, int length, Hit2DFilterDelegate filter)
    {
        float closestDistance = 0f;
        int index = -1;
        hitInfo = new RaycastHit2D();

        if (length == 0)
            return ClosestHitResult.NoHit;

        for (int i = 0; i < length; i++)
        {
            var h = array[i];

            if (filter != null)
                if (!filter.Invoke(ref h))
                    continue;

            if (h.distance > closestDistance)
            {
                closestDistance = h.distance;
                index = i;
            }
        }

        bool hit = index != -1;
        if (hit)
        {
            hitInfo = array[index];
            return ClosestHitResult.Hit;
        }

        return ClosestHitResult.NoHit;
    }

    public static ClosestHitResult GetClosestHit(this RaycastHit[] array, out RaycastHit hitInfo, int length, Hit3DFilterDelegate filter)
    {
        float closestDistance = Mathf.Infinity;
        int index = -1;
        hitInfo = new RaycastHit();

        if (length == 0)
            return ClosestHitResult.NoHit;

        for (int i = 0; i < length; i++)
        {
            var h = array[i];

            if (filter != null)
                if (!filter.Invoke(ref h))
                    continue;

            if (h.distance < closestDistance)
            {
                closestDistance = h.distance;
                index = i;
            }
        }

        bool hit = index != -1;
        if (hit)
        {
            hitInfo = array[index];
            return ClosestHitResult.Hit;
        }

        return ClosestHitResult.NoHit;
    }

    public static ClosestHitResult GetClosestHit(this RaycastHit2D[] array, out RaycastHit2D hitInfo, int length, Hit2DFilterDelegate filter)
    {
        float closestDistance = Mathf.Infinity;
        int index = -1;
        hitInfo = new RaycastHit2D();

        if (length == 0)
            return ClosestHitResult.NoHit;

        for (int i = 0; i < length; i++)
        {
            var h = array[i];

            if (filter != null)
                if (!filter.Invoke(ref h))
                    continue;

            if (h.distance < closestDistance)
            {
                closestDistance = h.distance;
                index = i;
            }
        }

        bool hit = index != -1;
        if (hit)
        {
            hitInfo = array[index];
            return ClosestHitResult.Hit;
        }

        return ClosestHitResult.NoHit;
    }
}
