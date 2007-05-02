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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[Cmd("&item", //command to handle
		(uint)ePrivLevel.GM, //minimum privelege level
		"Various Item commands!", //command description
		//usage
		"Slot numbers are optional, if not included the default is 79 (the last backpack slot)",
		"names with spaces are given in quotes \"<name>\"",
		"'/item blank' - create a blank item",
		"'/item info <ItemTemplateName>' - get Info on a ItemTemplate",
		"'/item create <ItemTemplateName> [count]' - create a new item from a template",
		"'/item count <amount> [slot #]' - change item count",
		"'/item maxcount <amount> [slot #]' - change max amount allowed in one slot",
		"'/item packsize <amount> [slot #]' - change amount of items sold at once",
		"'/item model <ModelID> [slot #]' - change item model",
		"'/item extension <extensionID> [slot #]' - change item extension",
		"'/item color <ColorID> [slot #]' - change item color",
		"'/item effect <EffectID> [slot #]' - change item effect",
		"'/item name <NameID> [slot #]' - change item name",
		"'/item craftername <CrafterNameID> [slot #]' - change item crafter name",
		"'/item type <TypeID> [slot #]' - change item type",
		"'/item object <ObjectID> [slot #]' - change object type",
		"'/item hand <HandID> [slot #]' - change item hand",
		"'/item damagetype <DamageTypeID> [slot #]' - change item damage type",
		"'/item emblem <EmblemID> [slot #]' - change item emblem",
		"'/item price <gold> <silver> <copper> [slot #]' - change the price of an item",
		"'/item condition <con> <maxCon> [slot #]' - change the condition of an item",
		"'/item quality <qua> [slot #]' - change the quality of an item",
		"'/item durability <dur> <maxDur> [slot #]' - change the durability of an item",
		"'/item ispickable <true or false> [slot #]' - sets whether or not an item can be picked up",
		"'/item isdropable <true or false> [slot #]' - sets whether or not an item can be dropped",
	   "'/item istradable <true or false> [slot #]' - sets whether or not an item can be traded",
		"'/item bonus <bonus> [slot #]' - sets the item bonus",
		"'/item mbonus <num> <bonus type> <value> [slot #]' - sets the item magical bonus (num 0 = ExtraBonus)",
		"'/item weight <weight> [slot #]' - sets the item weight",
		"'/item dps_af <NewDPS_AF> [slot #]' - change item DPS_AF",
		"'/item spd_abs <NewSPD_ABS> [slot #]' - change item SPD_ABS",
		"'/item material <Material> <MaterialLevel> [slot #]' - change item material",
		"'/item spell <Charges> <MaxCharges> <SpellID> [slot #]' - change item spell charges",
		"'/item proc <SpellID> [slot #]' - change item proc",
		"'/item poison <Charges> <MaxCharges> <SpellID> [slot #]' - change item poison",
		"'/item realm <num> [slot #]' - change items realm",
		"'/item savetemplate <TemplateID> [slot #]' - create a new template",
		"'/item templateid <TemplateID> [slot #] - change an items template ID'",
		"'/item findid <name>'")]
	public class ItemCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			try
			{
				if (args.Length < 2)
				{
					DisplaySyntax(client);
					return 0;
				}

				switch (args[1].ToLower())
				{
					case "blank":
						{
							InventoryItem item = new InventoryItem();
							Random rand = new Random();
							item.Id_nb = "blankItem" + rand.Next().ToString();
							item.Name = "a blank item";
							if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							{
								client.Out.SendMessage("Blank item created in first free backpack slot.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Out.SendMessage("Error in item creation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "create":
						{
							//Create a new object
							try
							{
								ItemTemplate template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), args[2]);
								if (template == null)
								{
									client.Out.SendMessage("ItemTemplate with id " + args[2] + " could not be found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 0;
								}
								else
								{
									int count = 1;
									if (args.Length >= 4)
									{
										try
										{
											count = Convert.ToInt32(args[3]);
											if (count < 1)
												count = 1;
										}
										catch (Exception)
										{
										}
									}

									InventoryItem item = new InventoryItem();
									item.CopyFrom(template);
									if (item.IsStackable)
									{
										item.Count = count;
										item.Weight = item.Count * item.Weight;
									}
									if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
									{
										string countStr = "";
										if (count > 1)
											countStr = " count= " + count;
										client.Out.SendMessage("Item created: level= " + item.Level + " name= " + item.GetName(0, false) + countStr, eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
								}
							}
							catch (Exception)
							{
								client.Out.SendMessage("Type /item for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "count":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							if (!item.IsStackable)
							{
								client.Out.SendMessage(item.GetName(0, true) + " is not stackable.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								if (Convert.ToInt32(args[2]) < 1)
								{
									item.Weight = item.Weight / item.Count;
									item.Count = 1;
								}
								else
								{
									item.Weight = Convert.ToInt32(args[2]) * item.Weight / item.Count;
									item.Count = Convert.ToInt32(args[2]);
								}
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								client.Player.UpdateEncumberance();
							}
							catch
							{
								client.Out.SendMessage("'/item count <amount> [slot #]' to change item count", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "maxcount":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.MaxCount = Convert.ToInt32(args[2]);
								if (item.MaxCount < 1)
									item.MaxCount = 1;
							}
							catch
							{
								client.Out.SendMessage("'/item maxcount <amount> [slot #]' to change max amount allowed in one slot", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "packsize":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.PackSize = Convert.ToInt32(args[2]);
								if (item.PackSize < 1)
									item.PackSize = 1;
							}
							catch
							{
								client.Out.SendMessage("'/item packsize <amount> [slot #]' to change amount of items sold at once", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "info":
						{
							ItemTemplate obj = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), args[2]);

							if (obj == null)
							{
								client.Out.SendMessage("Itemtemplate with ID:" + args[2] + " is unknown!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;

							}
							client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("Item Template: " + obj.Id_nb, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("         Name: " + obj.Name, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Level: " + obj.Level, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("         Type: " + GlobalConstants.ObjectTypeToName(obj.Object_Type), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("         Slot: " + GlobalConstants.SlotToName(obj.Item_Type), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Realm: " + obj.Realm, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("  Value/Price: " + obj.Gold + "g " + obj.Silver + "s " + obj.Copper + "c", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("       Weight: " + (obj.Weight / 10.0f) + "lbs", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("      Quality: " + obj.Quality + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("   Durability: " + obj.Durability + "/" + obj.MaxDurability + "(max)", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("    Condition: " + obj.Condition + "/" + obj.MaxCondition + "(max)", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("  Is dropable: " + (obj.IsDropable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("  Is pickable: " + (obj.IsPickable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage(" Is stackable: " + (obj.IsStackable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							if (GlobalConstants.IsWeapon(obj.Object_Type))
							{
								client.Out.SendMessage("         Hand: " + GlobalConstants.ItemHandToName(obj.Hand), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("Damage/Second: " + (obj.DPS_AF / 10.0f), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Speed: " + (obj.SPD_ABS / 10.0f), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(obj.Type_Damage), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Bonus: " + obj.Bonus, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							else if (GlobalConstants.IsArmor(obj.Object_Type))
							{
								client.Out.SendMessage("  Armorfactor: " + obj.DPS_AF, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("    Absorbage: " + obj.SPD_ABS, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Bonus: " + obj.Bonus, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							else if (obj.Object_Type == (int)eObjectType.Shield)
							{
								client.Out.SendMessage("  Shield type: " + GlobalConstants.ShieldTypeToName(obj.DPS_AF), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Bonus: " + obj.Bonus, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							else if (obj.Object_Type == (int)eObjectType.Arrow || obj.Object_Type == (int)eObjectType.Bolt)
							{
								client.Out.SendMessage(" Ammunition #: " + obj.DPS_AF, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(obj.SPD_ABS), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(obj.SPD_ABS), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(obj.SPD_ABS), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								client.Out.SendMessage("        Bonus: " + obj.Bonus, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							else if (obj.Object_Type == (int)eObjectType.Instrument)
							{
								client.Out.SendMessage("   Instrument: " + GlobalConstants.InstrumentTypeToName(obj.DPS_AF), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							break;
						}
					case "model":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.Model = Convert.ToUInt16(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
									client.Player.UpdateEquipmentAppearance();
							}
							catch
							{
								client.Out.SendMessage("'/item model <ModelID> <slot #>' to change item model", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "extension":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Extension = Convert.ToByte(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
									client.Player.UpdateEquipmentAppearance();
							}
							catch
							{
								client.Out.SendMessage("'/item extension <extension> <slot #>' to change item extension", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "color":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.Color = Convert.ToUInt16(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
									client.Player.UpdateEquipmentAppearance();
							}
							catch
							{
								client.Out.SendMessage("'/item color <ColorID> <slot #>' to change item color", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "effect":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Effect = Convert.ToUInt16(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
									client.Player.UpdateEquipmentAppearance();
							}
							catch
							{
								client.Out.SendMessage("'/item effect <EffectID> <slot #>' to change item effect", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "type":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Item_Type = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item type <TypeID> <slot #>' to change item type", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "object":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Object_Type = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item object <ObjectID> <slot #>' to change object type", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "hand":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Hand = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item hand <HandID> <slot #>' to change item hand", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "damagetype":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Type_Damage = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item damagetype <DamageTypeID> <slot #>' to change item damage type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "name":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Name = args[2];
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item name <Name> <slot #>' to change item name", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "craftername":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.CrafterName = args[2];
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item craftername <CrafterName> <slot #>' to change item crafter name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "emblem":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Emblem = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
									client.Player.UpdateEquipmentAppearance();
							}
							catch
							{
								client.Out.SendMessage("'/item emblem <EmblemID> <slot #>' to change item emblem", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "level":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Level = Convert.ToUInt16(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item <level> <NewLevel> <slot #>' to change item level", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "price":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 6)
							{
								try
								{
									slot = Convert.ToInt32(args[5]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Gold = (short)(Convert.ToInt16(args[2]) % 1000);
								item.Silver = (byte)(Convert.ToByte(args[3]) % 100);
								item.Copper = (byte)(Convert.ToByte(args[4]) % 100);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item price <gold> <silver> <copper> <slot #>' to change item price", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "condition":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 5)
							{
								try
								{
									slot = Convert.ToInt32(args[4]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								int con = Convert.ToInt32(args[2]);
								int maxcon = Convert.ToInt32(args[3]);
								item.Condition = con;
								item.MaxCondition = maxcon;
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item condition <Con> <MaxCon> <slot #>' to change item Con Stats", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "durability":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 5)
							{
								try
								{
									slot = Convert.ToInt32(args[4]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								int Dur = Convert.ToInt32(args[2]);
								int MaxDur = Convert.ToInt32(args[3]);
								item.Durability = Dur;
								item.MaxDurability = MaxDur;

								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item durability <Dur> <MaxDur> <slot #>' to change item Dur Stats", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "quality":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								int Qua = Convert.ToInt32(args[2]);
								item.Quality = Qua;

								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item quality <Qua> <MaxQua> <slot #>' to change item Qua Stats", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "bonus":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								int Bonus = Convert.ToInt32(args[2]);
								item.Bonus = Bonus;

								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item bonus <Bonus> <slot #>' to change item bonus %", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "mbonus":
						{
							int slot = (int)eInventorySlot.LastBackpack;
							int num = 0;
							int bonusType = 0;
							int bonusValue = 0;

							if (args.Length >= 6)
							{
								try
								{
									slot = Convert.ToInt32(args[5]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								num = Convert.ToInt32(args[2]);
							}
							catch
							{
								client.Out.SendMessage("Not set bonus number!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							try
							{
								bonusType = Convert.ToInt32(args[3]);
								if (bonusType < 0 || bonusType >= (int)eProperty.MaxProperty)
								{
									client.Out.SendMessage("Bonus type should be in range from 0 to " + (int)(eProperty.MaxProperty - 1), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									break;
								}
							}
							catch
							{
								client.Out.SendMessage("Not set bonus type!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							try
							{
								bonusValue = Convert.ToInt32(args[4]);
								switch (num)
								{
									case 0:
										{
											item.ExtraBonus = bonusValue;
											item.ExtraBonusType = bonusType;
											break;
										}
									case 1:
										{
											item.Bonus1 = bonusValue;
											item.Bonus1Type = bonusType;
											break;
										}
									case 2:
										{
											item.Bonus2 = bonusValue;
											item.Bonus2Type = bonusType;
											break;
										}
									case 3:
										{
											item.Bonus3 = bonusValue;
											item.Bonus3Type = bonusType;
											break;
										}
									case 4:
										{
											item.Bonus4 = bonusValue;
											item.Bonus4Type = bonusType;
											break;
										}
									case 5:
										{
											item.Bonus5 = bonusValue;
											item.Bonus5Type = bonusType;
											break;
										}
									case 6:
										{
											item.Bonus6 = bonusValue;
											item.Bonus6Type = bonusType;
											break;
										}
									case 7:
										{
											item.Bonus7 = bonusValue;
											item.Bonus7Type = bonusType;
											break;
										}
									case 8:
										{
											item.Bonus8 = bonusValue;
											item.Bonus8Type = bonusType;
											break;
										}
									case 9:
										{
											item.Bonus9 = bonusValue;
											item.Bonus9Type = bonusType;
											break;
										}
									case 10:
										{
											item.Bonus10 = bonusValue;
											item.Bonus10Type = bonusType;
											break;
										}
									default:
										client.Out.SendMessage("Unknown bonus number: " + num, eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
								}
								if (item.SlotPosition < (int)eInventorySlot.FirstBackpack)
								{
									client.Out.SendCharStatsUpdate();
									client.Out.SendCharResistsUpdate();
								}
							}
							catch
							{
								client.Out.SendMessage("Not set bonus value!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "weight":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.Weight = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item weight <Weight> <slot #>' to change item weight", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "dps_af":
					case "dps":
					case "af":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.DPS_AF = Convert.ToByte(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item DPS_AF <NewDPS_AF> <slot #>' to change item DPS_AF", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "spd_abs":
					case "spd":
					case "abs":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.SPD_ABS = Convert.ToByte(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item SPD_ABS <NewSPD_ABS> <slot #>' to change item SPD_ABS", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "isdropable":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.IsDropable = Convert.ToBoolean(args[2]);
							}
							catch
							{
								client.Out.SendMessage("'/item isdropable <true or false> <slot #>' to change allow item to be droped or not", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "ispickable":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.IsPickable = Convert.ToBoolean(args[2]);
							}
							catch
							{
								client.Out.SendMessage("'/item ispickable <true or false> <slot #>' to change allow item to be picked or not", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "istradable":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.IsTradable = Convert.ToBoolean(args[2]);
							}
							catch
							{
								client.Out.SendMessage("'/item ispickable <true or false> <slot #>' to change allow item to be picked or not", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "spell":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 6)
							{
								try
								{
									slot = Convert.ToInt32(args[5]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								int Charges = Convert.ToInt32(args[2]);
								int MaxCharges = Convert.ToInt32(args[3]);
								int SpellID = Convert.ToInt32(args[4]);
								item.Charges = Charges;
								item.MaxCharges = MaxCharges;
								item.SpellID = SpellID;

								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item spell <Charges> <MaxCharges> <SpellID> [slot #]' to change item spell charges", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "proc":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							try
							{
								item.ProcSpellID = Convert.ToInt32(args[2]);
								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item proc <ProcSpellID> <slot #>' to change proc type", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "poison":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 6)
							{
								try
								{
									slot = Convert.ToInt32(args[5]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								int Charges = Convert.ToInt32(args[2]);
								int MaxCharges = Convert.ToInt32(args[3]);
								int SpellID = Convert.ToInt32(args[4]);
								item.PoisonCharges = Charges;
								item.PoisonMaxCharges = MaxCharges;
								item.PoisonSpellID = SpellID;

								client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
							}
							catch
							{
								client.Out.SendMessage("'/item poison <Charges> <MaxCharges> <SpellID> [slot #]' to change item poison", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					case "realm":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.Realm = int.Parse(args[2]);
							}
							catch
							{
								client.Out.SendMessage("'/item realm <num> <slot #>' to change items realm", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "templateid":
						{
							int slot = (int)eInventorySlot.LastBackpack;

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							try
							{
								item.Id_nb = args[2];
							}
							catch
							{
								client.Out.SendMessage("'/item templateid <TemplateID> <slot #>' to change items template ID", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "savetemplate":
						{
							int slot = (int)eInventorySlot.LastBackpack;
							string name = "";

							if (args.Length >= 4)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
									slot = (int)eInventorySlot.LastBackpack;
								}

								if (slot > (int)eInventorySlot.LastBackpack)
								{
									slot = (int)eInventorySlot.LastBackpack;
								}

								if (slot < 0)
								{
									slot = 0;
								}

								name = args[2];
							}
							else if (args.Length >= 3)
							{
								name = args[2];
							}
							else
							{
								client.Out.SendMessage("Incorrect format for /item", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)slot);

							if (item == null)
							{
								client.Out.SendMessage("No item located in slot " + slot, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							//try
							//{


							ItemTemplate temp = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), name);

							bool add = false;

							if (temp == null)
							{
								add = true;
								temp = new ItemTemplate();
							}

							item.Id_nb = name;
							temp.Bonus = item.Bonus;
							temp.Bonus1 = item.Bonus1;
							temp.Bonus2 = item.Bonus2;
							temp.Bonus3 = item.Bonus3;
							temp.Bonus4 = item.Bonus4;
							temp.Bonus5 = item.Bonus5;
							temp.Bonus6 = item.Bonus6;
							temp.Bonus7 = item.Bonus7;
							temp.Bonus8 = item.Bonus8;
							temp.Bonus9 = item.Bonus9;
							temp.Bonus10 = item.Bonus10;
							temp.Bonus1Type = item.Bonus1Type;
							temp.Bonus2Type = item.Bonus2Type;
							temp.Bonus3Type = item.Bonus3Type;
							temp.Bonus4Type = item.Bonus4Type;
							temp.Bonus5Type = item.Bonus5Type;
							temp.Bonus6Type = item.Bonus6Type;
							temp.Bonus7Type = item.Bonus7Type;
							temp.Bonus8Type = item.Bonus8Type;
							temp.Bonus9Type = item.Bonus9Type;
							temp.Bonus10Type = item.Bonus10Type;
							temp.Gold = item.Gold;
							temp.Silver = item.Silver;
							temp.Copper = item.Copper;
							temp.Color = item.Color;
							temp.Condition = item.Condition;
							temp.DPS_AF = item.DPS_AF;
							temp.Durability = item.Durability;
							temp.Effect = item.Effect;
							temp.Emblem = item.Emblem;
							temp.ExtraBonus = item.ExtraBonus;
							temp.ExtraBonusType = item.ExtraBonusType;
							temp.Hand = item.Hand;
							temp.Id_nb = item.Id_nb;
							temp.IsDropable = item.IsDropable;
							temp.IsPickable = item.IsPickable;
							temp.IsTradable = item.IsTradable;
							temp.Item_Type = item.Item_Type;
							temp.Level = item.Level;
							temp.MaxCondition = item.MaxCondition;
							temp.MaxDurability = item.MaxDurability;
							temp.Model = item.Model;
							temp.Extension = item.Extension;
							temp.Name = item.Name;
							temp.Object_Type = item.Object_Type;
							temp.Quality = item.Quality;
							temp.SPD_ABS = item.SPD_ABS;
							temp.Type_Damage = item.Type_Damage;
							temp.Weight = item.Weight;
							temp.MaxCount = item.MaxCount;
							temp.PackSize = item.PackSize;
							temp.Charges = item.Charges;
							temp.MaxCharges = item.MaxCharges;
							temp.SpellID = item.SpellID;
							temp.ProcSpellID = item.ProcSpellID;
							temp.Realm = item.Realm;
							temp.PoisonCharges = item.PoisonCharges;
							temp.PoisonMaxCharges = item.PoisonMaxCharges;
							temp.PoisonSpellID = item.PoisonSpellID;

							if (add)
							{
								GameServer.Database.AddNewObject(temp);
							}
							else
							{
								GameServer.Database.SaveObject(temp);
							}

							client.Out.SendMessage("The ItemTemplate " + name + " was successfully saved", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
					case "findid":
						{
							string name = string.Join(" ", args, 2, args.Length - 2);
							ItemTemplate[] items = (ItemTemplate[])GameServer.Database.SelectObjects(typeof(ItemTemplate), "Name like '%" + GameServer.Database.Escape(name) + "%'");
							DisplayMessage(client, "Matching ID's for " + name + " count " + items.Length, new object[] { });
							foreach (ItemTemplate item in items)
							{
								DisplayMessage(client, item.Id_nb, new object[] { });
							}
							break;
						}
				}
			}
			catch (Exception e)
			{
				client.Out.SendMessage(e.Message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			return 1;
		}
	}
}