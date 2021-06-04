using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace DOL.GS.Geometry
{
	public class QuadtreeNode<T> : ICollider where T : ICollider
	{
		public static int MaxObjectsPerNode = 1000;
		public static int MaxLevel = 20;
		public static long createdNodes = 0;
		public AABoundingBox Box { get; private set; }
		public QuadtreeNode<T>[] Nodes;
		public List<T> Objects;
		public int Level;

		public QuadtreeNode(AABoundingBox box, int level)
		{
			Interlocked.Increment(ref createdNodes);
			Box = box;
			Nodes = null;
			Objects = new List<T>();
			Level = level;
		}
		~QuadtreeNode()
		{
			Interlocked.Decrement(ref createdNodes);
		}

		public void Iterate(Action<QuadtreeNode<T>> action)
		{
			action(this);
			if (Nodes != null)
				foreach (var n in Nodes)
					n.Iterate(action);
		}

		private void Subdivide()
		{
			var extents = Box.Max.ToVector2() - Box.Min.ToVector2();
			var center = extents / 2 + Box.Min.ToVector2();
			var translateX = new Vector3(extents.X / 2, 0, 0);
			var translateY = new Vector3(0, extents.Y / 2, 0);
			Nodes = new QuadtreeNode<T>[]
			{
					new QuadtreeNode<T>(new AABoundingBox(Box.Min, new Vector3(center, Box.Max.Z)), Level + 1),
					new QuadtreeNode<T>(new AABoundingBox(Box.Min + translateX, new Vector3(center, Box.Max.Z) + translateX), Level + 1),
					new QuadtreeNode<T>(new AABoundingBox(Box.Min + translateY, new Vector3(center, Box.Max.Z) + translateY), Level + 1),
					new QuadtreeNode<T>(new AABoundingBox(new Vector3(center, Box.Min.Z), Box.Max), Level + 1),
			};

			if (Objects != null)
				foreach (var obj in Objects)
					foreach (var n in Nodes)
						n.AddObject(obj);
			Objects = null;
		}

		public void AddObject(T obj)
		{
			if (!obj.CollideWithAABB(Box))
				return;
			if (Nodes != null)
				foreach (var n in Nodes)
					n.AddObject(obj);
			if (Objects != null)
			{
				Objects.Add(obj);
				if (Level < MaxLevel && Objects.Count > MaxObjectsPerNode && Vector3.Distance(Box.Min.To2D(), Box.Max.To2D()) > 256)
					Subdivide();
			}
		}

		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance)
		{
			var stats = new RaycastStats();
			return CollideWithRay(origin, direction, maxDistance, ref stats);
		}
		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance, ref RaycastStats stats)
		{
			stats.nbNodeTests += 1;
			var dist = Box.CollideWithRay(origin, direction, maxDistance, ref stats);
			if (dist >= maxDistance)
				return maxDistance;

			dist = maxDistance;
			if (Nodes != null)
				foreach (var node in Nodes)
					dist = Math.Min(dist, node.CollideWithRay(origin, direction, dist, ref stats));
			if (Objects != null)
				foreach (var obj in Objects)
					dist = Math.Min(dist, obj.CollideWithRay(origin, direction, dist, ref stats));
			return dist;
		}

		public bool CollideWithAABB(AABoundingBox box) => Box.CollideWithAABB(box);
	}
}
