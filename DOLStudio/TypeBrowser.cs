using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DOL.Database2;
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
