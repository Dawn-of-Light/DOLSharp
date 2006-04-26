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

namespace DOL.Database
{
    /// <summary>
    /// interface for data layer facter which will returne all data access object which manage all data object
    /// </summary>
    interface IDAOFactory
    {
        /// <summary>
        /// register the data access object and
        /// throws an exception if already registered
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dataAccessObject"></param>
        void RegisterDao(Type type, IDataAccessObject dataAccessObject);
        
        /// <summary>
        /// pick the data access object of this type and return it
        /// </summary>
        /// <param name="type"> the type of data object the dao manage</param>
        /// <returns></returns>
        IDataAccessObject GetDao(Type type);

        /// <summary>
        /// synchronous save that we can call on server stop
        /// probably could use a callback for GUI to watch the process
        /// </summary>
        void SaveAll();
    }
}
