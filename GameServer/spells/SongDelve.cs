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
	public class SongDelve
	{
		private ISpellHandler spellHandler;
		private Spell Spell => spellHandler.Spell;

		private int TooltipId => unchecked((short)Spell.InternalID);

		public SongDelve(ISpellHandler spellHandler)
		{
			this.spellHandler = spellHandler;
		}

		public string GetClientMessage()
		{
			var clientDelve = new ClientDelve("Song");
			clientDelve.AddElement("Index", TooltipId);
			clientDelve.AddElement("effect", TooltipId);
			clientDelve.AddElement("Name", Spell.Name);
			return clientDelve.ClientMessage;
		}

		public string GetNotFoundClientMessage(int tooltipId)
		{
			var clientDelve = new ClientDelve("Song");
			clientDelve.AddElement("Index", tooltipId);
			clientDelve.AddElement("Name", "(not found)");
			return clientDelve.ClientMessage;
		}
	}
}
