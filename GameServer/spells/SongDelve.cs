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

namespace DOL.GS.Spells
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

        public override ClientDelve GetClientDelve()
        {
			if (spellHandler == null) return NotFoundClientDelve;

			var clientDelve = new ClientDelve(DelveType);
			clientDelve.Index = Index;
			clientDelve.AddElement("effect", Index);
			clientDelve.AddElement("Name", Spell.Name);
			return clientDelve;
		}

		public override IEnumerable<ClientDelve> GetAssociatedClientDelves()
			=> new List<ClientDelve>() { new SpellDelve(Spell).GetClientDelve() };
	}
}
