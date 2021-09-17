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
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Delve
{
    public class AbilityDelve : SkillDelve
    {
        private Skill skill;

        public AbilityDelve(GameClient client, int skillIndex)
        {
            skill = client.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == skillIndex).OrderBy(e => e.Item1 is Ability ? 0 : 1).Select(e => e.Item1).FirstOrDefault();

            if (skill == null)
                skill = SkillBase.GetAbilityByInternalID(skillIndex);

            if (skill == null)
                skill = SkillBase.GetSpecializationByInternalID(skillIndex);

            DelveType = skill is Ability ? "Ability" : "Skill";
            Index = unchecked((short)skillIndex);
        }

        public override ClientDelve GetClientDelve()
        {
            var delve = new ClientDelve(DelveType);

            delve.AddElement("Index", Index);

            if (skill != null)
            {
                delve.AddElement("Name", skill.Name);
            }
            else
            {
                delve.AddElement("Name", "(not found)");
            }

            return delve;
        }
    }
}
