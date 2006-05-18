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
using DOL.Database.DataAccessInterfaces;

namespace DOL.Database
{
    /// <summary>
    /// An interface for data layer factory which will return all
    /// data access object which manage all data object.
    /// </summary>
	public interface IDatabaseMgr : IDisposable
    {
        /// <summary>
        /// Registers the data access object and
        /// throws an exception if already registered.
        /// </summary>
        /// <typeparam name="T">The DAO type to register.</typeparam>
        /// <param name="dataAccessObject">The DAO to register.</param>
        void Register<T>(IDataAccessObject dataAccessObject)
			where T : class, IDataAccessObject;

    	/// <summary>
		/// Creates an instance of <paramref name="dao"/> type and registers it
		/// as <paramref name="daoInterface"/> type.
    	/// </summary>
    	/// <param name="daoInterface">The DAO interface to register.</param>
    	/// <param name="dao">The DAO type to register.</param>
    	/// <param name="param">The DAO params that are passed to constructor.</param>
		void Register(Type daoInterface, Type dao, IDictionary<string, string> param);
        
        /// <summary>
        /// Gets the data access object of this type and return it.
        /// </summary>
		/// <typeparam name="T">The type of data object to register.</typeparam>
        /// <returns></returns>
        T Using<T>()
        	where T : class, IDataAccessObject;

        /// <summary>
        /// Saves all data in registered DAOs, synchronous.
        /// </summary>
        void SaveAll();

    	/// <summary>
		/// Erases old database schemas and creates new ones.
    	/// </summary>
		void CreateSchemas();

    	/// <summary>
		/// Verifies database schemas.
    	/// </summary>
		IList<string> VerifySchemas();
    }
}
