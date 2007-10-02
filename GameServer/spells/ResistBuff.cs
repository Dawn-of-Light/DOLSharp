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
             if (SpellHandler.FindEffectOnTarget(target, "VampiirMagicResistance") != null 
                || SpellHandler.FindEffectOnTarget(target, "VampiirMeleeResistance") != null)
            {
                MessageToLiving(target, "You did not receive the buff because you already have resists active!", eChatType.CT_SpellResisted);
                return;
            }
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
	}

	/// <summary>
	/// Body resistance buff
	/// </summary>
	[SpellHandlerAttribute("BodyResistBuff")]
	public class BodyResistBuff : AbstractResistBuff
	{
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
		public override int BonusCategory2 { get { return 1; } }
		public override int BonusCategory3 { get { return 1; } }

		public override eProperty Property1 { get { return eProperty.Resist_Body; } }
		public override eProperty Property2 { get { return eProperty.Resist_Spirit; } }
		public override eProperty Property3 { get { return eProperty.Resist_Energy; } }

		// constructor
		public BodySpiritEnergyBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Heat/Cold/Matter resistance buff
	/// </summary>
	[SpellHandlerAttribute("HeatColdMatterBuff")]
	public class HeatColdMatterBuff : AbstractResistBuff
	{
		public override int BonusCategory1 { get { return 1; } }
		public override int BonusCategory2 { get { return 1; } }
		public override int BonusCategory3 { get { return 1; } }

		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }
		public override eProperty Property2 { get { return eProperty.Resist_Cold; } }
		public override eProperty Property3 { get { return eProperty.Resist_Matter; } }

		// constructor
		public HeatColdMatterBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	/// <summary>
	/// Crush/Slash/Thrust resistance buff
	/// </summary>
	[SpellHandlerAttribute("CrushSlashThrustBuff")]
	public class CrushSlashThrustBuff : AbstractResistBuff
	{
		public override int BonusCategory1 { get { return 1; } }
		public override int BonusCategory2 { get { return 1; } }
		public override int BonusCategory3 { get { return 1; } }

		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }
		public override eProperty Property2 { get { return eProperty.Resist_Slash; } }
		public override eProperty Property3 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public CrushSlashThrustBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandlerAttribute("CrushResistBuff")]
	public class CrushResistBuff : AbstractResistBuff
	{
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
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
		public override int BonusCategory1 { get { return 1; } }
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public ThrustResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

}
