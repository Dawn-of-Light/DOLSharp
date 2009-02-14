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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell Handler for firing bolts
    /// </summary>
    [SpellHandlerAttribute("ChainBolt")]
    public class ChainBoltSpellHandler : BoltSpellHandler
    {

        public ChainBoltSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

        

        protected GameLiving m_currentSource;
        protected int m_maxTick;
        protected int m_currentTick;
        protected double m_effetiveness = 1.0;


        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
        public override void StartSpell(GameLiving target)
        {
            if (target == null) return;

            if (m_maxTick >= 0)
            {
                m_maxTick = (Spell.Pulse>1)?Spell.Pulse:1;
                m_currentTick = 1;
                m_currentSource = target;
            }

            int ticksToTarget = m_currentSource.GetDistance(target) * 100 / 85; // 85 units per 1/10s
            int delay = 1 + ticksToTarget / 100;
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(m_currentSource, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
            }
            BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
            bolt.Start(1 + ticksToTarget);
            m_currentSource = target;
            m_currentTick++;            
        }

        public override void DamageTarget(AttackData ad, bool showEffectAnimation)
        {
            ad.Damage = (int)(ad.Damage * m_effetiveness);
            base.DamageTarget(ad, showEffectAnimation);
            if (m_currentTick < m_maxTick)
            {
                m_effetiveness -= 0.1;   
                //fetch next target
                foreach (GamePlayer pl in m_currentSource.GetPlayersInRadius(500))
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(Caster,pl,true)){
                        StartSpell(pl);
                        break;
                    }
                }
            }
        }
    }
}
