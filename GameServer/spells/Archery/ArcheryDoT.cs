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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    [SpellHandler("ArcheryDoT")]
    public class ArcheryDoTSpellHandler : DoTSpellHandler
    {
		public ArcheryDoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override double CalculateDamageBase(GameLiving target)
        {
            return Spell.Damage;
        }

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			int speclevel = 1;
			if (m_caster is GamePlayer)
			{
				speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(Specs.Archery);
			}

			min = 1.25;
			max = 1.25;

			if (target.Level > 0)
			{
				min = 0.75 + (speclevel - 1) / (double)target.Level * 0.5;
			}

			if (speclevel - 1 > target.Level)
			{
				double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
				min += overspecBonus;
				max += overspecBonus;
			}

			if (min > max) min = max;
			if (min < 0) min = 0;
		}

    }
}
