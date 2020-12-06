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
using System.IO;
using System.Reflection;

using DOL.GS;
using DOL.Database.Connection;

using NUnit.Framework;
using DOL.Database;
using System.Linq;
using DOL.Database.Attributes;

namespace DOL.Integration.Server
{
	/// <summary>
	/// SetUpTests Start The Needed Environnement for Unit Tests
	/// </summary>
	[SetUpFixture]
	public class SetUpTests
	{
		public SetUpTests()
		{
		}

		/// <summary>
		/// Create Game Server Instance for Tests
		/// </summary>
		public static void CreateGameServerInstance()
		{
			Console.WriteLine("Create Game Server Instance");
			DirectoryInfo CodeBase = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory;
			Console.WriteLine("Code Base: " + CodeBase.FullName);
			DirectoryInfo FakeRoot = CodeBase.Parent;
			Console.WriteLine("Fake Root: " + FakeRoot.FullName);

			if (GameServer.Instance == null)
			{
				GameServerConfiguration config = new GameServerConfiguration();
				config.RootDirectory = FakeRoot.FullName;
				config.DBType = ConnectionType.DATABASE_SQLITE;
				config.DBConnectionString = string.Format("Data Source={0};Version=3;Pooling=False;Cache Size=1073741824;Journal Mode=Off;Synchronous=Off;Foreign Keys=True;Default Timeout=60",
												 Path.Combine(config.RootDirectory, "dol-tests-only.sqlite3.db"));
				config.Port = 0; // Auto Choosing Listen Port
				config.UDPPort = 0; // Auto Choosing Listen Port
				config.IP = System.Net.IPAddress.Parse("127.0.0.1");
				config.UDPIP = System.Net.IPAddress.Parse("127.0.0.1");
				config.RegionIP = System.Net.IPAddress.Parse("127.0.0.1");

				GameServer.LoadTestDouble(new GameServerWithDefaultDB(config));

				Console.WriteLine("Game Server Instance Created !");
			}
		}

		private class GameServerWithDefaultDB : GameServer
		{
			public GameServerWithDefaultDB(GameServerConfiguration config) : base(config) { }

			protected override void CheckAndInitDB()
			{
				if (m_database == null)
				{
					m_database = ObjectDatabase.GetObjectDatabase(Configuration.DBType, Configuration.DBConnectionString);

					//Load only default assembly
					var assembly = Assembly.Load("DOLDatabase");
					// Walk through each type in the assembly
					assembly.GetTypes().AsParallel().ForAll(type =>
						{
							if (!type.IsClass || type.IsAbstract)
							{
								return;
							}

							var attrib = type.GetCustomAttributes<DataTable>(false);
							if (attrib.Any())
							{

								m_database.RegisterDataObject(type);
							}
						});
				}
			}
		}
		
		[OneTimeSetUp]
		public virtual void Init()
		{
			CreateGameServerInstance();
			
			if (!GameServer.Instance.IsRunning)
			{
				Console.WriteLine("Starting GameServer");
				if (!GameServer.Instance.Start())
				{
					Console.WriteLine("Error init GameServer");
				}
			}
			else
			{
				Console.WriteLine("GameServer already running, skip init of Gameserver...");
			}
		}

		[OneTimeTearDown]
		public void Dispose()
		{
			GameServer.Instance.Stop();
		}
	}
}
