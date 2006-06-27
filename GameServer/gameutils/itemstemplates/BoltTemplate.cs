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
	/// Summary description for a BoltTemplate
	/// </summary> 
	public class BoltTemplate : AmmunitionTemplate
	{
		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Bolt; }
		}

		/// <summary>
		/// Create a object usable by players using this template (a stack of 20 bolts)
		/// </summary>
		public override GenericItem CreateInstance()
		{
			Bolt item = new Bolt();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight * m_packSize;
			item.Value = m_value * m_packSize;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
			item.Count = m_packSize;
			item.MaxCount = m_maxCount;
			item.Precision = m_precision;
			item.Damage = m_damage;
			item.Range = m_range;
			item.DamageType = m_damageType;
			return item;
		}
	}
}	