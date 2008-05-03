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
using System.Reflection;
using DOL.GS;
using DOL.Database2;
using DOL.GS.PacketHandler;


namespace DOL.GS.Commands
{
	[CmdAttribute("&merchant", //command to handle
		 ePrivLevel.GM, //minimum privelege level
		 "Various merchant creation commands!", //command description
		//Usage
		 "'/merchant create' to create an empty merchant",
		 "'/merchant info' to show info about merchant",
		 "'/merchant save' to save this merchant as new object in the DB",
		 "'/merchant remove' to remove this merchant from the DB",
		 "'/merchant sell <itemsListID>' to assign this merchant with an articles list template",
		 "'/merchant sellremove' to remove the articles list template from merchant",
		 "'/merchant articles add <itemTemplateID> <pageNumber> [slot]' to add an item to the merchant articles list template",
		 "'/merchant articles remove <pageNumber> <slot>' to remove item from the specified slot in this merchant inventory articles list template",
		 "'/merchant articles delete' to delete the inventory articles list template of the merchant",
		 "'/merchant type <classtype>")]
	public class MerchantCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			string param = "";
			if (args.Length > 2)
				param = String.Join(" ", args, 2, args.Length - 2);

			GameMerchant targetMerchant = null;
			if (client.Player.TargetObject != null && client.Player.TargetObject is GameMerchant)
				targetMerchant = (GameMerchant)client.Player.TargetObject;

			if (args[1] != "create" && targetMerchant == null)
			{
				DisplayMessage(client, "Type /merchant for command overview");
				return;
			}

