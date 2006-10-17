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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Base class for all resist debuffs, needed to set effectiveness and duration
	/// </summary>
	public abstract class AbstractResistDebuff : PropertyChangingSpell
	{
		/// <summary>
		/// Gets debuff type name for delve info
		/// </summary>
		public abstract string DebuffTypeName { get; }

		/// <summary>
		/// Debuff category is 3 for debuffs
		/// </summary>
		public override int BonusCategory1 { get { return 3; } }

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			//TODO: correct effectiveness formula
			// invoke direct effect if not resisted for DD w/ debuff spells
			if(Spell.Level > 0)
			{
				int specLevel = 0;
				if (Caster is GamePlayer)
				{
					specLevel = ((GamePlayer)Caster).GetModifiedSpecLevel(m_spellLine.Spec);
				}
				effectiveness = 0.75 + (specLevel-1) * 0.5 / Spell.Level;
				effectiveness = Math.Max(0.75, effectiveness);
				effectiveness = Math.Min(1.25, effectiveness);
			}

			base.ApplyEffectOnTarget(target, effectiveness);

			target.LastAttackedByEnemyTick = target.CurrentRegion.Time;
			Caster.LastAttackTick = Caster.CurrentRegion.Time;
			if(target is GameNPC) 
			{
				IAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			//TODO: duration depends on target resists (maybe body resist for all)
//			int resist = target.GetResist(m_spell.DamageType);
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration -= duration * target.GetResist(eDamageType.Body) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		/// <summary>
		/// Updates changes properties to living
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			base.SendUpdates(target);
			if (target is GamePlayer)
			{
				GamePlayer player = (GamePlayer)target;
				player.Out.SendCharResistsUpdate();
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Nullify Dissipation>
				Function: resistance decrease
 
				Decreases the target's resistance to the listed damage type.
 
				Resist decrease Energy: 15
				Target: Targetted
				Range: 1500
				Duration: 15 sec
				Power cost: 13
				Casting time:      2.0 sec
				Damage: Cold
 
				<End Info>
				*/

				ArrayList list = new ArrayList();

				list.Add("Function: resistance decrease");
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add(String.Format("Resist decrease {0}: {1}", DebuffTypeName, m_spell.Value));
				list.Add("Target: " + Spell.Target);
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Duration >= ushort.MaxValue*1000) list.Add("Duration: Permanent.");
				else if(Spell.Duration > 60000) list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if(Spell.Duration != 0) list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if(Spell.RecastDelay > 60000) list.Add("Recast time: " + Spell.RecastDelay/60000 + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if(Spell.RecastDelay > 0) list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if(Spell.Concentration != 0) list.Add("Concentration cost: " + Spell.Concentration);
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.DamageType != eDamageType.Natural) list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}

		//constructor
		public AbstractResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Body resistance debuff
	/// </summary>
	[SpellHandlerAttribute("BodyResistDebuff")]
	public class BodyResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Body; } }
		public override string DebuffTypeName { get { return "Body"; } }

		// constructor
		public BodyResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Cold resistance debuff
	/// </summary>
	[SpellHandlerAttribute("ColdResistDebuff")]
	public class ColdResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Cold; } }	
		public override string DebuffTypeName { get { return "Cold"; } }

		// constructor
		public ColdResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Energy resistance debuff
	/// </summary>
	[SpellHandlerAttribute("EnergyResistDebuff")]
	public class EnergyResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Energy; } }	
		public override string DebuffTypeName { get { return "Energy"; } }

		// constructor
		public EnergyResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Heat resistance debuff
	/// </summary>
	[SpellHandlerAttribute("HeatResistDebuff")]
	public class HeatResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }	
		public override string DebuffTypeName { get { return "Heat"; } }

		// constructor
		public HeatResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Matter resistance debuff
	/// </summary>
	[SpellHandlerAttribute("MatterResistDebuff")]
	public class MatterResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Matter; } }	
		public override string DebuffTypeName { get { return "Matter"; } }

		// constructor
		public MatterResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Spirit resistance debuff
	/// </summary>
	[SpellHandlerAttribute("SpiritResistDebuff")]
	public class SpiritResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Spirit; } }	
		public override string DebuffTypeName { get { return "Spirit"; } }

		// constructor
		public SpiritResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Slash resistance debuff
	/// </summary>
	[SpellHandlerAttribute("SlashResistDebuff")]
	public class SlashResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Slash; } }	
		public override string DebuffTypeName { get { return "Slash"; } }

		// constructor
		public SlashResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Thrust resistance debuff
	/// </summary>
	[SpellHandlerAttribute("ThrustResistDebuff")]
	public class ThrustResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }	
		public override string DebuffTypeName { get { return "Thrust"; } }

		// constructor
		public ThrustResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Crush resistance debuff
	/// </summary>
	[SpellHandlerAttribute("CrushResistDebuff")]
	public class CrushResistDebuff : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }	
		public override string DebuffTypeName { get { return "Crush"; } }

		// constructor
		public CrushResistDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
