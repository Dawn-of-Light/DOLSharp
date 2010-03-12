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
namespace DOL.Database.Connection
{
	/// <summary>
	/// Enum what Datatstorage should be used
	/// </summary>
	public enum ConnectionType
	{
		/// <summary>
		/// Use XML-Files as Database
		/// </summary>
		DATABASE_XML,
		/// <summary>
		/// Use the internal MySQL-Driver for Database
		/// </summary>
		DATABASE_MYSQL,
		/// <summary>
		/// Use Microsoft SQL-Server
		/// </summary>
		DATABASE_MSSQL,
		/// <summary>
		/// Use an ODBC-Datasource
		/// </summary>
		DATABASE_ODBC,
		/// <summary>
		/// Use an OLEDB-Datasource
		/// </summary>
		DATABASE_OLEDB
	}
}