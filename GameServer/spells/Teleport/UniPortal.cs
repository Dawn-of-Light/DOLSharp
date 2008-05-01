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
using System.Collections.Generic;
using System.Text;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
	/// <summary>
	/// The spell used by classic teleporters.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandlerAttribute("UniPortal")]
	public class UniPortal : SpellHandler
	{
		private Teleport m_destination;

		public UniPortal(GameLiving caster, Spell spell, SpellLine spellLine, Teleport destination)
			: base(caster, spell, spellLine) 
		{
			m_destination = destination;
		}

		/// <summary>
		/// Whether this spell can be cast on the selected target at all.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget))
				return false;
			return (selectedTarget is GamePlayer);
		}

		/// <summary>
		/// Apply the effect.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = target as GamePlayer;
			if (player == null)
				return;

			SendEffectAnimation(player, 0, false, 1);

			UniPortalEffect effect = new UniPortalEffect(this, 1000);
			effect.Start(player);

			player.MoveTo((ushort)m_destination.RegionID,
				m_destination.X, m_destination.Y, m_destination.Z,
				(ushort)m_destination.Heading);
		}
	}
}
