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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a SpellCraftGem
	/// </summary> 
	public class SpellCraftGem : GenericItem
	{
		#region Declaraction
		/// <summary>
		/// The quality percent of this gem
		/// </summary>
		private byte m_quality;

		/// <summary>
		/// The bonus type of this gem
		/// </summary>
		private eProperty m_bonusType;

		/// <summary>
		/// How much points the bonus type will be increased
		/// </summary>
		private byte m_bonus;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the quality percent of this gem
		/// </summary>
		public byte Quality
		{
			get { return m_quality; }
			set	{ m_quality = value; }
		}

		/// <summary>
		/// Gets or sets the bonus type the gem will increase
		/// </summary>
		public eProperty BonusType
		{
			get { return m_bonusType; }
			set	{ m_bonusType = value; }
		}

		/// <summary>
		/// Gets or sets how much points the bonus type will increased
		/// </summary>
		public byte Bonus
		{
			get { return m_bonus; }
			set	{ m_bonus = value; }
		}

		#endregion

		/// <summary>
		/// Gets the object type of the item (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.SpellcraftGem; }
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = (ArrayList) base.DelveInfo;

				if(SkillBase.CheckPropertyType(BonusType, ePropertyType.Focus))
				{
					list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName(BonusType), Bonus));
				}
				else
				{
					//- Axe: 5 pts
					//- Strength: 15 pts
					//- Constitution: 15 pts
					//- Hits: 40 pts
					//- Fatigue: 8 pts
					//- Heat: 7%
					//Bonus to casting speed: 2%
					//Bonus to armor factor (AF): 18
					//Power: 6 % of power pool.
					list.Add(string.Format("- {0}: {1}{2}", SkillBase.GetPropertyName(BonusType), Bonus.ToString("+0;-0;0"), ((BonusType == eProperty.PowerPool) || (BonusType >= eProperty.Resist_First && BonusType <= eProperty.Resist_Last)) ? ( (BonusType == eProperty.PowerPool) ? "% of power pool." : "%" ) : " pts"));
				}
				
				return list;
			}
		}
	}
}	