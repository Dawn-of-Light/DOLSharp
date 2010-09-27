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
namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.InviteToGroup, ClientStatus.PlayerInGame)]
	public class InviteToGroupHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			new HandleGroupInviteAction(client.Player).Start(1);

			return 1;
		}

		#endregion

		#region Nested type: HandleGroupInviteAction

		/// <summary>
		/// Handles group invlite actions
		/// </summary>
		protected class HandleGroupInviteAction : RegionAction
		{
			/// <summary>
			/// constructs a new HandleGroupInviteAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public HandleGroupInviteAction(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				if (player.TargetObject == null || player.TargetObject == player)
				{
					ChatUtil.SendSystemMessage(player, "You have not selected a valid player as your target.");
					return;
				}

				if (!(player.TargetObject is GamePlayer))
				{
					ChatUtil.SendSystemMessage(player, "You have not selected a valid player as your target.");
					return;
				}

				var target = (GamePlayer) player.TargetObject;

				if (player.Group != null && player.Group.Leader != player)
				{
					ChatUtil.SendSystemMessage(player, "You are not the leader of your group.");
					return;
				}
                
                if (player.Group != null && player.Group.MemberCount >= ServerProperties.Properties.GROUP_MAX_MEMBER)
				{
					ChatUtil.SendSystemMessage(player, "The group is full.");
					return;
				}

				if (!GameServer.ServerRules.IsAllowedToGroup(player, target, false))
					return;

				if (target.Group != null)
				{
					ChatUtil.SendSystemMessage(player, "The player is still in a group.");
					return;
				}

				ChatUtil.SendSystemMessage(player, "You have invited " + target.Name + " to join your group.");
				target.Out.SendGroupInviteCommand(player,
				                                  player.Name + " has invited you to join\n" + player.GetPronoun(1, false) +
				                                  " group. Do you wish to join?");
				ChatUtil.SendSystemMessage(target,
				                           player.Name + " has invited you to join " + player.GetPronoun(1, false) + " group.");
			}
		}

		#endregion
	}
}