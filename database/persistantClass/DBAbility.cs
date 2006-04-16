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

namespace DOL.GS.Database
{
	/// <summary>
	/// The ability table
	/// </summary>
	public class DBAbility
	{
		protected string m_keyName;
		protected string m_name;
		protected int	 m_iconID;		// 0 if no icon, ability icons start at 0x190
		protected string m_description;

		
		public string KeyName
		{
			get 
			{
				return m_keyName;	
			}
			set	
			{
				m_keyName = value;
			}
		}

		/// <summary>
		/// Name of this ability
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;	
			}
			set	
			{
				m_name = value;
			}
		}

		/// <summary>
		/// icon of ability
		/// </summary>
		public int IconID
		{
			get 
			{	
				return m_iconID;	
			}
			set	
			{
				m_iconID = value;
			}
		}

		/// <summary>
		/// Small description of this ability
		/// </summary>
		public string Description
		{
			get 
			{	
				return m_description;
			}
			set	
			{
				m_description = value;
			}
		}
	}
}
