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

using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Any Positive Effect Spell Handler can inherit this to have a common behavior
	/// Not Meant to be instanciated
	/// </summary>
	public abstract class BuffSpellHandler : SpellHandler
	{
		/// <summary>
		/// Buff Should always have positive Effect...
		/// </summary>
		public override bool HasPositiveEffect {
			get { return true; }
		}
		
		/// <summary>
		/// Buff Should Always start Effect
		/// </summary>
		public override bool ForceStartEffect {
			get { return true; }
		}
		
		public BuffSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
	
	public abstract class AttackModifierBuffSpellHandler : BuffSpellHandler
	{
		/// <summary>
		/// On Effect Start, Listen to AttackedByEnemy Events.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackedByEnemy));
			base.OnEffectStart(effect);
		}
		
		/// <summary>
		/// On Effect Start, Stop Listening to AttackedByEnemy Events.
		/// </summary>
		/// <param name="effect"></param>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackedByEnemy));
			return base.OnEffectExpires(effect, noMessages);
		}

		/// <summary>
		/// Implement Behavior when Attacked By Enemy.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected abstract void OnAttackedByEnemy(DOLEvent e, object sender, EventArgs args);
		
		public AttackModifierBuffSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
	}
}
