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
	/// defines what abilities are available at what speclevels
	/// </summary>
	[DataTable(TableName="SpecXAbility")]
	public class DBSpecXAbility : DataObject
	{
		protected string m_spec;
		protected string m_abilitykey;
		protected int m_abilitylevel;
		protected int m_speclevel;

		public DBSpecXAbility()
		{
			AllowAdd = false;
		}

		[DataElement(AllowDbNull=false)]
		public string Spec
		{
			get { return m_spec; }
			set { m_spec = value; Dirty = true; }
		}

		[DataElement(AllowDbNull=false)]
		public int SpecLevel
		{
			get { return m_speclevel; }
			set {
				Dirty = true;
				m_speclevel = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string AbilityKey
		{
			get { return m_abilitykey; }
			set { m_abilitykey = value; Dirty = true; }
		}

		[DataElement(AllowDbNull=false)]
		public int AbilityLevel
		{
			get { return m_abilitylevel; }
			set {
				Dirty = true;
				m_abilitylevel = value;
			}
		}
	}
}
