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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute("&merchant",
		 ePrivLevel.GM,
		 "GMCommands.Merchant.Description",
		 //Usage
		 "GMCommands.Merchant.Usage.Create",
		 "GMCommands.Merchant.Usage.CreateType",
		 "GMCommands.Merchant.Usage.Info",
		 "GMCommands.Merchant.Usage.Save",
		 "GMCommands.Merchant.Usage.Remove",
		 "GMCommands.Merchant.Usage.Sell",
		 "GMCommands.Merchant.Usage.SellRemove",
		 "GMCommands.Merchant.Usage.Articles.Add",
		 "GMCommands.Merchant.Usage.Articles.Remove",
		 "GMCommands.Merchant.Usage.Articles.Delete",
		 "GMCommands.Merchant.Usage.Type")]
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

			if (args[1].ToLower() != "create" && targetMerchant == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
				return;
			}

			switch (args[1].ToLower())
			{
				#region Create
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
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.ErrorCreateInstance", theType));
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
						merchant.Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GMCommands.Merchant.NewName");
						merchant.Model = 9;
						//Fill the living variables
						merchant.CurrentSpeed = 0;
						merchant.MaxSpeedBase = 200;
						merchant.GuildName = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GMCommands.Merchant.NewGuildName");
						merchant.Size = 50;
						merchant.AddToWorld();
						merchant.SaveIntoDatabase();
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Create.Created", merchant.ObjectID));
						break;
					}
				#endregion Create
				#region Info
				case "info":
					{
						if (args.Length == 2)
						{
							if (targetMerchant.TradeItems == null)
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Info.ArtListIsEmpty"));
							else
							{
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Info.ArtList", targetMerchant.TradeItems.ItemsListID));
							}
						}
						break;
					}
				#endregion Info
				#region Save
				case "save":
					{
						targetMerchant.SaveIntoDatabase();
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Save.SavedInDB"));
						break;
					}
				#endregion Save
				#region Remove
				case "remove":
					{
						targetMerchant.DeleteFromDatabase();
						targetMerchant.Delete();
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Remove.RemovedFromDB"));
						break;
					}
				#endregion Remove
				#region Sell
				case "sell":
					{
						switch (args[2].ToLower())
						{
							#region Add
							case "add":
								{
									if (args.Length == 4)
									{
										try
										{
											string templateID = args[3];
											targetMerchant.TradeItems = new MerchantTradeItems(templateID);
											targetMerchant.SaveIntoDatabase();
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Sell.Add.Loaded"));
										}
										catch (Exception)
										{
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
											return;
										}
									}
									break;
								}
							#endregion Add
							#region Remove
							case "remove":
								{
									if (args.Length == 3)
									{
										targetMerchant.TradeItems = null;
										targetMerchant.SaveIntoDatabase();
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Sell.Remove.Removed"));
									}
									break;
								}
							#endregion Remove
							#region Default
							default:
								{
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
									return;
								}
							#endregion Default
						}
						break;
					}
				#endregion Sell
				#region Articles
				case "articles":
					{
						if (args.Length < 3)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
							return;
						}

						switch (args[2].ToLower())
						{
							#region Add
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
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.ListNoFound"));
												return;
											}

											ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);
											if (template == null)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Add.ItemTemplateNoFound", templateID));
												return;
											}

											if (args.Length == 6)
											{
												slot = (eMerchantWindowSlot)Convert.ToInt32(args[5]);
											}

											slot = targetMerchant.TradeItems.GetValidSlot(page, slot);
											if (slot == eMerchantWindowSlot.Invalid)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Add.PageAndSlotInvalid", page, (MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS - 1), slot, (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS - 1)));
												return;
											}

											MerchantItem item = GameServer.Database.SelectObject<MerchantItem>("ItemListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "' AND PageNumber = '" + page + "' AND SlotPosition = '" + slot + "'");
											if (item == null)
											{
												item = new MerchantItem();
												item.ItemListID = targetMerchant.TradeItems.ItemsListID;
												item.ItemTemplateID = templateID;
												item.SlotPosition = (int)slot;
												item.PageNumber = page;

												GameServer.Database.AddObject(item);
											}
											else
											{
												item.ItemTemplateID = templateID;
												GameServer.Database.SaveObject(item);
											}
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Add.ItemAdded"));
										}
										catch (Exception)
										{
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
											return;
										}
									}
									else
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
									}
									break;
								}
							#endregion Add
							#region Remove
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
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Remove.PageInvalid", page, (MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS - 1)));
												return;
											}

											if (slot < 0 || slot >= MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Remove.SlotInvalid", slot, (MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS - 1)));
												return;
											}

											if (targetMerchant.TradeItems == null)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.ListNoFound"));
												return;
											}

											MerchantItem item = GameServer.Database.SelectObject<MerchantItem>("ItemListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "' AND PageNumber = '" + page + "' AND SlotPosition = '" + slot + "'");
											if (item == null)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Remove.SlotInPageIsAEmpty", slot, page));
												return;
											}
											GameServer.Database.DeleteObject(item);
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Remove.SlotInPageCleaned", slot, page));

										}
										catch (Exception)
										{
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
											return;
										}
									}
									else
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
									}
									break;
								}
							#endregion Remove
							#region Delete
							case "delete":
								{
									if (args.Length == 3)
									{
										try
										{
											if (targetMerchant.TradeItems == null)
											{
												DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.ListNoFound"));
												return;
											}
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Delete.DeletingListTemp"));

											var merchantitems = GameServer.Database.SelectObjects<MerchantItem>("ItemsListID = '" + GameServer.Database.Escape(targetMerchant.TradeItems.ItemsListID) + "'");
											if (merchantitems.Count > 0)
											{
												foreach (MerchantItem item in merchantitems)
												{
													GameServer.Database.DeleteObject(item);
												}
											}
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Articles.Delete.ListDeleted"));
										}
										catch (Exception)
										{
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
											return;
										}
									}
									break;
								}
							#endregion Delete
							#region Default
							default:
								{
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.CommandOverview"));
									return;
								}
							#endregion Default
						}
						break;
					}
				#endregion Articles
				#region Type
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
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.ErrorCreateInstance", theType));
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
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Merchant.Type.Changed", param));
						break;
					}
				#endregion Type
			}
		}
	}
}