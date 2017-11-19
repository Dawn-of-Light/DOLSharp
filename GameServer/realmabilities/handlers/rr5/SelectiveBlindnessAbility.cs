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
        private GamePlayer m_player = null;
        private GamePlayer m_targetPlayer = null;

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
                m_player = living as GamePlayer;
                if (m_player.TargetObject == null)
                {
                    m_player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    m_player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if (!(m_player.TargetObject is GamePlayer))
                {
                    m_player.Out.SendMessage("This work only on players!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    m_player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if (!GameServer.ServerRules.IsAllowedToAttack(m_player, (GamePlayer)m_player.TargetObject, true))
                {
                    m_player.Out.SendMessage("This work only on enemies!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    m_player.DisableSkill(this, 3 * 1000);
                    return;
                }
                if ( !m_player.IsWithinRadius( m_player.TargetObject, SpellRange ) )
                {
                    m_player.Out.SendMessage(m_player.TargetObject + " is too far away!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    m_player.DisableSkill(this, 3 * 1000);
                    return;
                }
                foreach (GamePlayer radiusPlayer in m_player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
					if (radiusPlayer == m_player)
					{
						radiusPlayer.MessageToSelf("You cast " + Name + "!", eChatType.CT_Spell);
					}
					else
					{
						radiusPlayer.MessageFromArea(m_player, m_player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}

                    radiusPlayer.Out.SendSpellCastAnimation(m_player, 7059, 0);
                }

                if (m_player.RealmAbilityCastTimer != null)
                {
                    m_player.RealmAbilityCastTimer.Stop();
                    m_player.RealmAbilityCastTimer = null;
                    m_player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }

                m_targetPlayer = m_player.TargetObject as GamePlayer;

                //[StephenxPimentel]
                //1.108 - this ability is now instant cast.
                EndCast();
            }
        }

        private void EndCast()
        {
            if (m_player == null || !m_player.IsAlive) return;
            if (m_targetPlayer == null || !m_targetPlayer.IsAlive) return;

            if (!GameServer.ServerRules.IsAllowedToAttack(m_player, m_targetPlayer, true))
            {
                m_player.Out.SendMessage("This work only on enemies.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                m_player.DisableSkill(this, 3 * 1000);
                return;
            }
            if ( !m_player.IsWithinRadius( m_targetPlayer, SpellRange ) )
            {
                m_player.Out.SendMessage(m_targetPlayer + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                m_player.DisableSkill(this, 3 * 1000);
                return;
            }
            foreach (GamePlayer radiusPlayer in m_targetPlayer.GetPlayersInRadius(SpellRadius))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(m_player, radiusPlayer, true))
                    continue;

				SelectiveBlindnessEffect SelectiveBlindness = radiusPlayer.EffectList.GetOfType<SelectiveBlindnessEffect>();
                if (SelectiveBlindness != null) SelectiveBlindness.Cancel(false);
                new SelectiveBlindnessEffect(m_player).Start(radiusPlayer);
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
