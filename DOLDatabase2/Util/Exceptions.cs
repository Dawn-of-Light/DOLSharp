using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database2
{
    class PropertyFieldNotFoundException : Exception
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
    class DatabaseObjectNotFoundException : Exception
    {
        public DatabaseObjectNotFoundException()
        {
        }
        public DatabaseObjectNotFoundException(UInt64 ID)
            : base("Could not retrieve DatabaseObject with ID "+ID)
        {
        }
    }
    class NotComparableException : Exception
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
