using System.Numerics;

namespace DOL.GS.Geometry
{
	public static class Vector3Exts
	{
		public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.X, v.Y);
		public static Vector3 To2D(this Vector3 v)  => new Vector3(v.X, v.Y, 0);

		public static Vector3 ToVector3(this IPoint3D pt) => new Vector3(pt.X, pt.Y, pt.Z);
	}
}