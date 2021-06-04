using System;
using System.Numerics;

namespace DOL.GS.Geometry
{
	public class Triangle : ICollider
	{
		public Vector3 A;
		public Vector3 edge1;
		public Vector3 edge2;

		public Vector3 B => A + edge1;
		public Vector3 C => A + edge2;
		public AABoundingBox Box => new AABoundingBox(Vector3.Min(Vector3.Min(A, B), C), Vector3.Max(Vector3.Max(A, B), C));

		public Triangle(Vector3[] vertices, int a, int b, int c)
		{
			var va = vertices[a];
			A = va;
			edge1 = vertices[b] - va;
			edge2 = vertices[c] - va;
		}

		public Triangle(Vector3 a, Vector3 b, Vector3 c)
		{
			A = a;
			edge1 = b - a;
			edge2 = c - a;
		}

		private const float epsilon = 0.000001f;
		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance)
		{
			// Möller–Trumbore algorithm
			// var edge1 = B - A;
			// var edge2 = C - A;
			var h = Vector3.Cross(direction, edge2);
			var a = Vector3.Dot(edge1, h);
			if (a > -epsilon && a < epsilon)
				return maxDistance;

			var f = 1.0f / a;
			var s = origin - A;
			var u = f * Vector3.Dot(s, h);
			if (u < 0 || u > 1)
				return maxDistance;
			var q = Vector3.Cross(s, edge1);
			var v = f * Vector3.Dot(direction, q);
			if (v < 0 || u + v > 1)
				return maxDistance;

			var t = f * Vector3.Dot(edge2, q);
			if (t < epsilon)
				return maxDistance;
			return Math.Min(maxDistance, t);
		}

		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance, ref RaycastStats stats)
		{
			stats.nbFaceTests += 1;
			return CollideWithRay(origin, direction, maxDistance);
		}

		public bool CollideWithAABB(AABoundingBox box)
		{
			var v0 = A;
			var v1 = B;
			var v2 = C;

			var min = Vector3.Min(Vector3.Min(v0, v1), v2);
			var max = Vector3.Max(Vector3.Max(v0, v1), v2);
			if (!box.CollideWithAABB(new AABoundingBox(min, max)))
				return false;

			// algorithm from https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/aabb-triangle.html

			// Convert AABB to center-extents form
			var e = box.Max - box.Min; // extents
			var c = e / 2 + box.Min; // center

			v0 -= c;
			v1 -= c;
			v2 -= c;

			var f0 = v1 - v0;
			var f1 = v2 - v1;
			var f2 = v0 - v2;

			var axXf0 = Vector3.Cross(Vector3.UnitX, f0);
			var axXf1 = Vector3.Cross(Vector3.UnitX, f1);
			var axXf2 = Vector3.Cross(Vector3.UnitX, f2);
			var axYf0 = Vector3.Cross(Vector3.UnitY, f0);
			var axYf1 = Vector3.Cross(Vector3.UnitY, f1);
			var axYf2 = Vector3.Cross(Vector3.UnitY, f2);
			var axZf0 = Vector3.Cross(Vector3.UnitZ, f0);
			var axZf1 = Vector3.Cross(Vector3.UnitZ, f1);
			var axZf2 = Vector3.Cross(Vector3.UnitZ, f2);

			var p0 = Vector3.Dot(v0, axXf0);
			var p1 = Vector3.Dot(v1, axXf0);
			var p2 = Vector3.Dot(v2, axXf0);
			var r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axXf0)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axXf0)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axXf0));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axXf1);
			p1 = Vector3.Dot(v1, axXf1);
			p2 = Vector3.Dot(v2, axXf1);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axXf1)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axXf1)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axXf1));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axXf2);
			p1 = Vector3.Dot(v1, axXf2);
			p2 = Vector3.Dot(v2, axXf2);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axXf2)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axXf2)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axXf2));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;

			p0 = Vector3.Dot(v0, axYf0);
			p1 = Vector3.Dot(v1, axYf0);
			p2 = Vector3.Dot(v2, axYf0);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axYf0)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axYf0)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axYf0));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axYf1);
			p1 = Vector3.Dot(v1, axYf1);
			p2 = Vector3.Dot(v2, axYf1);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axYf1)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axYf1)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axYf1));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axYf2);
			p1 = Vector3.Dot(v1, axYf2);
			p2 = Vector3.Dot(v2, axYf2);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axYf2)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axYf2)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axYf2));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;

			p0 = Vector3.Dot(v0, axZf0);
			p1 = Vector3.Dot(v1, axZf0);
			p2 = Vector3.Dot(v2, axZf0);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axZf0)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axZf0)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axZf0));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axZf1);
			p1 = Vector3.Dot(v1, axZf1);
			p2 = Vector3.Dot(v2, axZf1);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axZf1)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axZf1)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axZf1));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, axZf2);
			p1 = Vector3.Dot(v1, axZf2);
			p2 = Vector3.Dot(v2, axZf2);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, axZf2)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, axZf2)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, axZf2));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;

			p0 = Vector3.Dot(v0, Vector3.UnitX);
			p1 = Vector3.Dot(v1, Vector3.UnitX);
			p2 = Vector3.Dot(v2, Vector3.UnitX);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, Vector3.UnitX)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, Vector3.UnitX)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, Vector3.UnitX));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, Vector3.UnitY);
			p1 = Vector3.Dot(v1, Vector3.UnitY);
			p2 = Vector3.Dot(v2, Vector3.UnitY);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, Vector3.UnitY)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, Vector3.UnitY)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, Vector3.UnitY));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;
			p0 = Vector3.Dot(v0, Vector3.UnitZ);
			p1 = Vector3.Dot(v1, Vector3.UnitZ);
			p2 = Vector3.Dot(v2, Vector3.UnitZ);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, Vector3.UnitZ)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, Vector3.UnitZ)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, Vector3.UnitZ));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;

			var n = Vector3.Cross(f0, f1);
			p0 = Vector3.Dot(v0, n);
			p1 = Vector3.Dot(v1, n);
			p2 = Vector3.Dot(v2, n);
			r =
				e.X * Math.Abs(Vector3.Dot(Vector3.UnitX, n)) +
				e.Y * Math.Abs(Vector3.Dot(Vector3.UnitY, n)) +
				e.Z * Math.Abs(Vector3.Dot(Vector3.UnitZ, n));
			if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
				return false;

			return true;
		}
	}
}