			switch (args[1].ToLower())
			{
				case "create":
					{
						string theType = "DOL.GS.GameMerchant";
						if (args.Length > 2)
							theType = args[2];

						//Create a new merchant
						GameMerchant merchant = null;
						ArrayList asms = new ArrayList(ScriptMgr.Scripts);
						asms.Add(typeof(GameServer).Assembly);
						foreach (Assembly script in asms)
						{
							try
							{
								client.Out.SendDebugMessage(script.FullName);
								merchant = (GameMerchant)script.CreateInstance(theType, false);
								if (merchant != null)
									break;
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						if (merchant == null)
						{
							DisplayMessage(client, "There was an error creating an instance of " + theType + "!");
							return;
						}
						//Fill the object variables
						merchant.X = client.Player.X;
						merchant.Y = client.Player.Y;
						merchant.Z = client.Player.Z;
						merchant.CurrentRegion = client.Player.CurrentRegion;
						merchant.Heading = client.Player.Heading;
						merchant.Level = 1;
						merchant.Realm = client.Player.Realm;
						merchant.Name = "New merchant";
						merchant.Model = 9;
						//Fill the living variables
						merchant.CurrentSpeed = 0;
						merchant.MaxSpeedBase = 200;
						merchant.GuildName = "";
						merchant.Size = 50;
						merchant.AddToWorld();
						merchant.SaveIntoDatabase();
						DisplayMessage(client, "Merchant created: OID=" + merchant.ObjectID);
					}
					break;

				case "info":
					{
						if (args.Length == 2)
						{
							if (targetMerchant.TradeItems == null)
								DisplayMessage(client, "Merchant articles list is empty!");
							else
							{
								DisplayMessage(client, "Merchant articles list: \"" + targetMerchant.TradeItems.ItemsListID + "\"");
							}
						}
					}
					break;

				case "save":
					{
						targetMerchant.SaveIntoDatabase();
						DisplayMessage(client, "Target Merchant saved in DB!");
					}
					break;

				case "remove":
					{
						targetMerchant.DeleteFromDatabase();
						targetMerchant.Delete();
						DisplayMessage(client, "Target Merchant removed from DB!");
					}
					break;

				case "sell":
					{
						if (args.Length == 3)
						{
							try
							{
								string templateID = args[2];
								targetMerchant.TradeItems = new MerchantTradeItems(templateID);
								targetMerchant.SaveIntoDatabase();
								DisplayMessage(client, "Merchant articles list loaded!");
							}
							catch (Exception)
							{
								DisplayMessage(client, "Type /merchant for command overview");
								return;
							}
						}
					}
					break;

				case "sellremove":
					{
						if (args.Length == 2)
						{
							targetMerchant.TradeItems = null;
							targetMerchant.SaveIntoDatabase();
							DisplayMessage(client, "Merchant articles list removed!");
						}
					}
					break;

				case "articles":
					{
						if (args.Length < 3)
						{
							DisplayMessage(client, "Usage: /merchant articles add <ItemTemplate> <page> [slot]");
							break;
						}
						switch (args[2])
						{
							case "add":
								{
									if (args.Length <= 6)
									{
										try
										{
											string templateID = args[3];
											int page = Convert.ToInt32(args[4]);
											eMerchantWindowSlot slot = eMerchantWindowSlot.FirstEmptyInPage;

											if (targetMerchant.TradeItems == null)
											{
												DisplayMessage(client, "Merchant articles list no found!");
												return;
											}

											ItemTemplate template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), templateID);
											if (template == null)
											{
												DisplayMessage(client, "ItemTemplate with id " + templateID + " could not be found!");
												return;
											}

											if (args.Length == 6)
											{
												slot = (eMerchantWindowSlot)Convert.ToInt32(args[5]);
											}

											slot = targetMerchant.TradeItems.GetValidSlot(page, slot);
											if (slot == eMerchantWindowSlot.Invalid)
											{
												DisplayMessage(client, "Page number (" + page + ") must be from 0 to " + (MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS - 1) + " and slot (" + slot + ") must be from 0 to " + (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS - 1) + ".");
												return;
											}

											MerchantItem item = (MerchantItem)GameServer.Database.SelectObject(typeof(MerchantItem), "ItemListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "' AND PageNumber = '" + page + "' AND SlotPosition = '" + slot + "'");
											if (item == null)
											{
												item = new MerchantItem();
												item.ItemListID = targetMerchant.TradeItems.ItemsListID;
												item.ItemTemplateID = templateID;
												item.SlotPosition = (int)slot;
												item.PageNumber = page;

												GameServer.Database.AddNewObject(item);
											}
											else
											{
												item.ItemTemplateID = templateID;
												GameServer.Database.SaveObject(item);
											}
											DisplayMessage(client, "Item added to the merchant articles list!");
										}
										catch (Exception)
										{
											DisplayMessage(client, "Type /merchant for command overview");
											return;
										}
									}
									else
									{
										DisplayMessage(client, "Usage: /merchant articles add <ItemTemplate> <page> [slot]");
									}
									break;
								}
							case "remove":
								{
									if (args.Length == 5)
									{
										try
										{
											int page = Convert.ToInt32(args[3]);
											int slot = Convert.ToInt32(args[4]);

											if (page < 0 || page >= MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS)
											{
												DisplayMessage(client, "Page number (" + page + ") must be between [0;" + (MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS - 1) + "]!");
												return;
											}

											if (slot < 0 || slot >= MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS)
											{
												DisplayMessage(client, "Slot (" + slot + ") must be between [0;" + (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS - 1) + "]!");
												return;
											}

											if (targetMerchant.TradeItems == null)
											{
												DisplayMessage(client, "Merchant articles list no found!");
												return;
											}

											MerchantItem item = (MerchantItem)GameServer.Database.SelectObject(typeof(MerchantItem), "ItemListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "' AND PageNumber = '" + page + "' AND SlotPosition = '" + slot + "'");
											if (item == null)
											{
												DisplayMessage(client, "Slot " + slot + " in page " + page + " is already empty.");
												return;
											}
											GameServer.Database.DeleteObject(item);
											DisplayMessage(client, "Merchant articles list slot " + slot + " in page " + page + " cleaned!");

										}
										catch (Exception)
										{
											DisplayMessage(client, "Type /merchant for command overview");
											return;
										}
									}
									else
									{
										DisplayMessage(client, "Usage: /merchant articles remove <page> <slot>");
									}
									break;
								}
							case "delete":
								{
									if (args.Length == 3)
									{
										try
										{
											if (targetMerchant.TradeItems == null)
											{
												DisplayMessage(client, "Merchant articles list no found!");
												return;
											}
											DisplayMessage(client, "Deleting articles list template ...");

											MerchantItem[] merchantitems = (MerchantItem[])GameServer.Database.SelectObjects(typeof(MerchantItem), "ItemsListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "'");
											if (merchantitems.Length > 0)
											{
												foreach (MerchantItem item in merchantitems)
												{
													GameServer.Database.DeleteObject(item);
												}
											}
											DisplayMessage(client, "Merchant articles list deleted.");
										}
										catch (Exception)
										{
											DisplayMessage(client, "Type /merchant for command overview");
											return;
										}
									}
									break;
								}
							default:
								DisplayMessage(client, "Type /merchant for command overview");
								break;
						}
						break;
					}
				case "type":
					{
						string theType = param;
						if (args.Length > 2)
							theType = args[2];

						//Create a new merchant
						GameMerchant merchant = null;
						ArrayList asms = new ArrayList(ScriptMgr.Scripts);
						asms.Add(typeof(GameServer).Assembly);
						foreach (Assembly script in asms)
						{
							try
							{
								client.Out.SendDebugMessage(script.FullName);
								merchant = (GameMerchant)script.CreateInstance(theType, false);
								if (merchant != null)
									break;
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						if (merchant == null)
						{
							DisplayMessage(client, "There was an error creating an instance of " + theType + "!");
							return;
						}
						//Fill the object variables
						merchant.X = targetMerchant.X;
						merchant.Y = targetMerchant.Y;
						merchant.Z = targetMerchant.Z;
						merchant.CurrentRegion = targetMerchant.CurrentRegion;
						merchant.Heading = targetMerchant.Heading;
						merchant.Level = targetMerchant.Level;
						merchant.Realm = targetMerchant.Realm;
						merchant.Name = targetMerchant.Name;
						merchant.Model = targetMerchant.Model;
						//Fill the living variables
						merchant.CurrentSpeed = targetMerchant.CurrentSpeed; ;
						merchant.MaxSpeedBase = targetMerchant.MaxSpeedBase; ;
						merchant.GuildName = targetMerchant.GuildName;
						merchant.Size = targetMerchant.Size;
						merchant.Inventory = targetMerchant.Inventory;
						merchant.EquipmentTemplateID = targetMerchant.EquipmentTemplateID;
						merchant.TradeItems = targetMerchant.TradeItems;
						merchant.AddToWorld();
						merchant.SaveIntoDatabase();
						targetMerchant.Delete();
						targetMerchant.DeleteFromDatabase();
						DisplayMessage(client, "Merchant type changed to " + param);
						break;
					}
			}
		}
	}
}