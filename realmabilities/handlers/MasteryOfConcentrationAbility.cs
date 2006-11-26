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
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class MasteryofConcentrationAbility : TimedRealmAbility
	{
        public MasteryofConcentrationAbility(DBAbility dba, int level) : base(dba, level) { }
		public const Int32 Duration = 30;

        public void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;

			MasteryofConcentrationEffect MoCEffect = (MasteryofConcentrationEffect)player.EffectList.GetOfType(typeof(MasteryofConcentrationEffect));
            if (player != null)
            {
                if (MoCEffect != null)
                {
                    MoCEffect.Cancel(false);
                    return;
                }
                SendCasterSpellEffectAndCastMessage(living, 7007, true);
                foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {

                    if (WorldMgr.GetDistance(t_player, player) <= WorldMgr.INFO_DISTANCE)
                    {
                        if (t_player == player)
                        {
                            player.Out.SendMessage("You cast a Mastery of Concentration Spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage("You become steadier in your casting abilities!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            t_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                            t_player.Out.SendMessage(player.Name + "'s castings have perfect poise!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                    }
                }

                DisableSkill(living);

                new MasteryofConcentrationEffect().Start(player);
            }
		}
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
