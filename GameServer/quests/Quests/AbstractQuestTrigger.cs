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
using System.Reflection;
using log4net;

namespace DOL.GS.Quests
{
    /// <summary>
    /// A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    /// </summary>        
    public abstract class AbstractQuestTrigger<TypeK, TypeI> : IQuestTrigger
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TypeK k; //trigger keyword 
        private TypeI i;        
        private eTriggerType triggerType; // t## : trigger type, see following description (NONE:no trigger)        
		private GameLiving defaultNPC;
		private DOLEventHandler notifyHandler;
		private Type questType;

        /// <summary>
        /// Trigger Keyword
        /// </summary>
        public TypeK K
        {
            get { return k; }
			set { k = value; }
        }

        /// <summary>
        /// Trigger Variable
        /// </summary>
        public TypeI I
        {
            get { return i; }
			set { i = value; }
        }

        /// <summary>
        /// Triggertype
        /// </summary>
        public eTriggerType TriggerType
        {
            get { return triggerType; }
        }

    	/// <summary>
        /// returns the NPC of the trigger
        /// </summary>
        public GameLiving NPC
        {
            get { return defaultNPC; }
        }

		public DOLEventHandler NotifyHandler
		{
			get { return notifyHandler; }
		}

		public Type QuestType
		{
			get { return questType; }
		}

    	/// <summary>
    	/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
    	/// </summary>
		/// <param name="questPart"></param>
    	/// <param name="type">Triggertype</param>
    	/// <param name="k">keyword (K), meaning depends on triggertype</param>
    	/// <param name="i">variable (I), meaning depends on triggertype</param>
    	public AbstractQuestTrigger(BaseQuestPart questPart,eTriggerType type, Object k, Object i)
    	:this(questPart.NPC,questPart.QuestType,new DOLEventHandler(questPart.Notify),type,k,i) { }

		/// <summary>
		/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="notifyHandler"></param>
		/// <param name="type">Triggertype</param>
		/// <param name="k">keyword (K), meaning depends on triggertype</param>
		/// <param name="i">variable (I), meaning depends on triggertype</param>
		public AbstractQuestTrigger(GameLiving defaultNPC,Type questType,DOLEventHandler notifyHandler, eTriggerType type, object k, object i)
		{
			this.defaultNPC = defaultNPC;
			this.notifyHandler = notifyHandler;
			this.triggerType = type;
            this.questType = questType;

            QuestTriggerAttribute attr = QuestMgr.getQuestTriggerAttribute(this.GetType());

            // handle parameter K
            object defaultValueK = null;
            if (attr.DefaultValueK != null)
            {
                if (attr.DefaultValueK is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueK)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueK = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueK = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueK = attr.DefaultValueK;
                }
            }
            this.k = (TypeK)QuestPartUtils.ConvertObject(k, defaultValueK, typeof(TypeK));


            if (typeof(TypeK) == typeof(Unused))
            {
                if (this.k != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter K is not used for TriggerType=" + attr.TriggerType + ".\n The recieved parameter " + k + " will not be used for anything. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableK && this.k == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter K was null, expected type is " + typeof(TypeK).Name + "for TriggerType=" + attr.TriggerType + ".\nRecived parameter was " + k + " and DefaultValue for this parameter was " + attr.DefaultValueK);
                }
                if (this.k != null && !(this.k is TypeK))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter K was not of expected type, expected type is " + typeof(TypeK).Name + "for TriggerType=" + attr.TriggerType + ".\nRecived parameter was " + k + " and DefaultValue for this parameter was " + attr.DefaultValueK);
                }
            }


            // handle parameter I
            object defaultValueI = null;
            if (attr.DefaultValueI != null)
            {
                if (attr.DefaultValueI is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueI)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueI = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueI = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueI = attr.DefaultValueI;
                }
            }
            this.i = (TypeI)QuestPartUtils.ConvertObject(i, defaultValueI, typeof(TypeI));

            if (typeof(TypeI) == typeof(Unused))
            {
                if (this.i != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter I is not used for TriggerType=" + attr.TriggerType + ".\n The recieved parameter " + i + " will not be used for anthing. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableI && this.i == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter I was null, expected type is " + typeof(TypeI).Name + "for TriggerType=" + attr.TriggerType + ".\nRecived parameter was " + i + " and DefaultValue for this parameter was " + attr.DefaultValueI);
                }
                if (this.i != null && !(this.i is TypeI))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter I was not of expected type, expected type is " + typeof(TypeI).Name + "for TriggerType=" + attr.TriggerType + ".\nRecived parameter was " + i + " and DefaultValue for this parameter was " + attr.DefaultValueI);
                }
            }    		
		}

    	/// <summary>
        /// Checks the trigger, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if QuestPart should be executes, else false</returns>
        public abstract bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);

		/// <summary>
		/// Registers the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// This method will be called multiple times, so use AddHandlerUnique to make
		/// sure only one handler is actually registered
		/// </remarks>
        public abstract void Register();

		/// <summary>
		/// Unregisters the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// Don't remove handlers that will be used by other triggers etc.
		/// This is rather difficult since we don't know which events other triggers use.
		/// </remarks>
        public abstract void Unregister();
    }

}
