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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Palading heal chant works only in combat
	/// </summary>
	[SpellHandlerAttribute("CombatHeal")]
	public class CombatHealSpellHandler : HealSpellHandler
	{
		/// <summary>
		/// Execute heal spell
		/// </summary>
		/// <param name="target"></param>
		public override void StartSpell(GameLiving target)
		{
			m_startReuseTimer = true;
			// do not start spell if not in combat
			GamePlayer player = Caster as GamePlayer;
			if (!Caster.InCombat && (player==null || player.PlayerGroup==null || !player.PlayerGroup.IsGroupInCombat()))
				return;
			base.StartSpell(target);
		}

		// constructor
		public CombatHealSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}
