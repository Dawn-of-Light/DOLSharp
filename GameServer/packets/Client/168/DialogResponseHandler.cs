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

using DOL.Events;
using DOL.GS.Housing;
using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.DialogResponse, ClientStatus.PlayerInGame)]
	public class DialogResponseHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort data1 = packet.ReadShort();
			ushort data2 = packet.ReadShort();
			ushort data3 = packet.ReadShort();
			var messageType = (byte) packet.ReadByte();
			var response = (byte) packet.ReadByte();

			new DialogBoxResponseAction(client.Player, data1, data2, data3, messageType, response).Start(1);
		}

		#endregion

		#region Nested type: DialogBoxResponseAction

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
			protected readonly byte m_response;

			/// <summary>
			/// Constructs a new DialogBoxResponseAction
			/// </summary>
			/// <param name="actionSource">The responding player</param>
			/// <param name="data1">The general data field</param>
			/// <param name="data2">The general data field</param>
			/// <param name="data3">The general data field</param>
			/// <param name="messageType">The dialog type</param>
			/// <param name="response">The players response</param>
			public DialogBoxResponseAction(GamePlayer actionSource, int data1, int data2, int data3, int messageType,
			                               byte response)
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
				var player = (GamePlayer) m_actionSource;

				switch ((eDialogCode) m_messageType)
				{
					case eDialogCode.CustomDialog:
						{
							if (m_data2 == 0x01)
							{
								CustomDialogResponse callback;
								lock (player)
								{
									callback = player.CustomDialogCallback;
									player.CustomDialogCallback = null;
								}

								if (callback == null)
									return;

								callback(player, m_response);
							}
							break;
						}
					case eDialogCode.GuildInvite:
						{
							var guildLeader = WorldMgr.GetObjectByIDFromRegion(player.CurrentRegionID, (ushort) m_data1) as GamePlayer;
							if (m_response == 0x01) //accept
							{
								if (guildLeader == null)
								{
									player.Out.SendMessage("You need to be in the same region as the guild leader to accept an invitation.",
									                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (player.Guild != null)
								{
									player.Out.SendMessage("You are still in a guild, you'll have to leave it first.", eChatType.CT_System,
									                       eChatLoc.CL_SystemWindow);
									return;
								}
								if (guildLeader.Guild != null)
								{
									guildLeader.Guild.AddPlayer(player);
									return;
								}

								player.Out.SendMessage("Player doing the invite is not in a guild!", eChatType.CT_System,
								                       eChatLoc.CL_SystemWindow);
								return;
							}

							if (guildLeader != null)
							{
								guildLeader.Out.SendMessage(player.Name + " declined your invite.", eChatType.CT_System,
								                            eChatLoc.CL_SystemWindow);
							}
							return;
						}
					case eDialogCode.GuildLeave:
						{
							if (m_response == 0x01) //accepte
							{
								if (player.Guild == null)
								{
									player.Out.SendMessage("You are not in a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}

								player.Guild.RemovePlayer(player.Name, player);
							}
							else
							{
								player.Out.SendMessage("You decline to quit your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
							break;
						}
					case eDialogCode.QuestSubscribe:
						{
							var questNPC = (GameLiving) WorldMgr.GetObjectByIDFromRegion(player.CurrentRegionID, (ushort) m_data2);
							if (questNPC == null)
								return;

							var args = new QuestEventArgs(questNPC, player, (ushort) m_data1);
							if (m_response == 0x01) //accept
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
							if (m_data3 == 0x01)
							{
								player.Notify(GamePlayerEvent.ContinueQuest, player, args);
							}
							else
							{
								player.Notify(GamePlayerEvent.DeclineQuest, player, args);
							}
							return;
						}
					case eDialogCode.GroupInvite:
						{
							if (m_response == 0x01)
							{
								GameClient cln = WorldMgr.GetClientFromID(m_data1);
								if (cln == null)
									return;

								GamePlayer groupLeader = cln.Player;
								if (groupLeader == null)
									return;

								if (player.Group != null)
								{
									player.Out.SendMessage("You are still in a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (!GameServer.ServerRules.IsAllowedToGroup(groupLeader, player, false))
								{
									return;
								}
								if (player.InCombatPvE)
								{
									player.Out.SendMessage("You can't join a group while in combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (groupLeader.Group != null)
								{
									if (groupLeader.Group.Leader != groupLeader) return;
                                    if (groupLeader.Group.MemberCount >= ServerProperties.Properties.GROUP_MAX_MEMBER)
									{
										player.Out.SendMessage("The group is full.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return;
									}
									groupLeader.Group.AddMember(player);
									GameEventMgr.Notify(GamePlayerEvent.AcceptGroup, player);
									return;
								}

								var group = new Group(groupLeader);
								GroupMgr.AddGroup(group, group);

								group.AddMember(groupLeader);
								group.AddMember(player);

								GameEventMgr.Notify(GamePlayerEvent.AcceptGroup, player);

								return;
							}
							break;
						}
					case eDialogCode.KeepClaim:
						{
							if (m_response == 0x01)
							{
								if (player.Guild == null)
								{
									player.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!",
									                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}

								AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(player.CurrentRegionID, player, WorldMgr.VISIBILITY_DISTANCE);
								if (keep == null)
								{
									player.Out.SendMessage("You have to be near the keep to claim it.", eChatType.CT_System,
									                       eChatLoc.CL_SystemWindow);
									return;
								}

								if (keep.CheckForClaim(player))
								{
									keep.Claim(player);
								}
								break;
							}
							break;
						}
					case eDialogCode.HousePayRent:
						{
							if (m_response == 0x00)
							{
								if (player.TempProperties.getProperty<long>(HousingConstants.MoneyForHouseRent, -1) != -1)
								{
									player.TempProperties.removeProperty(HousingConstants.MoneyForHouseRent);
								}

								if (player.TempProperties.getProperty<long>(HousingConstants.BPsForHouseRent, -1) != -1)
								{
									player.TempProperties.removeProperty(HousingConstants.BPsForHouseRent);
								}

								player.TempProperties.removeProperty(HousingConstants.HouseForHouseRent);

								return;
							}

							var house = player.TempProperties.getProperty<House>(HousingConstants.HouseForHouseRent, null);
							var moneyToAdd = player.TempProperties.getProperty<long>(HousingConstants.MoneyForHouseRent, -1);
							var bpsToMoney = player.TempProperties.getProperty<long>(HousingConstants.BPsForHouseRent, -1);

							if (moneyToAdd != -1)
							{
								// if we're giving money and already have some in the lockbox, make sure we don't
								// take more than what would cover 4 weeks of rent.
								if (moneyToAdd + house.KeptMoney > HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS)
									moneyToAdd = (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS) - house.KeptMoney;

								// take the money from the player
								if (!player.RemoveMoney(moneyToAdd))
									return;

								// add the money to the lockbox
								house.KeptMoney += moneyToAdd;

								// save the house and the player
								house.SaveIntoDatabase();
								player.SaveIntoDatabase();

								// notify the player of what we took and how long they are prepaid for
								player.Out.SendMessage("You deposit " + Money.GetString(moneyToAdd) + " in the lockbox.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage("The lockbox now has " + Money.GetString(house.KeptMoney) + " in it.  The weekly payment is " +
									Money.GetString(HouseMgr.GetRentByModel(house.Model)) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage("The house is now prepaid for the next " + (house.KeptMoney/HouseMgr.GetRentByModel(house.Model)) +
									" payments.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

								// clean up
								player.TempProperties.removeProperty(HousingConstants.MoneyForHouseRent);
							}
							else
							{
								if (bpsToMoney + house.KeptMoney > HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS)
									bpsToMoney = (HouseMgr.GetRentByModel(house.Model) * ServerProperties.Properties.RENT_LOCKBOX_PAYMENTS) - house.KeptMoney;

								if (!player.RemoveBountyPoints(Money.GetGold(bpsToMoney)))
									return;

								// add the bps to the lockbox
								house.KeptMoney += bpsToMoney;

								// save the house and the player
								house.SaveIntoDatabase();
								player.SaveIntoDatabase();

								// notify the player of what we took and how long they are prepaid for
								player.Out.SendMessage("You deposit " + Money.GetString(bpsToMoney) + " in the lockbox.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage("The lockbox now has " + Money.GetString(house.KeptMoney) + " in it.  The weekly payment is " +
									Money.GetString(HouseMgr.GetRentByModel(house.Model)) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage("The house is now prepaid for the next " + (house.KeptMoney/HouseMgr.GetRentByModel(house.Model)) +
									" payments.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

								// clean up
								player.TempProperties.removeProperty(HousingConstants.BPsForHouseRent);
							}

							// clean up
							player.TempProperties.removeProperty(HousingConstants.MoneyForHouseRent);
							break;
						}
					case eDialogCode.MasterLevelWindow:
						{
							player.Out.SendMasterLevelWindow(m_response);
							break;
						}
				}
			}
		}

		#endregion
	}
}