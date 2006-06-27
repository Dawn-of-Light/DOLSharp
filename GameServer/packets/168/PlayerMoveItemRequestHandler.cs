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
using DOL.Events;
using DOL.GS;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x75^168,"Player move item")]
	public class PlayerMoveItemRequestHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort id		= (ushort) packet.ReadShort();
			ushort toSlot	= (ushort) packet.ReadShort();
			ushort fromSlot = (ushort) packet.ReadShort();
			ushort itemCount= (ushort) packet.ReadShort();

//			log.Warn("MoveItem, id="+id.ToString()+" toSlot="+toSlot.ToString()+" fromSlot="+fromSlot.ToString()+" itemCount="+itemCount.ToString());

			//GSMessages.SendMessage(client,id.ToString()+"("+Number.ToString()+"): ["+From.ToString()+"] -> ["+To.ToString()+"]",GSMessages.eChatType.CT_Say,GSMessages.eChatLoc.CL_SystemWindow);
					
			//If our slot is > 1000 it is (objectID+1000) of target
			if(toSlot>1000)
			{
				ushort objectID = (ushort)(toSlot-1000);
				GameObject obj = client.Player.Region.GetObject(objectID);
				if(obj==null || obj.ObjectState!=GameObject.eObjectState.Active)
				{
					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					client.Out.SendMessage("Invalid trade target. ("+objectID+")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}

				GamePlayer tradeTarget = null;
				//If our target is another player we set the tradetarget
				if(obj is GamePlayer)
				{
					tradeTarget = (GamePlayer) obj;
					if(tradeTarget.Client.ClientState != GameClient.eClientState.Playing)
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						client.Out.SendMessage("Can't trade with inactive players.", eChatType.CT_System,  eChatLoc.CL_SystemWindow);
						return 0;
					}
					if(tradeTarget == client.Player)
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						client.Out.SendMessage("You can't trade with yourself, silly!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 0;
					}
					if(!GameServer.ServerRules.IsAllowedToTrade(client.Player, tradeTarget, false))
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 0;
					}
				}

				if(!obj.Position.CheckSquareDistance(client.Player.Position, (uint)(WorldMgr.GIVE_ITEM_DISTANCE*WorldMgr.GIVE_ITEM_DISTANCE)))
				{ 
					client.Out.SendMessage("You are too far away to give anything to " + obj.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					return 0;
				}

				//Is the item we want to move in our backpack?
				if(fromSlot>=(ushort)eInventorySlot.FirstBackpack && fromSlot<=(ushort)eInventorySlot.LastBackpack)
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)fromSlot);
					if(item == null)
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						client.Out.SendMessage("Invalid item (slot# "+fromSlot+").",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 0;
					}

					client.Player.Notify(GamePlayerEvent.GiveItem, client.Player, new GiveItemEventArgs(client.Player, obj, item));
					
					//If the item has been removed by the event handlers, return;
					//item = client.Player.Inventory.GetItem((eInventorySlot)fromSlot);
					if(item == null || item.Owner == null) 
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 0;
					}

					if(tradeTarget!=null)
					{
						tradeTarget.ReceiveTradeItem(client.Player,item);
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 1;
					}
					
					if(obj.ReceiveItem(client.Player, item))
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 0;
					}

					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					return 0;
				}
				
				//Is the "item" we want to move money? For Version 1.78+
				if(client.Version >= GameClient.eClientVersion.Version178
				   && fromSlot >= (int)eInventorySlot.Mithril178 && fromSlot <= (int)eInventorySlot.Copper178)
				{
					fromSlot -= eInventorySlot.Mithril178 - eInventorySlot.Mithril;
				}
				
				//Is the "item" we want to move money?
				if(fromSlot>=(ushort)eInventorySlot.Mithril && fromSlot<=(ushort)eInventorySlot.Copper)
				{
					int[] money=new int[5];
					money[fromSlot-(ushort)eInventorySlot.Mithril]=itemCount;
					long flatmoney = Money.GetMoney(money[0],money[1],money[2],money[3],money[4]);

					if(client.Version >= GameClient.eClientVersion.Version178) // add it back for proper slot update...
						fromSlot += eInventorySlot.Mithril178 - eInventorySlot.Mithril;

					if(flatmoney > client.Player.GetCurrentMoney())
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 0;
					}

					client.Player.Notify(GamePlayerEvent.GiveMoney, client.Player, new GiveMoneyEventArgs(client.Player, obj, flatmoney));

					if(tradeTarget!=null)
					{
						tradeTarget.ReceiveTradeMoney(client.Player, flatmoney);
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 1;
					}
					
					if(obj.ReceiveMoney(client.Player, flatmoney))
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						return 0;
					}

					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					return 0;
				}

				client.Out.SendInventoryItemsUpdate(null);
				return 0;
			}
	
			//Do we want to move an item from inventory/vault/quiver into inventory/vault/quiver?
			if (((fromSlot>=(ushort)eInventorySlot.Ground && fromSlot<=(ushort)eInventorySlot.LastBackpack)
				|| (fromSlot>=(ushort)eInventorySlot.FirstVault && fromSlot<=(ushort)eInventorySlot.LastVault))
				&&((toSlot>=(ushort)eInventorySlot.Ground && toSlot<=(ushort)eInventorySlot.LastBackpack)
				|| (toSlot>=(ushort)eInventorySlot.FirstVault && toSlot<=(ushort)eInventorySlot.LastVault)))
			{
				//We want to drop the item
				if (toSlot==(ushort)eInventorySlot.Ground)
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)fromSlot);
					if (item == null)
					{
						client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
						client.Out.SendMessage("Invalid item (slot# "+fromSlot+").",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 0;
					}

					if (client.Player.DropItem(item))
					{
						client.Out.SendMessage("You drop " + item.Name + " on the ground!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					return 0;
				}
				//We want to move the item in inventory
				client.Player.Inventory.MoveItem((eInventorySlot)fromSlot, (eInventorySlot)toSlot, itemCount);
				return 1;
			}

			
			if (((fromSlot>=(ushort)eInventorySlot.Ground && fromSlot<=(ushort)eInventorySlot.LastBackpack)
				|| (fromSlot>=(ushort)eInventorySlot.FirstVault && fromSlot<=(ushort)eInventorySlot.LastVault))
				&& toSlot==(ushort)eInventorySlot.PlayerPaperDoll)
			{
				EquipableItem item = client.Player.Inventory.GetItem((eInventorySlot)fromSlot) as EquipableItem;
				if(item==null)
				{
					client.Out.SendMessage("This item can't be equipped!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 0;
				}
				toSlot=0;

				foreach(eInventorySlot slot in item.EquipableSlot)
				{
					if(null == client.Player.Inventory.GetItem(slot))
					{
						toSlot = (ushort)slot;
						break;
					}
				}
	
				if (toSlot==0)
				{
					client.Out.SendInventorySlotsUpdate(new int[] {fromSlot});
					return 0;
				}
				client.Player.Inventory.MoveItem((eInventorySlot)fromSlot,(eInventorySlot)toSlot, itemCount);
				return 1;
			}
			client.Out.SendInventoryItemsUpdate(null);
			return 0;
		}
	}
}
