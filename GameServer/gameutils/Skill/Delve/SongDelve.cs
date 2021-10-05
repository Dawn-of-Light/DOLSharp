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
using DOL.GS.Spells;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Delve
{
	public class SongDelve : SkillDelve
	{
		private ISpellHandler spellHandler;
		private Spell Spell => spellHandler.Spell;

		public SongDelve(int id)
		{
			Spell spell = SkillBase.GetSpellByTooltipID((ushort)id);
			spellHandler = ScriptMgr.CreateSpellHandler(null, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
			DelveType = "Song";
			Index = unchecked((short)spellHandler.Spell.InternalID);
		}
                
        public SongDelve(int id, GameClient client) : this(id)
        {
            int level = Spell.Level;
            int spellID = Spell.ID;

            foreach (SpellLine line in client.Player.GetSpellLines())
            {
                Spell s = SkillBase.GetSpellList(line.KeyName).Where(o => o.ID == spellID).FirstOrDefault();
                if (s != null)
                {
                    level = s.Level;
                    break;
                }
            }
            Spell.Level = level;

        }

        public override ClientDelve GetClientDelve()
        {
			if (spellHandler == null) return NotFoundClientDelve;

			var clientDelve = new ClientDelve(DelveType);
			clientDelve.Index = Index;
			clientDelve.AddElement("effect", Index);
			clientDelve.AddElement("Name", Spell.Name);
			return clientDelve;
		}

		public override IEnumerable<ClientDelve> GetClientDelves()
		{
			var result = new List<ClientDelve>() { GetClientDelve() };
			result.Add(new SpellDelve(Spell).GetClientDelve());
			return result;
		}
	}
}
