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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Npc Base Template Table, Holding basic data and appearance of Npc
	/// </summary>
	[DataTable(TableName = "npc_basetemplate")]
	public class NpcBaseTemplate : DataObject
	{
		private long m_npc_BaseTemplateID;
		
		/// <summary>
		/// Base Template Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_BaseTemplateID {
			get { return m_npc_BaseTemplateID; }
			set { m_npc_BaseTemplateID = value; }
		}
		
		private string m_templateID;
		
		/// <summary>
		/// String Key easier for management with Npc
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string TemplateID {
			get { return m_templateID; }
			set { m_templateID = value; Dirty = true; }
		}
		
		private string m_guild;
		
		/// <summary>
		/// Npc Template Guild Display.
		/// </summary>
		[DataElement(AllowDbNull = true, Index = true, Varchar = 200)]
		public string Guild {
			get { return m_guild; }
			set { m_guild = value; Dirty = true; }
		}
		
		private ushort m_model;
		
		/// <summary>
		/// Npc Template Model;
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Model {
			get { return m_model; }
			set { m_model = value; Dirty = true; }
		}
		
		private byte m_size;
		
		/// <summary>
		/// Npc Template Size.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Size {
			get { return m_size; }
			set { m_size = value; Dirty = true; }
		}
		
		private byte m_gender;
		
		/// <summary>
		/// Npc Template Gender.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte Gender {
			get { return m_gender; }
			set { if (value < 3) { m_gender = value; Dirty = true; } }
		}
		
		private bool m_ghost;
		
		/// <summary>
		/// Npc Template is Ghost
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Ghost {
			get { return m_ghost; }
			set { m_ghost = value; Dirty = true; }
		}
		
		private bool m_stealth;
		
		/// <summary>
		/// Npc Template is Stealth
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Stealth {
			get { return m_stealth; }
			set { m_stealth = value; Dirty = true; }
		}
		
		private bool m_dontShowName;
		
		/// <summary>
		/// Npc Template don't show his name
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool DontShowName {
			get { return m_dontShowName; }
			set { m_dontShowName = value; Dirty = true; }
		}
		
		private bool m_cantTarget;
		
		/// <summary>
		/// Npc Template can't be targetted
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool CantTarget {
			get { return m_cantTarget; }
			set { m_cantTarget = value; Dirty = true; }
		}
		
		private bool m_torch;
		
		/// <summary>
		/// Npc Template torch is lit
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Torch {
			get { return m_torch; }
			set { m_torch = value; Dirty = true; }
		}
		
		private bool m_statue;
		
		/// <summary>
		/// Npc Template is statue (not moving, not idling, no target...)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool Statue {
			get { return m_statue; }
			set { m_statue = value; Dirty = true; }
		}
		
		private string m_packageId;
		
		/// <summary>
		/// Package ID for import/export
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 255)]
		public string PackageId {
			get { return m_packageId; }
			set { m_packageId = value; Dirty = true; }
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public NpcBaseTemplate()
		{
			m_size = 50;
			m_gender = 0;
			m_ghost = false;
			m_stealth = false;
			m_dontShowName = false;
			m_cantTarget = false;
			m_torch = false;
			m_statue = false;
		}
	}
}
