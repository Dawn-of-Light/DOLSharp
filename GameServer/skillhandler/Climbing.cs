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
using DOL.GS.PacketHandler;
using DOL.GS;
using DOL.Database;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Fury shout
	/// </summary>
	[SkillHandlerAttribute(Abilities.ClimbSpikes)]
	public class ClimbingAbilityHandler : SpellCastingAbilityHandler
	{
		private static int spellid = -1;
		
		public override long Preconditions
		{
			get
			{
				return DEAD | SITTING | MEZZED | STUNNED;
			}
		}
		public override int SpellID
		{
			get
			{
				return spellid;
			}
		}

		public ClimbingAbilityHandler()
		{
			// Graveen: crappy, but not hardcoded. if we except by the ability name ofc...
			// problems are: 
			// 		- matching vs ability name / spell name needed
			//		- spell name is not indexed
			// perhaps a basis to think about, but definitively not the design we want.
			if (spellid == -1)
			{
				spellid=0;
				DBSpell climbSpell = GameServer.Database.SelectObject<DBSpell>("Name = '" + Abilities.ClimbSpikes.ToString() + "'");
				if (climbSpell != null)
					spellid = climbSpell.SpellID;
			}
		}
	}
}
