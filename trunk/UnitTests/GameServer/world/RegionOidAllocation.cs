/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Threading;
using DOL.Database;
using DOL.Tests;
using DOL.GS;
using NUnit.Framework;

namespace DOL.Server
{
	[TestFixture]
	public class RegionOidAllocationTest : ServerTests
	{
		public Region m_reg;
		public int id;
		public object sync;
		public volatile bool finished;
		public int started;

		[TestFixtureSetUp] public override void Init()
		{
			base.Init();
			RegionData data = new RegionData();
			data.Id = 5555;
			data.Name = "reg data1";
			data.Description = "reg test1";
			data.Mobs = new Mob[0];
			m_reg = WorldMgr.RegisterRegion(new GameTimer.TimeManager("RegTest1"), data);
			//WorldMgr.RegisterZone(5555, 5555, "test zone1", 0, 0, 16, 16);
			m_reg.StartRegionMgr();
		}

		[Test] public void AddRemoveObjects()
		{
			const int count = 30000;
			GameNPC[] mobs = new GameNPC[count];

			Console.Out.WriteLine("[{0}] init {1} mobs", id, count);
			// init mobs
			for (int i = 0; i < count; i++)
			{
				GameNPC mob = mobs[i] = new GameNPC();
				Assert.IsTrue(mob.ObjectID == -1, "mob {0} oid={1}, should be -1", i, mob.ObjectID);
				Assert.IsFalse(mob.ObjectState == GameObject.eObjectState.Active, "mob {0} state={1}, should be not Active", i, mob.ObjectState);
				mob.Name = "test mob " + i;
				mob.CurrentRegion = m_reg;
				m_reg.PreAllocateRegionSpace(1953);
			}

			for (int x = 10; x > 0; x--)
			{
				Console.Out.WriteLine("[{0}] loop {1} add mobs", id, count-x);
				// add mobs
				for (int i = 0; i <= count-x; i++)
				{
//					Console.Out.WriteLine("add "+i);
					GameNPC mob = mobs[i];
					Assert.IsTrue(mob.AddToWorld(), "failed to add {0} to the world", mob.Name);
					Assert.IsTrue(mob.ObjectID > 0 && mob.ObjectID <= DOL.GS.ServerProperties.Properties.REGION_MAX_OBJECTS, "{0} oid={1}", mob.Name, mob.ObjectID);
				}
				
				for (int i = count-x; i >= 0; i--)
				{
//					Console.Out.WriteLine("check "+i);
					GameNPC mob = mobs[i];
					GameNPC regMob = (GameNPC)m_reg.GetObject((ushort)mob.ObjectID);
					Assert.AreSame(mob, regMob, "expected to read '{0}' oid={1} but read '{2}' oid={3}", mob.Name, mob.ObjectID, regMob==null?"null":regMob.Name, regMob==null?"null":regMob.ObjectID.ToString());
				}
			
				Console.Out.WriteLine("[{0}] loop {1} remove mobs", id, count-x);
				// remove mobs
				for (int i = count-x; i >= 0; i--)
				{
//					Console.Out.WriteLine("remove "+i);
					GameNPC mob = mobs[i];
					int oid = mob.ObjectID;
					Assert.IsTrue(mob.RemoveFromWorld(), "failed to remove {0}", mob.Name);
					Assert.IsTrue(mob.ObjectID == -1, "{0}: oid is not -1 (oid={1})", mob.Name, mob.ObjectID);
					Assert.IsFalse(mob.ObjectState == GameObject.eObjectState.Active, "{0} is still active after remove", mob.Name);
					GameNPC regMob = (GameNPC)m_reg.GetObject((ushort)oid);
					Assert.IsNull(regMob, "{0} was removed from the region but oid {1} is still used by {2}", mob.Name, oid, regMob==null?"null":regMob.Name);
				}
			}
		}
		
		[Test] public void MultithreadAddRemove()
		{
			const int count = 10;
			RegionOidAllocationTest[] roas = new RegionOidAllocationTest[count];
			object obj = new object();
			Monitor.Enter(obj);

			for (int i = 0; i < count; i++)
			{
				RegionOidAllocationTest roaInstance = new RegionOidAllocationTest();
				roaInstance.id = i;
				roaInstance.sync = obj;
				roaInstance.m_reg = m_reg;
				roas[i] = roaInstance;
				Thread.MemoryBarrier();
				
				Thread thr = new Thread(new ThreadStart(roaInstance.ThreadMain));
				thr.Start();
			}
			
			while (true)
			{
				Thread.Sleep(100);
				bool done = true;
				for (int i = 0; i < count; i++)
				{
					RegionOidAllocationTest roa = roas[i];
					if (!roa.finished)
					{
						done = false;
						break;
					}
				}
				if (done)
					break;
			}

			Console.Out.WriteLine("done.");
		}

		public void ThreadMain()
		{
			try
			{
				started = Environment.TickCount;
				Console.Out.WriteLine("[{0}] started", id);
				AddRemoveObjects();
			}
			catch (Exception e)
			{
				Console.Out.WriteLine("[{0}] error: {1}", id, e.ToString());
			}
			finally
			{
				finished = true;
				Console.Out.WriteLine("[{0}] finished in {1}ms", id, Environment.TickCount - started);
			}
		}
	}
}
