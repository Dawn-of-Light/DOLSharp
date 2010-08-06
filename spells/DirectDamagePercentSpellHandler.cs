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
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for ToA weapons that use % damage.  Only supports players.
	/// </summary>
	[SpellHandlerAttribute("DirectDamagePercent")]
	public class DirectDamagePercentSpellHandler : DirectDamageSpellHandler
	{
		public DirectDamagePercentSpellHandler(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) {}


		public override double CalculateDamageBase(GameLiving target)
		{
			double spellDamage = 0;
			GamePlayer player = Caster as GamePlayer;

			if (player != null)
			{
				spellDamage = target.MaxHealth * Spell.Damage * .01;
			}

			if (spellDamage < 0)
				spellDamage = 0;

			return spellDamage;
		}


	}
}
