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
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	public abstract class SingleStatDebuff : SingleStatBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.Debuff; } }

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
		}

		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = Spell.Duration;
			duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
			duration -= duration * target.GetResist(Spell.DamageType) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}
		
        public override int CalculateSpellResistChance(GameLiving target)
        {
        	int basechance =  base.CalculateSpellResistChance(target);       	
 			GameSpellEffect rampage = SpellHandler.FindEffectOnTarget(target, "Rampage");
            if (rampage != null)
            {
            	basechance += (int)rampage.Spell.Value;
            }
            return Math.Min(100, basechance);
        }

		public SingleStatDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"Decreases the target's {ConvertPropertyToText(Property1)} by {Spell.Value}.";
    }

	[SpellHandler("StrengthDebuff")]
	public class StrengthDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Strength; } }

		public StrengthDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("DexterityDebuff")]
	public class DexterityDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Dexterity; } }	

		public DexterityDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("ConstitutionDebuff")]
	public class ConstitutionDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Constitution; } }	

		public ConstitutionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("ArmorFactorDebuff")]
	public class ArmorFactorDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.ArmorFactor; } }	

		public ArmorFactorDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("ArmorAbsorptionDebuff")]
	public class ArmorAbsorptionDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.ArmorAbsorption; } }

		protected override void SendUpdates(GameLiving target) { }

		public ArmorAbsorptionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("CombatSpeedDebuff")]
	public class CombatSpeedDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }

		protected override void SendUpdates(GameLiving target) { }

		public CombatSpeedDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"Target's attack speed reduced by {Spell.Value}%.";
	}

	[SpellHandler("MeleeDamageDebuff")]
	public class MeleeDamageDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeDamage; } }

		protected override void SendUpdates(GameLiving target) { }

		public MeleeDamageDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"The target does {Spell.Value}% less damage with melee attacks.";
	}

	[SpellHandler("FatigueConsumptionDebuff")]
	public class FatigueConsumptionDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.FatigueConsumption; } }

		protected override void SendUpdates(GameLiving target) { }

		public FatigueConsumptionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"Increases endurance the target uses in combat by {Spell.Value}%.";
	}

	[SpellHandler("FumbleChanceDebuff")]
	public class FumbleChanceDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.FumbleChance; } }

		protected override void SendUpdates(GameLiving target) { }

		public FumbleChanceDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"Increase target's fumble chance by {Spell.Value}%.";
    }
	
	[SpellHandler("DPSDebuff")]
	public class DPSDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.DPS; } }	

		public DPSDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("SkillsDebuff")]
	public class SkillsDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.AllSkills; } }	

		public SkillsDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("AcuityDebuff")]
	public class AcuityDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Acuity; } }	

		public AcuityDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("QuicknessDebuff")]
	public class QuicknessDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Quickness; } }	

		public QuicknessDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandler("ToHitDebuff")]
	public class ToHitSkillDebuff : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.ToHitBonus; } }	

		public ToHitSkillDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
 }
