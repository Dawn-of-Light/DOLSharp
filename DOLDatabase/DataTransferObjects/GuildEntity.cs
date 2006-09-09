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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public class GuildEntity
	{
		private int m_id;
		private int m_alliance;
		private long m_bountyPoints;
		private bool m_due;
		private string m_email;
		private int m_emblem;
		private string m_guildName;
		private int m_level;
		private long m_meritPoints;
		private string m_motd;
		private string m_oMotd;
		private long m_realmPoints;
		private long m_totalMoney;
		private string m_webpage;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int Alliance
		{
			get { return m_alliance; }
			set { m_alliance = value; }
		}
		public long BountyPoints
		{
			get { return m_bountyPoints; }
			set { m_bountyPoints = value; }
		}
		public bool Due
		{
			get { return m_due; }
			set { m_due = value; }
		}
		public string Email
		{
			get { return m_email; }
			set { m_email = value; }
		}
		public int Emblem
		{
			get { return m_emblem; }
			set { m_emblem = value; }
		}
		public string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
		}
		public int Level
		{
			get { return m_level; }
			set { m_level = value; }
		}
		public long MeritPoints
		{
			get { return m_meritPoints; }
			set { m_meritPoints = value; }
		}
		public string Motd
		{
			get { return m_motd; }
			set { m_motd = value; }
		}
		public string OMotd
		{
			get { return m_oMotd; }
			set { m_oMotd = value; }
		}
		public long RealmPoints
		{
			get { return m_realmPoints; }
			set { m_realmPoints = value; }
		}
		public long TotalMoney
		{
			get { return m_totalMoney; }
			set { m_totalMoney = value; }
		}
		public string Webpage
		{
			get { return m_webpage; }
			set { m_webpage = value; }
		}
	}
}
