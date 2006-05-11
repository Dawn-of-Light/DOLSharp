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

namespace DOL.Database.TransferObjects
{
	public class DbSinglePermission
	{
		private int		m_id;
		private string	m_playerID;
		private string	m_command;

		public DbSinglePermission()
		{
			m_playerID = "";
			m_command = "";
		}

		public int SinglePermissionID
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

		public string PlayerID
		{
			get
			{
				return m_playerID;
			}
			set
			{
				m_playerID = value;
			}
		}

		public string Command
		{
			get
			{
				return m_command;
			}
			set
			{
				m_command = value;
			}
		}
	}
}