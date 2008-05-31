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
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database2
{
    public class PropertyFieldNotFoundException : Exception
    {
        public PropertyFieldNotFoundException()
        {
        }
        public PropertyFieldNotFoundException(PropertyInfo pinf,Type tinfo)
            :base("Could not find property " + pinf.Name + " in type " + tinfo.Name)
        {        
        }
        public PropertyFieldNotFoundException(FieldInfo pinf,Type tinfo)
            :base("Could not find field " + pinf.Name + " in type " + tinfo.Name)
        {
        }
        public PropertyFieldNotFoundException(string Name, Type tinfo)
            :base("Could not find field " + Name + " in type " + tinfo.Name)
        {
            
        }
    }
    public class DatabaseObjectNotFoundException : Exception
    {
        public DatabaseObjectNotFoundException()
        {
        }
        public DatabaseObjectNotFoundException(UInt64 ID)
            : base("Could not retrieve DatabaseObject with ID "+ID)
        {
        }
    }
    public class NotComparableException : Exception
    {
        public NotComparableException(string SelectStatement,Type t)
            :base("Object of type " + t.Name + " in the statement " + SelectStatement +" do not implement IComparable")
        {
        }
        public NotComparableException( Type t)
            : base("Type " + t.Name + "does not implement IComparable")
        {
        }
    }
}
