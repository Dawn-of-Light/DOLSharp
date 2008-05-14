using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DOL.Database2;
namespace DOLStudio
{
    public class TypeBrowser :IEnumerable<Type>
    {
        public TypeBrowser()
        {
            m_Types = new List<Type>();
            RefreshTypes();   
        }
        public void RefreshTypes()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (typeof(DatabaseObject).IsAssignableFrom(t) && t != typeof(DatabaseObject))
                        m_Types.Add(t);
                }
            }
        }
        private List<Type> m_Types;


        #region IEnumerable<Type> Members

        public IEnumerator<Type> GetEnumerator()
        {
            return m_Types.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_Types.GetEnumerator();
        }

        #endregion
    }
}
