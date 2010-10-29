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
using DOL.GS.Effects;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handles effect cancel requests
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerCancelsEffect, ClientStatus.PlayerInGame)]
	public class PlayerCancelsEffectHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int effectID = packet.ReadShort();

			new CancelEffectHandler(client.Player, effectID).Start(1);
		}

		#endregion

		#region Nested type: CancelEffectHandler

		/// <summary>
		/// Handles players cancel effect actions
		/// </summary>
		protected class CancelEffectHandler : RegionAction
		{
			/// <summary>
			/// The effect Id
			/// </summary>
			protected readonly int m_effectId;

			/// <summary>
			/// Constructs a new CancelEffectHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="effectId">The effect Id</param>
			public CancelEffectHandler(GamePlayer actionSource, int effectId) : base(actionSource)
			{
				m_effectId = effectId;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				IGameEffect found = null;
				lock (player.EffectList)
				{
					foreach (IGameEffect effect in player.EffectList)
					{
						if (effect.InternalID == m_effectId)
						{
							found = effect;
							break;
						}
					}
				}
				if (found != null)
				{
					found.Cancel(true);
				}
			}
		}

		#endregion
	}
}