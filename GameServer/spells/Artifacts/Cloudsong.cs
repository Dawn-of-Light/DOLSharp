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
    [SpellHandler("CloudsongAura")]
    public class CloudsongAuraSpellHandler : DualStatBuff
    {
        public CloudsongAuraSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
        {
        }

        /// <summary>
        /// SpecBuffBonusCategory
        /// </summary>
		public override int BonusCategory1 { get { return 2; } }

        /// <summary>
        /// BaseBuffBonusCategory
        /// </summary>
		public override int BonusCategory2 { get { return 1; } }

        public override eProperty Property1
        {
            get { return eProperty.SpellRange; }
        }

        public override eProperty Property2
        {
            get { return eProperty.ResistPierce; }
        }

    }

    /// <summary>
    /// [Freya] Nidel : Handler for Fall damage reduction.
    /// Calcul located in PlayerPositionUpdateHandler.cs
    /// </summary>
    [SpellHandler("CloudsongFall")]
    public class CloudsongFallSpellHandler : SpellHandler
    {
        public CloudsongFallSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
        {
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }
    }
}
