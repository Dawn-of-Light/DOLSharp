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
using System.Reflection;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS;
using log4net;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Guard ability clicks
    /// </summary>
    [SkillHandler(Abilities.Bodyguard)]
    public class BodyguardAbilityHandler : IAbilityActionHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The guard distance
        /// </summary>
        public const int BODYGUARD_DISTANCE = 300;

        public void Execute(Ability ab, GamePlayer player)
        {
            if (!player.IsAlive)
            {
                player.Out.SendMessage("You're dead and can't Bodyguard!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (player == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not retrieve player in BodyguardAbilityHandler.");
                return;
            }

            GameObject targetObject = player.TargetObject;
            if (targetObject == null)
            {
                foreach (BodyguardEffect bg in player.EffectList.GetAllOfType(typeof(BodyguardEffect)))
                {
                    if (bg.GuardSource == player)
                        bg.Cancel(false);
                }
                player.Out.SendMessage("You are no longer Bodyguarding anyone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            // You cannot guard attacks on yourself            
            GamePlayer guardTarget = player.TargetObject as GamePlayer;
            if (guardTarget == player)
            {
                player.Out.SendMessage("You can't Bodyguard yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            // Only attacks on other players may be guarded. 
            // guard may only be used on other players in group
            PlayerGroup group = player.PlayerGroup;
            if (guardTarget == null || group == null || !group.IsInTheGroup(guardTarget))
            {
                player.Out.SendMessage("You can only guard players in your group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            // check if someone is guarding the target
            foreach (BodyguardEffect bg in guardTarget.EffectList.GetAllOfType(typeof(BodyguardEffect)))
            {
                if (bg.GuardTarget != guardTarget) continue;
                if (bg.GuardSource == player)
                {
                    bg.Cancel(false);
                    return;
                }
                player.Out.SendMessage(string.Format("{0} is already guarding {1}!", bg.GuardSource.GetName(0, true), bg.GuardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            foreach (BodyguardEffect bg in player.EffectList.GetAllOfType(typeof(BodyguardEffect)))
            {
                    if (bg != null && player == bg.GuardTarget)
                    {
                        player.Out.SendMessage("You cannot bodyguard someone if you are Bodyguarded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
            }

            // cancel all guard effects by this player before adding a new one
            foreach (BodyguardEffect bg in player.EffectList.GetAllOfType(typeof(BodyguardEffect)))
            {
                if (bg.GuardSource == player)
                    bg.Cancel(false);
            }

            new BodyguardEffect().Start(player, guardTarget);
        }
    }
}
