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
using System.Collections.Specialized;
using System.Reflection;

using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.SkillHandler;

using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// delve button shift+i = detail of spell object...
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x70 ^ 168, "Handles detail display")]
	public class DetailDisplayHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint unk_186 = 0;
			ushort objectType = packet.ReadShort();
			if (client.Version >= GameClient.eClientVersion.Version186)
				unk_186 = packet.ReadInt();
			ushort objectID = packet.ReadShort();
			string caption = "";
			ArrayList objectInfo = new ArrayList();
			//log.Debug("DetailDisplayHandler: type=" + objectType + " id=" + objectID);

			switch (objectType)
			{
				case 1: //Display Infos on inventory item
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null)
							return 1;


						caption = item.Name;

						//**********************************
						//show crafter name
						//**********************************
						if (item.CrafterName != null && item.CrafterName != "")
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add("Crafter : " + item.CrafterName);
						}

						//**********************************
						//show info for all types of weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.Scythe) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{

							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicWeaponInfos(objectInfo, item, client);

							// Text for object witch can't be sell or trade
							//				if(objectType == 1)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//						...
							//						objectInfo.Add("This object can't be traded.");
							//						...
							//				}
						}

						//*********************************
						//shows info for all types of armor
						//*********************************
						if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicArmorInfos(objectInfo, item, client);

							// Text for object witch can't be sell or trade
							//				if(objectType == 1)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//						...
							//						objectInfo.Add("This object can't be traded.");
							//						...
							//				}

						}

						//***********************************
						//shows info for Shields			*
						//***********************************
						if (item.Object_Type == (int)eObjectType.Shield)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicShieldInfos(objectInfo, item, client);

							// Text for object witch can't be sell or trade
							//				if(objectType == 1)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//						...
							//						objectInfo.Add("This object can't be traded.");
							//						...
							//				}
						}

						//***********************************
						//shows info for Magic Items
						//***********************************
						if (item.Object_Type == (int)eObjectType.Magical || item.Object_Type == (int)eObjectType.AlchemyTincture || item.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, item, client, false);
						}

						//***********************************
						//shows info for Poison Potions
						//***********************************
						if (item.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, item, client);
						}

						if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == (int)eInventorySlot.FirstBackpack) // potion
							WritePotionInfo(objectInfo, item, client);

						if (!item.IsDropable || !item.IsPickable)
							objectInfo.Add(" ");//empty line
						if (!item.IsDropable)
						{
							objectInfo.Add("Cannot be dropped or put in your bank");
							objectInfo.Add("Cannot be sold to merchants.");
						}
						if (!item.IsPickable)
							objectInfo.Add("Cannot be traded to other players.");
						//				objectInfo.Add("Cannot be destroyed.");

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, item);
						}

						break;
					}

				case 2: //spell
					{
						int lineID = objectID / 100;
						int spellID = objectID % 100;

						SpellLine spellLine = client.Player.GetSpellLines()[lineID] as SpellLine;
						if (spellLine == null)
							return 1;

						Spell spell = null;
						foreach (Spell spl in SkillBase.GetSpellList(spellLine.KeyName))
							if (spl.Level == spellID)
							{
								spell = spl;
								break;
							}

						if (spell == null)
							return 1;

						caption = spell.Name;
						ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellLine);
						if (spellHandler == null)
						{
							objectInfo.Add(" ");
							objectInfo.Add("Spell type (" + spell.SpellType + ") is not implemented.");
						}
						else
						{
							objectInfo.AddRange(spellHandler.DelveInfo);
							//Subspells
							if (spell.SubSpellID > 0)
							{
								Spell s = SkillBase.GetSpellByID(spell.SubSpellID);
								if (spell != null)
								{
									objectInfo.Add(" ");
									ISpellHandler sh = Scripts.ScriptMgr.CreateSpellHandler(client.Player, s, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
									objectInfo.AddRange(sh.DelveInfo);
								}
							}
							if (client.Account.PrivLevel > 1)
							{
								objectInfo.Add("----------Technical informations----------");
								objectInfo.Add("Line             :" + spellHandler.SpellLine.Name);
								objectInfo.Add("HasPositiveEffect:" + spellHandler.HasPositiveEffect);
							}
						}
						break;
					}

				case 3: //spell
					{
						IList skillList = client.Player.GetNonTrainableSkillList();
						IList styles = client.Player.GetStyleList();
						int index = objectID - skillList.Count - styles.Count - 100;

						IList spelllines = client.Player.GetSpellLines();
						if (spelllines == null || index < 0)
							break;

						lock (spelllines.SyncRoot)
						{
							foreach (SpellLine spellline in spelllines)
							{
								IList spells = client.Player.GetUsableSpellsOfLine(spellline);
								if (index >= spells.Count)
								{
									index -= spells.Count;
								}
								else
								{
									Spell spell = (Spell)spells[index];
									caption = spell.Name;
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellline);
									if (spellHandler == null)
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
						GameMerchant merchant = null;
						if (client.Player.TargetObject != null && client.Player.TargetObject is GameMerchant)
							merchant = (GameMerchant)client.Player.TargetObject;
						if (merchant == null)
							return 1;

						int pagenumber = objectID / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
						int slotnumber = objectID % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

						ItemTemplate item = merchant.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
						if (item == null)
							return 1;

						caption = item.Name;

						//**********************************
						//show crafter name
						//**********************************
						if (item.Item_Type == (int)eInventorySlot.Horse)
						{
							WriteHorseInfo(objectInfo, item, client, "");
						}

						if ((item.Item_Type == (int)eInventorySlot.HorseBarding || item.Item_Type == (int)eInventorySlot.HorseArmor) && item.Level > 0)
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" - Requires: Champion Level " + item.Level);
						}

						//**********************************
						//show info for all weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.Scythe) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{

							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicWeaponInfos(objectInfo, item, client);
						}

						//*********************************
						//shows info for all types of armor
						//*********************************
						if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicArmorInfos(objectInfo, item, client);

						}

						//***********************************
						//shows info for Shields			*
						//***********************************
						if (item.Object_Type == (int)eObjectType.Shield)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicShieldInfos(objectInfo, item, client);

						}

						//***********************************
						//shows info for Magic Items
						//***********************************
						if ((item.Item_Type != (int)eInventorySlot.Horse && item.Object_Type == (int)eObjectType.Magical) || item.Object_Type == (int)eObjectType.AlchemyTincture || item.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, item, client, false);
						}

						//***********************************
						//shows info for Poison Items
						//***********************************
						if (item.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, item, client);
						}

						if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
							WritePotionInfo(objectInfo, item, client);

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, item);
						}
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

						if (foundEffect == null) break;

						caption = foundEffect.Name;
						objectInfo.AddRange(foundEffect.DelveInfo);

						break;
					}
				case 6: //style
					{
						IList styleList = client.Player.GetStyleList();
						IList skillList = client.Player.GetNonTrainableSkillList();
						Style style = null;
						string temp;
						int styleID = objectID - skillList.Count - 100;
						//DOLConsole.WriteLine("style id="+styleID+"; skills count="+skillList.Count);

						if (styleID < 0 || styleID >= styleList.Count) break;

						style = styleList[styleID] as Style;
						if (style == null) break;


						caption = style.Name;
						objectInfo.Add("Weapon Type: " + style.GetRequiredWeaponName());
						temp = "Opening: ";
						if (Style.eOpening.Offensive == style.OpeningRequirementType)
						{
							//attacker action result is opening
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResult.Hit: temp += "You Hit"; break;
								case Style.eAttackResult.Miss: temp += "You Miss"; break;
								case Style.eAttackResult.Parry: temp += "Target Parrys"; break;
								case Style.eAttackResult.Block: temp += "Target Blocks"; break;
								case Style.eAttackResult.Evade: temp += "Target Evades"; break;
								case Style.eAttackResult.Fumble: temp += "You Fumble"; break;
								case Style.eAttackResult.Style:
									Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, client.Player.CharacterClass.ID);
									temp = "Opening Style: ";
									if (reqStyle == null) temp += "(style not found " + style.OpeningRequirementValue + ")";
									else temp += reqStyle.Name;
									break;
								default: temp += "Any"; break;
							}
						}
						else if (Style.eOpening.Defensive == style.OpeningRequirementType)
						{
							//defender action result is opening
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResult.Miss: temp += "Target Misses"; break;
								case Style.eAttackResult.Hit: temp += "Target Hits"; break;
								case Style.eAttackResult.Parry: temp += "You Parry"; break;
								case Style.eAttackResult.Block: temp += "You Block"; break;
								case Style.eAttackResult.Evade: temp += "You Evade"; break;
								case Style.eAttackResult.Fumble: temp += "Target Fumbles"; break;
								case Style.eAttackResult.Style: temp += "Target Style"; break;
								default: temp += "Any"; break;
							}
						}
						else if (Style.eOpening.Positional == style.OpeningRequirementType)
						{
							//attacker position to target is opening
							temp += "Positional ";
							switch (style.OpeningRequirementValue)
							{
								case (int)Style.eOpeningPosition.Front: temp += "(Front)"; break;
								case (int)Style.eOpeningPosition.Back: temp += "(Back)"; break;
								case (int)Style.eOpeningPosition.Side: temp += "(Side)"; break;
							}
						}
						objectInfo.Add(temp);

						temp = "";
						foreach (Style st in SkillBase.GetStyleList(style.Spec, client.Player.CharacterClass.ID))
						{
							if (st.AttackResultRequirement == Style.eAttackResult.Style && st.OpeningRequirementValue == style.ID)
								temp = (temp == "" ? st.Name : temp + " or " + st.Name);
						}
						if (temp != "")
							objectInfo.Add("Followup Style: " + temp);

						temp = "Fatigue Cost: ";
						if (style.EnduranceCost < 5) temp += "Very Low";
						else if (style.EnduranceCost < 10) temp += "Low";
						else if (style.EnduranceCost < 15) temp += "Medium";
						else if (style.EnduranceCost < 20) temp += "High";
						else temp += "Very High";
						objectInfo.Add(temp);

						temp = "Damage: ";
						if (style.GrowthRate == 0) temp += "No Bonus";
						else temp += style.GrowthRate;
						objectInfo.Add(temp);

						temp = "To Hit: ";
						if (style.BonusToHit <= -20) temp += "Very High Penalty";
						else if (style.BonusToHit <= -15) temp += "High Penalty";
						else if (style.BonusToHit <= -10) temp += "Medium Penalty";
						else if (style.BonusToHit <= -5) temp += "Low Penalty";
						else if (style.BonusToHit < 0) temp += "Very Low Penalty";
						else if (style.BonusToHit == 0) temp += "No Bonus";
						else if (style.BonusToHit < 5) temp += "Very Low Bonus";
						else if (style.BonusToHit < 10) temp += "Low Bonus";
						else if (style.BonusToHit < 15) temp += "Medium Bonus";
						else if (style.BonusToHit < 20) temp += "High Bonus";
						else temp += "Very High Bonus";
						objectInfo.Add(temp);

						temp = "Defense: ";
						if (style.BonusToDefense <= -20) temp += "Very High Penalty";
						else if (style.BonusToDefense <= -15) temp += "High Penalty";
						else if (style.BonusToDefense <= -10) temp += "Medium Penalty";
						else if (style.BonusToDefense <= -5) temp += "Low Penalty";
						else if (style.BonusToDefense < 0) temp += "Very Low Penalty";
						else if (style.BonusToDefense == 0) temp += "No Bonus";
						else if (style.BonusToDefense < 5) temp += "Very Low Bonus";
						else if (style.BonusToDefense < 10) temp += "Low Bonus";
						else if (style.BonusToDefense < 15) temp += "Medium Bonus";
						else if (style.BonusToDefense < 20) temp += "High Bonus";
						else temp += "Very High Bonus";
						objectInfo.Add(temp);

						if (style.Procs.Count > 0)
						{
							temp = "Target Effect: ";
							objectInfo.Add(temp);

							SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
							if (styleLine != null)
							{
								foreach (DBStyleXSpell proc in style.Procs)
								{
									Spell spell = SkillBase.GetSpellByID(proc.SpellID);
									if (spell != null)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, styleLine);
										if (spellHandler == null)
										{
											temp = spell.Name + " (Not implemented yet)";
											objectInfo.Add(temp);
										}
										else
										{
											temp = spell.Name;
											objectInfo.Add(temp);
											objectInfo.Add(" ");//empty line
											objectInfo.AddRange(spellHandler.DelveInfo);
										}
									}
								}
							}
						}
						break;
					}

				case 7: //trade windows
					{
						ITradeWindow playerTradeWindow = client.Player.TradeWindow;
						if (playerTradeWindow == null)
							return 1;

						InventoryItem item = null;

						if (playerTradeWindow.PartnerTradeItems != null && objectID < playerTradeWindow.PartnerItemsCount)
							item = (InventoryItem)playerTradeWindow.PartnerTradeItems[objectID];

						if (item == null)
							return 1;

						caption = item.Name;

						//**********************************
						//show crafter name
						//**********************************
						if (item.Item_Type == (int)eInventorySlot.Horse)
						{
							WriteHorseInfo(objectInfo, item, client, "");
						}

						if ((item.Item_Type == (int)eInventorySlot.HorseBarding || item.Item_Type == (int)eInventorySlot.HorseArmor) && item.Level > 0)
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" - Requires: Champion Level " + item.Level);
						}
						//**********************************
						//show info for all types of weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.Scythe) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{

							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicWeaponInfos(objectInfo, item, client);

							// Text for object witch can't be sold
							//				if(...)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//				}
						}

						//*********************************
						//shows info for all types of armor
						//*********************************
						if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicArmorInfos(objectInfo, item, client);

							// Text for object witch can't be sold
							//				if(objectType == 1)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//				}

						}

						//***********************************
						//shows info for Shields			*
						//***********************************
						if (item.Object_Type == (int)eObjectType.Shield)
						{
							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");

							WriteMagicalBonuses(objectInfo, item, client, false);

							WriteClassicShieldInfos(objectInfo, item, client);

							// Text for object witch can't be sold
							//				if(objectType == 1)
							//				{
							//						...
							//						objectInfo.Add("This object can't be sold.");
							//				}
						}

						//***********************************
						//shows info for Magic Items
						//***********************************
						if ((item.Item_Type != (int)eInventorySlot.Horse && item.Object_Type == (int)eObjectType.Magical) || item.Object_Type == (int)eObjectType.AlchemyTincture || item.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, item, client, false);
						}
						//***********************************
						//shows info for Poison Potions
						//***********************************
						if (item.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, item, client);
						}

						if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
							WritePotionInfo(objectInfo, item, client);

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, item);
						}

						break;
					}
				case 8://abilities
					{
						objectInfo.Add("Delving on abilities is not yet supported!");
						break;
						/*
						int index = objectID - 100;
						IList skillList = client.Player.GetNonTrainableSkillList();
						Ability ab = (Ability)skillList[index];

						if (ab != null)
						{
							caption = ab.Name;

							IAbilityActionHandler handler = SkillBase.GetAbilityActionHandler(ab.KeyName);
							if (handler is BaseCastingAbilityHandler)
							{
								int spellID = ((BaseCastingAbilityHandler)handler).GetSpellID(client.Player.GetAbilityLevel(ab.KeyName));
								SpellLine spline = SkillBase.GetSpellLine(((BaseCastingAbilityHandler)handler).GetAbilitiesSpellLine());
								Spell abSpell = null;
								if (spline != null)
								{
									IList spells = SkillBase.GetSpellList(spline.KeyName);
									if (spells != null)
									{
										foreach (Spell spell in spells)
										{
											if (spell.ID == spellID)
											{
												abSpell = spell;
												break;
											}
										}
									}
								}

								if (abSpell == null)
									return 1;

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, abSpell, spline);
								if (spellHandler == null)
								{
									objectInfo.Add(" ");
									objectInfo.Add("Spell type (" + abSpell.SpellType + ") is not implemented.");
								}
								else
								{
									objectInfo.AddRange(spellHandler.DelveInfo);
								}
								break;

							}
							else
							{
								objectInfo.Add(ab.Description);
							}
						}
						break;*/
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
							//realm abilities
							if (objectID >= 50)
							{
								IList ra_list = SkillBase.GetClassRealmAbilities(client.Player.CharacterClass.ID);
								RealmAbility ab = (RealmAbility)ra_list[objectID - 50];
								if (ab != null)
								{
									caption = ab.Name;
									objectInfo.AddRange(ab.DelveInfo);
									break;
								}
							}
							caption = "Specialization not found";
							objectInfo.Add("that specialization is not found, id=" + objectID);
							break;
						}


						IList styles = SkillBase.GetStyleList(spec.KeyName, client.Player.CharacterClass.ID);
						IList playerSpells = client.Player.GetSpellLines();
						SpellLine selectedSpellLine = null;

						lock (playerSpells.SyncRoot)
						{
							foreach (SpellLine line in playerSpells)
							{
								if (!line.IsBaseLine && line.Spec == spec.KeyName)
								{
									selectedSpellLine = line;
									break;
								}
							}
						}

						IList spells = new ArrayList();
						if (selectedSpellLine != null)
							spells = SkillBase.GetSpellList(selectedSpellLine.KeyName);

						caption = spec.Name;

						if (styles.Count <= 0 && playerSpells.Count <= 0)
						{
							objectInfo.Add("no info found for this spec");
							break;
						}

						objectInfo.Add("Lev   Name");
						foreach (Style style in styles)
						{
							//						objectInfo.Add(" ");
							objectInfo.Add(style.Level + ": " + style.Name);
						}
						foreach (Spell spell in spells)
						{
							//						objectInfo.Add(" ");
							objectInfo.Add(spell.Level + ": " + spell.Name);
						}
						break;
					}
				case 12: // Item info to Group Chat
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;
						string str = client.Player.Name + "/Item: \"" + GetShortItemInfo(item, client) + '"';
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
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;
						string str = "[Guild] " + client.Player.Name + "/Item: \"" + GetShortItemInfo(item, client) + '"';
						if (client.Player.Guild == null)
						{
							client.Out.SendMessage("You don't belong to a player guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.GcSpeak))
						{
							client.Out.SendMessage("You don't have permission to speak on the on guild line.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
						{
							if (!client.Player.Guild.GotAccess(ply, eGuildRank.GcHear)) continue;
							ply.Out.SendMessage(str, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
						}
						return 1;
					}
				case 15: // Item info to Chat group
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;

						ChatGroup mychatgroup = (ChatGroup)client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Player.Out.SendMessage("You must be in a chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (mychatgroup.Listen == true && (((bool)mychatgroup.Members[client.Player]) == false))
						{
							client.Player.Out.SendMessage("Only moderator can talk on this chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						string str = "[Chat] " + client.Player.Name + "/Item: \"" + GetShortItemInfo(item, client) + '"';
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(str, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}
						return 1;
					}
				case 100://repair
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.RepairItem(item);
						}
						else
						{
							client.Out.SendMessage("It's very strange, i cant recognise this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				case 101://selfcraft
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.OpenSelfCraft(item);
						}
						else
						{
							client.Out.SendMessage("It's very strange, i cant recognise this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				case 102://salvage
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.SalvageItem(item);
						}
						else
						{
							client.Out.SendMessage("It's very strange, i cant recognise this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				default:
					client.Out.SendMessage("No special information!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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

		public void WriteTechnicalInfo(ArrayList output, ItemTemplate item)
		{
			output.Add("----------Technical informations----------");
			output.Add("Item Template: " + item.Id_nb);
			output.Add("         Name: " + item.Name);
			output.Add("        Level: " + item.Level);
			output.Add("        Model: " + item.Model);
			output.Add("    Extension: " + item.Extension);
			output.Add("         Type: " + GlobalConstants.ObjectTypeToName(item.Object_Type) + " (" + item.Object_Type + ")");
			output.Add("         Slot: " + GlobalConstants.SlotToName(item.Item_Type) + " (" + item.Item_Type + ")");
			output.Add("        Color: " + item.Color);
			output.Add("       Emblem: " + item.Emblem);
			output.Add("       Effect: " + item.Effect);
			output.Add("  Value/Price: " + item.Gold + "g " + item.Silver + "s " + item.Copper + "c");
			output.Add("       Weight: " + (item.Weight / 10.0f) + "lbs");
			output.Add("      Quality: " + item.Quality + "/" + item.MaxQuality + "(max)");
			output.Add("   Durability: " + item.Durability + "/" + item.MaxDurability + "(max)");
			output.Add("    Condition: " + item.Condition + "/" + item.MaxCondition + "(max)");
			output.Add("        Realm: " + item.Realm);
			output.Add("  Is dropable: " + (item.IsDropable ? "yes" : "no"));
			output.Add("  Is pickable: " + (item.IsPickable ? "yes" : "no"));
			output.Add(" Is stackable: " + (item.IsStackable ? "yes" : "no"));
			output.Add("  ProcSpellID: " + item.ProcSpellID);
			output.Add(" ProcSpellID1: " + item.ProcSpellID1);
			output.Add("      SpellID: " + item.SpellID + " (" + item.Charges + "/" + item.MaxCharges + ")");
			output.Add("     SpellID1: " + item.SpellID1 + " (" + item.Charges1 + "/" + item.MaxCharges1 + ")");
			output.Add("PoisonSpellID: " + item.PoisonSpellID + " (" + item.PoisonCharges + "/" + item.PoisonMaxCharges + ") ");
			if (GlobalConstants.IsWeapon(item.Object_Type))
			{
				output.Add("         Hand: " + GlobalConstants.ItemHandToName(item.Hand));
				output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
				output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
				output.Add("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(item.Type_Damage));
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (GlobalConstants.IsArmor(item.Object_Type))
			{
				output.Add("  Armorfactor: " + item.DPS_AF);
				output.Add("    Absorbage: " + item.SPD_ABS);
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Shield)
			{
				output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
				output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
				output.Add("  Shield type: " + GlobalConstants.ShieldTypeToName(item.Type_Damage));
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Arrow || item.Object_Type == (int)eObjectType.Bolt)
			{
				output.Add(" Ammunition #: " + item.DPS_AF);
				output.Add("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(item.SPD_ABS));
				output.Add("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(item.SPD_ABS));
				output.Add("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(item.SPD_ABS));
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Instrument)
			{
				output.Add("   Instrument: " + GlobalConstants.InstrumentTypeToName(item.DPS_AF));
			}
		}

		protected string GetShortItemInfo(InventoryItem item, GameClient client)
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

			string str = "- [" + item.Name + "]: " + GlobalConstants.ObjectTypeToName(item.Object_Type);
			ArrayList objectInfo = new ArrayList();

			if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.Scythe))
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicWeaponInfos(objectInfo, item, client);
			}
			if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicArmorInfos(objectInfo, item, client);
			}
			if (item.Object_Type == (int)eObjectType.Shield)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicShieldInfos(objectInfo, item, client);
			}
			if (item.Object_Type == (int)eObjectType.Magical ||
				item.Object_Type == (int)eObjectType.Instrument)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
			}
			if (item.CrafterName != null && item.CrafterName != "")
			{
				objectInfo.Add(" ");//empty line
				objectInfo.Add("Crafter : " + item.CrafterName);
			}
			if (item.Object_Type == (int)eObjectType.Poison)
			{
				WritePoisonInfo(objectInfo, item, client);
			}

			if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
				WritePotionInfo(objectInfo, item, client);

			if (!item.IsDropable)
			{
				objectInfo.Add(" No Drop.");
				objectInfo.Add(" No Sell.");
			}
			if (!item.IsPickable)
				objectInfo.Add(" No Trade.");

			foreach (string s in objectInfo)
				str += " " + s;
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
		public void WriteClassicWeaponInfos(ArrayList output, ItemTemplate item, GameClient client)
		{
			double itemDPS = item.DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.SPD_ABS / 10.0;
			double effectiveDPS = itemDPS * item.Quality / 100.0 * item.Condition / item.MaxCondition;

			output.Add(" ");
			output.Add(" ");
			output.Add("Damage Modifiers:");
			if (itemDPS != 0)
			{
				output.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				output.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
			}

			if (item.SPD_ABS >= 0)
			{
				output.Add("- " + itemSPD.ToString("0.0") + " Weapon Speed");
			}

			if (item.Quality != 0)
			{
				output.Add("- " + item.Quality + "% Quality");
			}
			if (item.Condition != 0)
			{
				output.Add("- " + item.ConditionPercent + "% Condition");
			}

			output.Add("Damage Type: " + (item.Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(item.Type_Damage)));
			output.Add(" ");

			output.Add("Effective Damage:");
			if (itemDPS != 0)
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
		public void WriteClassicShieldInfos(ArrayList output, ItemTemplate item, GameClient client)
		{
			double itemDPS = item.DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.SPD_ABS / 10.0;

			output.Add(" ");
			output.Add(" ");
			output.Add("Damage Modifiers (when used with shield styles):");
			if (itemDPS != 0)
			{
				output.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				output.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add("- " + itemSPD.ToString("0.0") + " Shield Speed");
			}

			output.Add(" ");

			switch (item.Type_Damage)
			{
				case 1: output.Add("- Shield Size : Small"); break;
				case 2: output.Add("- Shield Size : Medium"); break;
				case 3: output.Add("- Shield Size : Large"); break;
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
		public void WriteClassicArmorInfos(ArrayList output, ItemTemplate item, GameClient client)
		{
			output.Add(" ");
			output.Add(" ");
			output.Add("Armor Modifiers:");
			if (item.DPS_AF != 0)
			{
				output.Add("- " + item.DPS_AF + " Base Factor");
			}
			double AF = 0;
			if (item.DPS_AF != 0)
			{
				int afCap = client.Player.Level;
				if (item.Object_Type != (int)eObjectType.Cloth)
				{
					afCap *= 2;
				}

				AF = Math.Min(afCap, item.DPS_AF);

				output.Add("- " + (int)AF + " Clamped Factor");
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add("- " + item.SPD_ABS + "% Absorption");
			}
			if (item.Quality != 0)
			{
				output.Add("- " + item.Quality + "% Quality");
			}
			if (item.Condition != 0)
			{
				output.Add("- " + item.ConditionPercent + "% Condition");
			}
			output.Add(" ");

			output.Add("Effective Armor:");
			double EAF = 0;
			if (item.DPS_AF != 0)
			{
				EAF = AF * item.Quality / 100.0 * item.Condition / item.MaxCondition * (1 + item.SPD_ABS / 100.0);
				output.Add("- " + (int)EAF + " Factor");
			}

		}

		public void WriteMagicalBonuses(ArrayList output, ItemTemplate item, GameClient client, bool shortInfo)
		{
			int oldCount = output.Count;

			WriteBonusLine(output, item.Bonus1Type, item.Bonus1);
			WriteBonusLine(output, item.Bonus2Type, item.Bonus2);
			WriteBonusLine(output, item.Bonus3Type, item.Bonus3);
			WriteBonusLine(output, item.Bonus4Type, item.Bonus4);
			WriteBonusLine(output, item.Bonus5Type, item.Bonus5);
			WriteBonusLine(output, item.Bonus6Type, item.Bonus6);
			WriteBonusLine(output, item.Bonus7Type, item.Bonus7);
			WriteBonusLine(output, item.Bonus8Type, item.Bonus8);
			WriteBonusLine(output, item.Bonus9Type, item.Bonus9);
			WriteBonusLine(output, item.Bonus10Type, item.Bonus10);
			WriteBonusLine(output, item.ExtraBonusType, item.ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, "Magical Bonuses:");
				output.Insert(oldCount, " ");
			}

			oldCount = output.Count;

			WriteFocusLine(output, item.Bonus1Type, item.Bonus1);
			WriteFocusLine(output, item.Bonus2Type, item.Bonus2);
			WriteFocusLine(output, item.Bonus3Type, item.Bonus3);
			WriteFocusLine(output, item.Bonus4Type, item.Bonus4);
			WriteFocusLine(output, item.Bonus5Type, item.Bonus5);
			WriteFocusLine(output, item.Bonus6Type, item.Bonus6);
			WriteFocusLine(output, item.Bonus7Type, item.Bonus7);
			WriteFocusLine(output, item.Bonus8Type, item.Bonus8);
			WriteFocusLine(output, item.Bonus9Type, item.Bonus9);
			WriteFocusLine(output, item.Bonus10Type, item.Bonus10);
			WriteFocusLine(output, item.ExtraBonusType, item.ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, "Focus Bonuses:");
				output.Insert(oldCount, " ");
			}

			if (!shortInfo)
			{
				#region Proc1
				if (item.ProcSpellID != 0)
				{
					string spellNote = "";
					output.Add(" ");
					output.Add("Magical Ability:");
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = "- Spell has a chance of casting when this weapon strikes an enemy.";
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = "- Spell has a chance of casting when enemy strikes at this armor.";
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						IList spells = SkillBase.GetSpellList(line.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.ProcSpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, line);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add(spellNote);
									break;
								}
							}
						}
					}
				}
				#endregion
				#region Proc2
				if (item.ProcSpellID1 != 0)
				{
					string spellNote = "";
					output.Add(" ");
					output.Add("Magical Ability:");
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = "- Spell has a chance of casting when this weapon strikes an enemy.";
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = "- Spell has a chance of casting when enemy strikes at this armor.";
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						IList spells = SkillBase.GetSpellList(line.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.ProcSpellID1)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, line);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add(spellNote);
									break;
								}
							}
						}
					}
				}
				#endregion
				#region Charge1
				if (item.SpellID != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						IList spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.SpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									output.Add("Charged Magic Ability:");
									output.Add("- " + item.Charges + " Charges");
									output.Add("- " + item.MaxCharges + " Max");
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add("- Spell has a chance of casting when this item is used.");
									break;
								}
							}
						}
					}
				}
				#endregion
				#region Charge2
				if (item.SpellID1 != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						IList spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.SpellID1)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									output.Add("Charged Magic Ability:");
									output.Add("- " + item.Charges1 + " Charges");
									output.Add("- " + item.MaxCharges1 + " Max");
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add("- Spell has a chance of casting when this item is used.");
									break;
								}
							}
						}
					}
				}
				#endregion
				#region Poison
				if (item.PoisonSpellID != 0)
				{
					if (GlobalConstants.IsWeapon(item.Object_Type))// Poisoned Weapon
					{
						SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
						if (poisonLine != null)
						{
							IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
							if (spells != null)
							{
								foreach (Spell spl in spells)
								{
									if (spl.ID == item.PoisonSpellID)
									{
										output.Add(" ");
										output.Add("Level Requirement:");
										output.Add("- " + spl.Level + " Level");
										output.Add(" ");
										output.Add("Charged Magic Ability:");
										output.Add("- " + item.PoisonCharges + " Charges");
										output.Add("- " + item.PoisonMaxCharges + " Max");
										output.Add(" ");

										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
										if (spellHandler != null)
										{
											output.AddRange(spellHandler.DelveInfo);
											output.Add(" ");
										}
										else
										{
											output.Add("-" + spl.Name + "(Not implemented yet)");
										}
										output.Add("- Spell has a chance of casting when this weapon strikes an enemy.");
										return;
									}
								}
							}
						}
					}

					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						IList spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.SpellID)
								{
									output.Add(" ");
									output.Add("Level Requirement:");
									output.Add("- " + spl.Level + " Level");
									output.Add(" ");
									if (item.MaxCharges > 0)
									{
										output.Add("Charged Magic Ability:");
										output.Add("- " + item.Charges + " Charges");
										output.Add("- " + item.MaxCharges + " Max");
									}
									else
									{
										output.Add("Magical Ability:");
									}
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add("- Spell has a chance of casting when this item is used.");
									output.Add(" ");
									if (spl.RecastDelay > 0)
										output.Add("Can use item every: " + Util.FormatTime(spl.RecastDelay / 1000));
									else
										output.Add("Can use item every: 3:00 min");
									long lastChargedItemUseTick = client.Player.TempProperties.getLongProperty(GamePlayer.LAST_CHARGED_ITEM_USE_TICK, 0L);
									long changeTime = client.Player.CurrentRegion.Time - lastChargedItemUseTick;
									long recastDelay = (spl.RecastDelay > 0) ? spl.RecastDelay : 60000 * 3;
									if (changeTime < recastDelay) //3 minutes reuse timer
										output.Add("Can use again in: " + Util.FormatTime((recastDelay - changeTime) / 1000));
									return;
								}
							}
						}
					}
				}
				#endregion
			}
		}

		protected void WriteBonusLine(ArrayList list, int bonusCat, int bonusValue)
		{
			if (bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus))
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
					SkillBase.GetPropertyName((eProperty)bonusCat),
					bonusValue.ToString("+0;-0;0"),
					((bonusCat == (int)eProperty.PowerPool) || (bonusCat >= (int)eProperty.Resist_First && bonusCat <= (int)eProperty.Resist_Last))
					? ((bonusCat == (int)eProperty.PowerPool) ? "% of power pool." : "%")
						: " pts"
				));
			}
		}

		protected void WriteFocusLine(ArrayList list, int focusCat, int focusLevel)
		{
			if (SkillBase.CheckPropertyType((eProperty)focusCat, ePropertyType.Focus))
			{
				//- Body Magic: 4 lvls
				list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName((eProperty)focusCat), focusLevel));
			}
		}

		protected void WriteHorseInfo(ArrayList list, ItemTemplate item, GameClient client, string horseName)
		{
			list.Add(" ");
			list.Add(" ");
			if (item.Level <= 35)
			{
				list.Add("This is a basic horse.");
				list.Add("(+35% speed)");
				list.Add(" ");
				list.Add("To make this horse your active mount, place this icon in the appropriate slot of your Mount window.");
				list.Add(" ");
				list.Add("To summon this horse, from your Mount window drag the horse icon to your quickbar, and summon by using the quickbar icon.");
				list.Add(" ");
				list.Add("Can't be summoned in RvR zones.");
			}
			else
			{
				list.Add("This is an advanced horse.");
				list.Add("(+45% speed)");
				list.Add(" ");
				list.Add(" ");
				list.Add("Click this icon in your quickbar to summon (or dismount) your horse.");
				list.Add(" ");
				list.Add("- Name: " + ((horseName == null || horseName == "") ? "None" : horseName) + ".");
				list.Add("  (Use /namemount to name your horse.)");
			}
			list.Add(" ");
			list.Add("- Armor: " + (item.DPS_AF == 0 ? "None" : item.DPS_AF.ToString()));
			list.Add("- Barding: None");
			list.Add("- Favorite food: Hay and the occasional apple");
		}

		protected void WritePoisonInfo(ArrayList list, ItemTemplate item, GameClient client)
		{
			if (item.PoisonSpellID != 0)
			{
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
					if (spells != null)
					{
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.PoisonSpellID)
							{
								list.Add(" ");
								list.Add("Level Requirement:");
								list.Add("- " + spl.Level + " Level");
								list.Add(" ");
								list.Add("Offensive Proc Ability:");
								list.Add("- " + item.PoisonCharges + " Charges");
								list.Add("- " + item.PoisonMaxCharges + " Max");
								list.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
								if (spellHandler != null)
								{
									list.AddRange(spellHandler.DelveInfo);
								}
								else
								{
									list.Add("-" + spl.Name + " (Not implemented yet)");
								}
								break;
							}
						}
					}
				}
			}
		}

		protected void WritePotionInfo(ArrayList list, ItemTemplate item, GameClient client)
		{
			if (item.SpellID != 0)
			{
				SpellLine potionLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
				if (potionLine != null)
				{
					IList spells = SkillBase.GetSpellList(potionLine.KeyName);
					if (spells != null)
					{
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								list.Add(" ");
								list.Add("Level Requirement:");
								list.Add("- " + spl.Level + " Level");
								list.Add(" ");
								list.Add("Charged Magic Ability:");
								list.Add("- " + item.Charges + " Charges");
								list.Add("- " + item.MaxCharges + " Max");
								list.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, potionLine);
								if (spellHandler != null)
									list.AddRange(spellHandler.DelveInfo);
								list.Add(" ");
								list.Add("Can use item every: 1:00 min");
								long lastPotionItemUseTick = client.Player.TempProperties.getLongProperty(GamePlayer.LAST_POTION_ITEM_USE_TICK, 0L);
								long changeTime = client.Player.CurrentRegion.Time - lastPotionItemUseTick;
								if (changeTime < 60000) //1 minutes reuse timer
									list.Add("Can use again in: " + Util.FormatTime((60000 - changeTime) / 1000));
								if (spl.CastTime > 0)
								{
									list.Add(" ");
									list.Add("Cannot be used in combat");
								}
								else
								{
									list.Add("-" + spl.Name + " (Not implemented yet)");
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
