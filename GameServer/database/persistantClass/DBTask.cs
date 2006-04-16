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
	/// Database Storage of Tasks
	/// </summary>
	public class DBTask
	{
		protected int		m_PersistantGameObjectID;		
		protected DateTime	m_TimeOut = DateTime.Now.AddHours(2);
		protected String	m_TaskType = null;		// name of classname		
		protected int		m_TasksDone = 0;
		protected string	m_customPropertiesString = null;
		

		public int PersistantGameObjectID
		{
			get
			{
				return PersistantGameObjectID;
			}
			set	
			{
				PersistantGameObjectID = value;
			}
		}		

		public DateTime TimeOut
		{
			get 
			{
				return m_TimeOut;
			}
			set	
			{
				m_TimeOut = value;
			}
		}		

		public String TaskType
		{
			get 
			{
				return m_TaskType;
			}
			set	
			{
				m_TaskType = value;
			}
		}

		public int TasksDone
		{
			get 
			{
				return m_TasksDone;
			}
			set	
			{
				m_TasksDone = value;
			}
		}		

		public string CustomPropertiesString
		{
			get
			{
				return m_customPropertiesString;
			}
			set
			{
				m_customPropertiesString = value;
			}
		}
	}
}
