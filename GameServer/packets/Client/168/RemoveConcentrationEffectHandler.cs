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
using DOL.GS.Effects;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Called when player removes concentration spell in conc window
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xDE^168,"Handles remove conc effect requests from client")]
	public class RemoveConcentrationEffectHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int index = packet.ReadByte();
			new CancelEffectHandler(client.Player, index).Start(1);
			return 1;
		}

		/// <summary>
		/// Handles player cancel effect requests
		/// </summary>
		protected class CancelEffectHandler : RegionAction
		{
			/// <summary>
			/// The effect index
			/// </summary>
			protected readonly int m_index;

			/// <summary>
			/// Constructs a new CancelEffectHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="index">The effect index</param>
			public CancelEffectHandler(GamePlayer actionSource, int index) : base(actionSource)
			{
				m_index = index;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				IConcentrationEffect effect = null;
				lock (player.ConcentrationEffects) 
				{
					if (m_index < player.ConcentrationEffects.Count) 
					{
						effect = player.ConcentrationEffects[m_index];
						player.ConcentrationEffects.Remove(effect);
					}
				}
				if (effect != null)
					effect.Cancel(false);
			}
		}
	}
}
