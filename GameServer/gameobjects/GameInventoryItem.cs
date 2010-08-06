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
using System.Collections;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents an inventory item
	/// </summary>
	public class GameInventoryItem : InventoryItem, IGameInventoryItem
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GameInventoryItem()
			: base()
		{
		}

		public GameInventoryItem(ItemTemplate template)
			: base(template)
		{
		}

		public GameInventoryItem(ItemUnique template)
			: base(template)
		{
		}

		public GameInventoryItem(InventoryItem item)
			: base(item)
		{
			OwnerID = item.OwnerID;
			ObjectId = item.ObjectId;
		}

		/// <summary>
		/// Is this a valid item for this player?
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CheckValid(GamePlayer player)
		{
			return true;
		}


		/// <summary>
		/// This is used to create a PlayerInventoryItem
		/// template.ClassType will be checked and the approrpiate GameInventoryItem created
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		/// <returns></returns>
		public static GameInventoryItem Create<T>(T item)
		{
			GameInventoryItem playerItem = null;
			Type tItem = item.GetType();
			PropertyInfo pClassType = tItem.GetProperty("ClassType");

			string classType = "";

			if (pClassType != null && pClassType.CanRead)
			{
				classType = pClassType.GetValue(item, null) as string;

				if (classType != null && classType != "")
				{
					foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (assembly.GetType(classType) != null)
						{
							try
							{
								playerItem = assembly.CreateInstance(classType, false, BindingFlags.CreateInstance, null, new object[] { item }, null, null) as GameInventoryItem;
							}
							catch (Exception)
							{
							}

							break;
						}
					}
				}
			}
			else
			{
				log.WarnFormat("ClassType property not found or can't be read for item of type {0}!", tItem.FullName);
			}

			if (playerItem == null)
			{
				if (classType != null && classType != "")
				{
					log.WarnFormat("Failed to construct inventory item of ClassType {0}!", classType);
				}

				playerItem = Activator.CreateInstance(typeof(GameInventoryItem), new object[] { item }) as GameInventoryItem;
			}

			return playerItem;
		}

		/// <summary>
		/// This inventory strikes an enemy
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target"></param>
		public virtual void OnStrikeTarget(GameLiving owner, GameObject target)
		{
			if (owner is GamePlayer)
			{
				GamePlayer player = owner as GamePlayer;

				if (ConditionPercent > 70 && Util.Chance(ServerProperties.Properties.ITEM_CONDITION_LOSS_CHANCE))
				{
					int oldPercent = ConditionPercent;
					double con = GamePlayer.GetConLevel(player.Level, Level);
					if (con < -3.0)
						con = -3.0;
					int sub = (int)(con + 4);
					if (oldPercent < 91)
					{
						sub *= 2;
					}

					// Subtract condition
					Condition -= sub;
					if (Condition < 0)
						Condition = 0;

					if (ConditionPercent != oldPercent)
					{
						if (ConditionPercent == 90)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.CouldRepair", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else if (ConditionPercent == 80)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NeedRepair", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else if (ConditionPercent == 70)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NeedRepairDire", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

						player.Out.SendUpdateWeaponAndArmorStats();
						player.Out.SendInventorySlotsUpdate(new int[] { SlotPosition });
					}
				}
			}
		}


		/// <summary>
		/// This inventory is struck by an enemy
		/// </summary>
		/// <param name="player"></param>
		/// <param name="enemy"></param>
		public virtual void OnStruckByEnemy(GameLiving owner, GameLiving enemy)
		{
			if (owner is GamePlayer)
			{
				GamePlayer player = owner as GamePlayer;

				if (ConditionPercent > 70 && Util.Chance(ServerProperties.Properties.ITEM_CONDITION_LOSS_CHANCE))
				{
					int oldPercent = ConditionPercent;
					double con = GamePlayer.GetConLevel(player.Level, Level);
					if (con < -3.0)
						con = -3.0;
					int sub = (int)(con + 4);
					if (oldPercent < 91)
					{
						sub *= 2;
					}

					// Subtract condition
					Condition -= sub;
					if (Condition < 0)
						Condition = 0;

					if (ConditionPercent != oldPercent)
					{
						if (ConditionPercent == 90)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.CouldRepair", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else if (ConditionPercent == 80)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NeedRepair", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else if (ConditionPercent == 70)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NeedRepairDire", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

						player.Out.SendUpdateWeaponAndArmorStats();
						player.Out.SendInventorySlotsUpdate(new int[] { SlotPosition });
					}
				}
			}
		}


		/// <summary>
		/// Delve this item
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="player"></param>
		public virtual void Delve(List<String> delve, GamePlayer player)
		{
			if (player == null)
				return;

			//**********************************
			//show crafter name
			//**********************************
			if (IsCrafted)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.HandlePacket.CrafterName", Creator));
				delve.Add(" ");
			}
			else if (Description != null && Description != "")
			{
				delve.Add(Description);
				delve.Add(" ");
			}

			if ((Object_Type >= (int)eObjectType.GenericWeapon) && (Object_Type <= (int)eObjectType._LastWeapon) ||
				Object_Type == (int)eObjectType.Instrument)
			{
				WriteUsableClasses(delve, player.Client);
				WriteMagicalBonuses(delve, player.Client, false);
				DelveWeaponStats(delve, player);
			}

			if (Object_Type >= (int)eObjectType.Cloth && Object_Type <= (int)eObjectType.Scale)
			{
				WriteUsableClasses(delve, player.Client);
				WriteMagicalBonuses(delve, player.Client, false);
				DelveArmorStats(delve, player);
			}

			if (Object_Type == (int)eObjectType.Shield)
			{
				WriteUsableClasses(delve, player.Client);
				WriteMagicalBonuses(delve, player.Client, false);
				DelveShieldStats(delve, player.Client);
			}

			if (Object_Type == (int)eObjectType.Magical || Object_Type == (int)eObjectType.AlchemyTincture || Object_Type == (int)eObjectType.SpellcraftGem)
			{
				WriteMagicalBonuses(delve, player.Client, false);
			}

			//***********************************
			//shows info for Poison Potions
			//***********************************
			if (Object_Type == (int)eObjectType.Poison)
			{
				WritePoisonInfo(delve, player.Client);
			}

			if (Object_Type == (int)eObjectType.Magical && Item_Type == (int)eInventorySlot.FirstBackpack) // potion
			{
				WritePotionInfo(delve, player.Client);
			}
			else if (CanUseEvery > 0)
			{
				// Items with a reuse timer (aka cooldown).
				delve.Add(" ");

				int minutes = CanUseEvery / 60;
				int seconds = CanUseEvery % 60;

				if (minutes == 0)
				{
					delve.Add(String.Format("Can use item every: {0} sec", seconds));
				}
				else
				{
					delve.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
				}

				// delve.Add(String.Format("Can use item every: {0:00}:{1:00}", minutes, seconds));

				int cooldown = CanUseAgainIn;

				if (cooldown > 0)
				{
					minutes = cooldown / 60;
					seconds = cooldown % 60;

					if (minutes == 0)
					{
						delve.Add(String.Format("Can use again in: {0} sec", seconds));
					}
					else
					{
						delve.Add(String.Format("Can use again in: {0}:{1:00} min", minutes, seconds));
					}

					// delve.Add(String.Format("Can use again in: {0:00}:{1:00}", minutes, seconds));
				}
			}

			if (!IsDropable || !IsPickable || IsIndestructible)
				delve.Add(" ");

			if (!IsPickable)
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.HandlePacket.CannotTraded"));

			if (!IsDropable)
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.HandlePacket.CannotSold"));

			if (IsIndestructible)
				delve.Add("Cannot be destroyed.");

			if (BonusLevel > 0)
			{
				delve.Add(" ");
				delve.Add(string.Format("Bonus Level: {0}", BonusLevel));
			}

			//Add admin info
			if (player.Client.Account.PrivLevel > 1)
			{
				WriteTechnicalInfo(delve);
			}
		}

		protected virtual void WriteUsableClasses(IList<string> output, GameClient client)
		{
			if (string.IsNullOrEmpty(AllowedClasses) || AllowedClasses == "0")
				return;

			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteUsableClasses.UsableBy"));

			string[] allowedclasses = AllowedClasses.Split(';');
			foreach (string allowed in allowedclasses)
			{
				int classID = -1;
				if (int.TryParse(allowed, out classID))
				{
					output.Add("- " + ((eCharacterClass)classID).ToString());
				}
				else 
				{
					log.Error(Id_nb + " has an invalid entry for allowed classes '" + allowed + "'");
				}
			}
		}


		protected virtual void WriteMagicalBonuses(IList<string> output, GameClient client, bool shortInfo)
		{
			int oldCount = output.Count;

			WriteBonusLine(output, Bonus1Type, Bonus1);
			WriteBonusLine(output, Bonus2Type, Bonus2);
			WriteBonusLine(output, Bonus3Type, Bonus3);
			WriteBonusLine(output, Bonus4Type, Bonus4);
			WriteBonusLine(output, Bonus5Type, Bonus5);
			WriteBonusLine(output, Bonus6Type, Bonus6);
			WriteBonusLine(output, Bonus7Type, Bonus7);
			WriteBonusLine(output, Bonus8Type, Bonus8);
			WriteBonusLine(output, Bonus9Type, Bonus9);
			WriteBonusLine(output, Bonus10Type, Bonus10);
			WriteBonusLine(output, ExtraBonusType, ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicBonus"));
				output.Insert(oldCount, " ");
			}

			oldCount = output.Count;

			WriteFocusLine(output, Bonus1Type, Bonus1);
			WriteFocusLine(output, Bonus2Type, Bonus2);
			WriteFocusLine(output, Bonus3Type, Bonus3);
			WriteFocusLine(output, Bonus4Type, Bonus4);
			WriteFocusLine(output, Bonus5Type, Bonus5);
			WriteFocusLine(output, Bonus6Type, Bonus6);
			WriteFocusLine(output, Bonus7Type, Bonus7);
			WriteFocusLine(output, Bonus8Type, Bonus8);
			WriteFocusLine(output, Bonus9Type, Bonus9);
			WriteFocusLine(output, Bonus10Type, Bonus10);
			WriteFocusLine(output, ExtraBonusType, ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.FocusBonus"));
				output.Insert(oldCount, " ");
			}

			if (!shortInfo)
			{
				if (ProcSpellID != 0 || ProcSpellID1 != 0 || SpellID != 0 || SpellID1 != 0)
				{
					int requiredLevel = LevelRequirement > 0 ? LevelRequirement : Math.Min(50, Level);
					output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired2", requiredLevel));
					output.Add(" ");
				}

				if (Object_Type == (int)eObjectType.Magical && Item_Type == (int)eInventorySlot.FirstBackpack) // potion
				{
					// let WritePotion handle the rest of the display
					return;
				}


				#region Proc1
				if (ProcSpellID != 0)
				{
					string spellNote = "";
					output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						Spell procSpell = SkillBase.FindSpell(ProcSpellID, line);

						if (procSpell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
							if (spellHandler != null)
							{
								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
							}
							else
							{
								output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
							}

							output.Add(spellNote);
						}
						else
						{
							output.Add("- Spell Not Found: " + ProcSpellID);
						}
					}
					else
					{
						output.Add("- Item_Effects Spell Line Missing");
					}

					output.Add(" ");
				}
				#endregion
				#region Proc2
				if (ProcSpellID1 != 0)
				{
					string spellNote = "";
					output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						Spell procSpell = SkillBase.FindSpell(ProcSpellID1, line);

						if (procSpell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
							if (spellHandler != null)
							{
								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
							}
							else
							{
								output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
							}

							output.Add(spellNote);
						}
						else
						{
							output.Add("- Spell Not Found: " + ProcSpellID1);
						}
					}
					else
					{
						output.Add("- Item_Effects Spell Line Missing");
					}

					output.Add(" ");
				}
				#endregion
				#region Charge1
				if (SpellID != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						Spell spell = SkillBase.FindSpell(SpellID, chargeEffectsLine);
						if (spell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

							if (spellHandler != null)
							{
								if (MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", Charges));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", MaxCharges));
									output.Add(" ");
								}

								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
								output.Add("- This spell is cast when the item is used.");
							}
							else
							{
								output.Add("- Item_Effects Spell Line Missing");
							}
						}
						else
						{
							output.Add("- Spell Not Found: " + SpellID);
						}
					}

					output.Add(" ");
				}
				#endregion
				#region Charge2
				if (SpellID1 != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						Spell spell = SkillBase.FindSpell(SpellID1, chargeEffectsLine);
						if (spell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

							if (spellHandler != null)
							{
								if (MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", Charges));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", MaxCharges));
									output.Add(" ");
								}

								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
								output.Add("- This spell is cast when the item is used.");
							}
							else
							{
								output.Add("- Item_Effects Spell Line Missing");
							}
						}
						else
						{
							output.Add("- Spell Not Found: " + SpellID1);
						}
					}

					output.Add(" ");
				}
				#endregion
				#region Poison
				if (PoisonSpellID != 0)
				{
					if (GlobalConstants.IsWeapon(Object_Type))// Poisoned Weapon
					{
						SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
						if (poisonLine != null)
						{
							List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);
							foreach (Spell spl in spells)
							{
								if (spl.ID == PoisonSpellID)
								{
									output.Add(" ");
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
									output.Add(" ");
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", PoisonCharges));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", PoisonMaxCharges));
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

					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == SpellID)
							{
								output.Add(" ");
								output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
								output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
								output.Add(" ");
								if (MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.Charges", Charges));
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", MaxCharges));
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
								long lastChargedItemUseTick = client.Player.TempProperties.getProperty<long>(GamePlayer.LAST_CHARGED_ITEM_USE_TICK);
								long changeTime = client.Player.CurrentRegion.Time - lastChargedItemUseTick;
								long recastDelay = (spl.RecastDelay > 0) ? spl.RecastDelay : 60000 * 3;
								if (changeTime < recastDelay) //3 minutes reuse timer
									output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UseItem3", Util.FormatTime((recastDelay - changeTime) / 1000)));
								return;
							}
						}
					}
				}
				#endregion
			}
		}


		protected virtual void WriteBonusLine(IList<string> list, int bonusCat, int bonusValue)
		{
			if (bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus))
			{
				if (IsPvEBonus((eProperty)bonusCat))
				{
					// Evade: {0}% (PvE Only)
					list.Add(string.Format(SkillBase.GetPropertyName((eProperty)bonusCat), bonusValue));
				}
				else
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
						bonusValue.ToString("+0 ;-0 ;0 "), //Eden
						((bonusCat == (int)eProperty.PowerPool)
						 || (bonusCat >= (int)eProperty.Resist_First && bonusCat <= (int)eProperty.Resist_Last)
						 || (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)
						 || bonusCat == (int)eProperty.Conversion
						 || bonusCat == (int)eProperty.ExtraHP
						 || bonusCat == (int)eProperty.RealmPoints
						 || bonusCat == (int)eProperty.StyleAbsorb
						 || bonusCat == (int)eProperty.ArcaneSyphon
						 || bonusCat == (int)eProperty.BountyPoints
						 || bonusCat == (int)eProperty.XpPoints)
						? ((bonusCat == (int)eProperty.PowerPool) ? LanguageMgr.GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, "DetailDisplayHandler.WriteBonusLine.PowerPool") : "%")
						: LanguageMgr.GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, "DetailDisplayHandler.WriteBonusLine.Points")
					));
				}
			}
		}

		protected virtual void WriteFocusLine(IList<string> list, int focusCat, int focusLevel)
		{
			if (SkillBase.CheckPropertyType((eProperty)focusCat, ePropertyType.Focus))
			{
				//- Body Magic: 4 lvls
				list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName((eProperty)focusCat), focusLevel));
			}
		}


		protected virtual bool IsPvEBonus(eProperty property)
		{
			switch (property)
			{
				//case eProperty.BlockChance:
				//case eProperty.ParryChance:
				//case eProperty.EvadeChance:
				case eProperty.DefensiveBonus:
				case eProperty.BladeturnReinforcement:
				case eProperty.NegativeReduction:
				case eProperty.PieceAblative:
				case eProperty.ReactionaryStyleDamage:
				case eProperty.SpellPowerCost:
				case eProperty.StyleCostReduction:
				case eProperty.ToHitBonus:
					return true;

				default:
					return false;
			}
		}


		protected virtual void WritePoisonInfo(IList<string> list, GameClient client)
		{
			if (PoisonSpellID != 0)
			{
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);

					foreach (Spell spl in spells)
					{
						if (spl.ID == PoisonSpellID)
						{
							list.Add(" ");
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.LevelRequired"));
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.Level", spl.Level));
							list.Add(" ");
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.ProcAbility"));
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.Charges", PoisonCharges));
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePoisonInfo.MaxCharges", PoisonMaxCharges));
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


		protected virtual void WritePotionInfo(IList<string> list, GameClient client)
		{
			if (SpellID != 0)
			{
				SpellLine potionLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
				if (potionLine != null)
				{
					List<Spell> spells = SkillBase.GetSpellList(potionLine.KeyName);

					foreach (Spell spl in spells)
					{
						if (spl.ID == SpellID)
						{
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.ChargedMagic"));
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Charges", Charges));
							list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.MaxCharges", MaxCharges));
							list.Add(" ");
							WritePotionSpellsInfos(list, client, spl, potionLine);
							list.Add(" ");
							long nextPotionAvailTime = client.Player.TempProperties.getProperty<long>("LastPotionItemUsedTick_Type" + spl.SharedTimerGroup);
							// Satyr Update: Individual Reuse-Timers for Pots need a Time looking forward
							// into Future, set with value of "itemtemplate.CanUseEvery" and no longer back into past
							if (nextPotionAvailTime > client.Player.CurrentRegion.Time)
							{
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.UseItem3", Util.FormatTime((nextPotionAvailTime - client.Player.CurrentRegion.Time) / 1000)));
							}
							else
							{
								int minutes = CanUseEvery / 60;
								int seconds = CanUseEvery % 60;

								if (minutes == 0)
								{
									list.Add(String.Format("Can use item every: {0} sec", seconds));
								}
								else
								{
									list.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
								}
							}

							if (spl.CastTime > 0)
							{
								list.Add(" ");
								list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.NoUseInCombat"));
							}
							break;
						}
					}
				}
			}
		}


		protected static void WritePotionSpellsInfos(IList<string> list, GameClient client, Spell spl, NamedSkill line)
		{
			if (spl != null)
			{
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Type", spl.SpellType));
				list.Add(" ");
				list.Add(spl.Description);
				list.Add(" ");
				if (spl.Value != 0)
				{
					list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Value", spl.Value));
				}
				list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Target", spl.Target));
				if (spl.Range > 0)
				{
					list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.Range", spl.Range));
				}
				list.Add(" ");
				list.Add(" ");
				if (spl.SubSpellID > 0)
				{
					List<Spell> spells = SkillBase.GetSpellList(line.KeyName);
					foreach (Spell subSpell in spells)
					{
						if (subSpell.ID == spl.SubSpellID)
						{
							WritePotionSpellsInfos(list, client, subSpell, line);
							break;
						}
					}
				}
			}
		}


		protected virtual void DelveShieldStats(IList<string> output, GameClient client)
		{
			double itemDPS = DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = SPD_ABS / 10.0;

			output.Add(" ");
			output.Add(" ");
			output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.DamageMod"));
			if (itemDPS != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.BaseDPS", itemDPS.ToString("0.0")));
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}
			if (SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.SPD", itemSPD.ToString("0.0")));
			}

			output.Add(" ");

			switch (Type_Damage)
			{
				case 1: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Small")); break;
				case 2: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Medium")); break;
				case 3: output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteClassicShieldInfos.Large")); break;
			}
		}


		protected virtual void DelveWeaponStats(List<String> delve, GamePlayer player)
		{
			double itemDPS = DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * player.Level);
			double itemSPD = SPD_ABS / 10.0;
			double effectiveDPS = itemDPS * Quality / 100.0 * Condition / MaxCondition;

			delve.Add(" ");
			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageMod"));

			if (itemDPS != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}

			if (SPD_ABS >= 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));
			}

			if (Quality != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.Quality", Quality));
			}

			if (Condition != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.Condition", ConditionPercent));
			}

			delve.Add(LanguageMgr.GetTranslation(player.Client, 
													"DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
													(Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(Type_Damage))));

			delve.Add(" ");

			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));

			if (itemDPS != 0)
			{
				delve.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
			}
		}

		protected virtual void DelveArmorStats(List<String> delve, GamePlayer player)
		{
			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));

			double af = 0;
			int afCap = player.Level;
			double effectiveAF = 0;

			if (DPS_AF != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", DPS_AF));

				if (Object_Type != (int)eObjectType.Cloth)
				{
					afCap *= 2;
				}

				af = Math.Min(afCap, DPS_AF);

				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)af));
			}

			if (SPD_ABS >= 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.Absorption", SPD_ABS));
			}

			if (Quality != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.Quality", Quality));
			}

			if (Condition != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.Condition", ConditionPercent));
			}

			delve.Add(" ");
			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));

			if (DPS_AF != 0)
			{
				effectiveAF = af * Quality / 100.0 * Condition / MaxCondition * (1 + SPD_ABS / 100.0);
				delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)effectiveAF));
			}
		}

		/// <summary>
		/// Write item technical info
		/// </summary>
		/// <param name="output"></param>
		/// <param name="item"></param>
		public virtual void WriteTechnicalInfo(List<String> delve)
		{
			delve.Add(" ");
			delve.Add("--- Item technical information ---");
			delve.Add(" ");
			delve.Add("Item Template: " + Id_nb);
			delve.Add("         Name: " + Name);
			delve.Add("    ClassType: " + this.GetType().FullName);
			delve.Add(" ");
			delve.Add("        Level: " + Level);
			delve.Add("       Object: " + GlobalConstants.ObjectTypeToName(Object_Type) + " (" + Object_Type + ")");
			delve.Add("         Type: " + GlobalConstants.SlotToName(Item_Type) + " (" + Item_Type + ")");
			delve.Add("    Extension: " + Extension);
			delve.Add("        Model: " + Model);
			delve.Add("        Color: " + Color);
			delve.Add("       Emblem: " + Emblem);
			delve.Add("       Effect: " + Effect);
			delve.Add("  Value/Price: " + Money.GetShortString(Price));
			delve.Add("       Weight: " + (Weight / 10.0f) + "lbs");
			delve.Add("      Quality: " + Quality + "%");
			delve.Add("   Durability: " + Durability + "/" + MaxDurability);
			delve.Add("    Condition: " + Condition + "/" + MaxCondition);
			delve.Add("        Realm: " + Realm);
			delve.Add("  Is dropable: " + (IsDropable ? "yes" : "no"));
			delve.Add("  Is pickable: " + (IsPickable ? "yes" : "no"));
			delve.Add("  Is tradable: " + (IsTradable ? "yes" : "no"));
			delve.Add(" Is alwaysDUR: " + (IsNotLosingDur ? "yes" : "no"));
			delve.Add("Is Indestruct: " + (IsIndestructible ? "yes" : "no"));
			delve.Add(" Is stackable: " + (IsStackable ? "yes" : "no"));
			delve.Add("  ProcSpellID: " + ProcSpellID);
			delve.Add(" ProcSpellID1: " + ProcSpellID1);
			delve.Add("   ProcChance: " + ProcChance);
			delve.Add("      SpellID: " + SpellID + " (" + Charges + "/" + MaxCharges + ")");
			delve.Add("     SpellID1: " + SpellID1 + " (" + Charges1 + "/" + MaxCharges1 + ")");
			delve.Add("PoisonSpellID: " + PoisonSpellID + " (" + PoisonCharges + "/" + PoisonMaxCharges + ") ");

			if (GlobalConstants.IsWeapon(Object_Type))
			{
				delve.Add("         Hand: " + GlobalConstants.ItemHandToName(Hand) + " (" + Hand + ")");
				delve.Add("Damage/Second: " + (DPS_AF / 10.0f));
				delve.Add("        Speed: " + (SPD_ABS / 10.0f));
				delve.Add("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(Type_Damage) + " (" + Type_Damage + ")");
				delve.Add("        Bonus: " + Bonus);
			}
			else if (GlobalConstants.IsArmor(Object_Type))
			{
				delve.Add("  Armorfactor: " + DPS_AF);
				delve.Add("   Absorption: " + SPD_ABS);
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Shield)
			{
				delve.Add("Damage/Second: " + (DPS_AF / 10.0f));
				delve.Add("        Speed: " + (SPD_ABS / 10.0f));
				delve.Add("  Shield type: " + GlobalConstants.ShieldTypeToName(Type_Damage) + " (" + Type_Damage + ")");
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Arrow || Object_Type == (int)eObjectType.Bolt)
			{
				delve.Add(" Ammunition #: " + DPS_AF);
				delve.Add("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(SPD_ABS));
				delve.Add("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(SPD_ABS));
				delve.Add("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(SPD_ABS));
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Instrument)
			{
				delve.Add("   Instrument: " + GlobalConstants.InstrumentTypeToName(DPS_AF));
			}

			delve.Add(" ");
			delve.Add("              Flags: " + Flags);
			delve.Add("          PackageID: " + PackageID);
			delve.Add("Requested ClassType: " + ClassType);
		}
	}
}
