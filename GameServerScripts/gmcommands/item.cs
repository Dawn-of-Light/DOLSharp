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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	public enum eItemProperty : int
	{
		Name = 1,
		Level = 2,
		Weight = 3,
		Value = 4,
		Realm = 5,
		Model = 6,
		IsSaleable = 7,
		IsTradable = 8,
		IsDropable = 9,
		QuestName = 10,
		CrafterName = 11,
		MaxCount = 12,
		Count = 13,
		Color = 14,
		AmmuPrecision = 15,
		AmmuDamage = 16,
		AmmuRange = 17,
		DamageType = 18,
		Quality = 19,
		Bonus = 20,
		Durability = 21,
		Condition = 22,
		MaterialLevel = 23,
		ArmorFactor = 24,
		ArmorLevel = 25,
		ModelExtension = 26,
		InstrumentType = 27,
		DPS = 28,
		SPD = 29,
		HandNeeded = 30,
		GlowEffect = 31,
		ShieldSize = 32,
	}

	[Cmd("&item", //command to handle
		(uint) ePrivLevel.GM, //minimum privelege level
		"Various Item commands!", //command description
		//usage
		"Slot number is optional, if not included the default is the last backpack slot (backpack slot : first = 40, last = 79)",
		"names with spaces are given in quotes \"<name>\"",
		"'/item blank' - create a blank item",
		"'/item info <ItemTemplateID>' - get Info on a ItemTemplate",
		"'/item create <type>' - create a new item",
		"'/item instance <ItemTemplateID>' - create a new item from a template",
		"'/item change <propertyID> <value> [slot #]' - change a property of a item",
		"'/item savetemplate <NewTemplateID> [slot #]' - create a new template")]
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
							GenericItem item = new GenericItem();
							item.Name = "a blank item";
							item.Level = 1;
							item.Weight = 1;
							item.Value = 1;
							item.Realm = 0;
							item.Model = 488;
							item.IsSaleable = true;
							item.IsTradable = true;
							item.IsDropable = true;
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
					case "info":
					{
						GenericItemTemplate obj = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), args[2]);

						if (obj == null)
						{
							client.Out.SendMessage("Itemtemplate with ID:" + args[2] + " is unknown!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;

						}
						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("Item Template: " + obj.ItemTemplateID, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("         Type: " + obj.GetType(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("         Name: " + obj.Name, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("        Level: " + obj.Level, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("        Realm: " + obj.Realm, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("  Value/Price: " + obj.Value+"c", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("       Weight: " + (obj.Weight/10.0f) + "lbs", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("        Model: " + obj.Model, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("  Is dropable: " + (obj.IsDropable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("  Is saleable: " + (obj.IsSaleable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("  Is Tradable: " + (obj.IsTradable ? "yes" : "no"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						
						if(obj is AmmunitionTemplate)
						{
							client.Out.SendMessage("  Precision: " + ((AmmunitionTemplate)obj).Precision + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("     Damage: " + ((AmmunitionTemplate)obj).Damage + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("      Range: " + ((AmmunitionTemplate)obj).Range + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("Damage type: " + ((AmmunitionTemplate)obj).DamageType + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}

						if(obj is EquipableItemTemplate)
						{
							client.Out.SendMessage("      Quality: " + ((EquipableItemTemplate)obj).Quality + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Bonus: " + ((EquipableItemTemplate)obj).Bonus + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("   Durability: " + ((EquipableItemTemplate)obj).Durability + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("    Condition: " + (int)(((EquipableItemTemplate)obj).Condition) + "%", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("MaterialLevel: " + ((EquipableItemTemplate)obj).MaterialLevel, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("Allowed class: " + ((((EquipableItemTemplate)obj).AllowedClass.Count == 0) ? "all" : ""), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							foreach(eCharacterClass cl in ((EquipableItemTemplate)obj).AllowedClass)
								client.Out.SendMessage(cl.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							if(((EquipableItemTemplate)obj).MagicalBonus.Count > 0)
								client.Out.SendMessage("Magical bonus: ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							foreach(ItemMagicalBonus bn in ((EquipableItemTemplate)obj).MagicalBonus)
								client.Out.SendMessage(" - "+ bn.BonusType +" : "+ bn.Bonus, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							
							if(obj is VisibleEquipmentTemplate)
							{
								client.Out.SendMessage("    Color: " + ((VisibleEquipmentTemplate)obj).Color , eChatType.CT_System, eChatLoc.CL_PopupWindow);
								if(obj is InstrumentTemplate)
								{
									client.Out.SendMessage("     Type: " + ((InstrumentTemplate)obj).Type, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}
								if(obj is ArmorTemplate)
								{
									client.Out.SendMessage("    Factor: " + ((ArmorTemplate)obj).ArmorFactor, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage("       ABS: " + ((ArmorTemplate)obj).ArmorLevel, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage(" Model ext: " + ((ArmorTemplate)obj).ModelExtension, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}
								if(obj is WeaponTemplate)
								{
									client.Out.SendMessage("       DPS: " + ((WeaponTemplate)obj).DamagePerSecond, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage("       SPD: " + ((WeaponTemplate)obj).Speed +"ms", eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage("DamageType: " + ((WeaponTemplate)obj).DamageType, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage("HandNeeded: " + ((WeaponTemplate)obj).HandNeeded, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									client.Out.SendMessage("GlowEffect: " + ((WeaponTemplate)obj).GlowEffect, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									if(obj is ShieldTemplate)
									{
										client.Out.SendMessage(" Shield size: " + ((ShieldTemplate)obj).Size, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									}
									if(obj is RangedWeaponTemplate)
									{
										client.Out.SendMessage("   Range: " + ((RangedWeaponTemplate)obj).WeaponRange, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									}
								}
							}
						}
						break;
					}
					case "create":
					{
						//Create a new object
						try
						{
							GenericItem item = Assembly.GetAssembly(typeof (GameServer)).CreateInstance(args[2], false) as GenericItem;
							if(item == null)
							{
								client.Out.SendMessage("Object of type "+args[2]+" can't be added to a player inventory.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}

							item.Name = "New item";
							item.Level = 0;
							item.Weight = 0;
							item.Value = 0;
							item.Realm = eRealm.None;
							item.Model = 488;
							item.IsSaleable = true;
							item.IsTradable = true;
							item.IsDropable = true;
							item.QuestName = "";
							item.CrafterName = "";

							if(item is StackableItem)
							{
								((StackableItem)item).Count = 1;
								((StackableItem)item).MaxCount = 1;
							}
							if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							{
								client.Out.SendMessage("Item with type "+args[2]+" created.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("Type /item for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}

						break;
					}
					case "instance":
						{
							//Create a new object
							try
							{
								GenericItemTemplate template = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), args[2]);
								if (template == null)
								{
									client.Out.SendMessage("ItemTemplate with id " + args[2] + " could not be found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 0;
								}
								
								GenericItem item = template.CreateInstance();
								if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
								{
									client.Out.SendMessage("Item created: level= " + item.Level + " name= " + item.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
							catch (Exception)
							{
								client.Out.SendMessage("Type /item for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}

							break;
						}
					case "change":
						{
							if(args.Length < 4 || args.Length > 5)
							{
								client.Out.SendMessage("Property ID description :",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					
								foreach(int valeur in Enum.GetValues(typeof(eItemProperty)))
								{
									client.Out.SendMessage(valeur +" = "+ Enum.GetName(typeof(eItemProperty), valeur),eChatType.CT_System,eChatLoc.CL_SystemWindow);
								}
								return 1;
							}

							try
							{
								eItemProperty itemProperty = (eItemProperty)Convert.ToInt32(args[2]);
								int slot = (int) eInventorySlot.LastBackpack;
								if (args.Length > 4)
								{
									try
									{
										slot = Convert.ToInt32(args[4]);
									}
									catch
									{
									}
								}

								GenericItem item = client.Player.Inventory.GetItem((eInventorySlot) slot) as GenericItem;
								if (item == null)
								{
									client.Out.SendMessage("No item in slot " + slot + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 0;
								}

								switch(itemProperty)
								{
									case eItemProperty.Name : item.Name = args[3];
										break;
									case eItemProperty.Level: item.Level = Convert.ToByte(args[3]);
										break;
									case eItemProperty.Weight: item.Weight = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.Value: item.Value = Convert.ToInt64(args[3]);
										break;
									case eItemProperty.Realm: item.Realm = (eRealm)Convert.ToByte(args[3]);
										break;
									case eItemProperty.Model: item.Model = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.IsSaleable: item.IsSaleable = Convert.ToBoolean(args[3]);
										break;
									case eItemProperty.IsTradable: item.IsTradable = Convert.ToBoolean(args[3]);
										break;
									case eItemProperty.IsDropable: item.IsDropable = Convert.ToBoolean(args[3]);
										break;
									case eItemProperty.QuestName : item.QuestName = args[3];
										break;
									case eItemProperty.CrafterName : item.CrafterName = args[3];
										break;
									case eItemProperty.MaxCount : if(item is StackableItem) ((StackableItem)item).MaxCount = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.Count : if(item is StackableItem) ((StackableItem)item).Count = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.Color :  if(item is VisibleEquipment) ((VisibleEquipment)item).Color = Convert.ToInt32(args[3]);
																if(item is Dye) ((Dye)item).Color = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.AmmuPrecision :  if(item is Ammunition) ((Ammunition)item).Precision = (ePrecision)Convert.ToByte(args[3]);
										break;
									case eItemProperty.AmmuDamage :  if(item is Ammunition) ((Ammunition)item).Damage = (eDamageLevel)Convert.ToByte(args[3]);
										break;
									case eItemProperty.AmmuRange :  if(item is Ammunition) ((Ammunition)item).Range = (eRange)Convert.ToByte(args[3]);
										break;
									case eItemProperty.DamageType :  if(item is Weapon) ((Weapon)item).DamageType = (eDamageType)Convert.ToByte(args[3]);
																	 if(item is Ammunition) ((Ammunition)item).DamageType = (eDamageType)Convert.ToByte(args[3]);
										break;
									case eItemProperty.Quality :  if(item is EquipableItem) ((EquipableItem)item).Quality = Convert.ToByte(args[3]);
																	if(item is SpellCraftGem) ((SpellCraftGem)item).Quality = Convert.ToByte(args[3]);
										break;
									case eItemProperty.Bonus :  if(item is EquipableItem) ((EquipableItem)item).Bonus = Convert.ToByte(args[3]);
																if(item is MagicalDust) ((MagicalDust)item).Bonus = Convert.ToByte(args[3]);
										break;
									case eItemProperty.Durability :  if(item is EquipableItem) ((EquipableItem)item).Durability = Convert.ToByte(args[3]);
										break;
									case eItemProperty.Condition :  if(item is EquipableItem) ((EquipableItem)item).Condition = Convert.ToByte(args[3]);
										break;
									case eItemProperty.MaterialLevel :  if(item is EquipableItem) ((EquipableItem)item).MaterialLevel = (eMaterialLevel)Convert.ToByte(args[3]);
										break;
									case eItemProperty.ArmorFactor :  if(item is Armor) ((Armor)item).ArmorFactor = Convert.ToByte(args[3]);
										break;
									case eItemProperty.ArmorLevel :  if(item is Armor) ((Armor)item).ArmorLevel = (eArmorLevel)Convert.ToByte(args[3]);
										break;
									case eItemProperty.ModelExtension :  if(item is Armor) ((Armor)item).ModelExtension = Convert.ToByte(args[3]);
										break;
									case eItemProperty.InstrumentType :  if(item is Instrument) ((Instrument)item).Type = (eInstrumentType)Convert.ToByte(args[3]);
										break;
									case eItemProperty.DPS :  if(item is Weapon) ((Weapon)item).DamagePerSecond = Convert.ToByte(args[3]);
										break;
									case eItemProperty.SPD :  if(item is Weapon) ((Weapon)item).Speed = Convert.ToInt32(args[3]);
										break;
									case eItemProperty.HandNeeded :  if(item is Weapon) ((Weapon)item).HandNeeded = (eHandNeeded)Convert.ToByte(args[3]);
										break;
									case eItemProperty.GlowEffect :  if(item is Weapon) ((Weapon)item).GlowEffect = Convert.ToByte(args[3]);
										break;
									case eItemProperty.ShieldSize :  if(item is Shield) ((Shield)item).Size = (eShieldSize)Convert.ToByte(args[3]);
										break;
								}

								client.Player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
								client.Player.UpdateEncumberance();
							}
							catch
							{
								client.Out.SendMessage("'/item change <propertyID> <value> [slot #]' to change a item property", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}
					
					case "savetemplate":
						{
							if (args.Length < 3)
							{
								client.Out.SendMessage("Incorrect format for /item", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							int slot = (int) eInventorySlot.LastBackpack;
							string name = args[2];

							if (args.Length > 3)
							{
								try
								{
									slot = Convert.ToInt32(args[3]);
								}
								catch
								{
								}
							}
								
				
							GenericItem item = client.Player.Inventory.GetItem((eInventorySlot) slot) as GenericItem;
							if (item == null)
							{
								client.Out.SendMessage("No item located in slot " + slot, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							
							GenericItemTemplate temp = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), name);
							if (temp != null)
							{
								client.Out.SendMessage("A template with this name already exist.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							
							temp = Assembly.GetAssembly(typeof(GameServer)).CreateInstance(item.GetType().FullName + "Template", false) as GenericItemTemplate;
							temp.ItemTemplateID = name;
							temp.Name = item.Name;
							
							temp.IsDropable = item.IsDropable;
							temp.Level = item.Level;
							temp.Model = item.Model;
							temp.Name = item.Name;
							temp.Weight = item.Weight;
							temp.Realm = item.Realm;

							
							GameServer.Database.AddNewObject(temp);
							

							client.Out.SendMessage("The ItemTemplate with the id " + name + " was successfully saved", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
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