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
	/// Summary description for a SpellCraftGemTemplate
	/// </summary> 
	public class SpellCraftGemTemplate : GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The qualitypercent of the template
		/// </summary>
		protected byte m_quality;

		/// <summary>
		/// The bonus type of this template
		/// </summary>
		protected eProperty m_bonusType;

		/// <summary>
		/// How much the bonus type will be increased
		/// </summary>
		protected byte m_bonus;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the quality percent of the template
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
		/// Gets or sets of how much the bonus type will increase
		/// </summary>
		public byte Bonus
		{
			get { return m_bonus; }
			set	{ m_bonus = value; }
		}

		#endregion

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.SpellcraftGem; }
		}

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			SpellCraftGem item = new SpellCraftGem();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight;
			item.Value = m_value;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
			item.Quality = m_quality;
			item.BonusType = m_bonusType;
			item.Bonus = m_bonus;
			return item;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList GetDelveInfo(GamePlayer player)
		{
			ArrayList list = (ArrayList) base.GetDelveInfo(player);

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