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
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;
using DOL.Database;

namespace DOL.GS.Behaviour.Actions
{

    [ActionAttribute(ActionType = eActionType.Timer)]
    public class TimerAction : AbstractAction<string,int>
    {
        /// <summary>
        /// Constant used to store timerid in RegionTimer.Properties
        /// </summary>
        const string TIMER_ID = "timerid";
        /// <summary>
        /// Constant used to store GameLiving Source in RegionTimer.Properties
        /// </summary>
        const string TIMER_SOURCE = "timersource";


        public TimerAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.Timer, p, q)
        { 
            
        }


        public TimerAction(GameNPC defaultNPC,   string timerID, int delay)
            : this(defaultNPC, (object)timerID,(object) delay) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);

            RegionTimer timer = new RegionTimer(player, new RegionTimerCallback(QuestTimerCallBack));
            timer.Properties.setProperty(TIMER_ID, P);
            timer.Properties.setProperty(TIMER_SOURCE, player);
            timer.Start(Q);
        }

        /// <summary>
        /// Callback for quest internal timers used via eActionType.Timer and eTriggerType.Timer
        /// </summary>
        /// <param name="callingTimer"></param>
        /// <returns>0</returns>
        private static int QuestTimerCallBack(RegionTimer callingTimer)
        {
            string timerid = callingTimer.Properties.getObjectProperty(TIMER_ID, null) as string;
            if (timerid == null)
                throw new ArgumentNullException("TimerId out of Range", "timerid");

            GameLiving source = callingTimer.Properties.getObjectProperty(TIMER_SOURCE, null) as GameLiving;
            if (source == null)
                throw new ArgumentNullException("TimerSource null", "timersource");


            TimerEventArgs args = new TimerEventArgs(source, timerid);
            source.Notify(GameLivingEvent.Timer, source, args);

            return 0;
        }
    }
}
