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
using NHibernate;
using NHibernate.Expression;
using NUnit.Framework;

namespace DOL.GS.Tests
{
	/// <summary>
	/// Unit tests for the Zone Class
	/// </summary>
	[TestFixture]
	public class DatabaseTestAlliance : DOLTestCase
	{				

		public DatabaseTestAlliance()
		{
		}

		[Test] public void TestAlliance()
		{
			Console.WriteLine("DatabaseAllianceTest();");

			Guild newguild = new Guild();
			newguild.GuildName = "Les boss";
			newguild.BountyPoints = 0;
			newguild.RealmPoints = 0;
			newguild.Emblem = 0;
			newguild.Motd = "Welcome to the new guild "+newguild.GuildName+".";
			newguild.OMotd = "";
	
			newguild.Alliance = new Alliance();			// create a new empty alliance
			newguild.Alliance.AMotd = "Your guild have no alliances.";
			newguild.Alliance.AllianceLeader = newguild;
			newguild.Alliance.AllianceGuilds.Add(newguild);

			GameServer.Database.AddNewObject(newguild);	// save the new guild in its empty alliance


			Guild newguild2 = new Guild();
			newguild2.GuildName = "Les mechants";
			newguild2.BountyPoints = 0;
			newguild2.RealmPoints = 0;
			newguild2.Emblem = 1;
			newguild2.Motd = "Welcome to the new guild "+newguild.GuildName+".";
			newguild2.OMotd = "";
			
			newguild2.Alliance = new Alliance();			// create a new empty alliance
			newguild2.Alliance.AMotd = "Your guild have no alliances.";
			newguild2.Alliance.AllianceLeader = newguild2;
			newguild2.Alliance.AllianceGuilds.Add(newguild2);

			GameServer.Database.AddNewObject(newguild2);	// save another new guild in another empty alliance

			// here two guild in the two differents empty alliance

			GameServer.Database.DeleteObject(newguild2.Alliance);
			newguild2.Alliance = newguild.Alliance;
			newguild2.Alliance.AllianceGuilds.Add(newguild2);
			GameServer.Database.SaveObject(newguild2);

			// here the two guilds are now in the same alliance

			newguild2.Alliance = new Alliance();
			newguild2.Alliance.AMotd = "Your guild have no alliances.";
			newguild2.Alliance.AllianceLeader = newguild2;
			newguild2.Alliance.AllianceGuilds.Add(newguild2);

			GameServer.Database.SaveObject(newguild2);

			// here two guild in the two differents empty alliance

		}
	}
}
