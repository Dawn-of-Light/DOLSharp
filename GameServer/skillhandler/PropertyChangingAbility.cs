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

using DOL.Database;

namespace DOL.GS.SkillHandler
{
    public class PropertyChangingAbility : Ability
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// property to modify
		protected eProperty[] m_property;
		public eProperty[] Properties
		{
			get { return m_property; }
		}

		public PropertyChangingAbility(DBAbility dba, int level, eProperty[] property)
			: base(dba, level)
		{
			m_property = property;
		}

		public PropertyChangingAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level)
		{
			m_property = new eProperty[] { property };
		}

		/// <summary>
		/// Get the Amount of Bonus for this ability at a particular level
		/// </summary>
		/// <param name="level">The level</param>
		/// <returns>The amount</returns>
		public virtual int GetAmountForLevel(int level)
		{
			return 0;
		}

		/// <summary>
		/// The bonus amount at this abilities level
		/// </summary>
		public int Amount
		{
			get
			{
				return GetAmountForLevel(Level);
			}
		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		public virtual void SendUpdates(GameLiving target)
		{
		}

		/// <summary>
		/// Unit for values like %
		/// </summary>
		protected virtual string ValueUnit { get { return ""; } }

		public override void Activate(GameLiving living, bool sendUpdates)
		{
			if (m_activeLiving == null)
			{
				m_activeLiving = living;
				foreach (eProperty property in m_property)
				{
					living.AbilityBonus[(int)property] += GetAmountForLevel(living.CalculateSkillLevel(this));
				}
				
				if (sendUpdates)
					SendUpdates(living);
			}
			else
			{
				log.WarnFormat("ability {0} already activated on {1}", Name, living.Name);
			}
		}

		public override void Deactivate(GameLiving living, bool sendUpdates)
		{
			if (m_activeLiving != null)
			{
				foreach (eProperty property in m_property)
				{
					living.AbilityBonus[(int)property] -= GetAmountForLevel(living.CalculateSkillLevel(this));
				}
				if (sendUpdates) SendUpdates(living);
				m_activeLiving = null;
			}
			else
			{
				log.Warn("ability " + Name + " already deactivated on " + living.Name);
			}
		}

		public override void OnLevelChange(int oldLevel, int newLevel = 0)
		{
			if (newLevel == 0)
				newLevel = Level;

			foreach (eProperty property in m_property)
			{
				m_activeLiving.AbilityBonus[(int)property] += GetAmountForLevel(newLevel) - GetAmountForLevel(oldLevel);
			}

			SendUpdates(m_activeLiving);
		}
	}
}
