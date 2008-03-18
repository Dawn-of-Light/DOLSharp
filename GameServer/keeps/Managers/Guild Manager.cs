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
			if (keep.Level != keep.TargetLevel && keep.TargetLevel != 0)
				message += ", it is on the way to level " + keep.TargetLevel;
			SendMessageToGuild(message, keep.Guild);
		}

		public static void SendChangeLevelTimeMessage(AbstractGameKeep keep)
		{
			string message;
			string changeleveltext = "";
			int nextlevel = 0;
			if (keep.TargetLevel > keep.Level)
			{
				changeleveltext = "upgrade";
				nextlevel = keep.Level + 1;
			}
			else
			{
				changeleveltext = "downgrade";
				nextlevel = keep.Level - 1;
			}
			message = "Your guild is starting to " + changeleveltext + " its area " + keep.Name + " to level " + keep.TargetLevel + ".";
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
