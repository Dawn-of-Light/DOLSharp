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
namespace DOL.GS.Spells
{
    /// <summary>
    /// [Freya] Nidel : Handler for Flask use1: Heals are more effective on the target
    /// </summary>
    [SpellHandler("HealFlask")]
    public class HealFlask : SpellHandler
    {
        public HealFlask(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine)
        {
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }
    }

    /// <summary>
    ///  [Freya] Nidel : Handler for Flask use2: Target gets chances not to die from the last hit.
    /// </summary>
    [SpellHandler("DeadFlask")]
    public class DeadFlask : SpellHandler
    {
        public DeadFlask(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine)
        {
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }
    }
}