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
using DOL.Database;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the news event
	/// </summary>
	public class NewsEventArgs : EventArgs
	{
		/// <summary>
		/// Holds the target news for this event
		/// </summary>
		private DBNews m_news;
		
		/// <summary>
		/// Constructs a new event argument class for the
		/// news events 
		/// </summary>
		/// <param name="account"></param>
		public NewsEventArgs(DBNews news)
		{
			m_news = news;
		}

		/// <summary>
		/// Gets the target news for this event
		/// </summary>
		public DBNews News
		{
			get
			{
				return m_news;
			}
		}
	}
}
