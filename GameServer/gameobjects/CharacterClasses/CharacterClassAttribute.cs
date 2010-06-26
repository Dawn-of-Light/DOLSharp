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
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// Denotes a class as a DOL Character class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CharacterClassAttribute : Attribute
	{
		protected string m_name;
		protected string m_femaleName;
		protected string m_basename;
		protected int m_id;

		public CharacterClassAttribute(int id, string name, string basename, string femalename)
		{
			m_basename = basename;
			m_name = name;
			m_id = id;
			m_femaleName = femalename;
		}

		public CharacterClassAttribute(int id, string name, string basename)
		{
			m_basename = basename;
			m_name = name;
			m_id = id;
		}

		public int ID
		{
			get
			{
				return m_id;
			}
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public string BaseName
		{
			get
			{
				return m_basename;
			}
		}

		public string FemaleName
		{
			get
			{
				return m_femaleName;
			}
		}
	}
}
