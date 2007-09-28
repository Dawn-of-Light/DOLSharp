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
using System.Text;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "BugReport")]
	public class BugReport : DataObject
	{
		private int m_reportID;
		private string m_message;
		private string m_submitter;
		private DateTime m_dateSubmitted;
		private string m_closedBy;
		private DateTime m_dateClosed;
		private bool m_autoSave;

		public BugReport()
		{
			m_message = "";
			m_submitter = "";
			m_dateSubmitted = DateTime.Now;
			m_closedBy = "";
			m_autoSave = true;
		}

		public override bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		[PrimaryKey]//DataElement(AllowDbNull = false, Unique = true)]
		public int ID
		{
			get { return m_reportID; }
			set
			{
				m_reportID = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Message
		{
			get { return m_message; }
			set
			{
				Dirty = true;
				m_message = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Submitter
		{
			get { return m_submitter; }
			set
			{
				Dirty = true;
				m_submitter = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public DateTime DateSubmitted
		{
			get { return m_dateSubmitted; }
			set
			{
				Dirty = true;
				m_dateSubmitted = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string ClosedBy
		{
			get { return m_closedBy; }
			set
			{
				Dirty = true;
				m_closedBy = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public DateTime DateClosed
		{
			get { return m_dateClosed; }
			set
			{
				Dirty = true;
				m_dateClosed = value;
			}
		}
	}
}
