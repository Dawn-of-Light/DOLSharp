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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class SpiritMartyrAbility : RR5RealmAbility
    {
        public SpiritMartyrAbility(DBAbility dba, int level) : base(dba, level) { }

        private const int HealRange = 2000;
        private double _healthpool = 3200;

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer player))
            {
                return;
            }

            if (player.ControlledBrain?.Body == null)
            {
                return;
            }

            ArrayList targets = new ArrayList();

            // select targets
            if (player.Group == null)
            {
                if (player.Health < player.MaxHealth)
                {
                    targets.Add(player);
                }
            }
            else
            {
                foreach (GamePlayer tplayer in player.Group.GetPlayersInTheGroup())
                {
                    if (tplayer.IsAlive && player.IsWithinRadius(tplayer, HealRange)
                        && tplayer.Health < tplayer.MaxHealth)
                    {
                        targets.Add(player);
                    }
                }
            }

            if (targets.Count == 0)
            {
                player.Out.SendMessage($"{(player.Group != null ? "Your group is" : "You are")} fully healed!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            // send spelleffect
            foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visPlayer.Out.SendSpellEffectAnimation(player, player, 7075, 0, false, 0x01);
            }

            int petHealthPercent = player.ControlledBrain.Body.HealthPercent;
            _healthpool *= petHealthPercent * 0.01;

            // [StephenxPimentel]
            // 1.108 - This ability will no longer stun or sacrifice the pet.

            // player.ControlledBrain.Body.Die(player);
            int pool = (int)_healthpool;
            while (pool > 0 && targets.Count > 0)
            {
                // get most injured player
                GamePlayer mostInjuredPlayer = null;
                int lowestHealthPercent = 100;
                foreach (GamePlayer tp in targets)
                {
                    byte tpHealthPercent = tp.HealthPercent;
                    if (tpHealthPercent < lowestHealthPercent)
                    {
                        lowestHealthPercent = tpHealthPercent;
                        mostInjuredPlayer = tp;
                    }
                }

                if (mostInjuredPlayer == null)
                {
                    break;
                }

                // target has been healed
                targets.Remove(mostInjuredPlayer);
                int healValue = Math.Min(600, mostInjuredPlayer.MaxHealth - mostInjuredPlayer.Health);
                healValue = Math.Min(healValue, pool);
                mostInjuredPlayer.ChangeHealth(player, GameLiving.eHealthChangeType.Spell, healValue);
            }

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}
