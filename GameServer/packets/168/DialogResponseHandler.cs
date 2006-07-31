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
using DOL.Events;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x2A ^ 168, "handle DialogBox Response")]
	public class DialogResponseHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort data1 = packet.ReadShort();
			ushort data2 = packet.ReadShort();
			ushort data3 = packet.ReadShort();
			byte messageType = (byte)packet.ReadByte();
			byte response = (byte)packet.ReadByte();

			//DOLConsole.WriteLine("MessageType="+messageType+" Response="+response+" data1="+data1+" data2="+data2+" data3="+data3);

			new DialogBoxResponseAction(client.Player, data1, data2, data3, messageType, response)
				.Start(1);

			return 1;
		}

		/// <summary>
		/// Handles dialog responses from players
		/// </summary>
		protected class DialogBoxResponseAction : RegionAction
		{
			/// <summary>
			/// The general data field
			/// </summary>
			protected readonly int m_data1;
			/// <summary>
			/// The general data field
			/// </summary>
			protected readonly int m_data2;
			/// <summary>
			/// The general data field
			/// </summary>
			protected readonly int m_data3;
			/// <summary>
			/// The dialog type
			/// </summary>
			protected readonly int m_messageType;
			/// <summary>
			/// The players response
			/// </summary>
			protected readonly int m_response;

			/// <summary>
			/// Constructs a new DialogBoxResponseAction
			/// </summary>
			/// <param name="actionSource">The responding player</param>
			/// <param name="data1">The general data field</param>
			/// <param name="data2">The general data field</param>
			/// <param name="data3">The general data field</param>
			/// <param name="messageType">The dialog type</param>
			/// <param name="response">The players response</param>
			public DialogBoxResponseAction(GamePlayer actionSource, int data1, int data2, int data3, int messageType, int response)
				: base(actionSource)
			{
				m_data1 = data1;
				m_data2 = data2;
				m_data3 = data3;
				m_messageType = messageType;
				m_response = response;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				if (m_messageType == 0x06 && m_data2 == 0x01) //Custom dialog
				{
					CustomDialogResponse callback = null;
					lock (player)
					{
						callback = player.CustomDialogCallback;
						player.CustomDialogCallback = null;
					}
					if (callback == null) return;
					callback(player, (byte)m_response);
					return;
				}
				else if (m_messageType == 0x03)// if Message is guild invit
				{
					GamePlayer guildLeader = WorldMgr.GetObjectByIDFromRegion(player.CurrentRegionID, (ushort)m_data1) as GamePlayer;
					if (m_response == 0x01)//accepte
					{
						if (guildLeader == null)
						{
							return;
						}
						if (player.Guild != null)
						{
							player.Out.SendMessage("You are still in a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						if (guildLeader.Guild != null)
						{
							guildLeader.Guild.AddPlayer(player);
							return;
						}
						else
						{
							player.Out.SendMessage("You are not in a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
					}
					else
					{
						if (guildLeader != null)
							guildLeader.Out.SendMessage(player.Name + " declined your invite.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}
				else if (m_messageType == 0x08)// if Message is guild leave
				{
					if (m_response == 0x01)//accepte
					{
						if (player.Guild == null)
						{
							player.Out.SendMessage("You are not in a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						else
						{
							player.Guild.RemovePlayer(player.Name, player);
						}
					}
					else
					{
						player.Out.SendMessage("You decline to quit your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}
				else if (m_messageType == 0x64)// if Message is quest dialogue
				{
					GameLiving questNPC = (GameLiving)WorldMgr.GetObjectByIDFromRegion(player.CurrentRegionID, (ushort)m_data2);
					if (questNPC == null)
						return;

					QuestEventArgs args = new QuestEventArgs(questNPC, player, (ushort)m_data1);
					if (m_response == 0x01)//accept
					{
						//TODO add quest to player
						//Note: This is done withing quest code since we have to check requirements, etc for each quest individually
						// i'm reusing the questsubscribe command for quest abort since its 99% the same, only different event dets fired
						if (m_data3 == 0x01)
							player.Notify(GamePlayerEvent.AbortQuest, player, args);
						else
							player.Notify(GamePlayerEvent.AcceptQuest, player, args);
						return;
					}
					else
					{
						if (m_data3 == 0x01)
							player.Notify(GamePlayerEvent.ContinueQuest, player, args);
						else
							player.Notify(GamePlayerEvent.DeclineQuest, player, args);
						//player.Out.SendMessage("You decline to subcribe to quest", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}
				else if (m_messageType == 0x05 && m_response == 0x01)// if Message is group invit and Response is Yes
				{
					GameClient cln = WorldMgr.GetClientFromID(m_data1);
					if (cln == null) return;
					GamePlayer groupLeader = cln.Player;
					if (groupLeader == null) return;
					if (player.PlayerGroup != null)
					{
						player.Out.SendMessage("You are still in a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (!GameServer.ServerRules.IsAllowedToGroup(groupLeader, player, false))
					{
						return;
					}
					if (player.InCombat && !player.CurrentRegion.IsRvR)
					{
						player.Out.SendMessage("You can't join a group while in combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (groupLeader.PlayerGroup != null)
					{
						if (groupLeader.PlayerGroup.Leader != groupLeader) return;
						if (groupLeader.PlayerGroup.PlayerCount >= PlayerGroup.MAX_GROUP_SIZE)
						{
							player.Out.SendMessage("The group is full.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						if (groupLeader.PlayerGroup.IsGroupInCombat())
						{
							player.Out.SendMessage("You can't join a group that is in combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						groupLeader.PlayerGroup.AddPlayer(player);
						return;
					}
					else
					{
						PlayerGroup group = new PlayerGroup(groupLeader);
						GroupMgr.AddGroup(group, group);
						groupLeader.PlayerGroup = group;
						group.AddPlayer(groupLeader);
						group.AddPlayer(player);
						return;
					}
				}
			}
		}
	}
}
