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
using System.Reflection;
using log4net;

namespace DOL.GS.Behaviour
{
    /// <summary>
    /// A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    /// </summary>        
    public abstract class AbstractTrigger<TypeK, TypeI> : IBehaviourTrigger
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TypeK k; //trigger keyword 
        private TypeI i;        
        private eTriggerType triggerType; // t## : trigger type, see following description (NONE:no trigger)        
		private GameLiving defaultNPC;
		private DOLEventHandler notifyHandler;		

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
            set { triggerType = value; }
        }

    	/// <summary>
        /// returns the NPC of the trigger
        /// </summary>
        public GameLiving NPC
        {
            get { return defaultNPC; }
            set { defaultNPC = value; }
        }

		public DOLEventHandler NotifyHandler
		{
			get { return notifyHandler; }
            set { notifyHandler = value; }
		}
		   
 	   /// <summary>
		/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="notifyHandler"></param>
		/// <param name="type">Triggertype</param>
		/// <param name="k">keyword (K), meaning depends on triggertype</param>
		/// <param name="i">variable (I), meaning depends on triggertype</param>
        public AbstractTrigger(GameLiving defaultNPC, DOLEventHandler notifyHandler, eTriggerType type)
        {
            this.defaultNPC = defaultNPC;
            this.notifyHandler = notifyHandler;
            this.triggerType = type;
        }

		/// <summary>
		/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="notifyHandler"></param>
		/// <param name="type">Triggertype</param>
		/// <param name="k">keyword (K), meaning depends on triggertype</param>
		/// <param name="i">variable (I), meaning depends on triggertype</param>
		public AbstractTrigger(GameLiving defaultNPC,DOLEventHandler notifyHandler, eTriggerType type, object k, object i) : this(defaultNPC,notifyHandler,type)
		{
            TriggerAttribute attr = BehaviourMgr.getTriggerAttribute(this.GetType());

            // handle parameter K
            object defaultValueK = GetDefaultValue(attr.DefaultValueK);
            this.k = (TypeK)BehaviourUtils.ConvertObject(k, defaultValueK, typeof(TypeK));
            CheckParameter(K, attr.IsNullableK, typeof(TypeK));

            // handle parameter I
            object defaultValueI = GetDefaultValue(attr.DefaultValueI);
            this.i = (TypeI)BehaviourUtils.ConvertObject(i, defaultValueI, typeof(TypeI));
            CheckParameter(I, attr.IsNullableI, typeof(TypeI));
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


    	/// <summary>
        /// Checks the trigger, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>        
        /// <returns>true if QuestPart should be executes, else false</returns>
        public abstract bool Check(DOLEvent e, object sender, EventArgs args);

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
