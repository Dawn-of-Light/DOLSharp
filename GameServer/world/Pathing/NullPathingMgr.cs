using System.Numerics;
using DOL.GS.Geometry;

namespace DOL.GS
{
	/// <summary>
	/// Always unavailable pathing mgr
	/// </summary>
	public class NullPathingMgr : IPathingMgr
	{
		public bool Init()
		{
			return true;
		}

		public void Stop()
		{
		}

        public (LinePath, PathingError) GetPathStraightAsync(Zone zone, Coordinate start, Coordinate end)
            => (new LinePath(), PathingError.NavmeshUnavailable);

        public Vector3? GetRandomPointAsync(Zone zone, Coordinate center, float radius)
            => null;

		public Vector3? GetClosestPointAsync(Zone zone, Vector3 position, float xRange = 256, float yRange = 256, float zRange = 256)
		{
			return position;
		}

		public bool HasNavmesh(Zone zone)
		{
			return false;
		}

        public bool IsAvailable => false;
	}
}
