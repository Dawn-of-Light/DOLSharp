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

namespace DOL.Database.DataTransferObjects
{
	/// <summary>
	/// 
	/// </summary>
	public class DbLineXSpell
	{
		protected int m_id;
		protected string m_line_name;
		protected int m_spellid;
		protected int m_level;

		public int LineXSpellID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}
		
		public string LineName
		{
			get
			{
				return m_line_name;
			}
			set
			{
				m_line_name = value;
			}
		}

		public int SpellID
		{
			get
			{
				return m_spellid;
			}
			set
			{
				m_spellid = value;
			}
		}

		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{
				m_level = value;
			}
		}
	}
}
