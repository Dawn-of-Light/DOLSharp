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
using System.Reflection;
using System.Text;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
    public class PrimerSpellHandler : SpellHandler
	{
		/// <summary>
		/// Cast Powerless
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			
			base.FinishSpellCast(target);
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, Spell.Duration, 0, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{			
			GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(OnMove));
			SendEffectAnimation(effect.Owner, 0, false, 1);			
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if(effect.Owner is GamePlayer && !noMessages)
				((GamePlayer)effect.Owner).Out.SendMessage("You modification spell effect has expired.", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);

			GameEventMgr.RemoveHandler(effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(OnMove));

			return base.OnEffectExpires (effect, false);
		}

	
		/// <summary>
		/// Handles attacks on player/by player
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void OnMove(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;
			if(living.IsMoving)
			{
				// remove speed buff if in combat
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
				if (effect != null)
				{
					effect.Cancel(false);
					((GamePlayer)living).Out.SendMessage("You move and break your modification spell.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				}
			}
		}


		// constructor
		public PrimerSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
