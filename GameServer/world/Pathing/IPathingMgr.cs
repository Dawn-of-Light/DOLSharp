using System.Numerics;
using System.Threading.Tasks;

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

		/// <summary>
		///   Returns a path that prevents collisions with the navmesh, but floats freely otherwise
		/// </summary>
		/// <param name="zone"></param>
		/// <param name="start">Start in GlobalXYZ</param>
		/// <param name="end">End in GlobalXYZ</param>
		/// <returns></returns>
		WrappedPathingResult GetPathStraightAsync(Zone zone, Vector3 start, Vector3 end);

		/// <summary>
		///   Returns a random point on the navmesh around the given position
		/// </summary>
		/// <param name="zone">Zone</param>
		/// <param name="position">Start in GlobalXYZ</param>
		/// <param name="radius">End in GlobalXYZ</param>
		/// <returns>null if no point found, Vector3 with point otherwise</returns>
		Vector3? GetRandomPointAsync(Zone zone, Vector3 position, float radius);

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