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

namespace DOL.GS.DatabaseConverters
{
	/// <summary>
	/// Attribute that denotes a class as a database converter
	/// from previous version to the specified in attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class DatabaseConverterAttribute : Attribute
	{
		private int m_targetVersion;

		/// <summary>
		/// Constructs new attribute for database converter classes
		/// </summary>
		/// <param name="targetVersion">Target database version after convertion</param>
		public DatabaseConverterAttribute(int targetVersion)
		{
			m_targetVersion = targetVersion;
		}

		public int TargetVersion
		{
			get { return m_targetVersion; }
		}
	}
}
