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
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Selective Blindness RA
    /// </summary>
    public class SelectiveBlindnessAbility : RR5RealmAbility
    {
        public const int DURATION = 20 * 1000;
        private const int SpellRange = 1500;
        private const ushort SpellRadius = 150;
        private GamePlayer player = null;
        private GamePlayer targetPlayer = null;

        public SelectiveBlindnessAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            if (living is GamePlayer)
            {
                player = living as GamePlayer;
                if (player.TargetObject == null)
                {
                    player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if (!(player.TargetObject is GamePlayer))
                {
                    player.Out.SendMessage("This work only on players!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if (!GameServer.ServerRules.IsAllowedToAttack(player, (GamePlayer)player.TargetObject, true))
                {
                    player.Out.SendMessage("This work only on enemies!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if ( !player.IsWithinRadius( player.TargetObject, SpellRange ) )
                {
                    player.Out.SendMessage(player.TargetObject + " is too far away!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    player.DisableSkill(this, 3 * 1000);
                    return;
                }
                foreach (GamePlayer radiusPlayer in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (radiusPlayer == player) radiusPlayer.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    else radiusPlayer.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

                    radiusPlayer.Out.SendSpellCastAnimation(player, 7059, 0);
                }

                if (player.RealmAbilityCastTimer != null)
                {
                    player.RealmAbilityCastTimer.Stop();
                    player.RealmAbilityCastTimer = null;
                    player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }

                targetPlayer = player.TargetObject as GamePlayer;

                //[StephenxPimentel]
                //1.108 - this ability is now instant cast.
                EndCast();
            }
        }

        private void EndCast()
        {
            if (player == null || !player.IsAlive) return;
            if (targetPlayer == null || !targetPlayer.IsAlive) return;

            if (!GameServer.ServerRules.IsAllowedToAttack(player, targetPlayer, true))
            {
                player.Out.SendMessage("This work only on enemies.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                player.DisableSkill(this, 3 * 1000);
                return;
            }
            if ( !player.IsWithinRadius( targetPlayer, SpellRange ) )
            {
                player.Out.SendMessage(targetPlayer + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                player.DisableSkill(this, 3 * 1000);
                return;
            }
            foreach (GamePlayer radiusPlayer in targetPlayer.GetPlayersInRadius(SpellRadius))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(player, radiusPlayer, true))
                    continue;

                SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)radiusPlayer.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
                if (SelectiveBlindness != null) SelectiveBlindness.Cancel(false);
                new SelectiveBlindnessEffect(player).Start(radiusPlayer);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("AE target 150 unit radius, 1500 unit range, blind enemies to user. 20s duration or attack by user, 5min RUT.");
            list.Add("");
            list.Add("Target: Enemy");
            list.Add("Duration: 20s");
            list.Add("Casting time: Instant");
        }

    }
}
