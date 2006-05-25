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
using System.Collections.Generic;
using System.Text;

namespace DOL.Database.DataAccessInterfaces
{
	/// <summary>
	/// Generic interface with base DAO methods.
	/// </summary>
	/// <typeparam name="TTransferObject">The transfer object's type.</typeparam>
	/// <typeparam name="TPrimaryKey">The transfer object's primary key type.</typeparam>
	public interface IGenericDao<TTransferObject, TPrimaryKey> : IDataAccessObject
	{
		/// <summary>
		/// Finds an object by its primary key.
		/// </summary>
		/// <param name="key">The primary key.</param>
		/// <returns>The found object or null.</returns>
		TTransferObject Find(TPrimaryKey key);

		/// <summary>
		/// Saves an object into a database.
		/// </summary>
		/// <param name="obj">The object to save.</param>
		void Save(TTransferObject obj);

		/// <summary>
		/// Updates the persistent instance with data from transfer object.
		/// </summary>
		/// <param name="obj">The data.</param>
		void Update(TTransferObject obj);

		/// <summary>
		/// Deletes an object from a database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		void Delete(TTransferObject obj);

		/// <summary>
		/// Gets the count of all stored objects.
		/// </summary>
		/// <value>The count of all objects.</value>
		int CountAll();
	}
}
