using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace DOL.GS.Geometry
{
	public class OctreeNode<T> : ICollider where T : ICollider
	{
		public static int MaxObjectsPerNode = 1000;
		public static int MaxLevel = 20;
		public static long createdNodes = 0;
		public AABoundingBox Box { get; private set; }
		public OctreeNode<T>[] Nodes;
		public List<T> Objects;
		public int Level;

		public OctreeNode(AABoundingBox box, int level, int nbObjects = int.MaxValue)
		{
			Interlocked.Increment(ref createdNodes);
			Box = box;
			Objects = new List<T>();
			Nodes = null;
			Level = level;
		}
		~OctreeNode()
		{
			Interlocked.Decrement(ref createdNodes);
		}

		public void Iterate(Action<OctreeNode<T>> action)
		{
			action(this);
			if (Nodes != null)
				foreach (var n in Nodes)
					n.Iterate(action);
		}

		private void Subdivide(int nbObjects = int.MaxValue)
		{
			var extents = Box.Max - Box.Min;
			var center = extents / 2 + Box.Min;
			var translateX = new Vector3(extents.X / 2, 0, 0);
			var translateY = new Vector3(0, extents.Y / 2, 0);
			var translateZ = new Vector3(0, 0, extents.Z / 2);
			Nodes = new OctreeNode<T>[]
			{
					new OctreeNode<T>(new AABoundingBox(Box.Min, center), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(Box.Min + translateX, center + translateX), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(Box.Min + translateY, center + translateY), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(Box.Min + translateZ, center + translateZ), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(center, Box.Max), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(center - translateX, Box.Max - translateX), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(center - translateY, Box.Max - translateY), Level + 1, nbObjects),
					new OctreeNode<T>(new AABoundingBox(center - translateZ, Box.Max - translateZ), Level + 1, nbObjects),
			};
			foreach (var n in Nodes)
				foreach (var o in Objects)
					n.AddObject(o);
			Objects = null;
		}

		public void AddObject(T obj)
		{
			if (!obj.CollideWithAABB(Box))
				return;
			if (Nodes != null)
				foreach (var n in Nodes)
					n.AddObject(obj);
			else
			{
				Objects.Add(obj);
				if (Level < MaxLevel && Objects.Count > MaxObjectsPerNode && Vector3.Distance(Box.Min, Box.Max) > 256)
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
