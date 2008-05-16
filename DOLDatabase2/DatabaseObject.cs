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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database2
{
    [Serializable]
    public class DatabaseObject : IDatabaseObject
    {
        public DatabaseObject()
        {
            m_ID = DatabaseLayer.Instance.GetNewUniqueID();
            DatabaseLayer.Instance.RegisterDatabaseObject(this);
        }
        private  UInt64 m_ID = 0;
        [NonSerialized]
        private bool m_writetodatabase = false;
        protected bool m_Dirty;
        [Category("Database Object"),Description("Has data been modified")]  
        public bool Dirty
        {
            get { return m_Dirty; }
        }
        /// <summary>
        /// Contains the DatabaseObjects UniqueID
        /// </summary>
        [Category("Database Object"),Description("Unique Object ID"),Browsable(false)]  
        public UInt64 ObjectId
        {
            get { return m_ID; }
        }
        [Category("Database Object"), Description("Unique Object ID")]  
        public UInt64 ID
        {
            get { return m_ID; }
        }
        /// <summary>
        /// If this is set  to  false the object will not be saved to the persistant storage.
        /// </summary>
        [Category("Database Object"), Description("Object is automatically saved"),Browsable(false)] 
        public bool WriteToDatabase
        {
            get { return (m_writetodatabase && m_Dirty); }
            set { m_writetodatabase = value; }
        }
        /// <summary>
        /// Mostly for compatibility
        /// </summary>
        [Category("Database Object"), Description("Object is automatically saved")] 
        public bool AutoSave
        {
            get { return m_writetodatabase; }
            set { m_writetodatabase = value; }
        }
        /// <summary>
        /// Saves this object into database
        /// </summary>
        public void Save()
        {
            if(m_writetodatabase)
                DatabaseLayer.Instance.SaveObject(this);
        }
        public void DeleteDB()
        {
            DatabaseLayer.Instance.DeleteObject(this);
        }
        public virtual void FillObjectRelations()
        {
        }
    }
}
