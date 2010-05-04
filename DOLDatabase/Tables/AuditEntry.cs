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
	/// Tracks and holds generic audit history (account changes, chat history, etc) across the game server.
	/// </summary>
	[DataTable(TableName = "AuditEntry")]
	public class AuditEntry : DataObject
	{
		/// <summary>
		/// The time when the audit occured.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public DateTime AuditTime { get; set; }

		/// <summary>
		/// The account ID, if applicable, related to the audit entry.
		/// </summary>
		/// <remarks>
		/// This would be the unique ID for a player's account.
		/// </remarks>
		[DataElement]
		public string AccountID { get; set; }

		/// <summary>
		/// The remote host, if applicable, related to the audit entry.
		/// </summary>
		/// <remarks>
		/// This would be the IP address of the player that initiated the action that triggered an audit.
		/// </remarks>
		[DataElement]
		public string RemoteHost { get; set; }
	
		/// <summary>
		/// The main type of the audit entry. 
		///	</summary>
		/// <remarks>
		/// This would refer to things like account history, character history, chat history, etc.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int AuditType { get; set; }

		/// <summary>
		/// The subtype of the audit entry.
		/// </summary>
		/// <remarks>
		/// This would refer to specific actions like an account login, character delete, a private chat message, etc.
		/// </remarks>
		[DataElement(AllowDbNull = false)]
		public int AuditSubtype { get; set; }

		/// <summary>
		/// The old value, if applicable, related to the audit entry.
		/// </summary>
		/// <remarks>
		/// This could be, for example, the old value set for the e-mail address of an account after the player updated it.
		/// </remarks>
		[DataElement]
		public string OldValue { get; set; }

		/// <summary>
		/// The new value of the audit entry.
		/// </summary>
		/// <remarks>
		/// This could be, for example, the new value set for the e-mail address of an account after the player updated it.
		/// </remarks>
		[DataElement(AllowDbNull = false)]
		public string NewValue { get; set; }
	}
}