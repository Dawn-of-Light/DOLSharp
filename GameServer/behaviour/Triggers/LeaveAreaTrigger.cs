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
using DOL.Database;
using log4net;
using System.Reflection;
using DOL.GS.Scripts;
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;

namespace DOL.GS.Behaviour.Triggers
{	
    /// <summary>
    /// A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    /// </summary>
    [TriggerAttribute(Global = true,TriggerType = eTriggerType.LeaveArea)]
    public class LeaveAreaTrigger : AbstractTrigger<Unused, IArea>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
		/// </summary>
		/// <param name="questPart">Parent QuestPart of this Trigger</param>
		/// <param name="type">Triggertype</param>
		/// <param name="k">keyword (K), meaning depends on triggertype</param>
		/// <param name="i">variable (I), meaning depends on triggertype</param>
        public LeaveAreaTrigger(GameNPC defaultNPC, DOLEventHandler notifyHandler,  Object k, Object i)
            : base(defaultNPC, notifyHandler, eTriggerType.LeaveArea, k, i)
        { }

        /// <summary>
        /// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
        /// </summary>
        /// <param name="questPart">Parent QuestPart of this Trigger</param>        
        /// <param name="i">variable (I), meaning depends on triggertype</param>
        public LeaveAreaTrigger(GameNPC defaultNPC, DOLEventHandler notifyHandler, IArea i)
            : this(defaultNPC,notifyHandler,  (object)null,(object) i)
        { }

        /// <summary>
        /// Checks the trigger, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if QuestPart should be executes, else false</returns>
        public override bool Check(DOLEvent e, object sender, EventArgs args)
        {
            bool result = false;
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);

            if (e == AreaEvent.PlayerLeave)
            {                
                AreaEventArgs aArgs = (AreaEventArgs)args;
                result |= aArgs.GameObject == player && I == aArgs.Area;
            }
            
            return result;
        }

		/// <summary>
		/// Registers the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// This method will be called multiple times, so use AddHandlerUnique to make
		/// sure only one handler is actually registered
		/// </remarks>
        public override void Register()
        {            
            GameEventMgr.AddHandler(I, AreaEvent.PlayerLeave, NotifyHandler);                                
        }

		/// <summary>
		/// Unregisters the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// Don't remove handlers that will be used by other triggers etc.
		/// This is rather difficult since we don't know which events other triggers use.
		/// </remarks>
        public override void Unregister()
        {            
            GameEventMgr.RemoveHandler(I, AreaEvent.PlayerLeave, NotifyHandler);            
        }		
    }
}
