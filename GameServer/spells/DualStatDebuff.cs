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
	public abstract class DualStatDebuff : SingleStatDebuff
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.Debuff; } }
		public override eBuffBonusCategory BonusCategory2 { get { return eBuffBonusCategory.Debuff; } }

		public DualStatDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override string ShortDescription => $"Target's {ConvertPropertyToText(Property1)} and {ConvertPropertyToText(Property2)} are reduced by {Spell.Value}.";
    }

	[SpellHandler("StrengthConstitutionDebuff")]
	public class StrengthConDebuff : DualStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Strength; } }
		public override eProperty Property2 { get { return eProperty.Constitution; } }

		public StrengthConDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("DexterityQuicknessDebuff")]
	public class DexterityQuiDebuff : DualStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Dexterity; } }
		public override eProperty Property2 { get { return eProperty.Quickness; } }

		public DexterityQuiDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("DexterityConstitutionDebuff")]
	public class DexterityConDebuff : DualStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.Dexterity; } }
		public override eProperty Property2 { get { return eProperty.Constitution; } }

		public DexterityConDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandler("WeaponSkillConstitutionDebuff")]
	public class WeaponskillConDebuff : DualStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.WeaponSkill; } }
		public override eProperty Property2 { get { return eProperty.Constitution; } }

		public WeaponskillConDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
