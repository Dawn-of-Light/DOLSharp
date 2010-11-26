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
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.ServerProperties;
using DOL.GS.Utils;
using DOL.Language;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0C, "Handles things like placing indoor/outdoor items")]
	public class HousingPlaceItemHandler : IPacketHandler
	{
		private const string DeedWeak = "deedItem";
		private const string TargetHouse = "targetHouse";
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private int _position;

		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			try
			{
				int unknow1 = packet.ReadByte(); // 1=Money 0=Item (?)
				int slot = packet.ReadByte(); // Item/money slot
				ushort housenumber = packet.ReadShort(); // N° of house
				int unknow2 = (byte)packet.ReadByte();
				_position = (byte)packet.ReadByte();
				int method = packet.ReadByte(); // 2=Wall 3=Floor
				int rotation = packet.ReadByte(); // garden items only
				var xpos = (short)packet.ReadShort(); // x for inside objs
				var ypos = (short)packet.ReadShort(); // y for inside objs.
				//Log.Info("U1: " + unknow1 + " - U2: " + unknow2);

				// house must exist
				House house = HouseMgr.GetHouse(client.Player.CurrentRegionID, housenumber);
				if (house == null)
					return;

				if (client.Player == null)
					return;

				ChatUtil.SendDebugMessage(client, string.Format("HousingPlaceItem: slot: {0}, position: {1}, method: {2}, xpos: {3}, ypos: {4}", slot, _position, method, xpos, ypos));

				if ((slot >= 244) && (slot <= 248)) // money
				{
					// check that player has permission to pay rent
					if (!house.CanPayRent(client.Player))
					{
						client.Out.SendInventorySlotsUpdate(new[] { slot });
						return;
					}

					long moneyToAdd = _position;
					switch (slot)
					{
						case 248:
							moneyToAdd *= 1;
							break;
						case 247:
							moneyToAdd *= 100;
							break;
						case 246:
							moneyToAdd *= 10000;
							break;
						case 245:
							moneyToAdd *= 10000000;
							break;
						case 244:
							moneyToAdd *= 10000000000;
							break;
					}

					client.Player.TempProperties.setProperty(HousingConstants.MoneyForHouseRent, moneyToAdd);
					client.Player.TempProperties.setProperty(HousingConstants.HouseForHouseRent, house);
					client.Player.Out.SendInventorySlotsUpdate(null);
					client.Player.Out.SendHousePayRentDialog("Housing07");

					return;
				}

				// make sure the item dropped still exists
				InventoryItem orgitem = client.Player.Inventory.GetItem((eInventorySlot)slot);
				if (orgitem == null)
					return;

				if (orgitem.Id_nb == "house_removal_deed")
				{
					client.Out.SendInventorySlotsUpdate(null);

					// make sure player has owner permissions
					if (!house.HasOwnerPermissions(client.Player))
					{
						ChatUtil.SendSystemMessage(client.Player, "You may not remove Houses that you don't own");
						return;
					}

					client.Player.TempProperties.setProperty(DeedWeak, new WeakRef(orgitem));
					client.Player.TempProperties.setProperty(TargetHouse, house);
					client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HouseRemoveOffer"), HouseRemovalDialogue);

					return;
				}

				if (orgitem.Id_nb.Contains("cottage_deed") || orgitem.Id_nb.Contains("house_deed") ||
					orgitem.Id_nb.Contains("villa_deed") || orgitem.Id_nb.Contains("mansion_deed"))
				{
					client.Out.SendInventorySlotsUpdate(null);

					// make sure player has owner permissions
					if (!house.HasOwnerPermissions(client.Player))
					{
						ChatUtil.SendSystemMessage(client, "You may not change other peoples houses");

						return;
					}

					client.Player.TempProperties.setProperty(DeedWeak, new WeakRef(orgitem));
					client.Player.TempProperties.setProperty(TargetHouse, house);
					client.Player.Out.SendMessage("Warning:\n This will remove *all* items from your current house!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					client.Player.Out.SendCustomDialog("Are you sure you want to upgrade your House?", HouseUpgradeDialogue);

					return;
				}

				if (orgitem.Name == "deed of guild transfer")
				{
					// player needs to be in a guild to xfer a house to a guild
					if (client.Player.Guild == null)
					{
						client.Out.SendInventorySlotsUpdate(new[] { slot });
						ChatUtil.SendSystemMessage(client, "You must be a member of a guild to do that");
						return;
					}

					// player needs to own the house to be able to xfer it
					if (!house.HasOwnerPermissions(client.Player))
					{
						client.Out.SendInventorySlotsUpdate(new[] { slot });
						ChatUtil.SendSystemMessage(client, "You do not own this house.");
						return;
					}

					// guild can't already have a house
					if (client.Player.Guild.GuildOwnsHouse)
					{
						client.Out.SendInventorySlotsUpdate(new[] { slot });
						ChatUtil.SendSystemMessage(client, "Your Guild already owns a house.");
						return;
					}

					// player needs to be a GM in the guild to xfer his personal house to the guild
					if (!client.Player.Guild.GotAccess(client.Player, Guild.eRank.Leader))
					{
						client.Out.SendInventorySlotsUpdate(new[] { slot });
						ChatUtil.SendSystemMessage(client, "You are not the leader of a guild.");
						return;
					}

					HouseMgr.HouseTransferToGuild(client.Player);
					client.Player.Inventory.RemoveItem(orgitem);
					client.Player.Guild.UpdateGuildWindow();
					return;
				}

				if (house.CanChangeInterior(client.Player, DecorationPermissions.Remove))
				{
					if (orgitem.Name == "interior banner removal")
					{
						house.IndoorGuildBanner = false;
						ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.InteriorBannersRemoved", null);
						return;
					}

					if (orgitem.Name == "interior shield removal")
					{
						house.IndoorGuildShield = false;
						ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.InteriorShieldsRemoved", null);
						return;
					}

					if (orgitem.Name == "carpet removal")
					{
						house.Rug1Color = 0;
						house.Rug2Color = 0;
						house.Rug3Color = 0;
						house.Rug4Color = 0;
						ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.CarpetsRemoved", null);
						return;
					}
				}

				if (house.CanChangeExternalAppearance(client.Player))
				{
					if (orgitem.Name == "exterior banner removal")
					{
						house.OutdoorGuildBanner = false;
						ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.OutdoorBannersRemoved", null);
						return;
					}

					if (orgitem.Name == "exterior shield removal")
					{
						house.OutdoorGuildShield = false;
						ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.OutdoorShieldsRemoved", null);
						return;
					}
				}

				int objType = orgitem.Object_Type;
				if (objType == 49) // Garden items 
				{
					method = 1;
				}
				else if (orgitem.Id_nb == "housing_porch_deed" || orgitem.Id_nb == "housing_porch_remove_deed" || orgitem.Id_nb == "housing_consignment_deed")
				{
					method = 4;
				}
				else if (objType >= 59 && objType <= 64) // Outdoor Roof/Wall/Door/Porch/Wood/Shutter/awning Material item type
				{
					ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.HouseUseMaterials", null);
					return;
				}
				else if (objType == 56 || objType == 52 || (objType >= 69 && objType <= 71)) // Indoor carpets 1-4
				{
					ChatUtil.SendSystemMessage(client.Player, "Scripts.Player.Housing.HouseUseCarpets", null);
					return;
				}
				else if (objType == 57 || objType == 58 // Exterior banner/shield
						 || objType == 66 || objType == 67) // Interior banner/shield
				{
					method = 6;
				}
				else if (objType == 53 || objType == 55 || objType == 68)
				{
					method = 5;
				}
				else if (objType == (int)eObjectType.HouseVault)
				{
					method = 7;
				}

				ChatUtil.SendDebugMessage(client, string.Format("Place Item: method: {0}", method));

				int pos;
				switch (method)
				{
					case 1:
						{
							if (client.Player.InHouse)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// no permissions to add to the garden, return
							if (!house.CanChangeGarden(client.Player, DecorationPermissions.Add))
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}


							// garden is already full, return
							if (house.OutdoorItems.Count >= Properties.MAX_OUTDOOR_HOUSE_ITEMS)
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.GardenMaxObjects", null);
								client.Out.SendInventorySlotsUpdate(new[] { slot });

								return;
							}

							// create an outdoor item to represent the item being placed
							var oitem = new OutdoorItem
											{
												BaseItem = GameServer.Database.FindObjectByKey<ItemTemplate>(orgitem.Id_nb),
												Model = orgitem.Model,
												Position = (byte)_position,
												Rotation = (byte)rotation
											};

							//add item in db
							pos = GetFirstFreeSlot(house.OutdoorItems.Keys);
							DBHouseOutdoorItem odbitem = oitem.CreateDBOutdoorItem(housenumber);
							oitem.DatabaseItem = odbitem;

							GameServer.Database.AddObject(odbitem);

							// remove the item from the player's inventory
							client.Player.Inventory.RemoveItem(orgitem);

							//add item to outdooritems
							house.OutdoorItems.Add(pos, oitem);

							ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.GardenItemPlaced",
													   Properties.MAX_OUTDOOR_HOUSE_ITEMS - house.OutdoorItems.Count);
							ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.GardenItemPlacedName", orgitem.Name);

							// update all nearby players
							foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(house.RegionID, house, WorldMgr.OBJ_UPDATE_DISTANCE))
							{
								player.Out.SendGarden(house);
							}

							// save the house
							house.SaveIntoDatabase();
							break;
						}
					case 2:
					case 3:
						{
							if (client.Player.InHouse == false)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// no permission to add to the interior, return
							if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// not a wall object, return
							if (!IsSuitableForWall(orgitem) && method == 2)
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.NotWallObject", null);
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// not a floor object, return
							if (objType != 51 && method == 3)
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.NotFloorObject", null);
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// interior already has max items, return
							if (house.IndoorItems.Count >= GetMaxIndoorItemsForHouse(house.Model))
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.IndoorMaxItems", GetMaxIndoorItemsForHouse(house.Model));
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// create an indoor item to represent the item being placed
							var iitem = new IndoorItem
											{
												Model = orgitem.Model,
												Color = orgitem.Color,
												X = xpos,
												Y = ypos,
												Size = 100,
												Position = _position,
												PlacementMode = method,
												BaseItem = null
											};

							// figure out proper rotation for item
							int properRotation = client.Player.Heading / 10;
							properRotation = properRotation.Clamp(0, 360);

							if (method == 2 && IsSuitableForWall(orgitem))
							{
								properRotation = 360;
								if (objType != 50)
								{
									client.Out.SendInventorySlotsUpdate(new[] { slot });
								}
							}

							iitem.Rotation = properRotation;

							pos = GetFirstFreeSlot(house.IndoorItems.Keys);
							if (objType == 50 || objType == 51)
							{
								//its a housing item, so lets take it!
								client.Player.Inventory.RemoveItem(orgitem);

								//set right base item, so we can recreate it on take.
								if (orgitem.Id_nb.Contains("GuildBanner"))
								{
									iitem.BaseItem = orgitem.Template;
									iitem.Size = 50; // Banners have to be reduced in size
								}
								else
								{
									iitem.BaseItem = GameServer.Database.FindObjectByKey<ItemTemplate>(orgitem.Id_nb);
								}
							}

							DBHouseIndoorItem idbitem = iitem.CreateDBIndoorItem(housenumber);
							iitem.DatabaseItem = idbitem;
							GameServer.Database.AddObject(idbitem);

							house.IndoorItems.Add(pos, iitem);

							// let player know the item has been placed
							ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.IndoorItemPlaced", (GetMaxIndoorItemsForHouse(house.Model) - house.IndoorItems.Count));

							switch (method)
							{
								case 2:
									ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.IndoorWallPlaced", orgitem.Name);
									break;
								case 3:
									ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.IndoorFloorPlaced", orgitem.Name);
									break;
							}

							// update furniture for all players in the house
							foreach (GamePlayer plr in house.GetAllPlayersInHouse())
							{
								plr.Out.SendFurniture(house, pos);
							}

							break;
						}
					case 4:
						{
							// no permission to add to the garden, return
							if (!house.CanChangeGarden(client.Player, DecorationPermissions.Add))
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							switch (orgitem.Id_nb)
							{
								case "housing_porch_deed":
									// try and add the porch
									if (house.AddPorch())
									{
										// remove the original item from the player's inventory
										client.Player.Inventory.RemoveItem(orgitem);
									}
									else
									{
										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.PorchAlready", null);
										client.Out.SendInventorySlotsUpdate(new[] { slot });
									}
									return;
								case "housing_porch_remove_deed":
									// try and remove the porch
									if (house.RemovePorch())
									{
										// remove the original item from the player's inventory
										client.Player.Inventory.RemoveItem(orgitem);
									}
									else
									{
										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.PorchNone", null);
										client.Out.SendInventorySlotsUpdate(new[] { slot });
									}
									return;
								case "housing_consignment_deed":
									{
										// make sure there is a porch for this consignment merchant!
										if (!house.Porch)
										{
											ChatUtil.SendSystemMessage(client, "Your House needs a Porch first.");
											client.Out.SendInventorySlotsUpdate(new[] { slot });
											return;
										}

										// try and add a new consignment merchant
										if (house.AddConsignment(0))
										{
											// remove the original item from the player's inventory
											client.Player.Inventory.RemoveItem(orgitem);
										}
										else
										{
											ChatUtil.SendSystemMessage(client, "You can not add a GameConsignmentMerchant Merchant here.");
											client.Out.SendInventorySlotsUpdate(new[] { slot });
										}
										return;
									}
								default:
									ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.PorchNotItem", null);
									client.Out.SendInventorySlotsUpdate(new[] { slot });
									return;
							}
						}
					case 5:
						{
							if (client.Player.InHouse == false)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// no permission to add to the interior, return
							if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// if the hookpoint doesn't exist, prompt player to Log it in the database for us
							if (house.GetHookpointLocation((uint)_position) == null)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });

								if (client.Account.PrivLevel == (int)ePrivLevel.Admin)
								{
									if (client.Player.TempProperties.getProperty<bool>(HousingConstants.AllowAddHouseHookpoint, false))
									{

										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointID", +_position);
										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointCloser", null);

										client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointLogLoc"), LogLocation);
									}
									else
									{
										ChatUtil.SendDebugMessage(client, "use '/house addhookpoints' to allow addition of new housing hookpoints.");
									}
								}
							}
							else if (house.GetHookpointLocation((uint)_position) != null)
							{
								var point = new DBHouseHookpointItem
												{
													HouseNumber = house.HouseNumber,
													ItemTemplateID = orgitem.Id_nb,
													HookpointID = (uint)_position
												};

								// If we already have soemthing here, do not place more
								foreach (var hpitem in GameServer.Database.SelectObjects<DBHouseHookpointItem>("HouseNumber = '" + house.HouseNumber + "'"))
								{
									if (hpitem.HookpointID == point.HookpointID)
									{
										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointAlready", null);
										return;
									}
								}

								// add the item to the database
								GameServer.Database.AddObject(point);

								if (house.HousepointItems.ContainsKey(point.HookpointID) == false)
								{
									house.HousepointItems.Add(point.HookpointID, point);
									house.FillHookpoint((uint)_position, orgitem.Id_nb, client.Player.Heading, 0);
								}
								else
								{
									string error = string.Format("Hookpoint already has item on attempt to attach {0} to hookpoint {1} for house {2}!", orgitem.Id_nb, _position, house.HouseNumber);
									log.ErrorFormat(error);
									throw new Exception(error);
								}


								// remove the original item from the player's inventory
								client.Player.Inventory.RemoveItem(orgitem);

								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointAdded", null);

								// save the house
								house.SaveIntoDatabase();
							}
							else
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointNot", null);
							}

							// broadcast updates
							house.SendUpdate();
							break;
						}
					case 6:
						{
							// no permission to change external appearance, return
							if (!house.CanChangeExternalAppearance(client.Player))
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							if (objType == 57) // We have outdoor banner
							{
								house.OutdoorGuildBanner = true;
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.OutdoorBannersAdded", null);
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else if (objType == 58) // We have outdoor shield
							{
								house.OutdoorGuildShield = true;
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.OutdoorShieldsAdded", null);
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else if (objType == 66) // We have indoor banner
							{
								house.IndoorGuildBanner = true;
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.InteriorBannersAdded", null);
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else if (objType == 67) // We have indoor shield
							{
								house.IndoorGuildShield = true;
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.InteriorShieldsAdded", null);
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else
							{
								ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.BadShieldBanner", null);
							}

							// save the house and broadcast updates
							house.SaveIntoDatabase();
							house.SendUpdate();
							break;
						}
					case 7: // House vault.
						{
							if (client.Player.InHouse == false)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });
								return;
							}

							// make sure the hookpoint position is valid
							if (_position > HousingConstants.MaxHookpointLocations)
							{
								ChatUtil.SendSystemMessage(client, "This hookpoint position is unknown, error logged.");
								log.Error("HOUSING: " + client.Player.Name + " working with invalid position " + _position + " in house " +
										  house.HouseNumber + " model " + house.Model);

								return;
							}

							// if hookpoint doesn't exist, prompt player to Log it in the database for us
							if (house.GetHookpointLocation((uint)_position) == null)
							{
								client.Out.SendInventorySlotsUpdate(new[] { slot });

								if (client.Account.PrivLevel == (int)ePrivLevel.Admin)
								{
									if (client.Player.TempProperties.getProperty<bool>(HousingConstants.AllowAddHouseHookpoint, false))
									{

										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointID", +_position);
										ChatUtil.SendSystemMessage(client, "Scripts.Player.Housing.HookPointCloser", null);

										client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointLogLoc"), LogLocation);
									}
									else
									{
										ChatUtil.SendDebugMessage(client, "use '/house addhookpoints' to allow addition of new housing hookpoints.");
									}
								}

								return;
							}

							// make sure we have space to add another vult
							int vaultIndex = house.GetFreeVaultNumber();
							if (vaultIndex < 0)
							{
								client.Player.Out.SendMessage("You can't add any more vaults to this house!", eChatType.CT_System,
															  eChatLoc.CL_SystemWindow);
								client.Out.SendInventorySlotsUpdate(new[] { slot });

								return;
							}

							log.Debug("HOUSING: " + client.Player.Name + " placing house vault at position " + _position + " in house " +
									  house.HouseNumber + " model " + house.Model);

							// create the new vault and attach it to the house
							var houseVault = new GameHouseVault(orgitem.Template, vaultIndex);
							houseVault.Attach(house, (uint)_position, (ushort)((client.Player.Heading + 2048) % 4096));

							// remove the original item from the player's inventory
							client.Player.Inventory.RemoveItem(orgitem);

							// save the house and broadcast uodates
							house.SaveIntoDatabase();
							house.SendUpdate();
							return;
						}
					default:
						{
							ChatUtil.SendDebugMessage(client, "Place Item: Unknown method, do nothing.");
							client.Out.SendInventorySlotsUpdate(null);
							break;
						}
				}
			}
			catch (Exception ex)
			{
				log.Error("HousingPlaceItemHandler", ex);
				client.Out.SendMessage("Error processing housing action; the error has been logged!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				client.Out.SendInventorySlotsUpdate(null);
			}
		}

		#endregion

		private static bool IsSuitableForWall(InventoryItem item)
		{
			#region item types

			switch (item.Object_Type)
			{
				case (int)eObjectType.HouseWallObject:
				case (int)eObjectType.Axe:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.CompositeBow:
				case (int)eObjectType.Crossbow:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.LeftAxe:
				case (int)eObjectType.Longbow:
				case (int)eObjectType.MaulerStaff:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.Scythe:
				case (int)eObjectType.Shield:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.Spear:
				case (int)eObjectType.Staff:
				case (int)eObjectType.Sword:
				case (int)eObjectType.Thrown:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:
					return true;
				default:
					return false;
			}

			#endregion
		}

		private static int GetMaxIndoorItemsForHouse(int model)
		{
			int maxitems = Properties.MAX_INDOOR_HOUSE_ITEMS;

			if (Properties.INDOOR_ITEMS_DEPEND_ON_SIZE)
			{
				switch (model)
				{
					case 1:
					case 5:
					case 9:
						maxitems = 40;
						break;

					case 2:
					case 6:
					case 10:
						maxitems = 60;
						break;

					case 3:
					case 7:
					case 11:
						maxitems = 80;
						break;

					case 4:
					case 8:
					case 12:
						maxitems = 100;
						break;
				}
			}
			return maxitems;
		}

		private static int GetFirstFreeSlot(ICollection<int> tbl)
		{
			int i = 0; //tbl.Count;

			while (tbl.Contains(i))
			{
				i++;
			}

			return i;
		}

		/// <summary>
		/// Does the player want to Log the offset location of the missing housepoint
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">1 = yes 0 = no</param>
		private void LogLocation(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			if (player.CurrentHouse == null)
				return;

			var a = new HouseHookpointOffset
			{
				HouseModel = player.CurrentHouse.Model,
				HookpointID = _position,
				X = player.X - player.CurrentHouse.X,
				Y = player.Y - player.CurrentHouse.Y,
				Z = player.Z - 25000,
				Heading = player.Heading - player.CurrentHouse.Heading
			};

			if (GameServer.Database.AddObject(a) && House.AddNewOffset(a))
			{
				ChatUtil.SendSystemMessage(player, "Scripts.Player.Housing.HookPointLogged", _position);

				string action = string.Format("HOUSING: {0} logged new HouseHookpointOffset for model {1}, position {2}, offset {3}, {4}, {5}",
								  player.Name, a.HouseModel, a.HookpointID, a.X, a.Y, a.Z);
				
				log.Debug(action);
				GameServer.Instance.LogGMAction(action);
			}
			else
			{
				log.Error(
					string.Format(
						"HOUSING: Player {0} error adding HouseHookpointOffset for model {1}, position {2}, offset {3}, {4}, {5}",
						player.Name, a.HouseModel, a.HookpointID, a.X, a.Y, a.Z));

				ChatUtil.SendSystemMessage(player, "Error adding position " + _position + ", error recorded in server error Log.");
			}
		}

		private static void HouseRemovalDialogue(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			var itemWeak = player.TempProperties.getProperty<WeakReference>(DeedWeak, new WeakRef(null));
			player.TempProperties.removeProperty(DeedWeak);

			var item = (InventoryItem)itemWeak.Target;
			var house = player.TempProperties.getProperty<House>(TargetHouse, null);
			player.TempProperties.removeProperty(TargetHouse);

			if (house == null)
			{
				ChatUtil.SendSystemMessage(player, "No House selected!");
				return;
			}

			if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				ChatUtil.SendSystemMessage(player, "You need a House removal Deed for this.");
				return;
			}

			player.Inventory.RemoveItem(item);
			HouseMgr.RemoveHouse(house);

			ChatUtil.SendSystemMessage(player, "Scripts.Player.Housing.HouseRemoved");
		}

		private static void HouseUpgradeDialogue(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			var itemWeak = player.TempProperties.getProperty<WeakReference>(DeedWeak, new WeakRef(null));
			player.TempProperties.removeProperty(DeedWeak);

			var item = (InventoryItem)itemWeak.Target;
			var house = (House)player.TempProperties.getProperty<object>(TargetHouse, null);
			player.TempProperties.removeProperty(TargetHouse);

			if (house == null)
			{
				ChatUtil.SendSystemMessage(player, "No House selected!");
				return;
			}

			if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				ChatUtil.SendSystemMessage(player, "This does not work without a House Deed.");
				return;
			}

			if (HouseMgr.UpgradeHouse(house, item))
				player.Inventory.RemoveItem(item);
		}
	}
}