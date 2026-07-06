namespace Fight.Projectile
{
    public interface IProjectileQuery
    {
        int Query(in ProjectileQueryRequest request, ProjectileRawHit[] results);
    }

    public readonly struct ProjectileQueryRequest
    {
        public readonly ProjectileDetectionSpec Detection;
        public readonly ProjectileTrace Trace;
        public readonly ProjectilePose Pose;
        public readonly int OwnerId;
        public readonly object OwnerObject;

        public ProjectileQueryRequest(
            in ProjectileDetectionSpec detection,
            in ProjectileTrace trace,
            in ProjectilePose pose,
            int ownerId,
            object ownerObject)
        {
            Detection = detection;
            Trace = trace;
            Pose = pose;
            OwnerId = ownerId;
            OwnerObject = ownerObject;
        }
    }

    public struct ProjectileRawHit
    {
        public ProjectileTargetKind TargetKind;
        public int TargetId;
        public ProjectileVector3 Point;
        public ProjectileVector3 Normal;
        public float Distance;
        public object UserData;
    }

    public struct ProjectileHit
    {
        public ProjectileTargetKind TargetKind;
        public int TargetId;
        public ProjectileVector3 Point;
        public ProjectileVector3 Normal;
        public float Distance;
        public object UserData;
    }

    public static class ProjectileHitAccumulator
    {
        public static int Accumulate(ProjectileRawHit[] rawHits, int rawCount, ProjectileHit[] results)
        {
            if (rawHits == null || results == null || rawCount <= 0)
            {
                return 0;
            }

            int count = 0;
            int maxRawCount = rawCount < rawHits.Length ? rawCount : rawHits.Length;
            for (int i = 0; i < maxRawCount; i++)
            {
                ProjectileRawHit rawHit = rawHits[i];
                if (rawHit.TargetKind == ProjectileTargetKind.None)
                {
                    continue;
                }

                int existingIndex = FindExisting(results, count, in rawHit);
                if (existingIndex >= 0)
                {
                    if (rawHit.Distance < results[existingIndex].Distance)
                    {
                        results[existingIndex] = Convert(in rawHit);
                    }

                    continue;
                }

                if (count >= results.Length)
                {
                    break;
                }

                results[count++] = Convert(in rawHit);
            }

            SortByDistance(results, count);
            return count;
        }

        static int FindExisting(ProjectileHit[] results, int count, in ProjectileRawHit rawHit)
        {
            if (rawHit.TargetKind == ProjectileTargetKind.Environment)
            {
                return -1;
            }

            for (int i = 0; i < count; i++)
            {
                if (results[i].TargetKind == rawHit.TargetKind && results[i].TargetId == rawHit.TargetId)
                {
                    return i;
                }
            }

            return -1;
        }

        static ProjectileHit Convert(in ProjectileRawHit rawHit)
        {
            return new ProjectileHit
            {
                TargetKind = rawHit.TargetKind,
                TargetId = rawHit.TargetId,
                Point = rawHit.Point,
                Normal = rawHit.Normal,
                Distance = rawHit.Distance,
                UserData = rawHit.UserData,
            };
        }

        static void SortByDistance(ProjectileHit[] results, int count)
        {
            for (int i = 1; i < count; i++)
            {
                ProjectileHit value = results[i];
                int index = i - 1;
                while (index >= 0 && results[index].Distance > value.Distance)
                {
                    results[index + 1] = results[index];
                    index--;
                }

                results[index + 1] = value;
            }
        }
    }
}
