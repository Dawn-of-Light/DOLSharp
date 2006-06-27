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
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class helps sending messages to other players
	/// </summary>
	public class Message
	{
		/// <summary>
		/// Sends a message to the chat window of players inside
		/// INFO_DISTANCE radius of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <param name="excludes">An optional list of excluded players</param>
		public static void ChatToArea(GameObject centerObject, string message, eChatType chatType, params GameObject[] excludes)
		{
			ChatToArea(centerObject, message, chatType, WorldMgr.INFO_DISTANCE, excludes);
		}

		/// <summary>
		/// Sends a message to the chat window of players inside
		/// INFO_DISTANCE radius of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <remarks>If the centerObject is a player, he won't receive the message</remarks>
		public static void ChatToOthers(GameObject centerObject, string message, eChatType chatType)
		{
			ChatToArea(centerObject, message, chatType, WorldMgr.INFO_DISTANCE, centerObject);
		}

		/// <summary>
		/// Sends a message to the chat window of players inside
		/// a specific distance of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <param name="distance">The distance around the center where players will receive the message</param>
		/// <param name="excludes">An optional list of excluded players</param>
		/// <remarks>If the center object is a player, he will get the message too</remarks>
		public static void ChatToArea(GameObject centerObject, string message, eChatType chatType, ushort distance, params GameObject[] excludes)
		{
			MessageToArea(centerObject, message, chatType, eChatLoc.CL_ChatWindow, distance, excludes);
		}

		/// <summary>
		/// Sends a message to the system window of players inside
		/// INFO_DISTANCE radius of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <param name="excludes">An optional list of excluded players</param>
		public static void SystemToArea(GameObject centerObject, string message, eChatType chatType, params GameObject[] excludes)
		{
			SystemToArea(centerObject, message, chatType, WorldMgr.INFO_DISTANCE, excludes);
		}

		/// <summary>
		/// Sends a message to the system window of players inside
		/// INFO_DISTANCE radius of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <remarks>If the centerObject is a player, he won't receive the message</remarks>
		public static void SystemToOthers(GameObject centerObject, string message, eChatType chatType)
		{
			SystemToArea(centerObject, message, chatType, WorldMgr.INFO_DISTANCE, centerObject);
		}

		/// <summary>
		/// Sends a message to the system window of players inside
		/// a specific distance of the center object
		/// </summary>
		/// <param name="centerObject">The center object of the message</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The type of message to send</param>
		/// <param name="distance">The distance around the center where players will receive the message</param>
		/// <param name="excludes">An optional list of excluded players</param>
		/// <remarks>If the center object is a player, he will get the message too</remarks>
		public static void SystemToArea(GameObject centerObject, string message, eChatType chatType, ushort distance, params GameObject[] excludes)
		{
			MessageToArea(centerObject, message, chatType, eChatLoc.CL_SystemWindow, distance, excludes);
		}

		/// <summary>
		/// Sends a text message to players within a specific radius of another object
		/// </summary>
		/// <param name="centerObject">The center object</param>
		/// <param name="message">The message to send</param>
		/// <param name="chatType">The chat typ</param>
		/// <param name="chatLoc">The chat location</param>
		/// <param name="distance">The distance</param>
		/// <param name="excludes">A list of GameObjects to exlude from the message</param>
		public static void MessageToArea(GameObject centerObject, string message, eChatType chatType, eChatLoc chatLoc, ushort distance, params GameObject[] excludes)
		{
			if (message == null || message.Length <= 0) return;
			bool excluded;
			foreach(GamePlayer player in centerObject.GetPlayersInRadius(distance))
			{
				excluded = false;
				if(excludes!=null)
				{
					foreach(GameObject obj in excludes)
						if(obj == player)
						{
							excluded = true;
							break;
						}
				}
				if(!excluded)
					player.Out.SendMessage(message, chatType, chatLoc);
			}
		}
	}
}
