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

using System.Collections;
using DOL.Events;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Effects
{
    public class RezDmgImmunityEffect : TimedEffect
    {
        private GamePlayer m_player = null;

        public RezDmgImmunityEffect() : base(6000) { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            m_player = target as GamePlayer;
            GameEventMgr.AddHandler(m_player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            GameEventMgr.AddHandler(m_player, GameLivingEvent.Dying, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnRemove));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRemove));
        }

        private void OnAttacked(DOLEvent e, object sender, EventArgs args)
        {
            AttackedByEnemyEventArgs attackArgs = args as AttackedByEnemyEventArgs;
            if (attackArgs == null) return;
            AttackData ad = null;
            ad = attackArgs.AttackData;

            int damageAbsorbed = (int)(ad.Damage + ad.CriticalDamage);

            //They shouldn't take any damamge at all
            //if (m_player.Health < (damageAbsorbed + (int)Math.Round((double)m_player.MaxHealth / 20))) m_player.Health += damageAbsorbed;
            m_player.Health += damageAbsorbed;
        }

        private void OnRemove(DOLEvent e, object sender, EventArgs args)
        {
            //((GamePlayer)Owner).Out.SendMessage("Sputins Legacy grants you a damage immunity!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            Stop();
        }

        public override void Stop()
        {
            if (m_player.EffectList.GetOfType(typeof(SputinsLegacyEffect)) != null) m_player.EffectList.Remove(this);
            GameEventMgr.RemoveHandler(m_player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            GameEventMgr.RemoveHandler(m_player, GameLivingEvent.Dying, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnRemove));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRemove));
            base.Stop();
        }









        public override string Name { get { return "Resurrection Damage Immunity"; } }

        public override ushort Icon { get { return 3069; } }

        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("Newly resserected players can't take damage for 5 seconds.");
                return list;
            }
        }
    }
}