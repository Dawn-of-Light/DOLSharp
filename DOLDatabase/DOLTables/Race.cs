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
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable( TableName = "Race" )]
	public class Race : DataObject
	{
		protected bool m_autoSave;
		protected int m_ID = 0;
		protected string m_Name;
		protected sbyte m_ResistBody;
		protected sbyte m_ResistCold;
		protected sbyte m_ResistCrush;
		protected sbyte m_ResistEnergy;
		protected sbyte m_ResistHeat;
		protected sbyte m_ResistMatter;
		protected sbyte m_ResistNatural;
		protected sbyte m_ResistSlash;
		protected sbyte m_ResistSpirit;
		protected sbyte m_ResistThrust;
		protected sbyte m_DamageType;

		public Race() : base()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
				Dirty = true;
			}
		}

		[DataElement( AllowDbNull = false, Index = true, Unique = true )]
		public int ID
		{
			get { return m_ID; }
			set
			{
				Dirty = true;
				m_ID = value;
			}
		}

		[DataElement( AllowDbNull = false, Index = true, Unique = true )]
		public string Name
		{
			get { return m_Name; }
			set
			{
				Dirty = true;
				m_Name = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistBody
		{
			get { return m_ResistBody; }
			set
			{
				Dirty = true;
				m_ResistBody = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistCold
		{
			get { return m_ResistCold; }
			set
			{
				Dirty = true;
				m_ResistCold = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistCrush
		{
			get { return m_ResistCrush; }
			set
			{
				Dirty = true;
				m_ResistCrush = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistEnergy
		{
			get { return m_ResistEnergy; }
			set
			{
				Dirty = true;
				m_ResistEnergy = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistHeat
		{
			get { return m_ResistHeat; }
			set
			{
				Dirty = true;
				m_ResistHeat = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistMatter
		{
			get { return m_ResistMatter; }
			set
			{
				Dirty = true;
				m_ResistMatter = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistNatural
		{
			get { return m_ResistNatural; }
			set
			{
				Dirty = true;
				m_ResistNatural = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistSlash
		{
			get { return m_ResistSlash; }
			set
			{
				Dirty = true;
				m_ResistSlash = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistSpirit
		{
			get { return m_ResistSpirit; }
			set
			{
				Dirty = true;
				m_ResistSpirit = value;
			}
		}

		[DataElement( AllowDbNull = true )]
		public sbyte ResistThrust
		{
			get { return m_ResistThrust; }
			set
			{
				Dirty = true;
				m_ResistThrust = value;
			}
		}
	}
}