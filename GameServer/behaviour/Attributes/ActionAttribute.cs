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
    public class ActionAttribute :Attribute
    {

        private eActionType actionType;

        public eActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }

        private bool isNullableP;

        public bool IsNullableP
        {
            get { return isNullableP; }
            set { isNullableP = value; }
        }

        private bool isNullableQ;

        public bool IsNullableQ
        {
            get { return isNullableQ; }
            set { isNullableQ = value; }
        }

        private Object defaultValueP;

        public Object DefaultValueP
        {
            get { return defaultValueP; }
            set { defaultValueP = value; }
        }

        private Object defaultValueQ;

        public Object DefaultValueQ
        {
            get { return defaultValueQ; }
            set { defaultValueQ = value; }
        }
        
    }
}
