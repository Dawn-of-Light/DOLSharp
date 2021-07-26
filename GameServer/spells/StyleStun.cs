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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("StyleStun")]
	public class StyleStun : StunSpellHandler
	{
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			// http://www.camelotherald.com/more/1749.shtml
			// immunity timer will now be exactly five times the length of the stun
			base.OnEffectExpires(effect, noMessages);
			return Spell.Duration * 5;
		}

		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (Spell.EffectGroup != 0 || compare.Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (compare.Spell.SpellType == "Stun") return true;
			return base.IsOverwritable(compare);
		}

		public StyleStun(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}