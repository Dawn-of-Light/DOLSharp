using NUnit.Framework;
using DOL.GS.Geometry;
using System.Numerics;
using System;

namespace DOL.UnitTests.Gameserver
{
	[TestFixture]
	class UT_Colliders
	{
		[Test]
		public void CheckAABB_AABBCollision()
		{
			var box1 = new AABoundingBox(Vector3.Zero, Vector3.One);
			var box2 = new AABoundingBox(Vector3.One * 2, Vector3.One * 3);
			Assert.False(box1.CollideWithAABB(box2), "shouldn't collide");
			Assert.False(box2.CollideWithAABB(box1), "shouldn't collide");

			var box3 = new AABoundingBox(Vector3.One * 0.5f, Vector3.One);
			Assert.False(box2.CollideWithAABB(box3), "shouldn't collide");
			Assert.False(box3.CollideWithAABB(box2), "shouldn't collide");
			Assert.True(box1.CollideWithAABB(box3), "box3 is inside box1");
			Assert.True(box3.CollideWithAABB(box1), "box3 is inside box1");

			var boxBig = new AABoundingBox(-Vector3.One, 3 * Vector3.One);
			Assert.True(box1.CollideWithAABB(boxBig), "box1 is completely inside the big box");
			Assert.True(boxBig.CollideWithAABB(box1), "box1 is completely inside the big box");
		}

		[Test]
		public void CheckAABB_RayCollision()
		{
			var origin = Vector3.Zero;
			var direction = Vector3.Normalize(Vector3.One);
			var box1 = new AABoundingBox(new Vector3(2, 0, 0), new Vector3(3, 1, 1));
			Assert.GreaterOrEqual(box1.CollideWithRay(origin, direction, 5), 5, "shouldn't collide");
			var box2 = new AABoundingBox(new Vector3(1, 0, 0), new Vector3(2, 1, 1));
			Assert.AreEqual(Math.Sqrt(3), box2.CollideWithRay(origin, direction, 5), 1e-6, "the ray should collide with the vertice of the aa box");
			Assert.AreEqual(box2.Max.Length() / 2, box2.CollideWithRay(origin, Vector3.Normalize(box2.Max), 5), 1e-6, "the ray should collide with the aa box");
		}

		[Test]
		public void CheckTriangle_AABBCollision()
		{
			var triangle = new Triangle(new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 1));
			var box1 = new AABoundingBox(Vector3.Zero, Vector3.One);
			var box2 = new AABoundingBox(Vector3.One * 2, Vector3.One * 3);
			var boxBig = new AABoundingBox(-Vector3.One, 3 * Vector3.One);
			Assert.True(triangle.CollideWithAABB(box1), "triangle is a face of the box");
			Assert.False(triangle.CollideWithAABB(box2), "shouldn't collide");

			var triangle2 = new Triangle(new Vector3(0.5f), new Vector3(0.5f, 2.0f, 0), new Vector3(0.5f, 0, 2.0f));
			Assert.True(triangle2.CollideWithAABB(box1), "triangle has one vertice in the box");
			Assert.True(triangle2.CollideWithAABB(boxBig), "triangle is inside the big box");
		}

		[Test]
		public void CheckTriangle_RayCollision()
		{
			var triangle = new Triangle(new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 1));
			Assert.AreEqual(triangle.CollideWithRay(Vector3.Zero, Vector3.Normalize(Vector3.One), 5), 5, 1e-6, "shouldn't collide");
			Assert.AreEqual(triangle.CollideWithRay(Vector3.Zero, Vector3.Normalize(triangle.B), 5), triangle.B.Length(), 1e-6, "the ray should collide with the triangle");
			Assert.AreEqual(triangle.CollideWithRay(Vector3.Zero, Vector3.Normalize(new Vector3(2, 1, 1)), 5), new Vector3(1, 0.5f, 0.5f).Length(), 1e-6, "the ray should collide with the triangle");
		}
	}
}
