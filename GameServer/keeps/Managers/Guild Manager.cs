using System;

using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class to manage all the dealings with Guilds
	/// </summary>
	public class KeepGuildMgr
	{
		/// <summary>
		/// Sends a message to the guild informing them that a door has been destroyed
		/// </summary>
		/// <param name="door">The door object</param>
		public static void SendDoorDestroyedMessage(GameKeepDoor door)
		{
			string message = door.Name + " in your area " + door.Component.Keep.Name + " has been destroyed";
			SendMessageToGuild(message, door.Component.Keep.Guild);
		}

		/// <summary>
		/// Send message to a guild
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="guild">The guild</param>
		public static void SendMessageToGuild(string message, Guild guild)
		{
			if (guild == null)
				return;

			message = "[Guild] [" + message +"]";
			guild.SendMessageToGuildMembers(message, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
		}

		public static void SendLevelChangeMessage(AbstractGameKeep keep)
		{
			string message = "Your guild's keep " + keep.Name + " is now level " + keep.Level;
			if (keep.Level != 10)
				message += ", it is on the way to level 10";
			SendMessageToGuild(message, keep.Guild);
		}

		public static void SendChangeLevelTimeMessage(AbstractGameKeep keep)
		{
			string message;
			string changeleveltext = "";
			int nextlevel = 0;
			if (10 > keep.Level)
			{
				changeleveltext = "upgrade";
				nextlevel = keep.Level + 1;
			}
			else
			{
				changeleveltext = "downgrade";
				nextlevel = keep.Level - 1;
			}
			message = "Your guild is starting to " + changeleveltext + " its area " + keep.Name + " to level 10.";
			TimeSpan time = keep.ChangeLevelTimeRemaining;
			message += " It will take ";
			if (time.Hours > 0)
				message += time.Hours + " hour(s) ";
			if (time.Minutes > 0)
				message += time.Minutes + " minute(s) ";
			else message += time.Seconds + " second(s)";
			message += " to reach the next level.";
			SendMessageToGuild(message, keep.Guild);
		}
	}
}
