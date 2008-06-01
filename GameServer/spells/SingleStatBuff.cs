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
	/// </summary>
	public abstract class SingleStatBuff : PropertyChangingSpell
	{
		// bonus category
		public override int BonusCategory1 { get { return 1; } }

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;	// need new prop system to not worry about updates
			if (player != null) {
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
			}

			if (target.IsAlive)
			{
				if (target.Health < target.MaxHealth) target.StartHealthRegeneration();
				else if (target.Health > target.MaxHealth) target.Health = target.MaxHealth;

				if (target.Mana < target.MaxMana) target.StartPowerRegeneration();
				else if (target.Mana > target.MaxMana) target.Mana = target.MaxMana;

				if (target.Endurance < target.MaxEndurance) target.StartEnduranceRegeneration();
				else if (target.Endurance > target.MaxEndurance) target.Endurance = target.MaxEndurance;
			}
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ClassType != eClassType.ListCaster && Spell.Level > 0 && ((GamePlayer)Caster).CharacterClass.ID!=(int)eCharacterClass.Savage)
			{
				int specLevel = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
				effectiveness = 0.75;
				if (Spell.Level > 0)
				{
					effectiveness += (specLevel-1.0) * 0.5 / Spell.Level;
					effectiveness = Math.Max(0.75, effectiveness);
					effectiveness = Math.Min(1.25, effectiveness);
					if (m_spell.Target=="Enemy") //debuff
						effectiveness *= (1.0 + m_caster.GetModified(eProperty.DebuffEffectivness)* 0.01);
					else //buff
						effectiveness *= (1.0 + m_caster.GetModified(eProperty.BuffEffectiveness)* 0.01);
				}
			}
			else
			{
				effectiveness = 1.0;
			}

			base.ApplyEffectOnTarget(target, effectiveness);
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
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (base.IsOverwritable(compare) == false) return false;
			if (Spell.Duration > 0 && compare.Concentration > 0)
				return compare.Spell.Value >= Spell.Value;
			return compare.SpellHandler.SpellLine.IsBaseLine ==
				SpellLine.IsBaseLine;
		}


		// constructor
		public SingleStatBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Str stat baseline buff
	/// </summary>
	[SpellHandlerAttribute("StrengthBuff")]
	public class StrengthBuff : SingleStatBuff
	{
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirStrength))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		public override eProperty Property1 { get { return eProperty.Strength; } }

		// constructor
		public StrengthBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Dex stat baseline buff
	/// </summary>
	[SpellHandlerAttribute("DexterityBuff")]
	public class DexterityBuff : SingleStatBuff
	{
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirDexterity))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		public override eProperty Property1 { get { return eProperty.Dexterity; } }

		// constructor
		public DexterityBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Con stat baseline buff
	/// </summary>
	[SpellHandlerAttribute("ConstitutionBuff")]
	public class ConstitutionBuff : SingleStatBuff
	{
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.VampiirConstitution))
            {
                MessageToCaster("Your target already has an effect of that type!", eChatType.CT_Spell);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
		public override eProperty Property1 { get { return eProperty.Constitution; } }

		// constructor
		public ConstitutionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Armor factor buff
	/// </summary>
	[SpellHandlerAttribute("ArmorFactorBuff")]
	public class ArmorFactorBuff : SingleStatBuff
	{
		public override int BonusCategory1 {
			get {
				if (Spell.Target == "Self") return 4; // no caps for self buffs
				if (m_spellLine.IsBaseLine) return 1; // baseline cap
				return 4; // no caps for spec line buffs
			}
		}
		public override eProperty Property1 { get { return eProperty.ArmorFactor; } }

		// constructor
		public ArmorFactorBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Armor absorbtion buff
	/// </summary>
	[SpellHandlerAttribute("ArmorAbsorbtionBuff")]
	public class ArmorAbsorbtionBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ArmorAbsorbtion; } }

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
		}

		// constructor
		public ArmorAbsorbtionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
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

		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if(compare.Spell.SpellType != Spell.SpellType) return false;
			if(compare.Spell.Target != Spell.Target) return false;//Celerity buff stacks with conc one
			return compare.SpellHandler.SpellLine.IsBaseLine == SpellLine.IsBaseLine;
		}

		// constructor
		public CombatSpeedBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
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
		public FatigueConsumptionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
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
		public MeleeDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Mesmerize duration buff
	/// </summary>
	[SpellHandlerAttribute("MesmerizeDurationBuff")]
	public class MesmerizeDurationBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MesmerizeDuration; } }

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
		}

		// constructor
		public MesmerizeDurationBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}


	/// <summary>
	/// Acuity buff
	/// </summary>
	[SpellHandlerAttribute("AcuityBuff")]
	public class AcuityBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.Acuity; } }

		// constructor
		public AcuityBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Quickness buff
	/// </summary>
	[SpellHandlerAttribute("QuicknessBuff")]
	public class QuicknessBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.Quickness; } }

		// constructor
		public QuicknessBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// DPS buff
	/// </summary>
	[SpellHandlerAttribute("DPSBuff")]
	public class DPSBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.DPS; } }

		// constructor
		public DPSBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Evade chance buff
	/// </summary>
	[SpellHandlerAttribute("EvadeBuff")]
	public class EvadeChanceBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.EvadeChance; } }

		// constructor
		public EvadeChanceBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Parry chance buff
	/// </summary>
	[SpellHandlerAttribute("ParryBuff")]
	public class ParryChanceBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ParryChance; } }

		// constructor
		public ParryChanceBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// WeaponSkill buff
	/// </summary>
	[SpellHandlerAttribute("WeaponSkillBuff")]
	public class WeaponSkillBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.WeaponSkill; } }

		// constructor
		public WeaponSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Stealth Skill buff
	/// </summary>
	[SpellHandlerAttribute("StealthSkillBuff")]
	public class StealthSkillBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.Skill_Stealth; } }

		// constructor
		public StealthSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// To Hit buff
	/// </summary>
	[SpellHandlerAttribute("ToHitBuff")]
	public class ToHitSkillBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ToHitBonus; } }

		// constructor
		public ToHitSkillBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Magic Resists Buff
	/// </summary>
	[SpellHandlerAttribute("MagicResistsBuff")]
	public class MagicResistsBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MagicAbsorbtion; } }

		// constructor
		public MagicResistsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
