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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net;
namespace DOL.Database2
{
    /// <summary>
    /// Provides a DBMS-near format for saving Object information
    /// </summary>
    public class DatabaseObjectInformation
    {
        public UInt64 UniqueID; 
        public string TypeName;
        public Stream Data;
    }
    /// <summary>
    /// Provides Database Service
    /// </summary>
    public abstract class DatabaseProvider
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected bool m_Connected;
        /// <summary>
        /// Connection state. If this is set to true , the DatabaseProvider should establish a connection.
        /// </summary>
        public bool Connected
        {
            get{return m_Connected;}
            set{
                if(value && !m_Connected)
                    OpenConnection();
                if(!value && m_Connected)
                    CloseConnection();
            }                    
        }
        public abstract string ConnectionString
        {
            get;
            set;
        }
        public abstract void OpenConnection();
        public abstract void CloseConnection();
        // The queue has been chosen here because it might be impossible for the DBMS to determine the information  
        public abstract Queue<DatabaseObjectInformation> GetAllObjects();
        public abstract DatabaseObjectInformation GetObjectByID(UInt64 ID);
        public abstract DatabaseObjectInformation[] GetObjectsByType(Type Type);
        public abstract DatabaseObjectInformation[] GetObjectsByTypeName(string TypeName);
        public abstract void InsertOrUpdateObjectData(UInt64 ObjectID, Type Type, Stream Data );
        public abstract UInt64 GetNewUniqueID();
        public abstract void DeleteObject(UInt64 ObjectID);
    }
}
