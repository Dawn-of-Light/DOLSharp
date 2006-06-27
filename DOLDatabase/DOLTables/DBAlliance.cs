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
//
// this code has been done by breeze and duff a long time ago
//

using System;
using DOL.Database;
using DOL.Database.Attributes;
//using System.Collections;


namespace DOL.Database
{
	/// <summary>
	/// DBalliance is table for alliance of guild
	/// </summary>
	[DataTable(TableName="GuildAlliance")]
	public class DBAlliance : DataObject
	{
		static bool		m_autoSave;

		private string	m_allianceName;
		private string	m_motd;

		/// <summary>
		/// create an alliance
		/// </summary>
		public DBAlliance()
		{
			m_allianceName = "default alliance name";
			m_autoSave=false;
		}

		/// <summary>
		/// autosave table
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		/// <summary>
		/// Name of the alliance 
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string AllianceName
		{
			get
			{
				return m_allianceName;
			}
			set
			{
				Dirty = true;
				m_allianceName = value;
			}
		}

		/// <summary>
		/// Message Of The Day  of the Alliance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Motd
		{
			get
			{
				return m_motd;
			}
			set
			{
				Dirty = true;
				m_motd = value;
			}
		}

		/// <summary>
		/// Guild leader of alliance
		/// </summary>
		[Relation(LocalField = "AllianceName", RemoteField = "GuildName", AutoLoad = true, AutoDelete=false)]
		public DBguild DBguildleader;

		/// <summary>
		/// All guild in this alliance
		/// </summary>
		[Relation(LocalField = "ObjectId", RemoteField = "AllianceID", AutoLoad = true, AutoDelete=false)]
		public DBguild[] DBguilds;
	}
}