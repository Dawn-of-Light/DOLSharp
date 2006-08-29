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

using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Scripts;

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
					case Style.eAttackResult.Any: requiredAttackResult = GameLiving.eAttackResult.Any; break;
					case Style.eAttackResult.Block: requiredAttackResult = GameLiving.eAttackResult.Blocked; break;
					case Style.eAttackResult.Evade: requiredAttackResult = GameLiving.eAttackResult.Evaded; break;
					case Style.eAttackResult.Fumble: requiredAttackResult = GameLiving.eAttackResult.Fumbled; break;
					case Style.eAttackResult.Hit: requiredAttackResult = GameLiving.eAttackResult.HitUnstyled; break;
					case Style.eAttackResult.Style: requiredAttackResult = GameLiving.eAttackResult.HitStyle; break;
					case Style.eAttackResult.Miss: requiredAttackResult = GameLiving.eAttackResult.Missed; break;
					case Style.eAttackResult.Parry: requiredAttackResult = GameLiving.eAttackResult.Parried; break;
				}

				AttackData lastAD = (AttackData)living.TempProperties.getObjectProperty(GameLiving.LAST_ATTACK_DATA, null);

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
						AttackData targetsLastAD = (AttackData)target.TempProperties.getObjectProperty(GameLiving.LAST_ATTACK_DATA, null);

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
						// get players angle on target
						float angle = target.GetAngleToTarget(living);
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
						player.Out.SendMessage("You can't enter combat mode while lying down!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}
				//Can't use styles with range weapon
				if (living.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
				{
					if (player != null)
						player.Out.SendMessage("You can't enter melee combat mode with a fired weapon!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
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
						player.Out.SendMessage("You must have a target for your next attack style!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				InventoryItem weapon = (style.WeaponTypeRequirement == (int)eObjectType.Shield) ? living.Inventory.GetItem(eInventorySlot.LeftHandWeapon) : living.AttackWeapon;
				//				if (weapon == null) return;	// no weapon = no style
				if (!CheckWeaponType(style, living, weapon))
				{
					if (player != null)
					{
						if (style.WeaponTypeRequirement == Style.SpecialWeaponType.DualWield)
							player.Out.SendMessage("You must be dual wielding to use this style!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else
							player.Out.SendMessage("This style requires a " + style.GetRequiredWeaponName() + " weapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}

				if (player != null) //Do mob use endurance?
				{
					int fatCost = CalculateEnduranceCost(player, style, weapon.SPD_ABS);
					if (player.Endurance < fatCost)
					{
						player.Out.SendMessage("You are too fatigued to use this style!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
				}

				if (player != null)
				{
					Style preRequireStyle = null;
					if (style.OpeningRequirementType == Style.eOpening.Offensive && style.AttackResultRequirement == Style.eAttackResult.Style)
						preRequireStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, player.CharacterClass.ID);

					//We have not set any primary style yet?
					if (player.NextCombatStyle == null)
					{
						if (preRequireStyle != null)
						{
							AttackData lastAD = (AttackData)living.TempProperties.getObjectProperty(GameLiving.LAST_ATTACK_DATA, null);
							if (lastAD == null
							|| lastAD.AttackResult != GameLiving.eAttackResult.HitStyle
							|| lastAD.Style == null
							|| lastAD.Style.ID != style.OpeningRequirementValue)
							{
								player.Out.SendMessage("You must perform the " + preRequireStyle.Name + " style before this one!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						}

						player.NextCombatStyle = style;
						player.NextCombatBackupStyle = null;
						player.Out.SendMessage("You prepare to perform a " + style.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

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
							player.Out.SendMessage("You have already selected your styles for this round!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							//Have we pressed the same style button used for the primary style again?
							if (player.NextCombatStyle.ID == style.ID)
							{
								if (player.CancelStyle)
								{
									//If yes, we cancel the style
									player.Out.SendMessage("You are no longer preparing to use your " + player.NextCombatStyle.Name + " style!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									player.NextCombatStyle = null;
									player.NextCombatBackupStyle = null;
								}
								else
								{
									player.Out.SendMessage("You are already preparing to use this style!  Type /cancelstyle to enable style cancelling.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
							else
							{
								if (preRequireStyle != null)
								{
									AttackData lastAD = (AttackData)living.TempProperties.getObjectProperty(GameLiving.LAST_ATTACK_DATA, null);
									if (lastAD == null
									|| lastAD.AttackResult != GameLiving.eAttackResult.HitStyle
									|| lastAD.Style == null
									|| lastAD.Style.ID != style.OpeningRequirementValue)
									{
										player.Out.SendMessage("You must perform the " + preRequireStyle.Name + " style before this one!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return;
									}
								}
								//If no, set the secondary backup style
								player.NextCombatBackupStyle = style;
								player.Out.SendMessage("You are now preparing to perform a " + style.Name + " style as a backup for " + player.NextCombatStyle.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
						player.Out.SendMessage("You fail to execute your " + attackData.Style.Name + " perfectly!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					}
					return false;
				}
				else
				{
					double effectivness = 1.0;
					if (attackData.Target is GameNPC && player != null)
					{
						IControlledBrain brain = ((GameNPC)attackData.Target).Brain as IControlledBrain;
						if (brain == null)
							effectivness *= 2;
					}
					//Style worked! Print out some nice info and add the damage! :)
					//Growth Rate * Style Spec * Effective Speed / Unstyled Damage Cap
					int styleddamagecap = (int)((living.UnstyledDamageCap(weapon) + (attackData.Style.GrowthRate * living.GetModifiedSpecLevel(attackData.Style.Spec) * living.AttackSpeed(weapon) * 0.001)) * effectivness);
					double factor = (attackData.Style.GrowthRate * living.GetModifiedSpecLevel(attackData.Style.Spec) * living.AttackSpeed(weapon) * 0.001) / living.UnstyledDamageCap(weapon);
					attackData.StyleDamage = (int)Math.Max(1, attackData.UncappedDamage * factor);
					attackData.StyleDamage = (int)(attackData.StyleDamage * living.GetModified(eProperty.StyleDamage) / 100.0);
					//					log.Debug("unstyled cap="+player.UnstyledDamageCap(weapon)+"  factor="+factor+"  weapon spec="+player.WeaponSpecLevel(weapon)+"  attack speed="+player.AttackSpeed(weapon)+"  GR="+attackData.Style.GrowthRate);
					//Increase regular damage by styledamage ... like on live servers
					attackData.Damage += attackData.StyleDamage;

					// apply styled damage cap
					attackData.Damage = Math.Min(attackData.Damage, styleddamagecap);

					//critical strike for rogue
					//TODO: remove this  from style processor and make a real critical shot
					//Starting damage are
					//PA: 75 + (CSSkill * 9) + Damage Cap 
					//BSII: 45 + (CSSkill * 6) + Damage Cap 
					//BS: 5 + (CSSkill * 14 / 3) + Damage Cap
					if (attackData.Style.Name == ("Backstab"))
					{
						player.Out.SendMessage("You inflict a critical strike in the back of your target  ", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						attackData.Damage += 5 + (player.GetModifiedSpecLevel(Specs.Critical_Strike) * 14 / 3);
					}
					else if (attackData.Style.Name == ("Backstab II"))
					{
						player.Out.SendMessage("You inflict a critical strike in the back of your target  ", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						attackData.Damage += 45 + (player.GetModifiedSpecLevel(Specs.Critical_Strike) * 6);
					}
					else if (attackData.Style.Name == ("Perforate Artery"))
					{
						player.Out.SendMessage("You inflict a critical strike in the throat of your target  ", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						attackData.Damage += 75 + (player.GetModifiedSpecLevel(Specs.Critical_Strike) * 9);
					}

					if (player != null)
					{
						// reduce players endurance
						player.Endurance -= fatCost;
						string damageAmount = (attackData.StyleDamage > 0) ? " (+" + attackData.StyleDamage + ")" : "";
						player.Out.SendMessage("You perform your " + attackData.Style.Name + " perfectly!" + damageAmount, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					}

					if (living is GameNPC)
					{
						ControlledNpc brain = ((GameNPC)living).Brain as ControlledNpc;
						if (brain != null && brain.Owner != null && brain.Owner.ControlledNpc == living)
						{
							string damageAmount = (attackData.StyleDamage > 0) ? " (+" + attackData.StyleDamage + ")" : "";
							brain.Owner.Out.SendMessage("Your " + living.Name + " performs " + attackData.Style.Name + " perfectly!" + damageAmount, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
					}

					// Effects on target! Eg.
					if (attackData.Style.SpecialValue != 0)
					{
						switch (attackData.Style.SpecialType)
						{
							case Style.eSpecialType.Effect:
								attackData.StyleEffect = CreateMagicEffect(living, attackData.Target, attackData.Style.SpecialValue);
								break;

							case Style.eSpecialType.Taunt:
								if (attackData.Target is GameNPC)
								{
									IAggressiveBrain aggroBrain = ((GameNPC)attackData.Target).Brain as IAggressiveBrain;
									if (aggroBrain != null)
										aggroBrain.AddToAggroList(living, attackData.Damage * attackData.Style.SpecialValue * living.AttackSpeed(weapon) / 4000);
								}
								break;

							default:
								if (log.IsWarnEnabled)
									log.Warn(string.Format("Unknown style special type: style={0} (id={1}), special={2})", attackData.Style.Name, attackData.Style.ID, attackData.Style.SpecialType));
								break;
						}
					}

					if (weapon != null)
						attackData.AnimationId = (weapon.Hand != 1) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
					else if (living.Inventory != null)
						attackData.AnimationId = (living.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null) ? attackData.Style.Icon : attackData.Style.TwoHandAnimation; // special animation for two-hand
					else
						attackData.AnimationId = attackData.Style.Icon;


					return true;
				}
			}
		}

		/// <summary>
		/// Calculates endurance needed to use style
		/// </summary>
		/// <param name="style">The style to be used</param>
		/// <param name="weaponSpd">The weapon speed</param>
		/// <returns>Endurance needed to use style</returns>
		public static int CalculateEnduranceCost(GameLiving living, Style style, int weaponSpd)
		{
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
					if (rightHand == null) return false;
					if (leftHand == null) return false;
					if (rightHand.Item_Type != Slot.RIGHTHAND && rightHand.Item_Type != Slot.LEFTHAND) return false;
					if (style.Spec == Specs.HandToHand
						&& (rightHand.Object_Type != (int)eObjectType.HandToHand
						|| leftHand.Object_Type != (int)eObjectType.HandToHand))
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

			IList spells = SkillBase.GetSpellList(styleLine.KeyName);
			if (spells == null) return null;

			Spell styleSpell = null;
			foreach (Spell spell in spells)
			{
				if (spell.ID == spellID)
				{
					styleSpell = spell;
					break;
				}
			}
			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(caster, styleSpell, styleLine);
			if (spellHandler == null && styleSpell != null && caster is GamePlayer)
			{
				((GamePlayer)caster).Out.SendMessage(styleSpell.Name + " not implemented yet (" + styleSpell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return spellHandler;
		}
	}
}
