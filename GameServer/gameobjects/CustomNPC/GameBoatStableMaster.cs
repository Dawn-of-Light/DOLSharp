
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
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Stable master that sells and takes horse route tickes
	/// </summary>
	public class GameBoatStableMaster : GameMerchant
	{
		/// <summary>
		/// Called when a player buys an item
		/// </summary>
		/// <param name="player">The player making the purchase</param>
		/// <param name="item_slot">slot of the item to be bought</param>
		/// <param name="number">Number to be bought</param>
		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			//Get the template
			int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
			if (template == null) return;

			//Calculate the amout of items
			int amountToBuy = number;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0) return;

			//Calculate the value of items
			long totalValue = number * template.Price;

			GameInventoryItem item = GameInventoryItem.Create(template);

			lock (player.Inventory)
			{

				if (player.GetCurrentMoney() < totalValue)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (!player.Inventory.AddTemplate(item, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
				//Generate the buy message
				string message;
				if (amountToBuy > 1)
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
				else
					message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

				// Check if player has enough money and subtract the money
				if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					throw new Exception("Money amount changed while adding items.");
				}
				InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalValue);
			}

			if (item.Name.ToUpper().Contains("TICKET TO") || item.Description.ToUpper() == "TICKET")
			{
				// Give the ticket to the merchant
				InventoryItem ticket = player.Inventory.GetFirstItemByName(item.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) as InventoryItem;
				if (ticket != null)
					ReceiveItem(player, ticket);
			}
		}

		/// <summary>
		/// Called when the living is about to get an item from someone
		/// else
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			if (source is GamePlayer)
			{
				GamePlayer player = (GamePlayer)source;

                if (item.Name.ToLower().StartsWith(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStableMaster.ReceiveItem.TicketTo")) && item.Item_Type == 40)
				{
					foreach (GameNPC npc in GetNPCsInRadius(1500))
					{
						if (npc is GameTaxiBoat)
						{
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameBoatStableMaster.ReceiveItem.Departed", this.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            return false;
						}
					}

                    String destination = item.Name.Substring(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStableMaster.ReceiveItem.TicketTo").Length);
					PathPoint path = MovementMgr.LoadPath(item.Id_nb);
					//PathPoint path = MovementMgr.Instance.LoadPath(this.Name + "=>" + destination);
                    if ((path != null) && ((Math.Abs(path.X - this.X)) < 500) && ((Math.Abs(path.Y - this.Y)) < 500))
					{
						player.Inventory.RemoveCountFromStack(item, 1);
                        InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template);

						GameTaxiBoat boat = new GameTaxiBoat();
						boat.Name = "Boat to " + destination;
						boat.Realm = source.Realm;
						boat.X = path.X;
						boat.Y = path.Y;
						boat.Z = path.Z;
						boat.CurrentRegion = CurrentRegion;
                        boat.Heading = path.GetHeading( path.Next );
						boat.AddToWorld();
						boat.CurrentWayPoint = path;
						GameEventMgr.AddHandler(boat, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
						//new MountHorseAction(player, boat).Start(400);
						new HorseRideAction(boat).Start(30 * 1000);

                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameBoatStableMaster.ReceiveItem.SummonedBoat", this.Name, destination), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return true;
					}
					else
					{
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameBoatStableMaster.ReceiveItem.UnknownWay", this.Name, destination), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// Handles 'horse route end' events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="o"></param>
		/// <param name="args"></param>
		public void OnHorseAtPathEnd(DOLEvent e, object o, EventArgs args)
		{
			if (!(o is GameNPC)) return;
			GameNPC npc = (GameNPC)o;
			GameEventMgr.RemoveHandler(npc, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
			npc.StopMoving();
			npc.RemoveFromWorld();
		}

		/// <summary>
		/// Handles delayed player mount on horse
		/// </summary>
		protected class MountHorseAction : RegionAction
		{
			/// <summary>
			/// The target horse
			/// </summary>
			protected readonly GameNPC m_horse;

			/// <summary>
			/// Constructs a new MountHorseAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="horse">The target horse</param>
			public MountHorseAction(GamePlayer actionSource, GameNPC horse)
				: base(actionSource)
			{
				if (horse == null)
					throw new ArgumentNullException("horse");
				m_horse = horse;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				player.MountSteed(m_horse, true);
			}
		}

		/// <summary>
		/// Handles delayed horse ride actions
		/// </summary>
		protected class HorseRideAction : RegionAction
		{
			/// <summary>
			/// Constructs a new HorseStartAction
			/// </summary>
			/// <param name="actionSource"></param>
			public HorseRideAction(GameNPC actionSource)
				: base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC horse = (GameNPC)m_actionSource;
				horse.MoveOnPath(horse.MaxSpeed);
			}
		}
	}
}
