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
	public enum eRespecType : byte
	{
		SingleLine		= 0,
		Full			= 1,
	}

	/// <summary>
	/// Summary description for a RespecStoneTemplate
	/// </summary> 
	public class RespecStoneTemplate : GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The type of respec point to give
		/// </summary>
		protected eRespecType m_respecType;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the spellId used by this template
		/// </summary>
		public eRespecType RespecType
		{
			get { return m_respecType; }
			set	{ m_respecType = value; }
		}

		#endregion

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			RespecStone item = new RespecStone();
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
			item.RespecType = m_respecType;
			return item;
		}
	}
}