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

namespace DOL.GS.Spells
{
	[Obsolete("This is going to be removed without replacement.")]
	public abstract class SingleStatAbilityBuffHandler : PropertyChangingSpell
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.AbilityBuff; } }

		protected override void SendUpdates(GameLiving target)
		{
			target.UpdateHealthManaEndu();
		}
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// Ability Bonus are not modified by any effectiveness modifier
			base.ApplyEffectOnTarget(target, 1.0);
		}

		protected SingleStatAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
	[SpellHandler("StrengthAbilityBuff")]
	public class StrengthAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.Strength; } }

		public StrengthAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
	[SpellHandler("DexterityAbilityBuff")]
	public class DexterityAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.Dexterity; } }

		public DexterityAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
	[SpellHandler("ConstitutionAbilityBuff")]
	public class ConstitutionAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.Constitution; } }

		public ConstitutionAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
	[SpellHandler("QuicknessAbilityBuff")]
	public class QuicknessAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.Quickness; } }

		public QuicknessAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
	[SpellHandler("AcuityAbilityBuff")]
	public class AcuityAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.Acuity; } }

		public AcuityAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}

	[Obsolete("This is going to be removed without replacement.")]
    [SpellHandler("MaxHealthAbilityBuff")]
	public class MaxHealthAbilityBuffHandler : SingleStatAbilityBuffHandler
	{
		public override eProperty Property1 { get { return eProperty.MaxHealth; } }

		public MaxHealthAbilityBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}
}
