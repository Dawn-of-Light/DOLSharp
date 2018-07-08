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
using DOL.Events;
using System;

namespace DOL.GS.Effects
{
    public class SputinsLegacyEffect : TimedEffect
    {
        private GamePlayer _player;

        public SputinsLegacyEffect() : base(20000) { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            _player = target as GamePlayer;
            GameEventMgr.AddHandler(_player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            GameEventMgr.AddHandler(_player, GameLivingEvent.Dying, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(_player, GamePlayerEvent.Quit, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(_player, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRemove));
        }

        private void OnAttacked(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is AttackedByEnemyEventArgs attackArgs))
            {
                return;
            }

            var ad = attackArgs.AttackData;

            int damageAbsorbed = ad.Damage + ad.CriticalDamage;

            if (_player.Health < (damageAbsorbed + (int)Math.Round((double)_player.MaxHealth / 20)))
            {
                _player.Health += damageAbsorbed;
            }
        }

        private void OnRemove(DOLEvent e, object sender, EventArgs args)
        {
            // ((GamePlayer)Owner).Out.SendMessage("Sputins Legacy grants you a damage immunity!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            Stop();
        }

        public override void Stop()
        {
            if (_player.EffectList.GetOfType<SputinsLegacyEffect>() != null)
            {
                _player.EffectList.Remove(this);
            }

            GameEventMgr.RemoveHandler(_player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            GameEventMgr.RemoveHandler(_player, GameLivingEvent.Dying, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(_player, GamePlayerEvent.Quit, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(_player, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRemove));
            base.Stop();
        }

        public override string Name => "Sputins Legacy";

        public override ushort Icon => 3069;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "The Healer won't die for 30sec."
                };

                return list;
            }
        }
    }
}