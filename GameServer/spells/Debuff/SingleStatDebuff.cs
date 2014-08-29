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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Debuffs a single stat
	/// </summary>
	public abstract class SingleStatDebuff : PropertyChangingSpell
	{
		/// <summary>
		/// Bonus Category Debuff
		/// </summary>
		public override int BonusCategory1 { get { return (int)eBuffBonusCategory.Debuff; } }

		/// <summary>
		/// Debuff don't have positive effect.
		/// </summary>
		public override bool HasPositiveEffect {
			get { return false; }
		}
		
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
        
		public override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			// Effectiveness Based on Caster Spec Bonus
			int specLevel = Caster.GetModifiedSpecLevel(SpellLine.Spec);
			
			if (specLevel > 0)
			{
				effectiveness = Math.Max(0, effectiveness + (Math.Max(0, Math.Min(1, (specLevel - 1.0) / Spell.Level)) * 0.5) - 0.25);
			}
			
			// Apply Bonus to Debuff Effectiveness (value)
			effectiveness *= (1.0 + Caster.GetModified(eProperty.DebuffEffectivness) * 0.01);
			
			return base.CreateSpellEffect(target, effectiveness);
		}
        
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			
			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
			
			if(target is GameNPC) 
			{
				IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
			
			// Casted Debuff interrupts
			if(Spell.CastTime > 0)
				target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			// Effectiveness doesn't reduce/improve Debuff duration but debuff value !
			int duration = base.CalculateEffectDuration(target, 1.0);
			duration = (int)(duration * GetResistBaseRatio(target, effectiveness));

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			
			return duration;
		}
		
		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>		
        public override int CalculateSpellResistChance(GameLiving target)
        {
        	int basechance =  base.CalculateSpellResistChance(target);
 			GameSpellEffect rampage = SpellHelper.FindEffectOnTarget(target, "Rampage");
            if (rampage != null)
            {
            	basechance += (int)rampage.Spell.Value;
            }
            return Math.Min(100, basechance);
        }
        
        /// <summary>
        /// Debuff is overwritable by debuff of same type.
        /// Or Composite Spells with Debuff of same type...
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (!base.IsOverwritable(compare))
			{
				if (!compare.SpellHandler.GetType().IsInstanceOfType(this) && !this.GetType().IsInstanceOfType(compare.SpellHandler)
				    && !(compare.SpellHandler is DirectDamageDebuffSpellHandler && ((DirectDamageDebuffSpellHandler)compare.SpellHandler).Property == Property1))
					return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Debuff with better value should overwrite !
		/// No matter remaining time...
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns></returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			if (!base.IsNewEffectBetter(oldeffect, neweffect))
			{
				if ((oldeffect.Spell.Value * oldeffect.Effectiveness) < (neweffect.Spell.Value * neweffect.Effectiveness) && !oldeffect.Spell.IsConcentration)
				{
					return true;
				}
				
				return false;
			}
			
			return true;
		}
        
		// constructor
		public SingleStatDebuff(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
	
	/// <summary>
	/// Str stat baseline debuff
	/// </summary>
	[SpellHandlerAttribute("StrengthDebuff")]
	public class StrengthDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Strength; } }

		// constructor
		public StrengthDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Dex stat baseline debuff
	/// </summary>
	[SpellHandlerAttribute("DexterityDebuff")]
	public class DexterityDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Dexterity; } }	

		// constructor
		public DexterityDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Con stat baseline debuff
	/// </summary>
	[SpellHandlerAttribute("ConstitutionDebuff")]
	public class ConstitutionDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Constitution; } }	

		// constructor
		public ConstitutionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Armor factor debuff
	/// </summary>
	[SpellHandlerAttribute("ArmorFactorDebuff")]
	public class ArmorFactorDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.ArmorFactor; } }	

		// constructor
		public ArmorFactorDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Armor Absorption debuff
	/// </summary>
	[SpellHandlerAttribute("ArmorAbsorptionDebuff")]
	public class ArmorAbsorptionDebuff : SingleStatDebuff
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
		public ArmorAbsorptionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Combat Speed debuff
	/// </summary>
	[SpellHandlerAttribute("CombatSpeedDebuff")]
	public class CombatSpeedDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }      
		
		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
		}

		// constructor
		public CombatSpeedDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Melee damage debuff
	/// </summary>
	[SpellHandlerAttribute("MeleeDamageDebuff")]
	public class MeleeDamageDebuff : SingleStatDebuff
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
		public MeleeDamageDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Fatigue reduction debuff
	/// </summary>
	[SpellHandlerAttribute("FatigueConsumptionDebuff")]
	public class FatigueConsumptionDebuff : SingleStatDebuff
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
		public FatigueConsumptionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Fumble chance debuff
	/// </summary>
	[SpellHandlerAttribute("FumbleChanceDebuff")]
	public class FumbleChanceDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.FumbleChance; } }      
		
		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
		}

		// constructor
		public FumbleChanceDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	
	/// <summary>
	/// DPS debuff
	/// </summary>
	[SpellHandlerAttribute("DPSDebuff")]
	public class DPSDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.DPS; } }	

		// constructor
		public DPSDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Skills Debuff
	/// </summary>
	[SpellHandlerAttribute("SkillsDebuff")]
	public class SkillsDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.AllSkills; } }	

		// constructor
		public SkillsDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Acuity stat baseline debuff
	/// </summary>
	[SpellHandlerAttribute("AcuityDebuff")]
	public class AcuityDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Acuity; } }	

		// constructor
		public AcuityDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Quickness stat baseline debuff
	/// </summary>
	[SpellHandlerAttribute("QuicknessDebuff")]
	public class QuicknessDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Quickness; } }	

		// constructor
		public QuicknessDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// ToHit Skill debuff
	/// </summary>
	[SpellHandlerAttribute("ToHitDebuff")]
	public class ToHitSkillDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.ToHitBonus; } }	

		// constructor
		public ToHitSkillDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
 }
