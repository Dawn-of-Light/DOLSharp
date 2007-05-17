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

namespace DOL.GS.Behaviour.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequirementAttribute :Attribute
    {
        private eRequirementType requirementType;

        public eRequirementType RequirementType
        {
            get { return requirementType; }
            set { requirementType = value; }
        }        

        private bool isNullableN;

        public bool IsNullableN
        {
            get { return isNullableN; }
            set { isNullableN = value; }
        }

        private bool isNullableV;

        public bool IsNullableV
        {
            get { return isNullableV; }
            set { isNullableV = value; }
        }

        private Object defaultValueN;

        public Object DefaultValueN
        {
            get { return defaultValueN; }
            set { defaultValueN = value; }
        }

        private Object defaultValueV;

        public Object DefaultValueV
        {
            get { return defaultValueV; }
            set { defaultValueV = value; }
        }
    }
}
