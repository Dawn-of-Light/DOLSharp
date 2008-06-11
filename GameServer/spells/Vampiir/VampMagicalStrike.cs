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
using DOL.GS.PacketHandler;
using DOL.GS.Keeps;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Vamps magical strike 
	/// </summary>
	[SpellHandlerAttribute("MagicalStrike")]
	public class VampMagicalStrike : DirectDamageSpellHandler
	{
		public VampMagicalStrike(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override int CalculateSpellResistChance(GameLiving target)
		{
			//This needs to be corrected as vampiir claws don't seem to act the same as normal damage spells
			//Same level or lower resists 0%
			//Every level above vamp level increases percent by .5%
			return target.Level <= Caster.Level ? 0 : (target.Level - Caster.Level) / 2;
			//return base.CalculateSpellResistChance(target);
		}
	}
}
