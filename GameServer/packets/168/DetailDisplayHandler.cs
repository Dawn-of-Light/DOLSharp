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
using DOL.GS.Database;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using DOL.GS.Styles;

namespace DOL.GS.PacketHandler.v168
{
	/// <summary>
	/// delve button shift+i = detail of spell object...
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x70^168,"Handles detail display")]
	public class DetailDisplayHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort objectType = packet.ReadShort();
			ushort objectID = packet.ReadShort();
			string caption = "";
			ArrayList objectInfo = new ArrayList();
//			DOLConsole.WriteLine("type="+objectType+" id="+objectID);

			switch (objectType)
			{
				case 1: //Display Infos on inventory item
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if(item == null)
						return 1;

					caption = item.Name;

					objectInfo.AddRange(item.DelveInfo);
					objectInfo.Add("");

					if(!item.IsSaleable) objectInfo.Add("This object can't be sold.");
					if(!item.IsTradable) objectInfo.Add("This object can't be traded.");
					if(!item.IsDropable) objectInfo.Add("This object can't be dropped.");

					//Add admin info
					if (client.Account.PrivLevel > ePrivLevel.Player)
					{
						objectInfo.Add("");
						objectInfo.Add("----------Technical informations----------");
						objectInfo.Add("	  Item ID: "+item.ItemID);
						objectInfo.Add("   Class Name: "+item.GetType());
						objectInfo.Add("   ObjectType: "+GlobalConstants.ObjectTypeToName((int)item.ObjectType));
						objectInfo.Add("         Name: "+item.Name);
						objectInfo.Add("        Level: "+item.Level);
						objectInfo.Add("       Weight: "+item.Weight);
						objectInfo.Add("        Model: "+item.Model);
						objectInfo.Add("        Realm: "+item.Realm);
						objectInfo.Add("        Value: "+item.Value);
						objectInfo.Add("  Is dropable: "+(item.IsDropable?"yes":"no"));
						objectInfo.Add("  Is tradable: "+(item.IsTradable?"yes":"no"));
						objectInfo.Add("  Is saleable: "+(item.IsSaleable?"yes":"no"));
						objectInfo.Add("   Quest name: "+item.QuestName);
						objectInfo.Add(" Crafter name: "+item.CrafterName);
						
						if(item is EquipableItem)
						{
							objectInfo.Add("         Quality: "+((EquipableItem)item).Quality);
							objectInfo.Add("      Durability: "+((EquipableItem)item).Durability);
							objectInfo.Add("       Condition: "+((EquipableItem)item).Condition);
							objectInfo.Add("           Bonus: "+((EquipableItem)item).Bonus);
							objectInfo.Add("   MaterialLevel: "+((EquipableItem)item).MaterialLevel);
							objectInfo.Add("  ProcEffectType: "+((EquipableItem)item).ProcEffectType);
							objectInfo.Add("ChargeEffectType: "+((EquipableItem)item).ChargeEffectType);
							
							if(item is VisibleEquipment)
							{
								objectInfo.Add("   Color: "+((VisibleEquipment)item).Color);
							
								if(item is Instrument)
								{
									objectInfo.Add("  InstrType: "+((Instrument)item).Type);
								}

								if(item is Armor)
								{
									objectInfo.Add("   ArmorFactor: "+((Armor)item).ArmorFactor);
									objectInfo.Add("   ArmorLevel : "+((Armor)item).ArmorLevel);
									objectInfo.Add("ModelExtension: "+((Armor)item).ModelExtension);
								}

								if(item is Weapon)
								{
									objectInfo.Add("         DPS: "+((Weapon)item).DamagePerSecond);
									objectInfo.Add("       Speed: "+((Weapon)item).Speed/1000+"."+((Weapon)item).Speed%1000);
									objectInfo.Add("  DamageType: "+((Weapon)item).DamageType);
									objectInfo.Add("  HandNeeded: "+((Weapon)item).HandNeeded);
									objectInfo.Add("GlowEffectID: "+((Weapon)item).GlowEffect);

									if(item is Shield)
									{
										objectInfo.Add("  Shield Size: "+((Shield)item).Size);
									}
								}
							}
						}
					}


					break;
				}

				case 2: //spell
				{
					int lineID = objectID/100;
					int spellID = objectID%100;
					//DOLConsole.WriteLine("lineID="+lineID+"; spellID="+spellID);

					SpellLine spellLine = client.Player.GetSpellLines()[lineID] as SpellLine;
					if(spellLine == null)
						return 1;

					Spell spell = null;
					foreach(Spell spl in SkillBase.GetSpellList(spellLine.KeyName))
						if(spl.Level == spellID)
						{
							spell = spl;
							break;
						}

					if(spell == null)
						return 1;

					caption = spell.Name;
					ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellLine);
					if(spellHandler == null)
					{
						objectInfo.Add(" ");
						objectInfo.Add("Spell type (" + spell.SpellType + ") is not implemented.");
					}
					else
					{
						objectInfo.AddRange(spellHandler.DelveInfo);
					}
					break;
				}

				case 3: //spell
				{
					IList skillList = client.Player.GetNonTrainableSkillList();
					IList styles = client.Player.GetStyleList();
					int index = objectID - skillList.Count - styles.Count - 100;

					IList spelllines = client.Player.GetSpellLines();
					if(spelllines == null || index < 0)
						break;

					lock (spelllines.SyncRoot)
					{
						foreach (SpellLine spellline in spelllines)
						{
							IList spells = client.Player.GetUsableSpellsOfLine(spellline);
							if( index >= spells.Count )
							{
									index -= spells.Count;
							}
							else
							{
								Spell spell= (Spell)spells[index];
								caption = spell.Name;
								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellline);
								if(spellHandler == null)
								{
									objectInfo.Add(" ");
									objectInfo.Add("Spell type (" + spell.SpellType + ") is not implemented.");
								}
								else
								{
									objectInfo.AddRange(spellHandler.DelveInfo);
								}
								break;
							}
						}
					}
					break;
				}


				case 4: //Display Infos on Merchant objects
				{
					IGameMerchant merchant = client.Player.TargetObject as IGameMerchant;
					if (merchant == null)
						return 1;

					int pageNumber= objectID / MerchantPage.MAX_ITEMS_IN_MERCHANTPAGE;
					int slotNumber= objectID % MerchantPage.MAX_ITEMS_IN_MERCHANTPAGE;

					GenericItemTemplate itemTemplate = null;

					MerchantWindow window = GameServer.Database.FindObjectByKey(typeof(MerchantWindow), merchant.MerchantWindowID) as MerchantWindow;	
					if (window != null)
					{
						MerchantPage page = window.MerchantPages[pageNumber] as MerchantPage;
						if(page != null)
						{
							MerchantItem merchantItem = page.MerchantItems[slotNumber] as MerchantItem;
							if (merchantItem != null)
							{
								itemTemplate = merchantItem.ItemTemplate;
							}
						}
					}

					if (itemTemplate == null) return 1;

					caption = itemTemplate.Name;

					objectInfo.AddRange(itemTemplate.GetDelveInfo(client.Player));
					objectInfo.Add("");

					if(!itemTemplate.IsSaleable) objectInfo.Add("This object can't be sold.");
					if(!itemTemplate.IsTradable) objectInfo.Add("This object can't be traded.");
					if(!itemTemplate.IsDropable) objectInfo.Add("This object can't be dropped.");

					break;
				}

				case 5: //icons on top (buffs/dots)
				{
					IGameEffect foundEffect = null;
					lock (client.Player.EffectList)
					{
						foreach (IGameEffect effect in client.Player.EffectList)
							if (effect.InternalID == objectID)
							{
								foundEffect = effect;
								break;
							}
					}

					if(foundEffect == null) break;

					caption = foundEffect.Name;
					objectInfo.AddRange(foundEffect.DelveInfo);

					break;
				}
				case 6: //style
				{
					IList styleList = client.Player.GetStyleList();
					IList skillList = client.Player.GetNonTrainableSkillList();
					Style style = null;
					int styleID = objectID - skillList.Count - 100;
					//DOLConsole.WriteLine("style id="+styleID+"; skills count="+skillList.Count);

					if(styleID < 0 || styleID >= styleList.Count) break;

					style = styleList[styleID] as Style;
					if(style == null) break;

					caption = style.Name;
					objectInfo.Add("Weapon Type:");
					objectInfo.Add("-" + style.GetRequiredWeaponName());
					objectInfo.Add(" "); //empty line


					objectInfo.Add("Opening:");
					if(Style.eOpening.Offensive == style.OpeningRequirementType)
					{
						//attacker action result is opening
						switch(style.AttackResultRequirement)
						{
							case Style.eAttackResult.Hit:    objectInfo.Add("-You Hit"); break;
							case Style.eAttackResult.Miss:   objectInfo.Add("-You Miss"); break;
							case Style.eAttackResult.Parry:  objectInfo.Add("-Target Parrys"); break;
							case Style.eAttackResult.Block:  objectInfo.Add("-Target Blocks"); break;
							case Style.eAttackResult.Evade:  objectInfo.Add("-Target Evades"); break;
							case Style.eAttackResult.Fumble: objectInfo.Add("-You Fumble"); break;
							case Style.eAttackResult.Style:
								Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, client.Player.CharacterClass.ID);
								if(reqStyle == null)         objectInfo.Add("-(style not found " + style.OpeningRequirementValue + ")");
								else                         objectInfo.Add("-"+reqStyle.Name);
								break;
							default: objectInfo.Add(" - None"); break;
						}
					}
					else if(Style.eOpening.Defensive == style.OpeningRequirementType)
					{
						//defender action result is opening
						switch(style.AttackResultRequirement)
						{
							case Style.eAttackResult.Miss:   objectInfo.Add("-Target Misses"); break;
							case Style.eAttackResult.Hit:    objectInfo.Add("-Target Hits"); break;
							case Style.eAttackResult.Parry:  objectInfo.Add("-You Parry"); break;
							case Style.eAttackResult.Block:  objectInfo.Add("-You Block"); break;
							case Style.eAttackResult.Evade:  objectInfo.Add("-You Evade"); break;
							case Style.eAttackResult.Fumble: objectInfo.Add("-Target Fumbles"); break;
							case Style.eAttackResult.Style:  objectInfo.Add("-Target Style"); break;
							default: objectInfo.Add(" - None"); break;
						}
					}
					else if(Style.eOpening.Positional == style.OpeningRequirementType)
					{
						//attacker position to target is opening
						objectInfo.Add(" - Position Based");
						switch(style.OpeningRequirementValue)
						{
							case (int)Style.eOpeningPosition.Front: objectInfo.Add("-Front"); break;
							case (int)Style.eOpeningPosition.Back:  objectInfo.Add("-Back"); break;
							case (int)Style.eOpeningPosition.Side:  objectInfo.Add("-Side"); break;
						}
					}
					objectInfo.Add(" "); //empty line

					if(style.SpecialType != Style.eSpecialType.None)
					{
						objectInfo.Add("Target Effect");
						if(style.SpecialType == Style.eSpecialType.Effect)
						{
							SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
							if (styleLine != null)
							{
								IList spells = SkillBase.GetSpellList(styleLine.KeyName);
								if (spells != null)
								{
									Spell spell = null;
									foreach(Spell spl in spells)
										if(spl.ID == style.SpecialValue)
										{
											spell = spl;
											break;
										}

									if (spell != null)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, styleLine);
										if(spellHandler == null)
										{
											objectInfo.Add("-"+ spell.Name +" (Not implemented yet)");
										}
										else
										{
											objectInfo.Add("-"+ spell.Name);
										}
									}
								}
							}
						}
						else if(style.SpecialType == Style.eSpecialType.Taunt && style.SpecialValue != 0)
						{
//							objectInfo.Add("Target Effect");
							if(style.SpecialValue < 0) objectInfo.Add("-Detaunt");
							else                       objectInfo.Add("-Taunt");
						}
						objectInfo.Add(" "); //empty line
					}


					objectInfo.Add("Fatigue Cost");
					if(style.EnduranceCost < 5)       objectInfo.Add(" - Very Low");
					else if(style.EnduranceCost < 10) objectInfo.Add(" - Low");
					else if(style.EnduranceCost < 15) objectInfo.Add(" - Medium");
					else if(style.EnduranceCost < 20) objectInfo.Add(" - High");
					else                              objectInfo.Add(" - Very High");
					objectInfo.Add(" "); //empty line


					objectInfo.Add("Damage");
					if(style.GrowthRate == 0) objectInfo.Add(" - None");
					else objectInfo.Add(" - " + style.GrowthRate);
					objectInfo.Add(" "); //empty line


					objectInfo.Add("To-Hit Bonus");
					if(style.BonusToHit <= -20)      objectInfo.Add(" - Very High Penalty");
					else if(style.BonusToHit <= -15) objectInfo.Add(" - High Penalty");
					else if(style.BonusToHit <= -10) objectInfo.Add(" - Med Penalty");
					else if(style.BonusToHit <= -5)  objectInfo.Add(" - Low Penalty");
					else if(style.BonusToHit < 0)    objectInfo.Add(" - Very Low Penalty");
					else if(style.BonusToHit == 0)   objectInfo.Add(" - None");
					else if(style.BonusToHit < 5)    objectInfo.Add(" - Very Low Bonus");
					else if(style.BonusToHit < 10)   objectInfo.Add(" - Low Bonus");
					else if(style.BonusToHit < 15)   objectInfo.Add(" - Med Bonus");
					else if(style.BonusToHit < 20)   objectInfo.Add(" - High Bonus");
					else                             objectInfo.Add(" - Very High Bonus");
					objectInfo.Add(" "); //empty line


					objectInfo.Add("Defense Bonus");
					if(style.BonusToDefense <= -20)      objectInfo.Add(" - Very High Penalty");
					else if(style.BonusToDefense <= -15) objectInfo.Add(" - High Penalty");
					else if(style.BonusToDefense <= -10) objectInfo.Add(" - Med Penalty");
					else if(style.BonusToDefense <= -5)  objectInfo.Add(" - Low Penalty");
					else if(style.BonusToDefense < 0)    objectInfo.Add(" - Very Low Penalty");
					else if(style.BonusToDefense == 0)   objectInfo.Add(" - None");
					else if(style.BonusToDefense < 5)    objectInfo.Add(" - Very Low Bonus");
					else if(style.BonusToDefense < 10)   objectInfo.Add(" - Low Bonus");
					else if(style.BonusToDefense < 15)   objectInfo.Add(" - Med Bonus");
					else if(style.BonusToDefense < 20)   objectInfo.Add(" - High Bonus");
					else                                 objectInfo.Add(" - Very High Bonus");
					break;
				}

				case 7: //trade windows
				{
					GenericItem item = null;

					if(client.Player.TradeWindow.PartnerTradeItems != null && objectID < client.Player.TradeWindow.PartnerItemsCount)
					{
						item = (GenericItem)client.Player.TradeWindow.PartnerTradeItems[objectID];
					}

					if(item == null)
						return 1;

					caption = item.Name;

					objectInfo.AddRange(item.DelveInfo);
					objectInfo.Add("");

					if(!item.IsSaleable) objectInfo.Add("This object can't be sold.");
					if(!item.IsTradable) objectInfo.Add("This object can't be traded.");
					if(!item.IsDropable) objectInfo.Add("This object can't be dropped.");

					break;

				}

				case 9: //trainer window "info" button
				{
					IList specList = client.Player.GetSpecList();
					Specialization spec;
					if (objectID < specList.Count)
					{
						spec = (Specialization)specList[objectID];
					}
					else
					{
						caption = "Specialization not found";
						objectInfo.Add("that specialization is not found, id=" + objectID);
						break;
					}


					IList styles = SkillBase.GetStyleList(spec.KeyName, client.Player.CharacterClass.ID);
					IList playerSpells = client.Player.GetSpellLines();
					SpellLine selectedSpellLine = null;

					lock (playerSpells.SyncRoot)
					{
						foreach(SpellLine line in playerSpells)
						{
							if(!line.IsBaseLine && line.Spec == spec.KeyName)
							{
								selectedSpellLine = line;
								break;
							}
						}
					}

					IList spells = new ArrayList();
					if(selectedSpellLine != null)
						spells = SkillBase.GetSpellList(selectedSpellLine.KeyName);




					caption = spec.Name;

					if(styles.Count <= 0 && playerSpells.Count <= 0)
					{
						//objectInfo.Add("no info found for this spec");
						break;
					}

					objectInfo.Add("Lev   Name");
					foreach(Style style in styles)
					{
						objectInfo.Add(" ");
						objectInfo.Add(style.Level + " - " + style.Name);
					}
					foreach(Spell spell in spells)
					{
						objectInfo.Add(" ");
						objectInfo.Add(spell.Level + " - " + spell.Name);
					}
					break;
				}
				case 12: // Item info to Group Chat
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if(item == null) return 1;
					string str = client.Player.Name+"/Item: \"" + GetShortItemInfo(item, client) + '"';
					if (client.Player.PlayerGroup == null)
					{
						client.Out.SendMessage("You are not part of a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					client.Player.PlayerGroup.SendMessageToGroupMembers(null, str);
					return 1;
				}
				case 13: // Item info to Guild Chat
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if(item == null) return 1;
					string str = "[Guild] "+client.Player.Name+"/Item: \"" + GetShortItemInfo(item, client) + '"';
					if (client.Player.Guild == null)
					{
						client.Out.SendMessage("You don't belong to a player guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.GcSpeak))
					{
						client.Out.SendMessage("You don't have permission to speak on the on guild line.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers)
					{
						if (!client.Player.Guild.CheckGuildPermission(ply, eGuildPerm.GcHear)) continue;
						ply.Out.SendMessage(str, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
					}
					return 1;
				}
				case 15: // Item info to Chat group
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if(item == null) return 1;

					ChatGroup mychatgroup = (ChatGroup) client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
					if (mychatgroup == null)
					{
						client.Player.Out.SendMessage("You must be in a chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					if (mychatgroup.Listen == true && (((bool) mychatgroup.Members[client.Player]) == false))
					{
						client.Player.Out.SendMessage("Only moderator can talk on this chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					string str = "[Chat] "+client.Player.Name+"/Item: \"" + GetShortItemInfo(item, client) + '"';
					foreach (GamePlayer ply in mychatgroup.Members.Keys)
					{
						ply.Out.SendMessage(str, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
					}
					return 1;
				}
				case 100://repair
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if (item != null)
					{
						client.Player.RepairItem(item);
					}
					else
					{
						client.Out.SendMessage("It's very strange, i cant recognise this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
					return 1;
				}
				case 101://selfcraft
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if (item != null)
					{
						client.Player.OpenSelfCraft(item);
					}
					else
					{
						client.Out.SendMessage("It's very strange, i cant recognise this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
					return 1;
				}
				case 102://salvage
				{
					GenericItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID) as GenericItem;
					if (item != null)
					{
						client.Player.SalvageItem(item);
					}
					else
					{
						client.Out.SendMessage("It's very strange, i cant recognise this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
					return 1;
				}
				default:
					client.Out.SendMessage("No special information!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
			}

			client.Out.SendCustomTextWindow(caption, objectInfo);

			return 1;
			//WorldMgr.GetObjectByIDFromRegion(client.Player.CurrentRegionID, objectid );
			/*
Type    Description           Id
1       Inventory item        Slot (ie. 0xC for 2 handed weapon)
2       Spell                 spell level + spell line ID * 100 (starting from 0)
3       ???
4       Merchant item         Slot (divide by 30 to get page)
5       Buff/effect           The buff id (each buff has a unique id)
6       Style                 style list index = ID-100-abilities count
7       Trade window          position in trade window (starting form 0)
8       Ability               100+position in players abilities list (?)
9       Trainers skill        position in trainers window list
			*/
		}

		protected string GetShortItemInfo(GenericItem item, GameClient client)
		{
			// TODO: correct info format if anyone is interested...
/*
			[Guild] Player/Item:  "- [Kerubis' Scythe]: scythe No Sell. No Destroy."
			[Guild] Player/Item: "- [Adroit Runed Duelist Rapier]: rapier 14.1 DPS 3.6 speed 89% qual 100% con (Thrust). Bonuses:  3% Spirit, 19 Dexterity, 2 Thrust, 6% Thrust, Tradeable.".
			[Party] Player/Item: "- [Matterbender Belt]: belt 4% Energy, 8% Matter, 9 Constitution, 4% Slash, Tradeable."

			multiline item...
			[Guild] Player/Item: "- [shimmering Archmage Focus Briny Staff]: staff for Theurgist, Wizard, Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, Theurgist, Wizard,
			Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, 16.5".
			[Guild] Player/Item: "- DPS 3.0 speed 94% qual 100% con (Crush) (Two Handed). Bonuses:
			22 Hits, 5% Energy, 2 All Casting, 50 lvls ALL focus 7% buff bonus, (10/10 char
			ges) health regen Value: 8  Tradeable.".
*/

			string str = "- ["+item.Name+"]: "+GlobalConstants.ObjectTypeToName((int)item.ObjectType);
			ArrayList objectInfo = new ArrayList();

			if (item is Weapon)
			{
				WriteMagicalBonuses(objectInfo, (EquipableItem)item, client, true);
				WriteClassicWeaponInfos(objectInfo, (Weapon)item, client);
			}
			if (item is Armor)
			{
				WriteMagicalBonuses(objectInfo, (EquipableItem)item, client, true);
				WriteClassicArmorInfos(objectInfo, (Armor)item, client);
			}
			if (item is Shield)
			{
				WriteMagicalBonuses(objectInfo, (EquipableItem)item, client, true);
				WriteClassicShieldInfos(objectInfo, (Shield)item, client);
			}
			
			if (item.CrafterName != null && item.CrafterName != "")
			{
				objectInfo.Add(" ");//empty line
				objectInfo.Add("Crafter : "+item.CrafterName);
			}
			if (item is Poison)
			{
				WritePoisonInfo(objectInfo, (Poison)item, client);
			}

			foreach(string s in objectInfo)
				str+=" "+s;
			return str;
		}


		/// <summary>
		///
		///
		/// Damage Modifiers:
		/// - X.X Base DPS
		/// - X.X Clamped DPS
		/// - XX Weapon Speed
		/// - XX% Quality
		/// - XX% Condition
		/// Damage Type: XXX
		///
		/// Effective Damage:
		/// - X.X DPS
		/// </summary>
		public void WriteClassicWeaponInfos(ArrayList output, Weapon item, GameClient client)
		{
			double itemDPS = item.DamagePerSecond/10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.Speed/1000.0;
			double effectiveDPS = itemDPS * item.Quality/100.0 * item.Condition/100;

			output.Add(" ");
			output.Add(" ");
			output.Add("Damage Modifiers:");
			if(itemDPS != 0)
			{
				output.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				output.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
			}

			if(item.Speed >= 0)
			{
                output.Add("- " + itemSPD.ToString("0.0") + " Weapon Speed");
			}

			if(item.Quality != 0)
			{
				output.Add("- " + item.Quality + "% Quality");
			}
			if(item.Condition != 0)
			{
				output.Add("- " + (byte)item.Condition + "% Condition");
			}

			string damageType = "None";
			switch(item.DamageType)
			{
				case eDamageType.Crush:  damageType="Crush"; break;
				case eDamageType.Slash:  damageType="Slash"; break;
				case eDamageType.Thrust: damageType="Thrust"; break;
				default: damageType="Natural"; break;
			}
			output.Add("Damage Type: " + damageType);
			
			output.Add(" ");

			output.Add("Effective Damage:");
			if(itemDPS != 0)
			{
				output.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
			}
		}

		/// <summary>
		///
		///
		/// Damage Modifiers (when used with shield styles):
		/// - X.X Base DPS
		/// - X.X Clamped DPS
		/// - XX Shield Speed
		/// </summary>
		public void WriteClassicShieldInfos(ArrayList output, Shield item, GameClient client)
		{
			double itemDPS = item.DamagePerSecond/10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.Speed/1000.0;

			output.Add(" ");
			output.Add(" ");
			output.Add("Damage Modifiers (when used with shield styles):");
			if(itemDPS != 0)
			{
				output.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				output.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
			}
			if(item.Speed >= 0)
			{
                output.Add("- " + itemSPD.ToString("0.0") + " Shield Speed");
			}

			output.Add(" ");

			switch (item.Size)
			{
				case eShieldSize.Small: output.Add("- Shield Size : Small"); break;
				case eShieldSize.Medium: output.Add("- Shield Size : Medium"); break;
				case eShieldSize.Large: output.Add("- Shield Size : Large"); break;
			}
		}

		/// <summary>
		///
		///
		/// Armor Modifiers:
		/// - X.X Base Factor
		/// - X.X Clamped Factor
		/// - XX% Absorption
		/// - XX% Quality
		/// - XX% Condition
		/// Damage Type: XXX
		///
		/// Effective Armor:
		/// - X.X Factor
		/// </summary>
		public void WriteClassicArmorInfos(ArrayList output, Armor item,GameClient client)
		{
			output.Add(" ");
			output.Add(" ");
			output.Add("Armor Modifiers:");
			if(item.ArmorFactor != 0)
			{
				output.Add("- " + item.ArmorFactor + " Base Factor");
			}
			double AF = 0;
			if(item.ArmorFactor != 0)
			{
				int afCap = client.Player.Level;
				if (item.ArmorLevel != eArmorLevel.VeryLow) // cloth
				{
					afCap *= 2;
				}

				AF = Math.Min(afCap, item.ArmorFactor);

				output.Add("- " + (int)AF + " Clamped Factor");
			}
			if((byte)item.ArmorLevel >= 0)
			{
				output.Add("- " + item.Absorbtion + "% Absorption");
			}
			if(item.Quality != 0)
			{
				output.Add("- " + item.Quality + "% Quality");
			}
			if(item.Condition != 0)
			{
				output.Add("- " + (byte)item.Condition + "% Condition");
			}
			output.Add(" ");

			output.Add("Effective Armor:");
			double EAF = 0;
			if(item.ArmorFactor != 0)
			{
				EAF = AF * item.Quality/100.0 * item.Condition/100 * (1 + item.Absorbtion/100.0 );
				output.Add("- " + (int)EAF + " Factor");
			}

		}

		public void WriteMagicalBonuses(ArrayList output, EquipableItem item, GameClient client, bool shortInfo)
		{
			int oldCount = output.Count;

			foreach(ItemMagicalBonus bonus in item.MagicalBonus)
			{
				WriteBonusLine(output, bonus.BonusType, bonus.Bonus);
			}
			
			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, "Magical Bonuses:");
				output.Insert(oldCount, " ");
			}

			oldCount = output.Count;

			foreach(ItemMagicalBonus bonus in item.MagicalBonus)
			{
				WriteFocusLine(output, bonus.BonusType, bonus.Bonus);
			}
			
			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, "Focus Bonuses:");
				output.Insert(oldCount, " ");
			}

			if (!shortInfo)
			{
				if(item.ProcEffectType == eMagicalEffectType.ActiveEffect || item.ProcEffectType == eMagicalEffectType.ReactiveEffect)
				{
					string spellNote = "";
					output.Add(" ");
					output.Add("Magical Ability:");
					if (item.ProcEffectType == eMagicalEffectType.ActiveEffect)
					{
						spellNote = "- Spell has a chance of casting when this weapon strikes an enemy.";
					}
					else if (item.ProcEffectType == eMagicalEffectType.ReactiveEffect)
					{
						spellNote = "- Spell has a chance of casting when enemy strikes at this armor.";
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						IList spells = SkillBase.GetSpellList(line.KeyName);
						if (spells != null)
						{
							foreach(Spell spl in spells)
							{
								if(spl.ID == item.ProcSpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, line);
									if(spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-"+ spl.Name +"(Not implemented yet)");
									}
									output.Add(spellNote);
									break;
								}
							}
						}
					}
				}

				if(item.ProcEffectType == eMagicalEffectType.PoisonEffect)
				{
					SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
					if (poisonLine != null)
					{
						IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
						if (spells != null)
						{
							foreach(Spell spl in spells)
							{
								if(spl.ID == item.ChargeSpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									output.Add("Charged Magic Ability:");
									output.Add("- " + item.Charge+ " Charges");
									output.Add("- " + item.MaxCharge+ " Max");
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
									if(spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-"+ spl.Name +"(Not implemented yet)");
									}
									output.Add("- Spell has a chance of casting when this weapon strikes an enemy.");

									return;
								}
							}
						}
					}
				}

				if(item.ProcEffectType == eMagicalEffectType.ChargeEffect)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						IList spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						if (spells != null)
						{
							foreach(Spell spl in spells)
							{
								if(spl.ID == item.ChargeSpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									output.Add("Charged Magic Ability:");
									output.Add("- " + item.Charge+ " Charges");
									output.Add("- " + item.MaxCharge+ " Max");
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if(spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-"+ spl.Name +"(Not implemented yet)");
									}
									output.Add("- Spell has a chance of casting when this item is used.");

									return;
								}
							}
						}
					}
				}
			}
		}

		protected void WriteBonusLine(ArrayList list, eProperty bonusCat, int bonusValue)
		{
			if(bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType(bonusCat, ePropertyType.Focus))
			{
				//- Axe: 5 pts
				//- Strength: 15 pts
				//- Constitution: 15 pts
				//- Hits: 40 pts
				//- Fatigue: 8 pts
				//- Heat: 7%
				//Bonus to casting speed: 2%
				//Bonus to armor factor (AF): 18
				//Power: 6 % of power pool.
				list.Add(string.Format(
					"- {0}: {1}{2}",
					SkillBase.GetPropertyName(bonusCat),
					bonusValue.ToString("+0;-0;0"),
					((bonusCat == eProperty.PowerPool) || (bonusCat >= eProperty.Resist_First && bonusCat <= eProperty.Resist_Last))
					? ( (bonusCat == eProperty.PowerPool) ? "% of power pool." : "%" )
						: " pts"
				));
			}
		}

		protected void WriteFocusLine(ArrayList list, eProperty focusCat, int focusLevel)
		{
			if(SkillBase.CheckPropertyType(focusCat, ePropertyType.Focus))
			{
				//- Body Magic: 4 lvls
				list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName(focusCat), focusLevel));
			}
		}

		protected void WritePoisonInfo(ArrayList list, Poison item, GameClient client)
		{
			if (item.SpellID != 0)
			{
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
					if (spells != null)
					{
						foreach(Spell spl in spells)
						{
							if(spl.ID == item.SpellID)
							{
								list.Add(" ");
								list.Add("Level Requirement:");
								list.Add("- " + spl.Level + " Level");
								list.Add(" ");
								list.Add("Offensive Proc Ability:");
								//removed due to poisons don't have charges
                                //list.Add("- " + item.Charge+ " Charges");
								//list.Add("- " + item.MaxCharge+ " Max");
								list.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
								if(spellHandler != null)
								{
									list.AddRange(spellHandler.DelveInfo);
								}
								else
								{
									list.Add("-"+ spl.Name +" (Not implemented yet)");
								}
								break;
							}
						}
					}
				}
			}
		}

		protected void WriteSpellInfo(ArrayList list, string spellLine, int spellID, GameClient client)
		{
			if (spellID != 0)
			{
				SpellLine spellLin = SkillBase.GetSpellLine(spellLine);
				if (spellLine != null)
				{
					IList spells = SkillBase.GetSpellList(spellLin.KeyName);
					if (spells != null)
					{
						foreach(Spell spl in spells)
						{
							if(spl.ID == spellID)
							{
								list.Add(" ");
								list.Add("Level Requirement:");
								list.Add("- " + spl.Level + " Level");
								list.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, spellLin);
								if(spellHandler != null)
								{
									list.AddRange(spellHandler.DelveInfo);
								}
								else
								{
									list.Add("-"+ spl.Name +" (Not implemented yet)");
								}
								break;
							}
						}
					}
				}
			}
		}
	}
}
