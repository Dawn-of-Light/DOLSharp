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

namespace DOL.GS.Spells
{
    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("DamageToPower")]
    public class DamageToPowerSpellHandler : LifedrainSpellHandler
    {
        /// <summary>
        /// Uses percent of damage to power the caster
        /// </summary>
        public override void StealLife(AttackData ad)
        {
            if (ad == null) return;
            if (!m_caster.IsAlive) return;

			GameServer.Instance.Logger.Error("DamageToPower check called");
			GameServer.Instance.Logger.Error("(" + ad.Damage + " + "  + ad.CriticalDamage + ")" + " * " + m_spell.LifeDrainReturn + " / 100");
            int heal = (ad.Damage + ad.CriticalDamage) * m_spell.LifeDrainReturn / 100;
			GameServer.Instance.Logger.Error("heal result: " + heal);
            // Return the spell power? + % calculated on HP value and caster maxmana
			GameServer.Instance.Logger.Error(m_spell.Power.ToString() + " + " + "(" + heal + " / " + m_caster.MaxHealth + " * " + m_caster.MaxMana + ")");
			double manareturned = m_spell.Power + (heal / m_caster.MaxHealth * m_caster.MaxMana);
			GameServer.Instance.Logger.Error("mana returned result: " + manareturned);
            
            if (heal <= 0) return;
            heal = m_caster.ChangeMana(m_caster, GameLiving.eManaChangeType.Spell, (int)manareturned);

			GameServer.Instance.Logger.Error("changemana result: " + heal);

            if (heal > 0)
            {
                MessageToCaster("You steal " + heal + " power point" + (heal == 1 ? "." : "s."), eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You cannot absorb any more power.", eChatType.CT_SpellResisted);
            }
        }

        // constructor
        public DamageToPowerSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}

