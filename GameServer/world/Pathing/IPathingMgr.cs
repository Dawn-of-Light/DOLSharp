using System.Numerics;
using System.Threading.Tasks;
using DOL.GS.Geometry;

namespace DOL.GS
{
	public interface IPathingMgr
	{
		/// <summary>
		///   Initializes the PathingMgr  by loading all available navmeshes
		/// </summary>
		/// <returns></returns>
		bool Init();

		/// <summary>
		///   Stops the PathingMgr and releases all loaded navmeshes
		/// </summary>
		void Stop();

        (LinePath Path,PathingError Error) GetPathStraightAsync(Zone zone, Coordinate start, Coordinate end);

		Vector3? GetRandomPointAsync(Zone zone, Coordinate center, float radius);

		/// <summary>
		///   Returns the closest point on the navmesh, if available, or no point found.
		///   Returns the input position if no navmesh is available
		/// </summary>
		Vector3? GetClosestPointAsync(Zone zone, Vector3 position, float xRange = 256f, float yRange = 256f, float zRange = 256f);

		/// <summary>
		///   True if pathing is enabled for the specified zone
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		bool HasNavmesh(Zone zone);

		/// <summary>
		/// True if currently running & working
		/// </summary>
		bool IsAvailable { get; }
	}
}