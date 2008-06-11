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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for speed decreasing spells.  Special for vampiirs
	/// </summary>
	[SpellHandler("VampSpeedDecrease")]
	public class VampMaintainedSpeedDecrease : SpeedDecreaseSpellHandler
	{
		public VampMaintainedSpeedDecrease(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		/// <summary>
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			//We don't want an immunity timer for this
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
		}

		protected override void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			//It is a maintained spell - it doesn't cancel when they are attacked
			//base.OnAttacked(e, sender, arguments);
		}
	}
}
