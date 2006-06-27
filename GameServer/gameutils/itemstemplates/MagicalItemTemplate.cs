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
	/// Summary description for a MagicalItemTemplate
	/// </summary> 
	public class MagicalItemTemplate : GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// How much charge the template actually have
		/// </summary>
		protected byte m_charge;

		/// <summary>
		/// The max charge number the template can have
		/// </summary>
		protected byte m_maxCharge;

		/// <summary>
		/// The spellId used by the object
		/// </summary>
		protected int m_spellID;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets how much charge the template actually have
		/// </summary>
		public byte Charge
		{
			get { return m_charge; }
			set	{ m_charge = value; }
		}

		/// <summary>
		/// Gets or sets the max number of charge the template can have
		/// </summary>
		public byte MaxCharge
		{
			get { return m_maxCharge; }
			set	{ m_maxCharge = value; }
		}

		/// <summary>
		/// Gets or sets the spellId used by this template
		/// </summary>
		public int SpellID
		{
			get { return m_spellID; }
			set	{ m_spellID = value; }
		}

		#endregion
	
		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get { return eObjectType.Magical; }
		}

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			MagicalItem item = new MagicalItem();
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
			item.Charge = m_charge;
			item.MaxCharge = m_maxCharge;
			item.SpellID = m_spellID;
			return item;
		}
	}
}	