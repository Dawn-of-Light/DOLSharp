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
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("TauntingShout")]
    public class TauntingShoutSpellHandler : TauntSpellHandler
    {
    	// Cone effect
		public override IList SelectTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList();

			foreach (GamePlayer player in m_caster.GetPlayersInRadius((ushort)Spell.Range)) 
			{
				if (player != (GamePlayer)m_caster && player.IsAlive)
					if (m_caster.IsObjectInFront(player, 120))
						if (GameServer.ServerRules.IsAllowedToAttack(m_caster, player, true)) 
							list.Add(player);
			}
			foreach (GameNPC npc in m_caster.GetNPCsInRadius((ushort)Spell.Range)) 
			{
				if (npc != (GameNPC)m_caster && npc.IsAlive)
					if (m_caster.IsObjectInFront(npc, 120))
						if (GameServer.ServerRules.IsAllowedToAttack(m_caster, npc, true)) 
							list.Add(npc);
			}
			return list;
		}
        public TauntingShoutSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
