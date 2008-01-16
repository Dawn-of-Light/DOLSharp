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
using DOL.Language;
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.SkillHandler;

using log4net;
using System.Collections.Generic;

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
				#region Inventory Item
				case 1: //Display Infos on inventory item
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null)
							return 1;


						caption = item.Name;

						// Aredhel: Start of a more sophisticated item delve system.
						// The idea is to have every item inherit from an item base class,
						// this base class will provide a method
						// 
						// public virtual void Delve(List<String>)
						// 
						// which can be overridden in derived classes to provide additional
						// information. Same goes for spells, just add the spell delve
						// in the Delve() hierarchy. This will on one hand make this class
						// much more concise (1800 lines at the time of this writing) and
						// on the other hand the whole delve system much more flexible, for
						// example when adding new item types (artifacts, for example) you
						// provide *only* an overridden Delve() method, use the base
						// Delve() and you're done, spells, charges and everything else.

						if (item is InventoryArtifact)
						{
							List<String> delve = new List<String>();
							(item as InventoryArtifact).Delve(delve, client.Player);

							foreach (string line in delve)
								objectInfo.Add(line);

							break;
						}

						//**********************************
						//show crafter name
						//**********************************
						if (item.CrafterName != null && item.CrafterName != "")
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.CrafterName", item.CrafterName));
						}

						//**********************************
						//show info for all types of weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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

						// Items with a reuse timer (aka cooldown).

						if (item.CanUseEvery > 0)
						{
							int minutes = item.CanUseEvery / 60;
							int seconds = item.CanUseEvery % 60;

							objectInfo.Add(String.Format("Can use item every: {0:00}:{1:00}",
									minutes, seconds));

							int cooldown = item.CanUseAgainIn;

							if (cooldown > 0)
							{
								minutes = cooldown / 60;
								seconds = cooldown % 60;

								objectInfo.Add(String.Format("Can use again in: {0:00}:{1:00}",
									minutes, seconds));
							}
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
						
						if (!item.IsPickable)
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.CannotTraded"));
						if (!item.IsDropable)
						{
							//objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.CannotDropped"));
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.CannotSold"));
						}

						if (item.MaxDurability == 0)
							objectInfo.Add("Cannot be destroyed.");

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, item);
						}

						break;
					}
				#endregion
				#region Spell
				case 2: //spell
					{
						int lineID = objectID / 100;
						int spellID = objectID % 100;

						SpellLine spellLine = client.Player.GetSpellLines()[lineID] as SpellLine;
						if (spellLine == null)
							return 1;

						Spell spell = null;
						foreach (Spell spl in SkillBase.GetSpellList(spellLine.KeyName))
						{
							if (spl.Level == spellID)
							{
								spell = spl;
								break;
							}
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
									ISpellHandler sh = ScriptMgr.CreateSpellHandler(client.Player, s, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
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
				#endregion
				#region Spell
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
				#endregion
				#region Merchant
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
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ChampionLevel", item.Level));
						}

						//**********************************
						//show info for all weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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
				#endregion
				#region Effect
				case 5: //icons on top (buffs/dots)
					{
						IGameEffect foundEffect = null;
						lock (client.Player.EffectList)
						{
							foreach (IGameEffect effect in client.Player.EffectList)
							{
								if (effect.InternalID == objectID)
								{
									foundEffect = effect;
									break;
								}
							}
						}

						if (foundEffect == null) break;

						caption = foundEffect.Name;
						objectInfo.AddRange(foundEffect.DelveInfo);

						break;
					}
				#endregion
				#region Style
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
						objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.WeaponType", style.GetRequiredWeaponName()));
						temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Opening");
						if (Style.eOpening.Offensive == style.OpeningRequirementType)
						{
							//attacker action result is opening
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResult.Hit:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouHit");
									break;
								case Style.eAttackResult.Miss:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouMiss");
									break;
								case Style.eAttackResult.Parry:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetParrys");
									break;
								case Style.eAttackResult.Block:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetBlocks");
									break;
								case Style.eAttackResult.Evade:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetEvades");
									break;
								case Style.eAttackResult.Fumble:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouFumble");
									break;

								case Style.eAttackResult.Style:
									Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, client.Player.CharacterClass.ID);
									temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.OpeningStyle");
									if (reqStyle == null) temp += "(style not found " + style.OpeningRequirementValue + ")";
									else temp += reqStyle.Name;
									break;
								default:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Any");
									break;
							}
						}
						else if (Style.eOpening.Defensive == style.OpeningRequirementType)
						{
							//defender action result is opening
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResult.Miss:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetMisses");
									break;
								case Style.eAttackResult.Hit:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetHits");
									break;
								case Style.eAttackResult.Parry:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouParry");
									break;
								case Style.eAttackResult.Block:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouBlock");
									break;
								case Style.eAttackResult.Evade:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.YouEvade");
									break;
								case Style.eAttackResult.Fumble:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetFumbles");
									break;
								case Style.eAttackResult.Style:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetStyle");
									break;
								default:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Any");
									break;
							}
						}
						else if (Style.eOpening.Positional == style.OpeningRequirementType)
						{
							//attacker position to target is opening
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Positional");
							switch (style.OpeningRequirementValue)
							{
								case (int)Style.eOpeningPosition.Front:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Front");
									break;
								case (int)Style.eOpeningPosition.Back:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Back");
									break;
								case (int)Style.eOpeningPosition.Side:
									temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Side");
									break;
							
							}
						}
						objectInfo.Add(temp);

						temp = "";
						foreach (Style st in SkillBase.GetStyleList(style.Spec, client.Player.CharacterClass.ID))
						{
							if (st.AttackResultRequirement == Style.eAttackResult.Style && st.OpeningRequirementValue == style.ID)
								temp = (temp == "" ? st.Name : temp +
									LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Or", st.Name));
						}
						if (temp != "")
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.FollowupStyle", temp));
						temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.FatigueCost");
						if (style.EnduranceCost < 5)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryLow");
						else if	(style.EnduranceCost < 10)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Low");
						else if (style.EnduranceCost < 15)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Medium");
						else if (style.EnduranceCost < 20)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.High");
						else temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryHigh");												
						objectInfo.Add(temp);

						temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Damage");
						if (style.GrowthRate == 0)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoBonus");
						else temp += style.GrowthRate;

						objectInfo.Add(temp);

						temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ToHit");
						if (style.BonusToHit <= -20)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryHighPenalty");
						else if (style.BonusToHit <= -15)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.HighPenalty");
						else if (style.BonusToHit <= -10)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MediumPenalty");
						else if (style.BonusToHit <= -5)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.LowPenalty");
						else if (style.BonusToHit < 0)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryLowPenalty");
						else if (style.BonusToHit == 0)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoBonus");
						else if (style.BonusToHit < 5)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryLowBonus");
						else if (style.BonusToHit < 10)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.LowBonus");
						else if (style.BonusToHit < 15)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MediumBonus");
						else if (style.BonusToHit < 20)
							temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.HighBonus");
						else temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryHighBonus");

						objectInfo.Add(temp);

						temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Defense");
						if (style.BonusToDefense <= -20) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryHighPenalty");
						else if (style.BonusToDefense <= -15) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.HighPenalty");
						else if (style.BonusToDefense <= -10) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MediumPenalty");
						else if (style.BonusToDefense <= -5) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.LowPenalty");
						else if (style.BonusToDefense < 0) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryLowPenalty");
						else if (style.BonusToDefense == 0) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoBonus");
						else if (style.BonusToDefense < 5) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryLowBonus");
						else if (style.BonusToDefense < 10) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.LowBonus");
						else if (style.BonusToDefense < 15) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MediumBonus");
						else if (style.BonusToDefense < 20) temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.HighBonus");
						else temp += LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryHighBonus");
						
						objectInfo.Add(temp);

						if (style.Procs.Count > 0)
						{
							temp = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.TargetEffect");
							objectInfo.Add(temp);

							SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
							if (styleLine != null)
							{
								foreach (DBStyleXSpell proc in style.Procs)
								{
									// RR4: we added all the procs to the style, now it's time to check for class ID
									if (proc.ClassID != 0 && proc.ClassID != client.Player.CharacterClass.ID) continue;

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
				#endregion
				#region Trade Window
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
							objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ChampionLevel", item.Level));
						}
						//**********************************
						//show info for all types of weapons
						//**********************************
						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff) ||
							item.Object_Type == (int)eObjectType.Instrument)
						{

							//						objectInfo.Add("Usable by:");
							//						objectInfo.Add("- ");
							//						objectInfo.Add(" ");
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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
							WriteUsableClasses(objectInfo, item, client);
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
				#endregion
				#region Ability
				case 8://abilities
				{
					int id = objectID - 100;
					IList skillList = client.Player.GetNonTrainableSkillList();
					Ability abil = (Ability)skillList[id];
					if (abil != null)
					{
						IList allabilitys = client.Player.GetAllAbilities();
						foreach (Ability checkab in allabilitys)
						{
							if (checkab.Name == abil.Name)
							{
								if (checkab.DelveInfo.Count > 0)
									objectInfo.AddRange(checkab.DelveInfo);
								else
									objectInfo.Add("There is no special information.");
							}
						}
					}
					break;
				}
				#endregion
				#region Trainer
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
								int clientclassID = client.Player.CharacterClass.ID;
								IList ra_list = SkillBase.GetClassRealmAbilities(clientclassID);
								Ability rr5abil = SkillBase.getClassRealmAbility(clientclassID);
								RealmAbility ab = (RealmAbility)ra_list[objectID - 50];
								if (rr5abil != null)
								{
									for (int i = 0; i <= (objectID - 50); i++)
									{
										RealmAbility rrabil = (RealmAbility)ra_list[i];
										if (rrabil.KeyName == ab.KeyName)
											ab = (RealmAbility)ra_list[objectID - 49];
									}

								}
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

						objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.LevName"));
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
				#endregion
				#region Group
				case 12: // Item info to Group Chat
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;
						string str = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.Item", client.Player.Name, GetShortItemInfo(item, client));
						if (client.Player.Group == null)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Group.SendMessageToGroupMembers(str, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
						return 1;
					}
				#endregion
				#region Guild
				case 13: // Item info to Guild Chat
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;
						string str = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.GuildItem", client.Player.Name, GetShortItemInfo(item, client));
						if (client.Player.Guild == null)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.DontBelongGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.GcSpeak))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoPermissionToSpeak"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
						{
							if (!client.Player.Guild.GotAccess(ply, eGuildRank.GcHear)) continue;
							ply.Out.SendMessage(str, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
						}
						return 1;
					}
				#endregion
				#region ChatGroup
				case 15: // Item info to Chat group
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item == null) return 1;

						ChatGroup mychatgroup = (ChatGroup)client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MustBeInChatGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (mychatgroup.Listen == true && (((bool)mychatgroup.Members[client.Player]) == false))
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						string str = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(item, client));
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(str, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
						}
						return 1;
					}
				#endregion
				#region Repair
				case 100://repair
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.RepairItem(item);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				#endregion
				#region Self Craft
				case 101://selfcraft
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.OpenSelfCraft(item);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				#endregion
				#region Salvage
				case 102://salvage
					{
						InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (item != null)
						{
							client.Player.SalvageItem(item);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
					}
				#endregion
                #region BattleGroup
                case 103: // Item info to battle group
                    {
                        InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (item == null) return 1;

                        BattleGroup mybattlegroup = (BattleGroup)client.Player.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
                        if (mybattlegroup == null)
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.MustBeInBattleGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                        if (mybattlegroup.Listen == true && (((bool)mybattlegroup.Members[client.Player]) == false))
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                        string str = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(item, client));
                        foreach (GamePlayer ply in mybattlegroup.Members.Keys)
                        {
                            ply.Out.SendMessage(str, eChatType.CT_Friend, eChatLoc.CL_ChatWindow);
                        }
                        return 1;
                    }
                #endregion
                default:
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.NoInformation"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			output.Add("      Quality: " + item.Quality + "%");
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

			if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff))
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
				objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.GetShortItemInfo.CrafterName", item.CrafterName));

				//LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.VeryStrange")
			}
			if (item.Object_Type == (int)eObjectType.Poison)
			{
				WritePoisonInfo(objectInfo, item, client);
			}

			if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
				WritePotionInfo(objectInfo, item, client);

			if (!item.IsDropable)
			{
				objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.GetShortItemInfo.NoDrop"));
				objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.GetShortItemInfo.NoSell"));
			}
			if (!item.IsPickable)
				objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.GetShortItemInfo.NoTrade"));

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
			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageMod"));
			if (itemDPS != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}

			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));
			}

			if (item.Quality != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.Quality", item.Quality));
			}
			if (item.Condition != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.Condition", item.ConditionPercent));
			}

			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
				(item.Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(item.Type_Damage))));
			output.Add(" ");

			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));
			if (itemDPS != 0)
			{
				output.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
			}
		}
		public void WriteUsableClasses(ArrayList output, ItemTemplate item, GameClient client)
		{
			if (item.AllowedClasses == "" || item.AllowedClasses == null || item.AllowedClasses == "0")
				return;
			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteUsableClasses.UsableBy"));
			string[] allowedclasses = item.AllowedClasses.Split(';');
			foreach (string allowed in allowedclasses)
			{
				int classID = -1;
				if (int.TryParse(allowed, out classID))
					output.Add("- " + ((eCharacterClass)classID)).ToString();
				else log.Error(item.Id_nb + " has an invalid entry for allowed classes '" + allowed + "'");
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
			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.DamageMod"));
			if (itemDPS != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.BaseDPS", itemDPS.ToString("0.0")));
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.SPD", itemSPD.ToString("0.0")));
			}

			output.Add(" ");

			switch (item.Type_Damage)
			{
				case 1: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Small")); break;
				case 2: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Medium")); break;
				case 3: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Large")); break;
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
			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));
			if (item.DPS_AF != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", item.DPS_AF));
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

				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)AF));
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.Absorption", item.SPD_ABS));
			}
			if (item.Quality != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.Quality", item.Quality));
			}
			if (item.Condition != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.Condition", item.ConditionPercent));
			}
			output.Add(" ");

			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));
			double EAF = 0;
			if (item.DPS_AF != 0)
			{
				EAF = AF * item.Quality / 100.0 * item.Condition / item.MaxCondition * (1 + item.SPD_ABS / 100.0);
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)EAF));
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
				output.Insert(oldCount, LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicBonus"));
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
				output.Insert(oldCount, LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.FocusBonus"));
				output.Insert(oldCount, " ");
			}

			if (!shortInfo)
			{
				#region Proc1
				if (item.ProcSpellID != 0)
				{
					string spellNote = "";
					output.Add(" ");
					output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
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
					output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired2", spl.Level));
									//output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
									output.Add(" ");
									output.Add(" ");
									if (item.MaxCharges > 0)
									{
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
										output.Add(" ");
									}
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add("- This spell is cast when the item is used.");
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
									output.Add(" ");
									if (item.MaxCharges1 > 0)
									{
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges1));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges1));
										output.Add(" ");
									}
									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add("- This spell is cast when the item is used.");
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
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
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
										output.Add(" ");
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.PoisonCharges));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.PoisonMaxCharges));
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
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy"));
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
									output.Add(" ");
									if (item.MaxCharges > 0)
									{
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
									}
									else
									{
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
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
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UsedItem"));
									output.Add(" ");
									if (spl.RecastDelay > 0)
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UseItem1", Util.FormatTime(spl.RecastDelay / 1000)));
									else
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UseItem2"));
									long lastChargedItemUseTick = client.Player.TempProperties.getLongProperty(GamePlayer.LAST_CHARGED_ITEM_USE_TICK, 0L);
									long changeTime = client.Player.CurrentRegion.Time - lastChargedItemUseTick;
									long recastDelay = (spl.RecastDelay > 0) ? spl.RecastDelay : 60000 * 3;
									if (changeTime < recastDelay) //3 minutes reuse timer
										output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UseItem3", Util.FormatTime((recastDelay - changeTime) / 1000)));
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
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.BasicHorse"));
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Speed1"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.MountWindow"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Quickbar"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.RvRZzones"));
			}
			else
			{
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.AdvancedHorse"));
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Speed2"));
				list.Add(" ");
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Summon"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Name", ((horseName == null || horseName == "") ? "None" : horseName)));
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.NameMount"));
			}
			list.Add(" ");
			list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Armor", (item.DPS_AF == 0 ? "None" : item.DPS_AF.ToString())));
			list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Barding"));
			list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteHorseInfo.Food"));
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
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.LevelRequired"));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.Level", spl.Level));
								list.Add(" ");
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.ProcAbility"));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.Charges", item.PoisonCharges));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.MaxCharges", item.PoisonMaxCharges));
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
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.LevelRequired"));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Level", spl.Level));
								list.Add(" ");
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.ChargedMagic"));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Charges", item.Charges));
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.MaxCharges", item.MaxCharges));
								list.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, potionLine);
								if (spellHandler != null)
									list.AddRange(spellHandler.DelveInfo);
								list.Add(" ");
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.UseItem2"));
								long lastPotionItemUseTick = client.Player.TempProperties.getLongProperty(GamePlayer.LAST_POTION_ITEM_USE_TICK, 0L);
								long changeTime = client.Player.CurrentRegion.Time - lastPotionItemUseTick;
								if (changeTime < 60000) //1 minutes reuse timer
									list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.UseItem3", Util.FormatTime((60000 - changeTime) / 1000)));
								if (spl.CastTime > 0)
								{
									list.Add(" ");
									list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.NoUseInCombat"));
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
