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
using DOL.GS.Database;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xD1^168,"Handles player selling")]
	public class PlayerSellRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort item_slot=packet.ReadShort();
			
			new SellActionHandler(client.Player, item_slot).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles item apprise actions
		/// </summary>
		protected class SellActionHandler : RegionAction
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
			public SellActionHandler(GamePlayer actionSource, int slot) : base(actionSource)
			{
				m_slot = slot;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				GenericItem item = player.Inventory.GetItem((eInventorySlot)m_slot) as GenericItem;
				if(item == null)
					return;

				IGameMerchant merchant = player.TargetObject as IGameMerchant;
				if(merchant == null)
				{
					player.Out.SendMessage("Only a merchant can buy your item.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
					return;
				}

				if(!item.IsSaleable)
				{
					player.Out.SendMessage("This item can't be sold.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
					return;
				}

				if(!player.TargetObject.Position.CheckSquareDistance(player.Position, (uint)(WorldMgr.PICKUP_DISTANCE*WorldMgr.PICKUP_DISTANCE))) // tested
				{ 
					player.Out.SendMessage(player.TargetObject.Name +" is too far away!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					return;
				}
					
				if(!merchant.OnPlayerSell(player, item))
				{
					player.Out.SendMessage("This merchant can't buy your item.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
					return;
				}
			
				if (!player.Inventory.RemoveItem(item))
				{
					player.Out.SendMessage("No possible to remove this item from your inventory !!!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					return;
				}

				long itemValue = item.Value / 2;
				
				string message = null;
				if(itemValue > 0)
				{
					message = player.TargetObject.GetName(0, true)+" gives you {0} for "+item.Name+".";
				}
				else
				{
					message = "You gain nothing for "+item.Name+".";
				}

				player.AddMoney(itemValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
			}
		}
	}
}