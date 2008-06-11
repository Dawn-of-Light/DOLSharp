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
using DOL.GS;
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

		private GamePlayer m_owner;
		private GameLiving m_target;
		private bool m_melee = false;
		private Spell spelllos;

		public TheurgistPetBrain(GamePlayer owner)
		{
			if (owner != null)
			{
				m_owner = owner;
				if (owner.TargetObject as GameLiving != null)
					m_target = m_owner.TargetObject as GameLiving;
			}
			AggroLevel = 100;
		}

		public override int ThinkInterval { get { return 1500; } }

		public override void Think() { AttackMostWanted(); }

		public void SetAggressionState(eAggressionState state) { }

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (!IsActive) return;
			if (sender == Body && e == GameLivingEvent.AttackedByEnemy)
			{
				m_melee = true;
				GameLiving target = m_target; //CalculateNextAttackTarget();
				if (target != null) Body.StartAttack(target);
				return;
			}
			if (e == GameNPCEvent.CastFailed)
			{
				switch ((args as CastFailedEventArgs).Reason)
				{
					case CastFailedEventArgs.Reasons.TargetTooFarAway:
						GameLiving target = m_target; //CalculateNextAttackTarget();
						if (target != null) Body.StartAttack(target);
						return;
				}
			}
		}

		protected override void AttackMostWanted()
		{
			if (!IsActive) return;
			GameLiving target = m_target; //CalculateNextAttackTarget();
			if (target != null)
			{
				if (!m_melee)
				{
					if (!CastSpell(target))
						Body.StartAttack(target);
				}
			}
		}

		private bool CastSpell(GameLiving target)
		{
			if (target == null || !target.IsAlive || m_melee)
				return false;

			if (Body.IsBeingInterrupted)
			{
				m_melee = true;
				return false;
			}

			if (Body.IsCasting)
				return true;

			foreach (Spell spell in Body.Spells)
			{
				if (spell.Target.ToLower() != "enemy") continue;
				spell.Level = Body.Level;
				if (spell.SpellType != "Stun" || Util.Chance(70))
				{
					if (Body.TargetObject != target) Body.TargetObject = target;
					if (spell.CastTime > 0) Body.TurnTo(target);

					//Eden - LoS check for ice pets
					if (Body.Name.ToLower().IndexOf("ice") >= 0)
					{
						GamePlayer LOSChecker = null;
						if (target is GamePlayer) LOSChecker = target as GamePlayer;
						else if (target is GameNPC)
						{
							foreach (GamePlayer ply in this.Body.GetPlayersInRadius(300))
								if (ply != null) { LOSChecker = ply; break; }
						}
						if (LOSChecker == null) return false;
						spelllos = spell;
						LOSChecker.Out.SendCheckLOS(LOSChecker, Body, new CheckLOSResponse(PetStartSpellAttackCheckLOS));
					}
					else Body.CastSpell(spell, m_mobSpellLine);
				}
				return true;
			}
			return false;
		}

		public void PetStartSpellAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
				Body.CastSpell(spelllos, m_mobSpellLine);
			else Body.Follow(m_target, 90, 5000);
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
		public GamePlayer GetPlayerOwner() { return m_owner; }
		public bool IsMainPet { get { return false; } set { } }
		#endregion
	}
}
