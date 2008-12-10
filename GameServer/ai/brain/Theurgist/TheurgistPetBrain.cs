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
using System.Collections;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.AI.Brain
{
	public class TheurgistPetBrain : StandardMobBrain, IControlledBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private GameLiving m_owner;
		private GameLiving m_target;
		private bool m_melee = false;
        private bool m_active = true;
		private Spell spelllos;

		public TheurgistPetBrain(GameLiving owner)
		{
			if (owner != null)
			{
				m_owner = owner;
                //m_target = (GameLiving)Body.TempProperties.getObjectProperty("target", null);
				/*if (owner.TargetObject as GameLiving != null)
					m_target = m_owner.TargetObject as GameLiving;*/
			}
			AggroLevel = 100;
			IsMainPet = false;
		}

		public override int ThinkInterval { get { return 1500; } }

		public override void Think() { AttackMostWanted(); }

		public void SetAggressionState(eAggressionState state) { }

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
            //log.Error("notify ! " + (e!=null?e.ToString():"") + "\n\n" + (args != null?args.ToString():""));
            if (!IsActive || m_melee || !m_active) return;

			//if (e == GameLivingEvent.AttackedByEnemy || e == GameLivingEvent.)
            if(args as AttackFinishedEventArgs != null)
			{
                m_melee = true;
                //log.Error("melee=true");
				GameLiving target = m_target; //CalculateNextAttackTarget();
				if (target != null) Body.StartAttack(target);
				return;
			}
            if (e == GameLivingEvent.CastFailed)
			{
				//switch ((args as CastFailedEventArgs).Reason)
				{
					//case CastFailedEventArgs.Reasons.TargetTooFarAway:
						GameLiving target = m_target; //CalculateNextAttackTarget();
						if (target != null) Body.StartAttack(target);
						return;
				}
			}
		}

		protected override void AttackMostWanted()
		{
			if (!IsActive/* || m_target == null*/ || !m_active) return;
            if (m_target == null) m_target = (GameLiving)Body.TempProperties.getObjectProperty("target", null);
            if (m_target == null) return;
			GameLiving target = m_target;
            if (target != null && target.IsAlive)
            {
                if (!CheckSpells(eCheckSpellType.Offensive))
                    Body.StartAttack(target);
            }
            else
            {
                m_target = null;
                m_active = false;
                Body.StopMoving();
                Body.MaxSpeedBase = 0;
            }
		}
		
		public override bool CheckSpells(eCheckSpellType type)
		{
			if (Body == null || Body.Spells == null || Body.Spells.Count < 1 || m_melee)
				return false;
				
			if (Body.IsCasting)
				return true;
				
			bool casted = false;
			if (type == eCheckSpellType.Defensive)
			{
				foreach (Spell spell in Body.Spells)
				{
					if (!Body.IsBeingInterrupted && Body.GetSkillDisabledDuration(spell) == 0 && CheckDefensiveSpells(spell))
					{
						casted = true;
						break;
					}
				}
			}
			else
			{
				foreach (Spell spell in Body.Spells)
				{
					if (Body.GetSkillDisabledDuration(spell) == 0)
					{
						if (spell.CastTime > 0)
						{
							if (!Body.IsBeingInterrupted && CheckOffensiveSpells(spell))
							{
								casted = true;
								break;
							}
						}
						else
							CheckInstantSpells(spell);
					}
				}
			}
			if (this is IControlledBrain && !Body.AttackState)
				((IControlledBrain)this).Follow(((IControlledBrain)this).Owner);
			return casted;
		}

		#region IControlledBrain Members
		public eWalkState WalkState { get { return eWalkState.Stay; } }
		public eAggressionState AggressionState { get { return eAggressionState.Aggressive; } set { } }
		public GameLiving Owner { get { return m_owner; } }
		public void Attack(GameObject target) { }
		public void Follow(GameObject target) { }
		public void FollowOwner() { }
		public void Stay() { }
		public void ComeHere() { }
		public void Goto(GameObject target) { }
		public void UpdatePetWindow() { }
		public GamePlayer GetPlayerOwner() { return m_owner as GamePlayer; }
		public bool IsMainPet { get { return false; } set { } }
		#endregion
	}
}
