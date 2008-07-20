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
using System.Collections.Generic;
using DOL.GS;

namespace DOL.AI.Brain
{
	public class TurretFNFBrain : TurretBrain
	{
		public TurretFNFBrain(GameLiving owner) : base(owner)
		{
		}

		/// <summary>
		/// [Ganrod] Nidel:
		/// Cast only Offensive or Defensive spell.
		/// <para>If Offensive spell is true, Defensive spell isn't casted.</para>
		/// </summary>
		public override void Think()
		{
			GamePlayer playerowner = GetPlayerOwner();
			if(!playerowner.CurrentUpdateArray[Body.ObjectID - 1])
			{
				playerowner.Out.SendObjectUpdate(Body);
				playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
			}
		  CheckSpells(eCheckSpellType.Offensive);
		  CheckSpells(eCheckSpellType.Defensive);
		}

		/// <summary>
		/// Get a random target from aggro table
		/// </summary>
		/// <returns></returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			List<GameLiving> livingList = new List<GameLiving>();
			base.CalculateNextAttackTarget();
			lock(m_aggroTable.SyncRoot)
			{
				foreach(GameLiving living in m_aggroTable.Keys)
				{
					if(!living.IsAlive || living.CurrentRegion != Body.CurrentRegion || living.ObjectState != GameObject.eObjectState.Active)
						continue;

					if(WorldMgr.GetDistance(Body, living) > MAX_AGGRO_DISTANCE)
						continue;

					if(WorldMgr.GetDistance(Body, living) > ((TurretPet) Body).TurretSpell.Range)
						continue;

					livingList.Add(living);
				}
			}
			if(livingList.Count > 0)
			{
				return livingList[Util.Random(livingList.Count - 1)];
			}
			m_aggroTable.Clear();
			return null;
		}

		protected override void OnAttackedByEnemy(AttackData ad)
		{
			AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1);
			CheckSpells(eCheckSpellType.Offensive);
		}

		/// <summary>
    /// Updates the pet window
    /// </summary>
    public override void UpdatePetWindow()
    {
    }
  }
}