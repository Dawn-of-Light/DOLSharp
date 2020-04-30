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
using System.Reflection;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using log4net;
using DOL.AI.Brain;
using System;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell to change up to 3 property bonuses at once
	/// in one their specific given bonus category
	/// </summary>
	
	public abstract class PropertyChangingSpell : SpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Execute property changing spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = Spell.Duration;
			if (HasPositiveEffect)
			{	
				duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
				if (Spell.InstrumentRequirement != 0)
				{
					InventoryItem instrument = Caster.AttackWeapon;
					if (instrument != null)
					{
						duration *= 1.0 + Math.Min(1.0, instrument.Level / (double)Caster.Level); // up to 200% duration for songs
						duration *= instrument.Condition / (double)instrument.MaxCondition * instrument.Quality / 100;
					}
				}
				if (duration < 1)
					duration = 1;
				else if (duration > (Spell.Duration * 4))
					duration = (Spell.Duration * 4);
				return (int)duration; 
			}
			duration = base.CalculateEffectDuration(target, effectiveness);
			return (int)duration;
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// vampiir, they cannot be buffed except with resists/armor factor/ haste / power regen
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				if (HasPositiveEffect && player.CharacterClass.ID == (int)eCharacterClass.Vampiir && m_caster != player)
				{
					//restrictions
					//if (this is PropertyChangingSpell
					//    && this is ArmorFactorBuff == false
					//    && this is CombatSpeedBuff == false
					//    && this is AbstractResistBuff == false
					//    && this is EnduranceRegenSpellHandler == false
					//    && this is EvadeChanceBuff == false
					//    && this is ParryChanceBuff == false)
					//{
					if (this is StrengthBuff || this is DexterityBuff || this is ConstitutionBuff || this is QuicknessBuff || this is StrengthConBuff || this is DexterityQuiBuff || this is AcuityBuff)
					{
						GamePlayer caster = m_caster as GamePlayer;
						if (caster != null)
						{
							caster.Out.SendMessage("Your buff has no effect on the Vampiir!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						player.Out.SendMessage("This buff has no effect on you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (this is ArmorFactorBuff)
					{
						if (SpellHandler.FindEffectOnTarget(target, "ArmorFactorBuff") != null && m_spellLine.IsBaseLine != true)
						{
							MessageToLiving(target, "You already have this effect!", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
				
				if (this is HeatColdMatterBuff || this is AllMagicResistsBuff)
				{
					if (this.Spell.Frequency <= 0)
					{
						GameSpellEffect Matter = FindEffectOnTarget(player, "MatterResistBuff");
						GameSpellEffect Cold = FindEffectOnTarget(player, "ColdResistBuff");
						GameSpellEffect Heat = FindEffectOnTarget(player, "HeatResistBuff");
						if (Matter != null || Cold != null || Heat != null)
						{
							MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
				
				if (this is BodySpiritEnergyBuff || this is AllMagicResistsBuff)
				{
					if (this.Spell.Frequency <= 0)
					{
						GameSpellEffect Body = FindEffectOnTarget(player, "BodyResistBuff");
						GameSpellEffect Spirit = FindEffectOnTarget(player, "SpiritResistBuff");
						GameSpellEffect Energy = FindEffectOnTarget(player, "EnergyResistBuff");
						if (Body != null || Spirit != null || Energy != null)
						{
							MessageToCaster(target.Name + " already has this effect", eChatType.CT_SpellResisted);
							return;
						}
					}
				}
			}
			
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// start changing effect on target
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			if (!ApplyNpcEffect(effect, false))
			{
				ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(Spell.Value * effect.Effectiveness), false);
				ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(Spell.Value * effect.Effectiveness), false);
			}

			SendUpdates(effect.Owner);

			eChatType toLiving = eChatType.CT_SpellPulse;
			eChatType toOther = eChatType.CT_SpellPulse;
			if (Spell.Pulse == 0 || !HasPositiveEffect)
			{
				toLiving = eChatType.CT_Spell;
				toOther = eChatType.CT_System;
				SendEffectAnimation(effect.Owner, 0, false, 1);
			}

			GameLiving player = null;

			if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
				player = ((Caster as GameNPC).Brain as IControlledBrain).Owner;
			else if (effect.Owner is GameNPC && (effect.Owner as GameNPC).Brain is IControlledBrain)
				player = ((effect.Owner as GameNPC).Brain as IControlledBrain).Owner;

			if (player != null)
			{
				// Controlled NPC. Show message in blue writing to owner...

				MessageToLiving(player, String.Format(Spell.Message2,
				                                      effect.Owner.GetName(0, true)), toLiving);

				// ...and in white writing for everyone else.

				foreach (GamePlayer gamePlayer in effect.Owner.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
					if (gamePlayer != player)
						MessageToLiving(gamePlayer, String.Format(Spell.Message2,
						                                          effect.Owner.GetName(0, true)), toOther);
			}
			else
			{
				MessageToLiving(effect.Owner, Spell.Message1, toLiving);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
			}
			if (ServerProperties.Properties.BUFF_RANGE > 0 && effect.Spell.Concentration > 0 && effect.SpellHandler.HasPositiveEffect && effect.Owner != effect.SpellHandler.Caster)
			{
				m_buffCheckAction = new BuffCheckAction(effect.SpellHandler.Caster, effect.Owner, effect);
				m_buffCheckAction.Start(BuffCheckAction.BUFFCHECKINTERVAL);
			}
			
		}

		BuffCheckAction m_buffCheckAction = null;

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (!noMessages && Spell.Pulse == 0)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}

			if (!ApplyNpcEffect(effect, true))
			{
				ApplyBonus(effect.Owner, BonusCategory1, Property1, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory2, Property2, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory3, Property3, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory4, Property4, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory5, Property5, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory6, Property6, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory7, Property7, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory8, Property8, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory9, Property9, (int)(Spell.Value * effect.Effectiveness), true);
				ApplyBonus(effect.Owner, BonusCategory10, Property10, (int)(Spell.Value * effect.Effectiveness), true);
			}

			SendUpdates(effect.Owner);

			if (m_buffCheckAction != null)
			{
				m_buffCheckAction.Stop();
				m_buffCheckAction = null;
			}

			return base.OnEffectExpires(effect, noMessages);
		}

		protected virtual void SendUpdates(GameLiving target)
		{
		}

		protected IPropertyIndexer GetBonusCategory(GameLiving target, eBuffBonusCategory categoryid)
		{
			IPropertyIndexer bonuscat = null;
			switch (categoryid)
			{
				case eBuffBonusCategory.BaseBuff:
					bonuscat = target.BaseBuffBonusCategory;
					break;
				case eBuffBonusCategory.SpecBuff:
					bonuscat = target.SpecBuffBonusCategory;
					break;
				case eBuffBonusCategory.Debuff:
					bonuscat = target.DebuffCategory;
					break;
				case eBuffBonusCategory.Other:
					bonuscat = target.BuffBonusCategory4;
					break;
				case eBuffBonusCategory.SpecDebuff:
					bonuscat = target.SpecDebuffCategory;
					break;
				case eBuffBonusCategory.AbilityBuff:
					bonuscat = target.AbilityBonus;
					break;
				default:
					if (log.IsErrorEnabled)
						log.Error("BonusCategory not found " + categoryid + "!");
					break;
			}
			return bonuscat;
		}

		/// <summary>
		/// Property 1 which bonus value has to be changed
		/// </summary>
		public abstract eProperty Property1 { get; }

		/// <summary>
		/// Property 2 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property2
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 3 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property3
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 4 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property4
		{
			get { return eProperty.Undefined; }
		}
		/// <summary>
		/// Property 5 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property5
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 6 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property6
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 7 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property7
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 8 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property8
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 9 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property9
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Property 10 which bonus value has to be changed
		/// </summary>
		public virtual eProperty Property10
		{
			get { return eProperty.Undefined; }
		}

		/// <summary>
		/// Bonus Category where to change the Property1
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory1
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property2
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory2
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property3
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory3
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property4
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory4
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property5
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory5
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property6
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory6
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property7
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory7
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property8
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory8
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property9
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory9
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		/// <summary>
		/// Bonus Category where to change the Property10
		/// </summary>
		public virtual eBuffBonusCategory BonusCategory10
		{
			get { return eBuffBonusCategory.BaseBuff; }
		}

		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			if (!ApplyNpcEffect(effect, false))
			{
				ApplyBonus(effect.Owner, BonusCategory1, Property1, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory2, Property2, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory3, Property3, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory4, Property4, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory5, Property5, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory6, Property6, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory7, Property7, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory8, Property8, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory9, Property9, vars[1], false);
				ApplyBonus(effect.Owner, BonusCategory10, Property10, vars[1], false);
			}

			SendUpdates(effect.Owner);
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			if (!noMessages && Spell.Pulse == 0)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}

			if (!ApplyNpcEffect(effect, true))
			{
				ApplyBonus(effect.Owner, BonusCategory1, Property1, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory2, Property2, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory3, Property3, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory4, Property4, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory5, Property5, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory6, Property6, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory7, Property7, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory8, Property8, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory9, Property9, vars[1], true);
				ApplyBonus(effect.Owner, BonusCategory10, Property10, vars[1], true);
			}

			SendUpdates(effect.Owner);
			return 0;
		}

		/// <summary>
		/// Apply npc specific spell effects
		/// </summary>
		/// <param name="effect">The effect to apply</param>
		/// <param name="isSubtracted">Are we removing the effect?</param>
		/// <returns>True if we handled the effect, false if it still needs to be processed.</returns>
		protected bool ApplyNpcEffect(GameSpellEffect effect, bool isSubtracted)
		{
			/* Friday Grab Bag - 10/12/2003, repeated in 05/05/2017 Grab Bag
			 * Q: How do player concentration buffs affect pets? (ie: druid base/specs on an enchanter's pet)
			 * A: Strength buffs increase a pet’s melee damage.
			 * Constitution buffs decrease damage done to a pet.
			 * Dexterity buffs reduce a pet’s spell cast timer, decrease damage done to the pet and increase its chance to hit with archery and bolts.
			 * Quickness buffs reduce a pet’s melee attack timer. */

			/* Tested on live on 4/21/2020 with a Champion:
			 *	Confirmed Con debuffs increase melee and spell damage taken, but dex only increases melee damage taken.
			 *	Debuffs are incredibly powerful: Str/Con + Dex/Qui increase melee damage taken by 47% and increase spell damage by 23%
			 *	Live no longer allow pets to be buffed, might be an EC restriction. 
			 *
			 * If we use the same multiplier for buffs as debuffs, a fully Str, Con, Dex, Str/Con & Dex/Qui buffed pet will do
			 *	40% more melee damage, have 81% melee damage resistance, and have 40% spell resistance.  That's crazy, so we halve
			 *	the effectiveness of buffs.
			 *	
			 * Debuffs will use SpecDebuffCategory, so we can tell which buffs are applied here.
			 */

			if (!(effect.Owner is GameNPC)
				|| (effect.Owner is GamePet && ServerProperties.Properties.PET_BUFF_EFFECT_MULTIPLIER <= 0)
				|| (effect.Owner is Keeps.GameKeepGuard && ServerProperties.Properties.GUARD_BUFF_EFFECT_MULTIPLIER <= 0)
				|| (effect.Owner is GameNPC && ServerProperties.Properties.MOB_BUFF_EFFECT_MULTIPLIER <= 0))
				return false;

			GameNPC owner = effect.Owner as GameNPC;
			int percent;

			// Only calculate the buff/debuff percentages when we absolutely have to
			void CalculatePercentage(bool specCap)
			{
				double multiplier;
				if (owner is GamePet)
					multiplier = ServerProperties.Properties.PET_BUFF_EFFECT_MULTIPLIER;
				else if (owner is Keeps.GameKeepGuard)
					multiplier = ServerProperties.Properties.GUARD_BUFF_EFFECT_MULTIPLIER;
				else
					multiplier = ServerProperties.Properties.MOB_BUFF_EFFECT_MULTIPLIER;

				// We have to cap both buffs and debuffs, or bad things happen with high level buffs/debuffs on low level NPCs.
				double cap;
				if (specCap)
					cap = owner.Level * 1.5;
				else
					cap = owner.Level;

				if (cap < Spell.Value)
					percent = (int)(multiplier * cap / owner.Level * effect.Effectiveness);
				else
					percent = (int)(multiplier * Spell.Value / owner.Level * effect.Effectiveness);

				if (Spell.IsHelpful) // Halve cap buffs
					percent = percent >> 1;

				if (isSubtracted)
					percent = -percent;
			} // CalculatePercentage()

			switch (this)
			{
				case ArmorFactorBuff _:
					// Only baseline AF buffs are allowed on pets, specifically because cleric 
					//	base+spec AF buffs were making necro pets nigh indestructible on live.
					// See 07/06/2018 Grab Bag for details on why caster AF self buffs are so high.  NPCs
					//	already have a high base AF, so we cap caster self AF buffs on them as well.
					if (Spell.EffectGroup == 1)
					{
						if (Spell.Value <= owner.Level)
							owner.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] += (int)(Spell.Value * effect.Effectiveness);
						else
							owner.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] += (int)(owner.Level * effect.Effectiveness);
					}
					return true;
				// Let ArmorFactorDebuffs apply normally.

				case StrengthBuff _:
					CalculatePercentage(false);
					owner.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] += percent;
					return true;
				case StrengthDebuff _:
					CalculatePercentage(false);
					owner.SpecDebuffCategory[(int)eProperty.MeleeDamage] += percent;
					return true;

				case ConstitutionBuff _:
					CalculatePercentage(false);
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;
				case ConstitutionDebuff _:
					CalculatePercentage(false);
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.SpecDebuffCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;

				case StrengthConBuff _:
					CalculatePercentage(true);
					owner.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;
				case StrengthConDebuff _:
					CalculatePercentage(true);
					owner.SpecDebuffCategory[(int)eProperty.MeleeDamage] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.SpecDebuffCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;

				// Dex also affects cast speed and ranged weapon skill, so we need to apply it normally as well.
				case DexterityBuff _:
					if (isSubtracted)
						owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] -= (int)(Spell.Value * effect.Effectiveness);
					else
						owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] += (int)(Spell.Value * effect.Effectiveness);
					CalculatePercentage(false);
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += percent;
					return true;
				case DexterityDebuff _:
					if (isSubtracted)
						owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)(Spell.Value * effect.Effectiveness);
					else
						owner.DebuffCategory[(int)eProperty.Dexterity] += (int)(Spell.Value * effect.Effectiveness);
					CalculatePercentage(false);
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += percent;
					return true;

				case DexterityQuiBuff _:
					if (isSubtracted)
						owner.SpecBuffBonusCategory[(int)eProperty.Dexterity] -= (int)(Spell.Value * effect.Effectiveness);
					else
						owner.SpecBuffBonusCategory[(int)eProperty.Dexterity] += (int)(Spell.Value * effect.Effectiveness);
					CalculatePercentage(true);
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.BaseBuffBonusCategory[(int)eProperty.MeleeSpeed] += percent;
					return true;
				case DexterityQuiDebuff _:
					if (isSubtracted)
						owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)(Spell.Value * effect.Effectiveness);
					else
						owner.DebuffCategory[(int)eProperty.Dexterity] += (int)(Spell.Value * effect.Effectiveness);
					CalculatePercentage(true);
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.SpecDebuffCategory[(int)eProperty.MeleeSpeed] += percent;
					return true;

				case DexterityConDebuff _:
					CalculatePercentage(true);
					if (percent > 0)
						owner.DebuffCategory[(int)eProperty.Dexterity] += (int)(Spell.Value * effect.Effectiveness);
					else
						owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)(Spell.Value * effect.Effectiveness);
					// Melee resist gets reduced by both Con and Dex
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += 2 * percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += 2 * percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += 2 * percent;
					owner.SpecDebuffCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;

				case WeaponskillConDebuff _:
					CalculatePercentage(true);
					owner.SpecDebuffCategory[(int)eProperty.WeaponSkill] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Crush] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Slash] += percent;
					owner.SpecDebuffCategory[(int)eProperty.Resist_Thrust] += percent;
					owner.SpecDebuffCategory[(int)eProperty.MagicAbsorption] += percent;
					return true;
			}

			return false;
		}

		/// <summary>
		/// Method used to apply bonuses
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="BonusCat"></param>
		/// <param name="Property"></param>
		/// <param name="Value"></param>
		/// <param name="IsSubstracted"></param>
		protected void ApplyBonus(GameLiving owner,  eBuffBonusCategory BonusCat, eProperty Property, int Value, bool IsSubstracted)
		{
			IPropertyIndexer tblBonusCat;
			if (Property != eProperty.Undefined)
			{
				tblBonusCat = GetBonusCategory(owner, BonusCat);
				if (IsSubstracted)
					tblBonusCat[(int)Property] -= Value;
				else
					tblBonusCat[(int)Property] += Value;
			}
		}
		
		public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
		{
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.Var2 = (int)(Spell.Value * e.Effectiveness);
			eff.SpellLine = SpellLine.KeyName;
			return eff;

		}

		// constructor
		public PropertyChangingSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}
	}

	public class BuffCheckAction : RegionAction
	{
		public const int BUFFCHECKINTERVAL = 60000;//60 seconds

		private GameLiving m_caster = null;
		private GameLiving m_owner = null;
		private GameSpellEffect m_effect = null;

		public BuffCheckAction(GameLiving caster, GameLiving owner, GameSpellEffect effect)
			: base(caster)
		{
			m_caster = caster;
			m_owner = owner;
			m_effect = effect;
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected override void OnTick()
		{
			if (m_caster == null ||
			    m_owner == null ||
			    m_effect == null)
				return;

			if ( !m_caster.IsWithinRadius( m_owner, ServerProperties.Properties.BUFF_RANGE ) )
				m_effect.Cancel(false);
			else
				Start(BUFFCHECKINTERVAL);
		}
	}
}
