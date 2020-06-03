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
using System.Collections.Generic;
using System.Reflection;

using DOL.Language;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS.Styles
{
	/// <summary>
	/// Processes styles and style related stuff.
	/// </summary>
	public class StyleProcessor
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Returns wether this player can use a particular style
		/// right now. Tests for all preconditions like prerequired
		/// styles, previous attack result, ...
		/// </summary>
		/// <param name="living">The living wanting to execute a style</param>
		/// <param name="style">The style to execute</param>
		/// <param name="weapon">The weapon used to execute the style</param>
		/// <returns>true if the player can execute the style right now, false if not</returns>
		public static bool CanUseStyle(GameLiving living, Style style, InventoryItem weapon)
		{
			//First thing in processors, lock the objects you modify
			//This way it makes sure the objects are not modified by
			//several different threads at the same time!
			lock (living)
			{
				GameLiving target = living.TargetObject as GameLiving;
				if (target == null) return false;

				//Required attack result
				GameLiving.eAttackResult requiredAttackResult = GameLiving.eAttackResult.Any;
				switch (style.AttackResultRequirement)
				{
					case Style.eAttackResultRequirement.Any: requiredAttackResult = GameLiving.eAttackResult.Any; break;
					case Style.eAttackResultRequirement.Block: requiredAttackResult = GameLiving.eAttackResult.Blocked; break;
					case Style.eAttackResultRequirement.Evade: requiredAttackResult = GameLiving.eAttackResult.Evaded; break;
					case Style.eAttackResultRequirement.Fumble: requiredAttackResult = GameLiving.eAttackResult.Fumbled; break;
					case Style.eAttackResultRequirement.Hit: requiredAttackResult = GameLiving.eAttackResult.HitUnstyled; break;
					case Style.eAttackResultRequirement.Style: requiredAttackResult = GameLiving.eAttackResult.HitStyle; break;
					case Style.eAttackResultRequirement.Miss: requiredAttackResult = GameLiving.eAttackResult.Missed; break;
					case Style.eAttackResultRequirement.Parry: requiredAttackResult = GameLiving.eAttackResult.Parried; break;
				}

				AttackData lastAD = (AttackData)living.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);

				switch (style.OpeningRequirementType)
				{

					case Style.eOpening.Offensive:
						//Style required before this one?
						if (style.OpeningRequirementValue != 0
							&& (lastAD == null
							|| lastAD.AttackResult != GameLiving.eAttackResult.HitStyle
							|| lastAD.Style == null
							|| lastAD.Style.ID != style.OpeningRequirementValue
							|| lastAD.Target != target)) // style chains are possible only on the same target
						{
							//DOLConsole.WriteLine("Offensive: Opening Requirement style needed failed!("+style.OpeningRequirementValue+")");
							return false;
						}

						//Last attack result
						GameLiving.eAttackResult lastRes = (lastAD != null) ? lastAD.AttackResult : GameLiving.eAttackResult.Any;

						if (requiredAttackResult != GameLiving.eAttackResult.Any && lastRes != requiredAttackResult)
						{
							//DOLConsole.WriteLine("Offensive: AttackResult Requirement failed!("+requiredAttackResult.ToString()+", was "+lastRes+")");
							return false;
						}
						break;

					case Style.eOpening.Defensive:
						AttackData targetsLastAD = (AttackData)target.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);

						//Last attack result
						if (requiredAttackResult != GameLiving.eAttackResult.Any)
						{
							if (targetsLastAD == null || targetsLastAD.Target != living)
							{
								return false;
							}

							if (requiredAttackResult != GameLiving.eAttackResult.HitStyle && targetsLastAD.AttackResult != requiredAttackResult)
							{
								//DOLConsole.WriteLine("Defensive: AttackResult Requirement failed!("+requiredAttackResult.ToString()+", was "+lastEnemyRes+")");
								return false;
							}
							else if (requiredAttackResult == GameLiving.eAttackResult.HitStyle && targetsLastAD.Style == null)
							{
								//DOLConsole.WriteLine("Defensive: AttackResult Requirement failed!("+requiredAttackResult.ToString()+", was "+lastEnemyRes+")");
								return false;
							}
						}
						break;

					case Style.eOpening.Positional:
						//check here if target is in front of attacker
						if (!living.IsObjectInFront(target, 120))
							return false;

						//you can't use positional styles on keep doors or walls
						if ((target is GameKeepComponent || target is GameKeepDoor) && (Style.eOpeningPosition)style.OpeningRequirementValue != Style.eOpeningPosition.Front)
							return false;

						// get players angle on target
                        float angle = target.GetAngle( living );
						//player.Out.SendDebugMessage("Positional check: "+style.OpeningRequirementValue+" angle "+angle+" target heading="+target.Heading);						

						switch ((Style.eOpeningPosition)style.OpeningRequirementValue)
						{
							//Back Styles
							//60 degree since 1.62 patch
							case Style.eOpeningPosition.Back:
								if (!(angle >= 150 && angle < 210)) return false;
								break;
							// Side Styles  
							//105 degree since 1.62 patch
							case Style.eOpeningPosition.Side:
								if (!(angle >= 45 && angle < 150) && !(angle >= 210 && angle < 315)) return false;
								break;
							// Front Styles
							// 90 degree
							case Style.eOpeningPosition.Front:
								if (!(angle >= 315 || angle < 45)) return false;
								break;
						}
						//DOLConsole.WriteLine("Positional check success: "+style.OpeningRequirementValue);
						break;
				}

				if (style.StealthRequirement && !living.IsStealthed)
					return false;

				if (!CheckWeaponType(style, living, weapon))
					return false;

				//				if(player.Endurance < CalculateEnduranceCost(style, weapon.SPD_ABS))
				//					return false;

				return true;
			}
		}

		/// <summary>
		/// Tries to queue a new style in the player's style queue.
		/// Takes care of all conditions like setting backup styles and
		/// canceling styles if the style was queued already.
		/// </summary>
		/// <param name="living">The living to execute the style</param>
		/// <param name="style">The style to execute</param>
		public static void TryToUseStyle(GameLiving living, Style style)
		{
			//First thing in processors, lock the objects you modify
			//This way it makes sure the objects are not modified by
			//several different threads at the same time!
			GamePlayer player = living as GamePlayer;
			lock (living)
			{
				//Dead players can't use styles
				if (!living.IsAlive)
				{
					if (player != null)
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.CantCombatMode"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					
					return;
				}
				if (living.IsDisarmed)
				{
					if(living is GamePlayer) (living as GamePlayer).Out.SendMessage("You are disarmed and cannot attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}
				//Can't use styles with range weapon
				if (living.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
				{
					if (player != null)
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.CantMeleeCombat"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}

				//Put player into attack state before setting the styles
				//Changing the attack state clears out the styles...
				if (living.AttackState == false)
				{
					living.StartAttack(player.TargetObject);
				}

				if (living.TargetObject == null)
				{
					if (player != null)
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.MustHaveTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				InventoryItem weapon = (style.WeaponTypeRequirement == (int)eObjectType.Shield) ? living.Inventory.GetItem(eInventorySlot.LeftHandWeapon) : living.AttackWeapon;
				//				if (weapon == null) return;	// no weapon = no style
				if (!CheckWeaponType(style, living, weapon))
				{
					if (player != null)
					{
						if (style.WeaponTypeRequirement == Style.SpecialWeaponType.DualWield)
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.DualWielding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.StyleRequires", style.GetRequiredWeaponName()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}

				if (player != null) //Do mob use endurance?
				{
					int fatCost = CalculateEnduranceCost(player, style, weapon.SPD_ABS);
					if (player.Endurance < fatCost)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.Fatigued"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}

				if (player != null)
				{
					Style preRequireStyle = null;
					if (style.OpeningRequirementType == Style.eOpening.Offensive && style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
						preRequireStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, player.CharacterClass.ID);

					//We have not set any primary style yet?
					if (player.NextCombatStyle == null)
					{
						if (preRequireStyle != null)
						{
							AttackData lastAD = (AttackData)living.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
							if (lastAD == null
							|| lastAD.AttackResult != GameLiving.eAttackResult.HitStyle
							|| lastAD.Style == null
							|| lastAD.Style.ID != style.OpeningRequirementValue)
							{
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.PerformStyleBefore", preRequireStyle.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						}

						player.NextCombatStyle = style;
						player.NextCombatBackupStyle = null;
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.PreparePerform", style.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

						if (living.IsEngaging)
						{
							// cancel engage effect if exist
							EngageEffect effect = living.EffectList.GetOfType<EngageEffect>();
							if (effect != null)
								effect.Cancel(false);
						}

						// unstealth only on primary style to not break
						// stealth with non-stealth backup styles
						if (!style.StealthRequirement)
							player.Stealth(false);
					}
					else
					{
						//Have we also set the backupstyle already?
						if (player.NextCombatBackupStyle != null)
						{
							//All styles set, can't change anything now
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.AlreadySelectedStyles"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							//Have we pressed the same style button used for the primary style again?
							if (player.NextCombatStyle.ID == style.ID)
							{
								if (player.CancelStyle)
								{
									//If yes, we cancel the style
									player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.NoLongerPreparing", player.NextCombatStyle.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									player.NextCombatStyle = null;
									player.NextCombatBackupStyle = null;
								}
								else
								{
									player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.AlreadyPreparing"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
							else
							{
								if (preRequireStyle != null)
								{
									AttackData lastAD = (AttackData)living.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
									if (lastAD == null
									|| lastAD.AttackResult != GameLiving.eAttackResult.HitStyle
									|| lastAD.Style == null
									|| lastAD.Style.ID != style.OpeningRequirementValue)
									{
										player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.PerformStyleBefore", preRequireStyle.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return;
									}
								}
								//If no, set the secondary backup style
								player.NextCombatBackupStyle = style;
								player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.TryToUseStyle.BackupStyle", style.Name, player.NextCombatStyle.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Executes the style of the given player. Prints
		/// out messages to the player notifying him of his success or failure.
		/// </summary>
		/// <param name="living">The living executing the styles</param>
		/// <param name="attackData">
		/// The AttackData that will be modified to contain the 
		/// new damage and the executed style.
		/// </param>
		/// <param name="weapon">The weapon used to execute the style</param>
		/// <returns>true if a style was performed, false if not</returns>
		public static bool ExecuteStyle(GameLiving living, AttackData attackData, InventoryItem weapon)
		{
			//First thing in processors, lock the objects you modify
			//This way it makes sure the objects are not modified by
			//several different threads at the same time!

			GamePlayer player = living as GamePlayer;
			lock (living)
			{
				//Does the player want to execute a style at all?
				if (attackData.Style == null)
					return false;

				if (weapon != null && weapon.Object_Type == (int)eObjectType.Shield)
				{
					attackData.AnimationId = (weapon.Hand != 1) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // 2h shield?
				}
				int fatCost = 0;
				if (weapon != null)
					fatCost = CalculateEnduranceCost(living, attackData.Style, weapon.SPD_ABS);

				//Reduce endurance if styled attack missed
				switch (attackData.AttackResult)
				{
					case GameLiving.eAttackResult.Blocked:
					case GameLiving.eAttackResult.Evaded:
					case GameLiving.eAttackResult.Missed:
					case GameLiving.eAttackResult.Parried:
						if (player != null) //No mob endu lost yet
							living.Endurance -= Math.Max(1, fatCost / 2);
						return false;
				}

				//Ignore all other attack results
				if (attackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
					&& attackData.AttackResult != GameLiving.eAttackResult.HitStyle)
					return false;

				//Did primary and backup style fail?
				if (!CanUseStyle(living, attackData.Style, weapon))
				{
					if (player != null)
					{
						// reduce players endurance, full endurance if failed style
						player.Endurance -= fatCost;

						//"You must be hidden to perform this style!"
						//Print a style-fail message
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.ExecuteFail", attackData.Style.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					}
					return false;
				}
				else
				{
					//Style worked! Print out some nice info and add the damage! :)
					//Growth * Style Spec * Effective Speed / Unstyled Damage Cap

					bool staticGrowth = attackData.Style.StealthRequirement;  //static growth is not a function of (effective) weapon speed
					double absorbRatio = attackData.Damage / living.UnstyledDamageCap(weapon); //scaling factor for style damage
					double effectiveWeaponSpeed = living.AttackSpeed(weapon) * 0.001;
					double styleGrowth = Math.Max(0,attackData.Style.GrowthOffset + attackData.Style.GrowthRate * living.GetModifiedSpecLevel(attackData.Style.Spec));
					double styleDamageBonus = living.GetModified(eProperty.StyleDamage) * 0.01 - 1;

					if (staticGrowth)
					{
						if (living.AttackWeapon.Item_Type == Slot.TWOHAND)
						{
							styleGrowth = styleGrowth * 1.25 + living.WeaponDamage(living.AttackWeapon) * Math.Max(0,living.AttackWeapon.SPD_ABS - 21) * 10 / 66d;
						}
						attackData.StyleDamage = (int)(absorbRatio * styleGrowth * ServerProperties.Properties.CS_OPENING_EFFECTIVENESS);
					}
					else
						attackData.StyleDamage = (int)(absorbRatio * styleGrowth * effectiveWeaponSpeed);

					attackData.StyleDamage += (int)(attackData.Damage * styleDamageBonus);

					//Eden - style absorb bonus
					int absorb=0;
					if(attackData.Target is GamePlayer && attackData.Target.GetModified(eProperty.StyleAbsorb) > 0)
					{
						absorb=(int)Math.Floor((double)attackData.StyleDamage * ((double)attackData.Target.GetModified(eProperty.StyleAbsorb)/100));
						attackData.StyleDamage -= absorb;
					}

					//Increase regular damage by styledamage ... like on live servers
					attackData.Damage += attackData.StyleDamage;


					if (player != null)
					{
						// reduce players endurance
						player.Endurance -= fatCost;

						if(absorb > 0)
						{
							player.Out.SendMessage("A barrier absorbs " + absorb + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							if(living is GamePlayer) (living as GamePlayer).Out.SendMessage("A barrier absorbs " + absorb + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
					}

					#region StyleProcs
					if (attackData.Style.Procs.Count > 0)
					{
						ISpellHandler effect;

						// If ClassID = 0, use the proc for any class, unless there is also a proc with a ClassID
						// that matches the player's CharacterClass.ID, or for mobs, the style's ClassID - then use
						// the class-specific proc instead of the ClassID=0 proc
						if (!attackData.Style.RandomProc)
						{
							List<Tuple<Spell, int, int>> procsToExecute = new List<Tuple<Spell, int, int>>();
							bool onlyExecuteClassSpecific = false;

							foreach (Tuple<Spell, int, int> proc in attackData.Style.Procs)
							{
								if (player != null && proc.Item2 == player.CharacterClass.ID)
								{
									procsToExecute.Add(proc);
									onlyExecuteClassSpecific = true;
								}
								else if (proc.Item2 == attackData.Style.ClassID || proc.Item2 == 0)
								{
									procsToExecute.Add(proc);
								}
							}

							foreach (Tuple<Spell, int, int> procToExecute in procsToExecute)
							{
								if (onlyExecuteClassSpecific && procToExecute.Item2 == 0)
									continue;

								if (Util.Chance(procToExecute.Item3))
								{
									effect = CreateMagicEffect(living, attackData.Target, procToExecute.Item1.ID);
									//effect could be null if the SpellID is bigger than ushort
									if (effect != null)
									{
										attackData.StyleEffects.Add(effect);
										if ((attackData.Style.OpeningRequirementType == Style.eOpening.Offensive && attackData.Style.OpeningRequirementValue > 0) 
											|| attackData.Style.OpeningRequirementType == Style.eOpening.Defensive)
										{
											effect.UseMinVariance = true;
										}
									}
								}
							}
						}
						else
						{
							//Add one proc randomly
							int random = Util.Random(attackData.Style.Procs.Count - 1);
							//effect could be null if the SpellID is bigger than ushort
							effect = CreateMagicEffect(living, attackData.Target, attackData.Style.Procs[random].Item1.ID);
							if (effect != null)
							{
								attackData.StyleEffects.Add(effect);
								if ((attackData.Style.OpeningRequirementType == Style.eOpening.Offensive && attackData.Style.OpeningRequirementValue > 0) 
									|| attackData.Style.OpeningRequirementType == Style.eOpening.Defensive)
								{
									effect.UseMinVariance = true;
								}
							}
						}
					}
					#endregion StyleProcs

					#region Animation
					if (weapon != null)
						attackData.AnimationId = (weapon.Hand != 1) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
					else if (living.Inventory != null)
						attackData.AnimationId = (living.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
					else
						attackData.AnimationId = attackData.Style.Icon;
					#endregion Animation

					return true;
				}
			}
		}

		/// <summary>
		/// Calculates endurance needed to use style
		/// </summary>
		/// <param name="living">The living doing the style</param>
		/// <param name="style">The style to be used</param>
		/// <param name="weaponSpd">The weapon speed</param>
		/// <returns>Endurance needed to use style</returns>
		public static int CalculateEnduranceCost(GameLiving living, Style style, int weaponSpd)
		{

            //[StephenxPimentel]
            //1.108 - Valhallas Blessing now has a 75% chance to not use endurance.

			// Apply Valkyrie RA5L effect
			ValhallasBlessingEffect ValhallasBlessing = living.EffectList.GetOfType<ValhallasBlessingEffect>();
			if (ValhallasBlessing != null && Util.Chance(75)) return 0;

            //Camelot Herald 1.90 : Battlemaster styles will now cost a flat amount of Endurance, regardless of weapon speed
            if (style.Spec == Specs.Battlemaster)
                return Math.Max(1, (int)Math.Ceiling((30 * style.EnduranceCost / 40) * living.GetModified(eProperty.FatigueConsumption) * 0.01));
            
            int fatCost = weaponSpd * style.EnduranceCost / 40;
			if (weaponSpd < 40)
				fatCost++;
			
            fatCost = (int)Math.Ceiling(fatCost * living.GetModified(eProperty.FatigueConsumption) * 0.01);
			return Math.Max(1, fatCost);
		}

		/// <summary>
		/// Returns whether player has correct weapon
		/// active for particular style
		/// </summary>
		/// <param name="style">The style to execute</param>
		/// <param name="living">The living wanting to execute the style</param>
		/// <param name="weapon">The weapon used to execute the style</param>
		/// <returns>true if correct weapon active</returns>
		protected static bool CheckWeaponType(Style style, GameLiving living, InventoryItem weapon)
		{
			if (living is GameNPC)
				return true;
			GamePlayer player = living as GamePlayer;
			if (player == null) return false;

			switch (style.WeaponTypeRequirement)
			{
				case Style.SpecialWeaponType.DualWield:
					// both weapons are needed to use style,
					// shield is not a weapon here
					InventoryItem rightHand = player.AttackWeapon;
					InventoryItem leftHand = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);

					if (rightHand == null || leftHand == null || (rightHand.Item_Type != Slot.RIGHTHAND && rightHand.Item_Type != Slot.LEFTHAND))
						return false;

					if (style.Spec == Specs.HandToHand && (rightHand.Object_Type != (int)eObjectType.HandToHand || leftHand.Object_Type != (int)eObjectType.HandToHand))
						return false;
					else if (style.Spec == Specs.Fist_Wraps && (rightHand.Object_Type != (int)eObjectType.FistWraps || leftHand.Object_Type != (int)eObjectType.FistWraps))
						return false;

					return leftHand.Object_Type != (int)eObjectType.Shield;

				case Style.SpecialWeaponType.AnyWeapon:
					// TODO: style can be used with any weapon type,
					// shield is not a weapon here
					return weapon != null;

				default:
					// WeaponTypeRequirement holds eObjectType of weapon needed for style
					// no weapon = can't use style
					if (weapon == null)
						return false;

					// can't use shield styles if no active weapon
					if (style.WeaponTypeRequirement == (int)eObjectType.Shield
						&& (player.AttackWeapon == null || (player.AttackWeapon.Item_Type != Slot.RIGHTHAND && player.AttackWeapon.Item_Type != Slot.LEFTHAND)))
						return false;

					// weapon type check
					return GameServer.ServerRules.IsObjectTypesEqual(
							(eObjectType)style.WeaponTypeRequirement,
							(eObjectType)weapon.Object_Type);
			}
		}

		/// <summary>
		/// Add the magical effect to target
		/// </summary>
		/// <param name="caster">The player who execute the style</param>
		/// <param name="target">The target of the style</param>
		/// <param name="spellID">The spellid of the magical effect</param>
		protected static ISpellHandler CreateMagicEffect(GameLiving caster, GameLiving target, int spellID)
		{
			SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
			if (styleLine == null) return null;

			List<Spell> spells = SkillBase.GetSpellList(styleLine.KeyName);

			Spell styleSpell = null;
			foreach (Spell spell in spells)
			{
				if (spell.ID == spellID)
				{
					// We have to scale style procs when cast
					if (caster is GamePet pet)
						pet.ScalePetSpell(spell);

					styleSpell = spell;
					break;
				}
			}

			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(caster, styleSpell, styleLine);
			if (spellHandler == null && styleSpell != null && caster is GamePlayer)
			{
				((GamePlayer)caster).Out.SendMessage(styleSpell.Name + " not implemented yet (" + styleSpell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			// No negative effects can be applied on a keep door or via attacking a keep door
			if ((target is GameKeepComponent || target is GameKeepDoor) && spellHandler.HasPositiveEffect == false)
			{
				return null;
			}


			return spellHandler;
		}


		/// <summary>
		/// Delve a Style handled by this processor
		/// </summary>
		/// <param name="delveInfo"></param>
		/// <param name="style"></param>
		/// <param name="player"></param>
		public static void DelveWeaponStyle(IList<string> delveInfo, Style style, GamePlayer player)
		{
			delveInfo.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.WeaponType", style.GetRequiredWeaponName()));
			string temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Opening") + " ";
			if (Style.eOpening.Offensive == style.OpeningRequirementType)
			{
				//attacker action result is opening
				switch (style.AttackResultRequirement)
				{
					case Style.eAttackResultRequirement.Hit:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouHit");
						break;
					case Style.eAttackResultRequirement.Miss:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouMiss");
						break;
					case Style.eAttackResultRequirement.Parry:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetParrys");
						break;
					case Style.eAttackResultRequirement.Block:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetBlocks");
						break;
					case Style.eAttackResultRequirement.Evade:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetEvades");
						break;
					case Style.eAttackResultRequirement.Fumble:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouFumble");
						break;
					case Style.eAttackResultRequirement.Style:
						Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, player.CharacterClass.ID);
						if (reqStyle == null)
						{
							reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, 0);
						}
						temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.OpeningStyle") + " ";
						if (reqStyle == null)
						{
							temp += "(style not found " + style.OpeningRequirementValue + ")";
						}
						else
						{
							temp += reqStyle.Name;
						}
						break;
					default:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Any");
						break;
				}
			}
			else if (Style.eOpening.Defensive == style.OpeningRequirementType)
			{
				//defender action result is opening
				switch (style.AttackResultRequirement)
				{
					case Style.eAttackResultRequirement.Miss:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetMisses");
						break;
					case Style.eAttackResultRequirement.Hit:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetHits");
						break;
					case Style.eAttackResultRequirement.Parry:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouParry");
						break;
					case Style.eAttackResultRequirement.Block:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouBlock");
						break;
					case Style.eAttackResultRequirement.Evade:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.YouEvade");
						break;
					case Style.eAttackResultRequirement.Fumble:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetFumbles");
						break;
					case Style.eAttackResultRequirement.Style:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetStyle");
						break;
					default:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Any");
						break;
				}
			}
			else if (Style.eOpening.Positional == style.OpeningRequirementType)
			{
				//attacker position to target is opening
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Positional");
				switch (style.OpeningRequirementValue)
				{
					case (int)Style.eOpeningPosition.Front:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Front");
						break;
					case (int)Style.eOpeningPosition.Back:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Back");
						break;
					case (int)Style.eOpeningPosition.Side:
						temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Side");
						break;

				}
			}

			delveInfo.Add(temp);

			if (style.OpeningRequirementValue != 0 && style.AttackResultRequirement == 0 && style.OpeningRequirementType == 0)
			{
				delveInfo.Add(string.Format("- Error: Opening Requirement '{0}' but requirement type is Any!", style.OpeningRequirementValue));
			}

			temp = "";

			foreach (Style st in SkillBase.GetStyleList(style.Spec, player.CharacterClass.ID))
			{
				if (st.AttackResultRequirement == Style.eAttackResultRequirement.Style && st.OpeningRequirementValue == style.ID)
				{
					temp = (temp == "" ? st.Name : temp + LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Or", st.Name));
				}
			}

			if (temp != "")
			{
				delveInfo.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.FollowupStyle", temp));
			}

			temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.FatigueCost") + " ";

			if (style.EnduranceCost < 5)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLow");
			else if (style.EnduranceCost < 10)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Low");
			else if (style.EnduranceCost < 15)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Medium");
			else if (style.EnduranceCost < 20)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.High");
			else
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHigh");

			delveInfo.Add(temp);

			temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Damage") + " ";

			double tempGrowth = (style.GrowthRate * 50 + style.GrowthOffset) / 0.295; //0.295 is the rounded down style quantum that is used on Live

			if (style.GrowthRate == 0 && style.GrowthOffset == 0)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.None");
			else if (tempGrowth < 49)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLow");
			else if (tempGrowth < 99)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Low");
			else if (tempGrowth < 149)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Medium");
			else if (tempGrowth < 199)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.High");
			else if (tempGrowth < 249)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHigh");
			else
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Devastating");

			delveInfo.Add(temp);

			temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.ToHit") + " ";

			if (style.BonusToHit <= -20)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHighPenalty");
			else if (style.BonusToHit <= -15)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.HighPenalty");
			else if (style.BonusToHit <= -10)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.MediumPenalty");
			else if (style.BonusToHit <= -5)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.LowPenalty");
			else if (style.BonusToHit < 0)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLowPenalty");
			else if (style.BonusToHit == 0)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.NoBonus");
			else if (style.BonusToHit < 5)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLowBonus");
			else if (style.BonusToHit < 10)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.LowBonus");
			else if (style.BonusToHit < 15)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.MediumBonus");
			else if (style.BonusToHit < 20)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.HighBonus");
			else
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHighBonus");

			delveInfo.Add(temp);

			temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.Defense") + " ";

			if (style.BonusToDefense <= -20)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHighPenalty");
			else if (style.BonusToDefense <= -15)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.HighPenalty");
			else if (style.BonusToDefense <= -10)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.MediumPenalty");
			else if (style.BonusToDefense <= -5)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.LowPenalty");
			else if (style.BonusToDefense < 0)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLowPenalty");
			else if (style.BonusToDefense == 0)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.NoBonus");
			else if (style.BonusToDefense < 5)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryLowBonus");
			else if (style.BonusToDefense < 10)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.LowBonus");
			else if (style.BonusToDefense < 15)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.MediumBonus");
			else if (style.BonusToDefense < 20)
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.HighBonus");
			else
				temp += LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryHighBonus");

			delveInfo.Add(temp);

			if (style.Procs.Count > 0)
			{
				temp = LanguageMgr.GetTranslation(player.Client.Account.Language, "DetailDisplayHandler.HandlePacket.TargetEffect") + " ";

				SpellLine styleLine = SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect);
				if (styleLine != null)
				{
					/*check if there is a class specific style proc*/
					bool hasClassSpecificProc = false;
					foreach (Tuple<Spell, int, int> proc in style.Procs)
					{
						if (proc.Item2 == player.CharacterClass.ID)
						{
							hasClassSpecificProc = true;
							break;
						}
					}

					foreach (Tuple<Spell, int, int> proc in style.Procs)
					{
						// RR4: we added all the procs to the style, now it's time to check for class ID
						if (hasClassSpecificProc && proc.Item2 != player.CharacterClass.ID) continue;
						else if (!hasClassSpecificProc && proc.Item2 != 0) continue;

						Spell spell = proc.Item1;
						if (spell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(player.Client.Player, spell, styleLine);
							if (spellHandler == null)
							{
								temp += spell.Name + " (Not implemented yet)";
								delveInfo.Add(temp);
							}
							else
							{
								temp += spell.Name;
								delveInfo.Add(temp);
								delveInfo.Add(" ");//empty line
								Util.AddRange(delveInfo, spellHandler.DelveInfo);
							}
						}
					}
				}
			}

			if (player.Client.Account.PrivLevel > 1)
			{
				delveInfo.Add(" ");
				delveInfo.Add("--- Style Technical Information ---");
				delveInfo.Add(" ");
				delveInfo.Add(string.Format("ID: {0}", style.ID));
				delveInfo.Add(string.Format("ClassID: {0}", style.ClassID));
				delveInfo.Add(string.Format("Icon: {0}", style.Icon));
				delveInfo.Add(string.Format("TwoHandAnimation: {0}", style.TwoHandAnimation));
				delveInfo.Add(string.Format("Spec: {0}", style.Spec));
				delveInfo.Add(string.Format("SpecLevelRequirement: {0}", style.SpecLevelRequirement));
				delveInfo.Add(string.Format("Level: {0}", style.Level));
				delveInfo.Add(string.Format("GrowthOffset: {0}", style.GrowthOffset));
				delveInfo.Add(string.Format("GrowthRate: {0}", style.GrowthRate));
				delveInfo.Add(string.Format("Endurance: {0}", style.EnduranceCost));
				delveInfo.Add(string.Format("StealthRequirement: {0}", style.StealthRequirement));
				delveInfo.Add(string.Format("WeaponTypeRequirement: {0}", style.WeaponTypeRequirement));
				string indicator = "";
				if (style.OpeningRequirementValue != 0 && style.AttackResultRequirement == 0 && style.OpeningRequirementType == 0)
				{
					indicator = "!!";
				}
				delveInfo.Add(string.Format("AttackResultRequirement: {0}({1}) {2}", style.AttackResultRequirement, (int)style.AttackResultRequirement, indicator));
				delveInfo.Add(string.Format("OpeningRequirementType: {0}({1}) {2}", style.OpeningRequirementType, (int)style.OpeningRequirementType, indicator));
				delveInfo.Add(string.Format("OpeningRequirementValue: {0}", style.OpeningRequirementValue));
				delveInfo.Add(string.Format("ArmorHitLocation: {0}({1})", style.ArmorHitLocation, (int)style.ArmorHitLocation));
				delveInfo.Add(string.Format("BonusToDefense: {0}", style.BonusToDefense));
				delveInfo.Add(string.Format("BonusToHit: {0}", style.BonusToHit));

				if (style.Procs != null && style.Procs.Count > 0)
				{
					delveInfo.Add(" ");

					string procs = "";
					foreach (Tuple<Spell, int, int> spell in style.Procs)
					{
						if (procs != "")
							procs += ", ";

						procs += spell.Item1.ID;
					}

					delveInfo.Add(string.Format("Procs: {0}", procs));
					delveInfo.Add(string.Format("RandomProc: {0}", style.RandomProc));
				}
			}

		}
	}
}
