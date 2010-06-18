using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOL.GS.PacketHandler
{
	public enum ClientStatus
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
		private static readonly Dictionary<int, int> _packetIdToPreprocessMap;
		private static readonly Dictionary<int, Func<GameClient, GSPacketIn, bool>> _preprocessors;

		static PacketPreprocessing()
		{
			_packetIdToPreprocessMap = new Dictionary<int, int>();
			_preprocessors = new Dictionary<int, Func<GameClient, GSPacketIn, bool>>();

			RegisterPreprocessors((int)ClientStatus.LoggedIn, (client, packet) => client.Account != null);		// player must be logged into an account
			RegisterPreprocessors((int)ClientStatus.PlayerInGame, (client, player) => client.Player != null);	// player must be logged into a character
		}

		/// <summary>
		/// Registers a packet definition with a preprocessor.
		/// </summary>
		/// <param name="packetId">the ID of the packet in question</param>
		/// <param name="preprocessorId">the ID of the preprocessor for the given packet ID</param>
		public static void RegisterPacketDefinition(int packetId, int preprocessorId)
		{
			_packetIdToPreprocessMap.Add(packetId, preprocessorId);
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
