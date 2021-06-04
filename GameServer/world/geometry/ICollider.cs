using System.Numerics;

namespace DOL.GS.Geometry
{
	public struct RaycastStats
	{
		public long nbTests;
		public long nbNodeTests;
		public long nbFaceTests;
		public long nbAABBTests;
	}

	public interface ICollider
	{
		AABoundingBox Box { get; }
		float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance);
		float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance, ref RaycastStats stats);
		bool CollideWithAABB(AABoundingBox box);
	}
}
