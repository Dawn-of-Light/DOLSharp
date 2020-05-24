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
using System.Net.Sockets;
using DOL.Database;
using DOL.GS;
using NUnit.Framework;

namespace DOL.Server.Tests
{
	public class ServerTests
	{
		public ServerTests()
		{
		}

		protected GamePlayer CreateMockGamePlayer()
		{
			DOLCharacters character= null;
			Account account = GameServer.Database.SelectObject<Account>("", new QueryParameter());
			Assert.IsNotNull(account);

			foreach (DOLCharacters charact in account.Characters)
			{
				if (charact!=null)
					character = charact;
			}
			Assert.IsNotNull(character);
			
			GameClient client = new GameClient(GameServer.Instance);
			client.Version = GameClient.eClientVersion.Version1105;
			client.Socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
			client.Account = account;
			client.PacketProcessor = new DOL.GS.PacketHandler.PacketProcessor(client);
			client.Out = new DOL.GS.PacketHandler.PacketLib1105(client);
			client.Player = new GamePlayer(client,character);
			Assert.IsNotNull(client.Player,"GamePlayer instance created");
			
			return client.Player;
		}
				
		public void cd()
		{
			Console.WriteLine("GC: "+Directory.GetCurrentDirectory());
		}
		
		#region Watch

		static long gametick;

		/// <summary>
		/// use startWatch to start taking the time
		/// </summary>
		public static void StartWatch()
		{
			//Tickcount is more accurate than gametimer ticks :)
			gametick = Environment.TickCount;
			Console.WriteLine("StartWatch: "+gametick);
		}

		/// <summary>
		/// stop watch will count the Gamticks since last call of startWatch
		/// 
		/// Note: This value does not represent the time it will take on a
		/// actual server since we have no actual user load etc...
		/// </summary>
		public static void StopWatch()
		{
			Console.WriteLine("Stop watch: "+Environment.TickCount);
			long elapsed = Environment.TickCount - gametick;
			Console.WriteLine(elapsed+" ticks(ms) elapsed");
		}
		
		#endregion
	}
}
