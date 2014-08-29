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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Buffs a single stat,
    /// considered as a baseline buff (regarding the bonuscategories on statproperties)
    /// // TODO does effectiveness have effect on buffs ?
    /// </summary>
    public abstract class SingleStatBuff : PropertyChangingSpell
    {
        /// <summary>
        /// Base Bonus Category for Single Buffs
        /// </summary>
        public override int BonusCategory1 { get { return (int)eBuffBonusCategory.BaseBuff; } }

        /// <summary>
		/// Holds the range Helper in case of limited range check
		/// </summary>
		protected EffectRangeCheckHelper m_rangeHelper;
        
        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
            GamePlayer player = target as GamePlayer;	// need new prop system to not worry about updates
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            }

            if (target.IsAlive)
            {
                if (target.Health < target.MaxHealth)
                {
                	target.StartHealthRegeneration();
                }
                else if (target.Health > target.MaxHealth)
                {
                	target.Health = target.MaxHealth;
                }

                if (target.Mana < target.MaxMana)
                {
                	target.StartPowerRegeneration();
                }
                else if (target.Mana > target.MaxMana)
                {
                	target.Mana = target.MaxMana;
                }

                if (target.Endurance < target.MaxEndurance)
                {
                	target.StartEnduranceRegeneration();
                }
                else if (target.Endurance > target.MaxEndurance)
                {
                	target.Endurance = target.MaxEndurance;
                }
            }
        }

		public override void OnEffectStart(GameSpellEffect effect)
		{
			// Start Range Check Helper ?
			if (m_rangeHelper != null)
			{
				m_rangeHelper.OnEffectStart(effect);
			}
			
			base.OnEffectStart(effect);
		}
        
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
        	// if already Expired exit.
        	if (m_rangeHelper != null)
        	{
        		if (m_rangeHelper.IsAlreadyExpired(effect))
        			return 0;
        	}			
			return base.OnEffectExpires(effect, noMessages);
		}
		
        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public override bool IsOverwritable(GameSpellEffect compare)
        {
        	// TODO this is already done in Base...
            if (Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            
            if (base.IsOverwritable(compare) == false)
            	return false;
            
            if (Spell.Duration > 0 && compare.Concentration > 0)
                return compare.Spell.Value >= Spell.Value;
            
            return compare.SpellHandler.SpellLine.IsBaseLine == SpellLine.IsBaseLine;
        }


        // constructor
        public SingleStatBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        	// Range Check
        	if (Spell.AmnesiaChance > 0)
        	{
        		m_rangeHelper = new EffectRangeCheckHelper(this, Spell.AmnesiaChance);
        	}
        }
    }
    
    /// <summary>
    /// Buff Classes for Base Stats that allows ToA Bonuses and effectiveness spec bonus
    /// </summary>
    public abstract class SingleBaseStatBuff : SingleStatBuff
    {
		public override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			// Caster Spec Bonus
			int specLevel = Caster.GetModifiedSpecLevel(SpellLine.Spec);
			
			if (Caster is GamePlayer && specLevel > 0)
			{
				effectiveness += (Math.Max(0, Math.Min(1, (specLevel - 1.0) / Spell.Level)) * 0.5) - 0.25;
			}
			
			// Caster ToA Bonus
			effectiveness *= (1.0 + Caster.GetModified(eProperty.BuffEffectiveness) * 0.01);
			
			return base.CreateSpellEffect(target, effectiveness);
		}
    	
    	// constructor
        public SingleBaseStatBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }

    }

    /// <summary>
    /// Str stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("StrengthBuff")]
    public class StrengthBuff : SingleBaseStatBuff
    {
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirStrength))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                OnFailedApplyEffectOnTarget(target, null, null);
                return;
            }
            
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        
        public override eProperty Property1 { get { return eProperty.Strength; } }

        // constructor
        public StrengthBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Dex stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("DexterityBuff")]
    public class DexterityBuff : SingleBaseStatBuff
    {
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirDexterity))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                OnFailedApplyEffectOnTarget(target, null, null);
                return;
            }
            
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        
        public override eProperty Property1 { get { return eProperty.Dexterity; } }

        // constructor
        public DexterityBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Con stat baseline buff
    /// </summary>
    [SpellHandlerAttribute("ConstitutionBuff")]
    public class ConstitutionBuff : SingleBaseStatBuff
    {
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirConstitution))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                OnFailedApplyEffectOnTarget(target, null, null);
                return;
            }
            
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        
        public override eProperty Property1 { get { return eProperty.Constitution; } }

        // constructor
        public ConstitutionBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// Quickness buff
    /// </summary>
    [SpellHandlerAttribute("QuicknessBuff")]
    public class QuicknessBuff : SingleBaseStatBuff
    {
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirQuickness))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                OnFailedApplyEffectOnTarget(target, null, null);
                return;
            }
            
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        
        public override eProperty Property1 { get { return eProperty.Quickness; } }

        // constructor
        public QuicknessBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Acuity buff (spec Category)
    /// </summary>
    [SpellHandlerAttribute("AcuityBuff")]
    public class AcuityBuff : SingleBaseStatBuff
    {
        public override eProperty Property1 { get { return eProperty.Acuity; } }
        public override int BonusCategory1 { get { return (int)eBuffBonusCategory.SpecBuff; } }

        // constructor
        public AcuityBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Armor factor buff
    /// </summary>
    [SpellHandlerAttribute("ArmorFactorBuff")]
    public class ArmorFactorBuff : SingleBaseStatBuff
    {
        public override int BonusCategory1
        {
            get
            {
                if (SpellLine.IsBaseLine)
                	return (int)eBuffBonusCategory.BaseBuff; // baseline cap
                
                return (int)eBuffBonusCategory.SpecBuff; // caps for spec line buffs
            }
        }
        
        public override eProperty Property1 { get { return eProperty.ArmorFactor; } }

		public override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			// Self caster buff (and hybrid) should have no variance nor spec bonuses...
			if (SpellLine.IsBaseLine && Spell.Target.ToLower() == "self")
			{
				effectiveness *= (1.0 + Caster.GetModified(eProperty.BuffEffectiveness) * 0.01);
				return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
			}
			
			return base.CreateSpellEffect(target, effectiveness);
		}
        
		/// <summary>
		/// AF buff overwrite on same spec type. and base is overwritable with Other...
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
        public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (compare.SpellHandler is ArmorFactorBuff)
			{
				ArmorFactorBuff newAF = ((ArmorFactorBuff)compare.SpellHandler);
				if ((newAF.BonusCategory1 == (int)eBuffBonusCategory.BaseBuff && BonusCategory1 == (int)eBuffBonusCategory.BaseBuff)
				    || (newAF.BonusCategory1 == (int)eBuffBonusCategory.SpecBuff && BonusCategory1 == (int)eBuffBonusCategory.SpecBuff))
				{
					return true;
				}
			}
			
			return base.IsOverwritable(compare);
		}
        
        // constructor
        public ArmorFactorBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// Paladin Self Armor factor buff (is Specline but act as Baseline)
    /// </summary>
    [SpellHandlerAttribute("PaladinArmorFactorBuff")]
    public class PaladinArmorFactorBuff : ArmorFactorBuff
    {
        public override int BonusCategory1
        {
            get
            {
            	// Base line
            	return (int)eBuffBonusCategory.BaseBuff;
            }
        }
        
        public override eProperty Property1 { get { return eProperty.ArmorFactor; } }

        // constructor
        public PaladinArmorFactorBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }


    /// <summary>
    /// Armor Absorption buff
    /// </summary>
    [SpellHandlerAttribute("ArmorAbsorptionBuff")]
    public class ArmorAbsorptionBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ArmorAbsorption; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public ArmorAbsorptionBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Combat speed buff
    /// </summary>
    [SpellHandlerAttribute("CombatSpeedBuff")]
    public class CombatSpeedBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // TODO Identify correctly Haste and Celerity, allow for stacking with no effect for short buff ?
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            if (compare.Spell.SpellType != Spell.SpellType) return false;
            if (compare.Spell.Target != Spell.Target) return false;//Celerity buff stacks with conc one
            return compare.SpellHandler.SpellLine.IsBaseLine == SpellLine.IsBaseLine;
        }

        // constructor
        public CombatSpeedBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Fatigue reduction buff
    /// </summary>
    [SpellHandlerAttribute("FatigueConsumptionBuff")]
    public class FatigueConsumptionBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.FatigueConsumption; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public FatigueConsumptionBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Melee damage buff
    /// </summary>
    [SpellHandlerAttribute("MeleeDamageBuff")]
    public class MeleeDamageBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MeleeDamage; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public MeleeDamageBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Mesmerize duration buff
    /// </summary>
    [SpellHandlerAttribute("MesmerizeDurationBuff")]
    public class MesmerizeDurationBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MesmerizeDurationReduction; } }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected override void SendUpdates(GameLiving target)
        {
        }

        // constructor
        public MesmerizeDurationBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// DPS buff
    /// </summary>
    [SpellHandlerAttribute("DPSBuff")]
    public class DPSBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.DPS; } }

        // constructor
        public DPSBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// Evade chance buff
    /// </summary>
    [SpellHandlerAttribute("EvadeBuff")]
    public class EvadeChanceBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.EvadeChance; } }

        // constructor
        public EvadeChanceBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// Parry chance buff
    /// </summary>
    [SpellHandlerAttribute("ParryBuff")]
    public class ParryChanceBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ParryChance; } }

        // constructor
        public ParryChanceBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// WeaponSkill buff
    /// </summary>
    [SpellHandlerAttribute("WeaponSkillBuff")]
    public class WeaponSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.WeaponSkill; } }

        // constructor
        public WeaponSkillBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// To Hit buff
    /// </summary>
    [SpellHandlerAttribute("ToHitBuff")]
    public class ToHitSkillBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ToHitBonus; } }

        // constructor
        public ToHitSkillBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// Magic Resists Buff
    /// </summary>
    [SpellHandlerAttribute("MagicAbsorptionBuff")]
    public class MagicAbsorptionBuffHandler : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.MagicAbsorption; } }

        // constructor
        public MagicAbsorptionBuffHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    [SpellHandlerAttribute("StyleAbsorbBuff")]
    public class StyleAbsorbBuff : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.StyleAbsorb; } }
        
        public StyleAbsorbBuff(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    [SpellHandlerAttribute("ExtraHP")]
    public class ExtraHP : SingleStatBuff
    {
        public override eProperty Property1 { get { return eProperty.ExtraHP; } }
        public ExtraHP(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
}