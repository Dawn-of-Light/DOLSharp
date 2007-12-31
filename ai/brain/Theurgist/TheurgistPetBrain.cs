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
using DOL.Events;
using DOL.GS;
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
	public class TheurgistPetBrain : StandardMobBrain, IControlledBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private GamePlayer m_owner;
		private bool m_melee = false;

		/// <summary>
		/// Create a new TheurgistPetBrain.
		/// </summary>
		/// <param name="owner"></param>
		public TheurgistPetBrain(GamePlayer owner)
		{
			m_owner = owner;
			AggroLevel = 100;
		}

		/// <summary>
		/// Brain main loop.
		/// </summary>
		public override void Think()
		{
			AttackMostWanted();
		}

		/// <summary>
		/// Receives all messages of the body.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (!IsActive) return;

			if (sender == Body && e == GameLivingEvent.AttackedByEnemy)
			{
				m_melee = true;
				GameLiving target = CalculateNextAttackTarget();
				if (target != null)
					Body.StartAttack(target);
				return;
			}

			if (e == GameNPCEvent.CastFailed)
			{
				switch ((args as CastFailedEventArgs).Reason)
				{
					case CastFailedEventArgs.Reasons.TargetTooFarAway:
						GameLiving target = CalculateNextAttackTarget();
						if (target != null)
							Body.StartAttack(target);
						return;
				}
			}
		}

		/// <summary>
		/// Select and attacks the next target.
		/// </summary>
		protected override void AttackMostWanted()
		{
			if (!IsActive)
				return;

			GameLiving target = CalculateNextAttackTarget();
			if (target != null)
			{
				if (!m_melee)
					if (!CastSpell(target))
						Body.StartAttack(target);
			}
		}

		/// <summary>
		/// Try to cast a spell on the target.
		/// </summary>
		/// <param name="target"></param>
		/// <returns>True, if cast successful or already casting, false otherwise.</returns>
		private bool CastSpell(GameLiving target)
		{
			if (m_melee)
				return false;

			if (Body.IsBeingInterrupted)
			{
				m_melee = true;
				return false;
			}

			if (Body.IsCasting)
				return true;

			Body.TargetObject = target;
			Body.TurnTo(target);

			foreach (Spell spell in Body.Spells)
			{
				if (spell.Target.ToLower() != "enemy")
					continue;

				if (LivingHasEffect((GameLiving)Body.TargetObject, spell))
					continue;

				if (Body.GetSkillDisabledDuration(spell) > 0)
					continue;

				spell.Level = Body.Level;
				Body.CastSpell(spell, m_mobSpellLine);
				return true;
			}

			return false;
		}

		#region IControlledBrain Members

		public eWalkState WalkState
		{
			get { return eWalkState.Stay; }
		}

		public eAggressionState AggressionState
		{
			get	{ return eAggressionState.Aggressive; }
			set { }
		}

		public GameLiving Owner
		{
			get { return m_owner; }
		}

		public void Attack(GameObject target) { }
		public void Follow(GameObject target) { }
		public void FollowOwner() { }
		public void Stay() { }
		public void ComeHere() { }
		public void Goto(GameObject target) { }
		public void UpdatePetWindow() { }

		public GamePlayer GetPlayerOwner()
		{
			return m_owner;
		}

		public bool IsMainPet
		{
			get { return false; }
			set { }
		}

		#endregion
	}
}
