using System;
using System.Numerics;

namespace DOL.GS.Geometry
{
	public struct AABoundingBox : ICollider
	{
		public readonly Vector3 Min;
		public readonly Vector3 Max;
		public AABoundingBox Box => this;

		public AABoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance)
		{
			if ((origin.X >= Min.X && origin.X <= Max.X) &&
				(origin.Y >= Min.Y && origin.Y <= Max.Y) &&
				(origin.Z >= Min.Z && origin.Z <= Max.Z))
				return 0;

			var t1 = (Min - origin) / direction;
			var t2 = (Max - origin) / direction;
			var tMin = Math.Max(Math.Max(Math.Min(t1.X, t2.X), Math.Min(t1.Y, t2.Y)), Math.Min(t1.Z, t2.Z));
			var tMax = Math.Min(Math.Min(Math.Max(t1.X, t2.X), Math.Max(t1.Y, t2.Y)), Math.Max(t1.Z, t2.Z));

			// AABB is behind the origin
			if (tMax < 0)
				return maxDistance;

			// doesn't intersect
			if (tMin > tMax)
				return maxDistance;

			if (tMin < 0)
				return Math.Min(maxDistance, tMax);
			return Math.Min(maxDistance, tMin);
		}
		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance, ref RaycastStats stats)
		{
			stats.nbAABBTests += 1;
			return CollideWithRay(origin, direction, maxDistance);
		}

		public bool CollideWithAABB(AABoundingBox box)
		{
			return
				(Min.X <= box.Max.X && Max.X >= box.Min.X) &&
				(Min.Y <= box.Max.Y && Max.Y >= box.Min.Y) &&
				(Min.Z <= box.Max.Z && Max.Z >= box.Min.Z);
		}
	}
}
