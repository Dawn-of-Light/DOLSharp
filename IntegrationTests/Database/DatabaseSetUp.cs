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

using DOL.Database;
using DOL.Database.Connection;

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Database SetUp for Unit Tests
	/// </summary>
	[SetUpFixture]
	public class DatabaseSetUp
	{
		public DatabaseSetUp()
		{
		}
		
		public static SQLObjectDatabase Database { get; set; }
		public static string ConnectionString { get; set; }
				
		[OneTimeSetUp]
		public void SetUp()
		{
			var CodeBase = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory;
			ConnectionString = string.Format("Data Source={0};Version=3;Pooling=False;Cache Size=1073741824;Journal Mode=Off;Synchronous=Off;Foreign Keys=True;Default Timeout=60",
			                                     Path.Combine(CodeBase.Parent.FullName, "dol-database-tests-only.sqlite3.db"));
			                                     
			Database = (SQLObjectDatabase)ObjectDatabase.GetObjectDatabase(ConnectionType.DATABASE_SQLITE, ConnectionString);
			
			Console.WriteLine("DB Configured : {0}, {1}", Database.ConnectionType, ConnectionString);
			
			log4net.Config.BasicConfigurator.Configure(
				new log4net.Appender.ConsoleAppender {
					Layout = new log4net.Layout.SimpleLayout(),
					Threshold = log4net.Core.Level.Info
				});
		}
		
		[OneTimeTearDown]
		public void TearDown()
		{
			log4net.LogManager.Shutdown();
		}
	}
}
