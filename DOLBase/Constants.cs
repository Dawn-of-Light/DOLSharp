using System.Text;

namespace DOL
{
	/// <summary>
	/// The status of the gameserver
	/// </summary>
	public enum eGameServerStatus
	{
		/// <summary>
		/// Server is open for connections
		/// </summary>
		GSS_Open = 0,
		/// <summary>
		/// Server is closed and won't accept connections
		/// </summary>
		GSS_Closed,
		/// <summary>
		/// Server is down
		/// </summary>
		GSS_Down,
		/// <summary>
		/// Server is full, no more connections accepted
		/// </summary>
		GSS_Full,
		/// <summary>
		/// Unknown server status
		/// </summary>
		GSS_Unknown,
		/// <summary>
		/// Server is banned for the user
		/// </summary>
		GSS_Banned,
		/// <summary>
		/// User is not invited
		/// </summary>
		GSS_NotInvited,
		/// <summary>
		/// The count of server stati
		/// </summary>
		_GSS_Count,
	}

	/// <summary>
	/// The different game server types
	/// </summary>
	public enum eGameServerType
	{
		/// <summary>
		/// Normal server
		/// </summary>
		GST_Normal = 0,
		/// <summary>
		/// Test server
		/// </summary>
		GST_Test = 1,
		/// <summary>
		/// Player vs Player
		/// </summary>
		GST_PvP = 2,
		/// <summary>
		/// Player vs Monsters
		/// </summary>
		GST_PvE = 3,
		/// <summary>
		/// Roleplaying server
		/// </summary>
		GST_Roleplay = 4,
		/// <summary>
		/// Casual server
		/// </summary>
		GST_Casual = 5,
		/// <summary>
		/// Unknown server type
		/// </summary>
		GST_Unknown = 6,
		/// <summary>
		/// The count of server types
		/// </summary>
		_GST_Count = 7,
	}

	public static class Constants
	{
		/// <summary>
		/// The size of the send buffer for a client socket.
		/// </summary>
		public const int SendBuffSize = 16*1024;

		/// <summary>
		/// The default encoding to use for all string operations in packet writing or reading.
		/// </summary>
		public static readonly Encoding DefaultEncoding = Encoding.Unicode;
	}
}