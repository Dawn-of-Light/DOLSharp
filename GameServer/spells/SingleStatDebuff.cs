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
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Debuffs a single stat
	/// </summary>
	public abstract class SingleStatDebuff : SingleStatBuff
	{
		// bonus category
		public override int BonusCategory1 { get { return 3; } }

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			target.LastAttackedByEnemyTick = target.Region.Time;
			Caster.LastAttackTick = Caster.Region.Time;
			if(target is GameNPC) 
			{
				IAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration -= duration * target.GetResist(Spell.DamageType) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		// constructor
		public SingleStatDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
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
	/// Armor absorbtion debuff
	/// </summary>
	[SpellHandlerAttribute("ArmorAbsorbtionDebuff")]
	public class ArmorAbsorbtionDebuff : SingleStatDebuff
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
		public ArmorAbsorbtionDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
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
 }
