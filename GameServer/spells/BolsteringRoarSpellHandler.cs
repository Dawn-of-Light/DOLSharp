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
using DOL.GS;
using DOL.GS.PacketHandler;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("BolsteringRoar")]
    public class BolsteringRoarSpellHandler : RemoveSpellEffectHandler
    {
        public override System.Collections.IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList();
            GameLiving target = castTarget as GameLiving;

            if (Caster is GamePlayer)
            {
                GamePlayer casterPlayer = (GamePlayer)Caster;
                PlayerGroup group = casterPlayer.PlayerGroup;
                if(group == null) return list; // Should not appen since it is checked in ability handler
                int spellRange = CalculateSpellRange();
                if (group != null)
                {
                    lock (group)
                    {

                        foreach (GamePlayer groupPlayer in casterPlayer.GetPlayersInRadius((ushort)m_spell.Radius))
                        {
                            if (casterPlayer.PlayerGroup.IsInTheGroup(groupPlayer))
                            {
                                if (groupPlayer != casterPlayer && groupPlayer.IsAlive)
                                {
                                    list.Add(groupPlayer);
                                    IControlledBrain npc = groupPlayer.ControlledNpc;
                                    if (npc != null)
                                    {
                                        if (WorldMgr.CheckDistance(casterPlayer, npc.Body, spellRange))
                                            list.Add(npc.Body);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        // constructor
        public BolsteringRoarSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
 			// RR4: now it's a list
			m_spellTypesToRemove = new List<string>();       	
            m_spellTypesToRemove.Add("Mesmerize");
            m_spellTypesToRemove.Add("SpeedDecrease");
            m_spellTypesToRemove.Add("StyleSpeedDecrease");
            m_spellTypesToRemove.Add("DamageSpeedDecrease");
            m_spellTypesToRemove.Add("HereticSpeedDecrease");
            m_spellTypesToRemove.Add("HereticDamageSpeedDecreaseLOP");
            m_spellTypesToRemove.Add("VampiirSpeedDecrease");
            m_spellTypesToRemove.Add("ValkyrieSpeedDecrease");
            m_spellTypesToRemove.Add("WarlockSpeedDecrease");
        }
    }
}
