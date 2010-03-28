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

// Original code from Dinberg

using DOL.GS;

namespace DOL.AI.Brain
{
	public class NoveltyPetBrain : DOL.AI.ABrain, IControlledBrain
	{
		public const string HAS_PET = "HasNoveltyPet";

		private GamePlayer m_owner;

		public NoveltyPetBrain(GamePlayer owner)
			: base()
		{
			m_owner = owner;
		}

		#region Think

		public override int ThinkInterval
		{
			get { return 5000; }
			set { }
		}

		public override void Think()
		{

			if (m_owner == null || 
				m_owner.IsAlive == false || 
				m_owner.Client.ClientState != GameClient.eClientState.Playing || 
				Body.IsWithinRadius(m_owner, WorldMgr.VISIBILITY_DISTANCE) == false)
			{
				Body.Delete();
				Body = null;
				if (m_owner != null && m_owner.TempProperties.getProperty<bool>(HAS_PET, false))
				{
					m_owner.TempProperties.setProperty(HAS_PET, false);
				}
				m_owner = null;
			}
		}

		#endregion Think

		#region IControlledBrain Members
		public void SetAggressionState(eAggressionState state) { }
		public eWalkState WalkState { get { return eWalkState.Follow; } }
		public eAggressionState AggressionState { get { return eAggressionState.Passive; } set { } }
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