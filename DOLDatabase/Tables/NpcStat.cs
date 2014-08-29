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
	/// Npc Stat Table allows to build Stat Template for Mobs
	/// </summary>
	[DataTable(TableName = "npc_stat")]
	public class NpcStat : DataObject
	{
		private long m_npc_statID;
		
		/// <summary>
		/// Npc Stat Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_statID {
			get { return m_npc_statID; }
			set { m_npc_statID = value; }
		}
		
		private string m_statID;
		
		/// <summary>
		/// Npc Stat Template ID for reference
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string StatID {
			get { return m_statID; }
			set { m_statID = value; Dirty = true; }
		}
		
		private ushort m_strength;
		
		/// <summary>
		/// Npc Base Strength 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Strength {
			get { return m_strength; }
			set { m_strength = value; Dirty = true; }
		}
		
		private ushort m_constitution;
		
		/// <summary>
		/// Npc Base Constitution 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Constitution {
			get { return m_constitution; }
			set { m_constitution = value; Dirty = true; }
		}
		
		private ushort m_dexterity;
		
		/// <summary>
		/// Npc Base Dexterity 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Dexterity {
			get { return m_dexterity; }
			set { m_dexterity = value; Dirty = true; }
		}
		
		private ushort m_quickness;
		
		/// <summary>
		/// Npc Base Quickness 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Quickness {
			get { return m_quickness; }
			set { m_quickness = value; Dirty = true; }
		}
		
		private ushort m_acuity;
		
		/// <summary>
		/// Npc Base Acuity 
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Acuity {
			get { return m_acuity; }
			set { m_acuity = value; Dirty = true; }
		}

		private double m_strengthIncrement;
		
		/// <summary>
		/// Npc Strength Increment * Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public double StrengthIncrement {
			get { return m_strengthIncrement; }
			set { m_strengthIncrement = value; Dirty = true; }
		}
		
		private double m_constitutionIncrement;
		
		/// <summary>
		/// Npc Constitution Increment * Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public double ConstitutionIncrement {
			get { return m_constitutionIncrement; }
			set { m_constitutionIncrement = value; Dirty = true; }
		}
		
		private double m_dexterityIncrement;
		
		/// <summary>
		/// Npc Dexterity Increment * Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public double DexterityIncrement {
			get { return m_dexterityIncrement; }
			set { m_dexterityIncrement = value; Dirty = true; }
		}
		
		private double m_quicknessIncrement;
		
		/// <summary>
		/// Npc Quickness Increment * Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public double QuicknessIncrement {
			get { return m_quicknessIncrement; }
			set { m_quicknessIncrement = value; Dirty = true; }
		}
		
		private double m_acuityIncrement;
		
		/// <summary>
		/// Npc Acuity Increment * Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public double AcuityIncrement {
			get { return m_acuityIncrement; }
			set { m_acuityIncrement = value; Dirty = true; }
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcStat()
		{
			m_strength = 60;
			m_constitution = 60;
			m_dexterity = 60;
			m_quickness = 60;
			m_acuity = 60;
			
			m_strengthIncrement = 1.0;
			m_constitutionIncrement = 1.0;
			m_dexterityIncrement = 1.0;
			m_quicknessIncrement = 1.0;
			m_acuityIncrement = 1.0;
		}
	}
}
