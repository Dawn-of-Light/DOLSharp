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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace DOL.GS.PacketHandler
{
	public enum eClientStatus
	{
		None = 0,
		LoggedIn = 1,
		PlayerInGame = 2
	}

	/// <summary>
	/// Handles preprocessing for incoming game packets.
	/// </summary>
	/// <remarks>
	/// <para>Preprocessing includes things like checking if a certain precondition exists or if a packet meets a 
	/// certain criteria before we actually handle it.
	/// </para>
	/// <para>
	/// Any time that a packet comes thru with a preprocessor ID of 0, it means there is no preprocessor associated 
	/// with it, and thus we pass it thru. (return true)
	/// </para>
	/// </remarks>
	public static class PacketPreprocessing
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private static readonly Dictionary<int, int> _packetIdToPreprocessMap;
		private static readonly Dictionary<int, Func<GameClient, GSPacketIn, bool>> _preprocessors;

		static PacketPreprocessing()
		{
			_packetIdToPreprocessMap = new Dictionary<int, int>();
			_preprocessors = new Dictionary<int, Func<GameClient, GSPacketIn, bool>>();

			RegisterPreprocessors((int)eClientStatus.LoggedIn, (client, packet) => client.Account != null);		// player must be logged into an account
			RegisterPreprocessors((int)eClientStatus.PlayerInGame, (client, player) => client.Player != null);	// player must be logged into a character
		}

		/// <summary>
		/// Registers a packet definition with a preprocessor.
		/// </summary>
		/// <param name="packetId">the ID of the packet in question</param>
		/// <param name="preprocessorId">the ID of the preprocessor for the given packet ID</param>
		public static void RegisterPacketDefinition(int packetId, int preprocessorId)
		{
			// if they key doesn't exist, add it, and if it does, replace it
			if (!_packetIdToPreprocessMap.ContainsKey(packetId))
			{
				_packetIdToPreprocessMap.Add(packetId, preprocessorId);
			}
			else
			{
				log.InfoFormat("Replacing Packet Processor for packet ID {0} with preprocessorId {1}", packetId, preprocessorId);
				_packetIdToPreprocessMap[packetId] = preprocessorId;
			}
	}

		/// <summary>
		/// Registers a preprocessor.
		/// </summary>
		/// <param name="preprocessorId">the ID for the preprocessor</param>
		/// <param name="preprocessorFunc">the preprocessor delegate to use</param>
		public static void RegisterPreprocessors(int preprocessorId, Func<GameClient, GSPacketIn, bool> preprocessorFunc)
		{
			_preprocessors.Add(preprocessorId, preprocessorFunc);
		}

		/// <summary>
		/// Checks if a packet can be processed by the server.
		/// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet in question</param>
		/// <returns>true if the packet passes all preprocessor checks; false otherwise</returns>
		public static bool CanProcessPacket(GameClient client, GSPacketIn packet)
		{
			int preprocessorId;
			if(!_packetIdToPreprocessMap.TryGetValue(packet.ID, out preprocessorId))
				return false;

			if(preprocessorId == 0)
			{
				// no processing, pass thru.
				return true;
			}

			Func<GameClient, GSPacketIn, bool> preprocessor;
			if(!_preprocessors.TryGetValue(preprocessorId, out preprocessor))
				return false;

			return preprocessor(client, packet);
		}
	}
}
