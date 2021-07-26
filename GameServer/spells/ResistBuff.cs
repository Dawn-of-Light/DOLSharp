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
namespace DOL.GS.Spells
{
	public abstract class AbstractResistBuff : PropertyChangingSpell
	{
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

		public BodyResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("ColdResistBuff")]
	public class ColdResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Cold; } }

		public ColdResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("EnergyResistBuff")]
	public class EnergyResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Energy; } }

		public EnergyResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("HeatResistBuff")]
	public class HeatResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Heat; } }

		public HeatResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("MatterResistBuff")]
	public class MatterResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Matter; } }

		public MatterResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("SpiritResistBuff")]
	public class SpiritResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Spirit; } }

		public SpiritResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("BodySpiritEnergyBuff")]
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

	[SpellHandler("HeatColdMatterBuff")]
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

	[SpellHandler("AllMagicResistsBuff")]
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

	[SpellHandler("CrushSlashThrustBuff")]
	[SpellHandler("AllMeleeResistsBuff")]
	public class CrushSlashThrustBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eBuffBonusCategory BonusCategory3 { get { return eBuffBonusCategory.BaseBuff; } }

		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }
		public override eProperty Property2 { get { return eProperty.Resist_Slash; } }
		public override eProperty Property3 { get { return eProperty.Resist_Thrust; } }

		public CrushSlashThrustBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override string ShortDescription => $"Increases the target's resistance to all melee damage by {Spell.Value}%.";
	}

	[SpellHandler("CrushResistBuff")]
	public class CrushResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }

		public CrushResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("SlashResistBuff")]
	public class SlashResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Slash; } }

		public SlashResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("ThrustResistBuff")]
	public class ThrustResistBuff : AbstractResistBuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }

		public ThrustResistBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("AllResistsBuff")]
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
