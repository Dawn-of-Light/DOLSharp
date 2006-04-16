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
using System.Collections;

namespace DOL.GS
{
	/// <summary>
	/// CraftItemData
	/// </summary>
	public class CraftItemData
	{
		#region Declaraction

		/// <summary>
		/// The unique id of the craft data
		/// </summary>
		private int m_id;

		/// <summary>
		/// The GenericItemTemplate to craft
		/// </summary>
		private GenericItemTemplate m_templateToCraft;

		/// <summary>
		/// The level needed to buils this item
		/// </summary>
		private int m_craftingLevel;

		/// <summary>
		/// Witch crafting kill is needed to build this item
		/// </summary>
		private eCraftingSkill m_craftingSkill;

		/// <summary>
		/// The set of all needed raw material
		/// </summary>
		private Iesi.Collections.ISet m_rawMaterial;
		
		#endregion
		
		#region All get and set
		/// <summary>
		/// Get or set the unique id of this craft item
		/// </summary>
		public int CraftItemDataID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Get or set the item template to craft
		/// </summary>
		public GenericItemTemplate TemplateToCraft
		{
			get { return m_templateToCraft; }
			set	{ m_templateToCraft = value; }
		}

		/// <summary>
		/// Get or set the level needed to craft this iem
		/// </summary>
		public int CraftingLevel
		{
			get { return m_craftingLevel; }
			set	{ m_craftingLevel = value; }
		}
		
		/// <summary>
		/// Get or set the crafting skill needed to craft this item
		/// </summary>
		public eCraftingSkill CraftingSkill
		{
			get { return m_craftingSkill; }
			set { m_craftingSkill = value; }
		}

		/// <summary>
		/// Get or set the set of all needed raw material
		/// </summary>
		public Iesi.Collections.ISet RawMaterials
		{
			get
			{
				if(m_rawMaterial == null) m_rawMaterial = new Iesi.Collections.HybridSet();
				return m_rawMaterial;
			}
			set { m_rawMaterial = value; }
		}
		#endregion
	}
}
