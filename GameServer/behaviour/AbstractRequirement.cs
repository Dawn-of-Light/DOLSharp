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
using System.Text;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;
using log4net;
using System.Reflection;

namespace DOL.GS.Behaviour
{
    /// <summary>
    /// Requirements describe what must be true to allow a QuestAction to fire.
    /// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
    /// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
    /// </summary>
    public abstract class AbstractRequirement<TypeN,TypeV> : IBehaviourRequirement
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private eRequirementType type;
        private TypeN n;
        private TypeV v;
        private eComparator comparator;
		private GameNPC defaultNPC;		

        /// <summary>
        /// R: RequirmentType
        /// </summary>
        public eRequirementType RequirementType
        {
            get { return type; }
            set { type = value; }
        }
        /// <summary>
        /// N: first Requirment Variable
        /// </summary>
        public TypeN N
        {
            get { return n; }
			set { n = value; }
        }
        /// <summary>
        /// V: Secoond Requirmenet Variable
        /// </summary>
        public TypeV V
        {
            get { return v; }
			set { v = value; }
        }
        /// <summary>
        /// C: Requirement Comparator
        /// </summary>
        public eComparator Comparator
        {
            get { return comparator; }
            set { comparator = value; }
        }

        /// <summary>
        /// returns the NPC of the requirement
        /// </summary>
        public GameNPC NPC
        {
            get { return defaultNPC; }
            set { defaultNPC = value; }
        }		
        

        public AbstractRequirement(GameNPC npc, eRequirementType type, eComparator comp)
        {
            this.defaultNPC = npc;
            this.type = type;
            this.comparator = comp;
        }

		/// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="type">RequirementType</param>
        /// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="defaultNPC">Default npc to use is non is given</param>
        /// <param name="questType">Type of quest this requirement is attached to</param>
        /// <param name="comp">Comparator used if some values are veeing compared</param>
        public AbstractRequirement(GameNPC defaultNPC, eRequirementType type, Object n, Object v, eComparator comp) : this(defaultNPC,type,comp)
        {            			            

            RequirementAttribute attr = BehaviourMgr.getRequirementAttribute(this.GetType());
            // handle parameter N
            object defaultValueN = GetDefaultValue(attr.DefaultValueN);            
            this.N = (TypeN)BehaviourUtils.ConvertObject(n, defaultValueN, typeof(TypeN));
            CheckParameter(this.N, attr.IsNullableN, typeof(TypeN));
            
            // handle parameter V
            object defaultValueV = GetDefaultValue(attr.DefaultValueV);            
            this.v = (TypeV)BehaviourUtils.ConvertObject(v, defaultValueV, typeof(TypeV));
            CheckParameter(this.V, attr.IsNullableV, typeof(TypeV));
            
        }

        protected virtual object GetDefaultValue(Object defaultValue)
        {
            if (defaultValue != null)
            {
                if (defaultValue is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)defaultValue)
                    {                        
                        case eDefaultValueConstants.NPC:
                            defaultValue = NPC;
                            break;
                    }
                }
            }
            return defaultValue;
        }

        protected virtual bool CheckParameter(object value, Boolean isNullable, Type destinationType)
        {
            if (destinationType == typeof(Unused))
            {
                if (value != null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Parameter is not used for =" + this.GetType().Name + ".\n The recieved parameter " + value + " will not be used for anthing. Check your quest code for inproper usage of parameters!");
                        return false;
                    }
                }
            }
            else
            {
                if (!isNullable && value == null)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error("Not nullable parameter was null, expected type is " + destinationType.Name + "for =" + this.GetType().Name + ".\nRecived parameter was " + value);
                        return false;
                    }
                }
                if (value != null && !(destinationType.IsInstanceOfType(value)))
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error("Parameter was not of expected type, expected type is " + destinationType.Name + "for " + this.GetType().Name + ".\nRecived parameter was " + value);
                        return false;
                    }
                }
            }

            return true;
        }

        public abstract bool Check(DOLEvent e, object sender, EventArgs args);

        /// <summary>
        /// Compares value1 with value2 
        /// Allowed Comparators: Less,Greater,Equal, NotEqual, None
        /// </summary>
        /// <param name="value1">Value1 one to compare</param>
        /// <param name="value2">Value2 to cmopare</param>
        /// <param name="comp">Comparator to use for Comparison</param>
        /// <returns>result of comparison</returns>
        protected static bool compare(long value1, long value2, eComparator comp)
        {
            switch (comp)
            {
                case eComparator.Less:
                    return (value1 < value2);
                case eComparator.Greater:
                    return (value1 > value2);
                case eComparator.Equal:
                    return (value1 == value2);
                case eComparator.NotEqual:
                    return (value1 != value2);
                case eComparator.None:
                    return true;
                default:
                    throw new ArgumentException("Comparator not supported:" + comp, "comp");
            }
        }

        /// <summary>
        /// Compares value1 with value2 
        /// Allowed Comparators: Less,Greater,Equal, NotEqual, None
        /// </summary>
        /// <param name="value1">Value1 one to compare</param>
        /// <param name="value2">Value2 to cmopare</param>
        /// <param name="comp">Comparator to use for Comparison</param>
        /// <returns>result of comparison</returns>
        protected static bool compare(int value1, int value2, eComparator comp)
        {
            return compare((long)value1, (long)value2, comp);
        }
    }
}
