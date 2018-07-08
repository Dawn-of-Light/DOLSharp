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
using System.Linq;
using DOL.Database;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Fury shout
    /// </summary>
    [SkillHandler(Abilities.ClimbSpikes)]
    public class ClimbingAbilityHandler : SpellCastingAbilityHandler
    {
        private static int _spellid = -1;

        public override long Preconditions => DEAD | SITTING | MEZZED | STUNNED;

        public override int SpellID => _spellid;

        public ClimbingAbilityHandler()
        {
            // Graveen: crappy, but not hardcoded. if we except by the ability name ofc...
            // problems are:
            //      - matching vs ability name / spell name needed
            //      - spell name is not indexed
            // perhaps a basis to think about, but definitively not the design we want.
            if (_spellid == -1)
            {
                _spellid = 0;
                DBSpell climbSpell = GameServer.Database.SelectObjects<DBSpell>("`Name` = @Name", new QueryParameter("@Name", Abilities.ClimbSpikes)).FirstOrDefault();
                if (climbSpell != null)
                {
                    _spellid = climbSpell.SpellID;
                }
            }
        }
    }
}
