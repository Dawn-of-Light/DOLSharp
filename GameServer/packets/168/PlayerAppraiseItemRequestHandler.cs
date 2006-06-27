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
using DOL.GS.Housing;
using DOL.GS.Database;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x48^168,"Player Appraise Item")]
	public class PlayerAppraiseItemRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint X = packet.ReadInt();
			uint Y = packet.ReadInt();
			ushort id =(ushort) packet.ReadShort();
			ushort item_slot=(ushort) packet.ReadShort();

			new AppraiseActionHandler(client.Player, item_slot).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles item apprise actions
		/// </summary>
		protected class AppraiseActionHandler : RegionAction
		{
			/// <summary>
			/// The item slot
			/// </summary>
			protected readonly int m_slot;

			/// <summary>
			/// Constructs a new AppraiseAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="slot">The item slot</param>
			public AppraiseActionHandler(GamePlayer actionSource, int slot) : base(actionSource)
			{
				m_slot = slot;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				if(player.TargetObject==null)
					return;

				GenericItem item = player.Inventory.GetItem((eInventorySlot)m_slot);
				if(item == null)
					return;

				if(!item.IsSaleable)
				{
					player.Out.SendMessage("This item can't be sold.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
					return;
				}

				long val = 0;
				if(player.TargetObject is GameMerchant)
				{
					val = ((GameMerchant)player.TargetObject).OnPlayerAppraise(player,item);
				}
				else if (player.TargetObject is GameLotMarker)
				{
					val = ((GameLotMarker)player.TargetObject).OnPlayerAppraise(player,item);
				}
				else
				{
					//If the player is apraising with another target, add the handling here in else if blocks
					return;
				}

				string message;
				if(val == 0)
				{
					if(player.TargetObject is GameLiving)
						message = player.TargetObject.GetName(0, true)+" isn't interested in "+item.Name;
					else
						message = item.Name + " isn't worth any value!";
				}
				else
				{
					if(player.TargetObject is GameLiving)
						message = player.TargetObject.GetName(0, true)+" offers you "+Money.GetString(val)+" for "+item.Name;
					else
						message = "You would gain "+Money.GetString(val)+" for "+item.Name;
				}
				player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
		}
	}
}