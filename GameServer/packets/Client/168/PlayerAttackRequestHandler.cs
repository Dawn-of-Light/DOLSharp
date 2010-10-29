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
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerAttackRequest, ClientStatus.PlayerInGame)]
	public class PlayerAttackRequestHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			var mode = (byte) packet.ReadByte();
			bool userAction = packet.ReadByte() == 0;
				// set to 0 if user pressed the button, set to 1 if client decided to stop attack

			new AttackRequestHandler(client.Player, mode != 0, userAction).Start(1);
		}

		#endregion

		#region Nested type: AttackRequestHandler

		/// <summary>
		/// Handles change attack mode requests
		/// </summary>
		protected class AttackRequestHandler : RegionAction
		{
			/// <summary>
			/// True if attack should be started
			/// </summary>
			protected readonly bool m_start;

			/// <summary>
			/// True if user initiated the action else was done by the client
			/// </summary>
			protected readonly bool m_userAction;

			/// <summary>
			/// Constructs a new AttackRequestHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="start">True if attack should be started</param>
			/// <param name="userAction">True if user initiated the action else was done by the client</param>
			public AttackRequestHandler(GamePlayer actionSource, bool start, bool userAction) : base(actionSource)
			{
				m_start = start;
				m_userAction = userAction;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				if (player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
				{
					if (m_userAction)
						player.Out.SendMessage("You can't enter melee combat mode with a fired weapon!", eChatType.CT_YouHit,
						                       eChatLoc.CL_SystemWindow);
					return;
				}

				if (m_start)
				{
					player.StartAttack(player.TargetObject);
					// unstealth right after entering combat mode if anything is targeted
					if (player.AttackState && player.TargetObject != null)
						player.Stealth(false);
				}
				else
				{
					player.StopAttack(false);
				}
			}
		}

		#endregion
	}
}