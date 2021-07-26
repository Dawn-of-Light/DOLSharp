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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Base class for all resist buffs, needed to set effectiveness
	/// </summary>
	public abstract class AbstractResistBuff : PropertyChangingSpell
	{
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, 1);
		}

		protected override void SendUpdates(GameLiving target)
		{
			base.SendUpdates(target);
			if (target is GamePlayer)
			{
				GamePlayer player = (GamePlayer)target;
				player.Out.SendCharResistsUpdate();
			}
		}

		public AbstractResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override string ShortDescription => $"Increases the target's resistance to {ConvertPropertyToText(Property1).ToLower()} damage by {Spell.Value}%.";
    }

	[SpellHandler("BodyResistBuff")]
	public class BodyResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Body; } }

		// constructor
		public BodyResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Cold resistance buff
	/// </summary>
	[SpellHandlerAttribute("ColdResistBuff")]
	public class ColdResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Cold; } }

		// constructor
		public ColdResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Energy resistance buff
	/// </summary>
	[SpellHandlerAttribute("EnergyResistBuff")]
	public class EnergyResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Energy; } }

		// constructor
		public EnergyResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Heat resistance buff
	/// </summary>
	[SpellHandlerAttribute("HeatResistBuff")]
	public class HeatResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }

		// constructor
		public HeatResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Matter resistance buff
	/// </summary>
	[SpellHandlerAttribute("MatterResistBuff")]
	public class MatterResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Matter; } }

		// constructor
		public MatterResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Spirit resistance buff
	/// </summary>
	[SpellHandlerAttribute("SpiritResistBuff")]
	public class SpiritResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Spirit; } }

		// constructor
		public SpiritResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Body/Spirit/Energy resistance buff
	/// </summary>
	[SpellHandlerAttribute("BodySpiritEnergyBuff")]
	public class BodySpiritEnergyBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }

		public override eProperty Property1 { get { return eProperty.Resist_Body; } }
		public override eProperty Property2 { get { return eProperty.Resist_Spirit; } }
		public override eProperty Property3 { get { return eProperty.Resist_Energy; } }

		public BodySpiritEnergyBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"Increases the target's resistance to body, spirit and energy damage by {Spell.Value}%.";
	}

	/// <summary>
	/// Heat/Cold/Matter resistance buff
	/// </summary>
	[SpellHandlerAttribute("HeatColdMatterBuff")]
	public class HeatColdMatterBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }

		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }
		public override eProperty Property2 { get { return eProperty.Resist_Cold; } }
		public override eProperty Property3 { get { return eProperty.Resist_Matter; } }

		public HeatColdMatterBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"Increases the target's resistance to heat, cold and matter damage by {Spell.Value}%.";
	}

	/// <summary>
	/// Body/Spirit/Energy/Heat/Cold/Matter resistance buff
	/// </summary>
	[SpellHandlerAttribute("AllMagicResistsBuff")]
	public class AllMagicResistsBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory4 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory5 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory6 { get { return eBuffBonusCategory.BaseBuff; } }
		
		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }
		public override eProperty Property2 { get { return eProperty.Resist_Cold; } }
		public override eProperty Property3 { get { return eProperty.Resist_Matter; } }
		public override eProperty Property4 { get { return eProperty.Resist_Body; } }
		public override eProperty Property5 { get { return eProperty.Resist_Spirit; } }
		public override eProperty Property6 { get { return eProperty.Resist_Energy; } }

		public AllMagicResistsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override string ShortDescription => $"Increases the target's resistance to all magic damage by {Spell.Value}%.";
    }

	/// <summary>
	/// Crush/Slash/Thrust resistance buff
	/// </summary>
	[SpellHandlerAttribute("CrushSlashThrustBuff")]
	[SpellHandlerAttribute("AllMeleeResistsBuff")]
	public class CrushSlashThrustBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }

		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }
		public override eProperty Property2 { get { return eProperty.Resist_Slash; } }
		public override eProperty Property3 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public CrushSlashThrustBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"Increases the target's resistance to all melee damage by {Spell.Value}%.";
	}

	[SpellHandlerAttribute("CrushResistBuff")]
	public class CrushResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }

		// constructor
		public CrushResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Slash buff
	/// </summary>
	[SpellHandlerAttribute("SlashResistBuff")]
	public class SlashResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Slash; } }

		// constructor
		public SlashResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Thrust buff
	/// </summary>
	[SpellHandlerAttribute("ThrustResistBuff")]
	public class ThrustResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public ThrustResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Resist all 
	/// </summary>
	[SpellHandlerAttribute("AllResistsBuff")]
	public class AllResistsBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory4 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory5 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory6 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory7 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory8 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory9 { get { return eBuffBonusCategory.BaseBuff; } }

		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }
		public override eProperty Property2 { get { return eProperty.Resist_Cold; } }
		public override eProperty Property3 { get { return eProperty.Resist_Matter; } }
		public override eProperty Property4 { get { return eProperty.Resist_Body; } }
		public override eProperty Property5 { get { return eProperty.Resist_Spirit; } }
		public override eProperty Property6 { get { return eProperty.Resist_Energy; } }
		public override eProperty Property7 { get { return eProperty.Resist_Crush; } }
		public override eProperty Property8 { get { return eProperty.Resist_Slash; } }
		public override eProperty Property9 { get { return eProperty.Resist_Thrust; } }

		public AllResistsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override string ShortDescription => $"Increases the target's resistance to all damage by {Spell.Value}%.";
    }

}
