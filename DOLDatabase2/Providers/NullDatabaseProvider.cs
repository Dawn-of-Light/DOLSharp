/*
 * (c) 2008 Julian Bangert 
 * This file is part of
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
using System.Linq;
using System.Text;

namespace DOL.Database2.Providers
{
    public class NullDatabaseProvider : DatabaseProvider
    {
        private UInt64 m_currentID = 1;
        public override string ConnectionString
        {
            get{ return "/dev/null virtual implementation ;-)";}
            set {  }
        }

        public override void OpenConnection()
        {
        }

        public override void CloseConnection()
        {
        }

        public override Queue<DatabaseObjectInformation> GetAllObjects()
        {
            return new Queue<DatabaseObjectInformation>();
        }

        public override DatabaseObjectInformation GetObjectByID(ulong ID)
        {
            return null;
        }

        public override Queue<DatabaseObjectInformation> GetObjectsByTypeName(string TypeName)
        {
            return new Queue<DatabaseObjectInformation>();
        }
        public override void InsertOrUpdateObjectData(ulong ObjectID, Type Type, System.IO.Stream Data)
        {
            return;
        }

        public override ulong GetNewUniqueID()
        {
            return m_currentID++;
        }

        public override void DeleteObject(ulong ObjectID)
        {
            return;
        }
    }
}
