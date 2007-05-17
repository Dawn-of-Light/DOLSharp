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
    public class TriggerAttribute :Attribute
    {

        private bool global;

        /// <summary>
        /// Defiens wether the trigger is registered globaly (doesn't depends on implicity quests notifying of player etc or not
        /// </summary>
        public bool Global
        {
            get { return global; }
            set { global = value; }
        }
	

        private eTriggerType triggerType;

        public eTriggerType TriggerType
        {
            get { return triggerType; }
            set { triggerType = value; }
        }

        private bool isNullableK;

        public bool IsNullableK
        {
            get { return isNullableK; }
            set { isNullableK = value; }
        }

        private bool isNullableI;

        public bool IsNullableI
        {
            get { return isNullableI; }
            set { isNullableI = value; }
        }

        private Object defaultValueK;

        public Object DefaultValueK
        {
            get { return defaultValueK; }
            set { defaultValueK = value; }
        }

        private Object defaultValueI;

        public Object DefaultValueI
        {
            get { return defaultValueI; }
            set { defaultValueI = value; }
        }
	
    }
}
