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
	/// Database Storage of ServerStats
	/// </summary>
	[DataTable(TableName = "serverstats")]
	public class ServerStats : DataObject
	{
		protected DateTime m_statdate;
		protected int m_clients;
		protected float m_cpu;
		protected int m_upload;
		protected int m_download;
		protected long m_memory;

		public ServerStats()
		{
			m_statdate = DateTime.Now;
			m_clients = 0;
			m_cpu = 0;
			m_upload = 0;
			m_download = 0;
			m_memory = 0;
		}

		[DataElement(AllowDbNull = false)]
		public DateTime StatDate
		{
			get { return m_statdate; }
			set
			{
				Dirty = true;
				m_statdate = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Clients
		{
			get { return m_clients; }
			set
			{
				Dirty = true;
				m_clients = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public float CPU
		{
			get { return m_cpu; }
			set
			{
				Dirty = true;
				m_cpu = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Upload
		{
			get { return m_upload; }
			set
			{
				Dirty = true;
				m_upload = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Download
		{
			get { return m_download; }
			set
			{
				Dirty = true;
				m_download = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public long Memory
		{
			get { return m_memory; }
			set
			{
				Dirty = true;
				m_memory = value;
			}
		}
	}
}