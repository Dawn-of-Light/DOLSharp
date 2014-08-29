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

using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Description of UninterruptibleIncreasingDirectDamage.
	/// </summary>
	[SpellHandlerAttribute("UninterruptibleIncreasingDirectDamage")]
	public class UninterruptibleIncreasingDirectDamageSpellHandler : IncreasingDirectDamageSpellHandler
	{
		/// <summary>
		/// Override the Focus Action Handler to remove Ranged Attack Interrupt
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void FocusSpellAction(DOLEvent e, object sender, EventArgs args)
		{
			if(e == GameLivingEvent.AttackedByEnemy)
			{
				if(args is AttackedByEnemyEventArgs && !((AttackedByEnemyEventArgs)args).AttackData.IsMeleeAttack)
					return;
			}
			base.FocusSpellAction(e, sender, args);
		}

		/// <summary>
		/// Sends the cast animation
		/// Special Fix for Heretic Focus
		/// </summary>
		/// <param name="castTime">The cast time</param>
		/// <param name="clientEffect">Cast Animation</param>
		public override void SendCastAnimation(ushort castTime, ushort clientEffect)
		{
			if (Spell.ID > 14079 && Spell.ID < 14083)
				clientEffect = (ushort)(clientEffect - 30);
			
			base.SendCastAnimation(castTime, clientEffect);
		}
		
		public UninterruptibleIncreasingDirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
