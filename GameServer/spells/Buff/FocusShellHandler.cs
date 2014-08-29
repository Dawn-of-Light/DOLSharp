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

using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Base Handler for the focus shell
	/// </summary>
	[SpellHandlerAttribute("FocusShell")]
	public class FocusShellHandler : BothAblativeArmorSpellHandler
	{
		
		public FocusShellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}

		/// <summary>
		/// This spell only work on realm mate (RvR use)
		/// </summary>
		public override bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
		{
			
			if(!base.CheckBeginCast(selectedTarget, quiet))
				return false;
			
			//This spell doesn't work on pets or monsters
			if (selectedTarget != null && !(selectedTarget is GamePlayer))
			{
				if(!quiet)
					MessageToCaster("This spell can only be cast on Players!", eChatType.CT_SpellResisted);
				
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Only Absorb Damage from Players or Player Controlled Pet
		/// </summary>
		protected override void OnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving living = sender as GameLiving;

			if (living == null)
				return;
			
			AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
			AttackData ad = null;
			
			if (attackedByEnemy != null)
				ad = attackedByEnemy.AttackData;
			
			if(ad.Attacker is GamePlayer || (ad.Attacker is GameNPC && ((GameNPC)(ad.Attacker)).Brain is IControlledBrain && ((IControlledBrain)((GameNPC)(ad.Attacker)).Brain).GetPlayerOwner() != null))
				base.OnAttack(e, sender, arguments);
		}

		/// <summary>
		/// Check if Focus Pulse Should Apply (LoS check on ennemy)
		/// This won't break focus on returning false.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns>True if the Focus Should Apply and Continue, False if Los Check is Pending.</returns>		
		public override bool CheckFocusPulseApply(GameLiving target, bool quiet)
		{
			// Purely Needed for LoS check
			GameEventMgr.AddHandlerUnique(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusShellLOSToTarget));
			if(!CheckTargetLoS(target))
			{
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusShellLOSToTarget));
				// Continue with focus if no LoS checks...
				return true;
			}

			// Don't apply wait for focus LoS
			return false;
		}
		
		/// <summary>
		/// Los Check Event Handler for Focus Shell 
		/// </summary>
		private void CheckFocusShellLOSToTarget(DOLEvent e, object sender, EventArgs args)
		{
			if(e == GameLivingEvent.FinishedLosCheck && sender == Caster && args is LosCheckData)
			{
				if(!((LosCheckData)args).LosOK)
				{
					MessageToCaster("The player protected by your Magic Shell is out of view!", eChatType.CT_SpellResisted);
					
					// Remove effect during the lost pulse.
					RemoveEffectFromFocusTarget();
				}
				else
				{
					// Apply Pulse Spell - Consume mana and start spell !
					SpellPulseStart();
				}
			}
			
			GameEventMgr.RemoveHandler(Caster, GameLivingEvent.FinishedLosCheck, new DOLEventHandler(CheckFocusShellLOSToTarget));
		}
	}
}
