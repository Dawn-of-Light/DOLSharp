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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct SpecializationEntity
	{
		private string m_keyName;
		private string m_description;
		private int m_icon;
		private string m_name;

		public string KeyName
		{
			get { return m_keyName; }
			set { m_keyName = value; }
		}
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}
		public int Icon
		{
			get { return m_icon; }
			set { m_icon = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
	}
}
