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
/*
 * LightBringer Task System
 * Class:	DBTask
 * Author:	LightBringer
 * Editor:  Gandulf Kohlweiss
 * Date:	2004/06/19
*/

using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Database Storage of Tasks
	/// </summary>
#warning TODO comments
	[DataTable(TableName="character_task")]
	public class DBTask : DataObject
	{
		protected string	m_charName = "Empty";		
		protected DateTime	m_TimeOut = DateTime.Now.AddHours(2);
		protected String	m_TaskType = null;		// name of classname		
		protected int		m_TasksDone = 0;
		protected string	m_customPropertiesString = null;
		static bool			m_autoSave;

		public DBTask()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
		}

		//[DataElement(AllowDbNull=false,Unique=true)]
		[PrimaryKey]
		public string CharName
		{
			get {return m_charName;}
			set	
			{
				Dirty = true;
				m_charName = value;
			}
		}		

		[DataElement(AllowDbNull=true,Unique=false)]
		public DateTime TimeOut
		{
			get {return m_TimeOut;}
			set	
			{
				Dirty = true;
				m_TimeOut = value;
			}
		}		

		[DataElement(AllowDbNull=true,Unique=false)]
		public String TaskType
		{
			get {return m_TaskType;}
			set	
			{
				Dirty = true;
				m_TaskType = value;
			}
		}

		[DataElement(AllowDbNull=true,Unique=false)]
		public int TasksDone
		{
			get {return m_TasksDone;}
			set	
			{
				Dirty = true;
				m_TasksDone = value;
			}
		}		

		[DataElement(AllowDbNull=true,Unique=false)]
		public string CustomPropertiesString
		{
			get
			{
				return m_customPropertiesString;
			}
			set
			{
				Dirty = true;
				m_customPropertiesString = value;
			}
		}
	}
}
