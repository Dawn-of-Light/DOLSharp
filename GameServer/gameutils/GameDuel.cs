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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GameDuel is an Helper Class for Player Duels
	/// </summary>
	public class GameDuel
	{
		/// <summary>
		/// Duel Initiator
		/// </summary>
		public GamePlayer Starter { get; protected set; }
		
		/// <summary>
		/// Duel Target
		/// </summary>
		public GamePlayer Target { get; protected set; }
		
		/// <summary>
		/// Is Duel Started ?
		/// </summary>
		public bool Started { get { return m_started; } protected set { m_started = value; } }
		protected volatile bool m_started;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="starter"></param>
		/// <param name="target"></param>
		public GameDuel(GamePlayer starter, GamePlayer target)
		{
			Starter = starter;
			Target = target;
			Started = false;
		}
		
		/// <summary>
		/// Start Duel if is not running.
		/// </summary>
		public virtual void Start()
		{
			if (Started)
				return;
			
			Target.DuelStart(Starter);
			Started = true;
			GameEventMgr.AddHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
			GameEventMgr.AddHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
			GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
		}
		
		/// <summary>
		/// Stops the duel if it is running
		/// </summary>
		public virtual void Stop()
		{
			if (!Started || Starter.DuelTarget != Target)
				return;

			var target = Target;
			Target = null;
			
			foreach (GameSpellEffect effect in Starter.EffectList.GetAllOfType<GameSpellEffect>())
			{
				if (effect.SpellHandler.Caster == target && !effect.SpellHandler.HasPositiveEffect)
					effect.Cancel(false);
			}

			GameEventMgr.RemoveHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
			
			lock (Starter.XPGainers.SyncRoot)
			{
				Starter.XPGainers.Clear();
			}
			
			Starter.Out.SendMessage(LanguageMgr.GetTranslation(Starter.Client, "GamePlayer.DuelStop.DuelEnds"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
			Started = false;
			target.DuelStop();
		}

		/// <summary>
		/// Stops the duel if player attack or is attacked by anything other that duel target
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void DuelOnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackData ad = null;
			GameLiving target = null;
			var afea = arguments as AttackFinishedEventArgs;
			var abeea = arguments as AttackedByEnemyEventArgs;
			
			if (afea != null)
			{
				ad = afea.AttackData;
				target = ad.Target;
			}
			else if (abeea != null)
			{
				ad = abeea.AttackData;
				target = ad.Attacker;
			}

			if (ad == null)
				return;

			// check pets owner for my and enemy attacks
			GameNPC npc = target as GameNPC;
			if (npc != null)
			{
				IControlledBrain brain = npc.Brain as IControlledBrain;
				if (brain != null)
					target = brain.GetPlayerOwner();
			}

			// Duel should end if players join group and trys to attack
			if (ad.Attacker.Group != null && ad.Attacker.Group.IsInTheGroup(ad.Target))
				Stop();
			
			if (ad.IsHit && target != Target)
				Stop();
		}

		/// <summary>
		/// Stops the duel on quit/link death
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void DuelOnPlayerQuit(DOLEvent e, object sender, EventArgs arguments)
		{
			Stop();
		}

	}
}
