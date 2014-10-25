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
	/// Spell Lines Tables Referencing Spell from Base or Spec Lines, Attach to Specialization using Spec KeyName
	/// </summary>
	[DataTable(TableName="SpellLine")]
	public class DBSpellLine : DataObject
	{
		protected int m_spellLineID;
		protected string m_name="unknown";
		protected string m_keyname;
		protected string m_spec="unknown";
		protected bool m_isBaseLine=true;
		protected int m_classIDHint;
		
		public DBSpellLine()
		{
			AllowAdd = false;
		}

		/// <summary>
		/// Primary Key Auto Inc
		/// </summary>
		[PrimaryKey(AutoIncrement=true)]
		public int SpellLineID {
			get { return m_spellLineID; }
			set { Dirty = true; m_spellLineID = value; }
		}
		
		/// <summary>
		/// Spell Line Key Name
		/// </summary>
		[DataElement(AllowDbNull=false, Unique=true)]
		public string KeyName
		{
			get
			{
				return m_keyname;
			}
			set
			{
				Dirty = true;
				m_keyname = value;
			}
		}

		/// <summary>
		/// Spell Line Display Name
		/// </summary>
		[DataElement(AllowDbNull=true, Varchar=255)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
		/// Specialization Key Name for Reference. (FK)
		/// </summary>
		[DataElement(AllowDbNull=true, Varchar=100, Index=true)]
		public string Spec
		{
			get
			{
				return m_spec;
			}
			set
			{
				Dirty = true;
				m_spec = value;
			}
		}

		/// <summary>
		/// Baseline or Specline ?
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public bool IsBaseLine
		{
			get
			{
				return m_isBaseLine;
			}
			set
			{
				Dirty = true;
				m_isBaseLine = value;
			}
		}
		
		/// <summary>
		/// Class ID hint or other values used by Specialization Handler
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int ClassIDHint {
			get { return m_classIDHint; }
			set { m_classIDHint = value; }
		}


	}
}
