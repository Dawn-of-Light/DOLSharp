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
	///NpcXEquipment allows to set a Mob inventory equipment, and link default combat behavior depending on equipment.
	/// </summary>
	[DataTable(TableName = "npc_xequipment")]
	public class NpcXEquipment : DataObject
	{
		private long m_npc_xequipmentID;
		
		/// <summary>
		/// NpcXEquipment Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xequipmentID {
			get { return m_npc_xequipmentID; }
			set { m_npc_xequipmentID = value; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Reference to Spot ID
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 150)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_templateID;
		
		/// <summary>
		/// Reference to Equipment List Template ID
		/// </summary>
		[DataElement(AllowDbNull = true, Index = true, Varchar = 150)]
		public string TemplateID {
			get { return m_templateID; }
			set { m_templateID = value; Dirty = true; }
		}
		
		private string m_fightingTemplateID;
		
		/// <summary>
		/// Link to Fighting Template Table
		/// </summary>
		[DataElement(AllowDbNull = true, Index = true, Varchar = 150)]
		public string FightingTemplateID {
			get { return m_fightingTemplateID; }
			set { m_fightingTemplateID = value; }
		}
		
		private byte m_absorb;
		
		/// <summary>
		/// Armor Physical Absorb to mimic equipment Absorb
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte Absorb {
			get { return m_absorb; }
			set { m_absorb = value; Dirty = true; }
		}
		
		private byte m_magicAbsorb;
		
		/// <summary>
		/// Magical Absorb to set natural magic resistance (not added to resist bonus/malus)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MagicAbsorb {
			get { return m_magicAbsorb; }
			set { m_magicAbsorb = value; }
		}
		
		private bool m_isCloakHoodUp;
		
		/// <summary>
		/// Activate Cloak's Hood (should need a cloack)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool IsCloakHoodUp {
			get { return m_isCloakHoodUp; }
			set { m_isCloakHoodUp = value; Dirty = true; }
		}
		
		private string m_visibleWeaponSlots;
		
		/// <summary>
		/// Visible Weapon Slots, can be : null(auto), none, right, left, both, twohand, ranged
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 7)]
		public string VisibleWeaponSlots {
			get { return m_visibleWeaponSlots; }
			set { m_visibleWeaponSlots = value; Dirty = true; }
		}
		
		private string m_packageID;
		
		/// <summary>
		/// Used for Import / export
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string PackageID {
			get { return m_packageID; }
			set { m_packageID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Link to Equipment Template (n,n), multiple sloted equipment could result in randomizing or warning...
		/// </summary>
		[Relation(LocalField = "TemplateID", RemoteField = "TemplateID", AutoLoad = true, AutoDelete = false)]
		public NPCEquipment[] EquipmentList;
		
		/// <summary>
		/// Link to Fighting Template (n, 1), one attachment by Equiptemplate.
		/// </summary>
		[Relation(LocalField = "FightingTemplateID", RemoteField = "FightingTemplateID", AutoLoad = true, AutoDelete = false)]
		public NpcFightingTemplate FightingTemplate;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcXEquipment()
		{
		}
	}
}
