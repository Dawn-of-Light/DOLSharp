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
using DOL.GS.Quests.Attributes;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests
{
    /// <summary>
    /// Requirements describe what must be true to allow a QuestAction to fire.
    /// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
    /// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
    /// </summary>
    public abstract class AbstractQuestRequirement<TypeN,TypeV> : IQuestRequirement
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private eRequirementType type;
        private TypeN n;
        private TypeV v;
        private eComparator comparator;
		private GameNPC defaultNPC;
		private Type questType;

        /// <summary>
        /// R: RequirmentType
        /// </summary>
        public eRequirementType RequirementType
        {
            get { return type; }
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
        }

        /// <summary>
        /// returns the NPC of the requirement
        /// </summary>
        public GameNPC NPC
        {
            get { return defaultNPC; }
        }

		public Type QuestType
		{
			get { return questType; }
		}

        /// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="type">RequirementType</param>
        /// <param name="questPart">Questpart this requirement is attached to</param>
        /// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="comp">Comparator used if some values are veeing compared</param>
        public AbstractQuestRequirement(BaseQuestPart questPart, eRequirementType type, Object n, Object v, eComparator comp) 
            : this(questPart.NPC,questPart.QuestType,type,n,v,comp) { }

		/// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="type">RequirementType</param>
        /// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="defaultNPC">Default npc to use is non is given</param>
        /// <param name="questType">Type of quest this requirement is attached to</param>
        /// <param name="comp">Comparator used if some values are veeing compared</param>
        public AbstractQuestRequirement(GameNPC defaultNPC, Type questType, eRequirementType type, Object n, Object v, eComparator comp)
        {
            this.defaultNPC = defaultNPC;
			this.questType = questType;
            this.type = type;            
            this.comparator = comp;

            QuestRequirementAttribute attr = QuestMgr.getQuestRequirementAttribute(this.GetType());
            // handle parameter N
            object defaultValueN = null;
            if (attr.DefaultValueN != null)
            {
                if (attr.DefaultValueN is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueN)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueN = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueN = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueN = attr.DefaultValueN;
                }
            }
            this.N = (TypeN)QuestPartUtils.ConvertObject(n, defaultValueN, typeof(TypeN));


            if (typeof(TypeN) == typeof(Unused))
            {
                if (this.n != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter N is not used for RequirementType=" + attr.RequirementType + ".\n The recieved parameter " + n + " will not be used for anything. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableN && this.n == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter N was null, expected type is " + typeof(TypeN).Name + "for RequirementType=" + attr.RequirementType+" parameter was " + n + " and DefaultValue for this parameter was " + attr.DefaultValueN);
                }
                if (this.n != null && !(this.n is TypeN))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter N was not of expected type, expected type is " + typeof(TypeN).Name + "for RequirementType=" + attr.RequirementType + ".\nRecived parameter was " + n + " and DefaultValue for this parameter was " + attr.DefaultValueN);
                }
            }


            // handle parameter V
            object defaultValueV = null;
            if (attr.DefaultValueV != null)
            {
                if (attr.DefaultValueV is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueV)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueV = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueV = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueV = attr.DefaultValueV;
                }
            }
            this.v = (TypeV)QuestPartUtils.ConvertObject(v, defaultValueV, typeof(TypeV));

            if (typeof(TypeV) == typeof(Unused))
            {
                if (this.v != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter V is not used for RequirementType=" + attr.RequirementType + ".\n The recieved parameter " + v + " will not be used for anthing. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableV && this.v == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter V was null, expected type is " + typeof(TypeV).Name + "for RequirementType=" + attr.RequirementType + ".\nRecived parameter was " + v + " and DefaultValue for this parameter was " + attr.DefaultValueV);
                }
                if (this.v != null && !(this.v is TypeV))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter V was not of expected type, expected type is " + typeof(TypeV).Name + "for RequirementType=" + attr.RequirementType + ".\nRecived parameter was " + v + " and DefaultValue for this parameter was " + attr.DefaultValueV);
                }
            }    
        }

        public abstract bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);

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
