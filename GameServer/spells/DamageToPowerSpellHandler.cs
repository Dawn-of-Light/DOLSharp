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
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    ///
    /// </summary>
    [SpellHandler("DamageToPower")]
    public class DamageToPowerSpellHandler : LifedrainSpellHandler
    {
        /// <summary>
        /// Uses percent of damage to power the caster
        /// </summary>
        public override void StealLife(AttackData ad)
        {
            if (ad == null)
            {
                return;
            }

            if (!Caster.IsAlive)
            {
                return;
            }

            int heal = (ad.Damage + ad.CriticalDamage) * Spell.LifeDrainReturn / 100;

            // Return the spell power? + % calculated on HP value and caster maxmana
            double manareturned = Spell.Power + (heal * Caster.MaxMana / 100);

            if (heal <= 0)
            {
                return;
            }

            heal = Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, (int)manareturned);

            if (heal > 0)
            {
                MessageToCaster($"You steal {heal} power point{(heal == 1 ? "." : "s.")}", eChatType.CT_Spell);
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

