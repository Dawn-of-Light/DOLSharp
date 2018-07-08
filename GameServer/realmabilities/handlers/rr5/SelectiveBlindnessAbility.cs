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
        private const int SpellRange = 1500;
        private const ushort SpellRadius = 150;
        private GamePlayer _player;
        private GamePlayer _targetPlayer;

        public SelectiveBlindnessAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living is GamePlayer player)
            {
                _player = player;
                if (_player.TargetObject == null)
                {
                    _player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    _player.DisableSkill(this, 3 * 1000);
                    return;
                }

                if (!(_player.TargetObject is GamePlayer))
                {
                    _player.Out.SendMessage("This work only on players!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    _player.DisableSkill(this, 3 * 1000);
                    return;
                }

                if (!GameServer.ServerRules.IsAllowedToAttack(_player, (GamePlayer)_player.TargetObject, true))
                {
                    _player.Out.SendMessage("This work only on enemies!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    _player.DisableSkill(this, 3 * 1000);
                    return;
                }

                if (!_player.IsWithinRadius(_player.TargetObject, SpellRange))
                {
                    _player.Out.SendMessage($"{_player.TargetObject} is too far away!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    _player.DisableSkill(this, 3 * 1000);
                    return;
                }

                foreach (GamePlayer radiusPlayer in _player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (radiusPlayer == _player)
                    {
                        radiusPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                    }
                    else
                    {
                        radiusPlayer.MessageFromArea(_player, $"{_player.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }

                    radiusPlayer.Out.SendSpellCastAnimation(_player, 7059, 0);
                }

                if (_player.RealmAbilityCastTimer != null)
                {
                    _player.RealmAbilityCastTimer.Stop();
                    _player.RealmAbilityCastTimer = null;
                    _player.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }

                _targetPlayer = _player.TargetObject as GamePlayer;

                // [StephenxPimentel]
                // 1.108 - this ability is now instant cast.
                EndCast();
            }
        }

        private void EndCast()
        {
            if (_player == null || !_player.IsAlive)
            {
                return;
            }

            if (_targetPlayer == null || !_targetPlayer.IsAlive)
            {
                return;
            }

            if (!GameServer.ServerRules.IsAllowedToAttack(_player, _targetPlayer, true))
            {
                _player.Out.SendMessage("This work only on enemies.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _player.DisableSkill(this, 3 * 1000);
                return;
            }

            if (!_player.IsWithinRadius(_targetPlayer, SpellRange))
            {
                _player.Out.SendMessage($"{_targetPlayer} is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _player.DisableSkill(this, 3 * 1000);
                return;
            }

            foreach (GamePlayer radiusPlayer in _targetPlayer.GetPlayersInRadius(SpellRadius))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(_player, radiusPlayer, true))
                {
                    continue;
                }

                SelectiveBlindnessEffect selectiveBlindness = radiusPlayer.EffectList.GetOfType<SelectiveBlindnessEffect>();
                selectiveBlindness?.Cancel(false);

                new SelectiveBlindnessEffect(_player).Start(radiusPlayer);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("AE target 150 unit radius, 1500 unit range, blind enemies to user. 20s duration or attack by user, 5min RUT.");
            list.Add(string.Empty);
            list.Add("Target: Enemy");
            list.Add("Duration: 20s");
            list.Add("Casting time: Instant");
        }
    }
}
