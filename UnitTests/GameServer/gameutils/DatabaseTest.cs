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
using System.Collections;
using DOL;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using DOL.Tests;
using NHibernate.Expression;
using NUnit.Framework;

namespace DOL.GS.Tests
{
	/// <summary>
	/// Unit tests for the Zone Class
	/// </summary>
	[TestFixture]
	public class DatabaseTest : DOLTestCase
	{				

		public DatabaseTest()
		{
		}

		[Test] public void TestSelect()
		{
			Console.WriteLine("DatabaseTest();");
			int startTick = System.Environment.TickCount;
			for(int i = 0 ; i < 10000 ; i++)
			{
				BindPoint newBind = new BindPoint();
				newBind.X = i;
				newBind.Y = 2;
				newBind.Z = 3;
				newBind.Radius = 60000;
				newBind.Region = 2;
				newBind.Realm = 1;
				GameServer.Database.AddNewObject(newBind);
			}
			int endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10000 insert : "+(int)(endTick-startTick)+"ms");
						

			startTick = System.Environment.TickCount;
			IList allBind = GameServer.Database.SelectAllObjects(typeof(BindPoint));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10000 select: "+(int)(endTick-startTick)+"ms");


			startTick = System.Environment.TickCount;
			IList halfBind = GameServer.Database.SelectObjects(typeof(BindPoint),Expression.Lt("X", 5000));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 5000 select: "+(int)(endTick-startTick)+"ms (halfBind:"+halfBind.Count+")");

			startTick = System.Environment.TickCount;
			halfBind = GameServer.Database.SelectObjects(typeof(BindPoint),Expression.And(Expression.Ge("X", 5000),Expression.Le("X", 5010)));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10 select : "+(int)(endTick-startTick)+"ms");

			startTick = System.Environment.TickCount;
			halfBind = GameServer.Database.SelectObjects(typeof(BindPoint),Expression.And(Expression.Ge("X", 5000),Expression.Le("X", 5010)));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10 select : "+(int)(endTick-startTick)+"ms");

			startTick = System.Environment.TickCount;
			halfBind = GameServer.Database.SelectObjects(typeof(BindPoint),Expression.And(Expression.Ge("X", 5000),Expression.Le("X", 5010)));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10 select : "+(int)(endTick-startTick)+"ms");

			startTick = System.Environment.TickCount;
			object singleBind = GameServer.Database.SelectObject(typeof(BindPoint),Expression.Eq("X", 5000));
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 1 select : "+(int)(endTick-startTick)+"ms");

			/*startTick = System.Environment.TickCount;
			foreach(BindPoint bind in allBind)
			{
				bind.Radius = 21;
				GameServer.Database.SaveObject(bind);
			}
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10000 update : "+(int)(endTick-startTick)+"ms");
		
			startTick = System.Environment.TickCount;
			foreach(BindPoint bind in allBind)
			{
				GameServer.Database.DeleteObject(bind);
			}
			endTick = System.Environment.TickCount;

			Console.WriteLine("Temps d'execution 10000 delete : "+(int)(endTick-startTick)+"ms");*/
		}			

	}
}
