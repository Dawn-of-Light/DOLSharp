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
using System.Collections.Generic;
using System.Text;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Bonuses for artifacts.
	/// </summary>
	/// <author>Aredhel</author>
	[DataTable(TableName = "ArtifactBonus")]
	public class ArtifactBonus : DataObject
	{
		private String m_artifactID;
		private String m_version;
		private int m_level;
		private int m_bonus;
		private int m_bonusType;
		private int m_spellID;
		private int m_procSpellID;

		/// <summary>
		/// Create a new artifact bonus.
		/// </summary>
		public ArtifactBonus()
			: base() { }

		/// <summary>
		/// Whether to auto-save this object or not.
		/// </summary>
		public override bool AutoSave
		{
			get { return false; }
			set { }
		}

		/// <summary>
		/// The book ID.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String ArtifactID
		{
			get { return m_artifactID; }
			set
			{
				Dirty = true;
				m_artifactID = value;
			}
		}

		/// <summary>
		/// The artifact version.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public String Version
		{
			get { return m_version; }
			set
			{
				Dirty = true;
				m_version = value;
			}
		}

		/// <summary>
		/// The level this bonus is granted.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Level
		{
			get { return m_level; }
			set
			{
				Dirty = true;
				m_level = value;
			}
		}

		/// <summary>
		/// The bonus amount.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Bonus
		{
			get { return m_bonus; }
			set
			{
				Dirty = true;
				m_bonus = value;
			}
		}

		/// <summary>
		/// The bonus type.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int BonusType
		{
			get { return m_bonusType; }
			set
			{
				Dirty = true;
				m_bonusType = value;
			}
		}

		/// <summary>
		/// The spell ID.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int SpellID
		{
			get { return m_spellID; }
			set
			{
				Dirty = true;
				m_spellID = value;
			}
		}

		/// <summary>
		/// The proc spell ID.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int ProcSpellID
		{
			get { return m_procSpellID; }
			set
			{
				Dirty = true;
				m_procSpellID = value;
			}
		}
	}
}