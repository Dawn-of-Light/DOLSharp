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

}
