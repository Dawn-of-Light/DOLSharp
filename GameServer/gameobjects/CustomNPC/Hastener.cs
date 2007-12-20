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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Keeps;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game Game Hastener NPC
	/// </summary>
	[NPCGuildScript("Hastener")]
	public class GameHastener : GameNPC
	{
		public override uint Flags
		{
			get { return (uint)GameNPC.eFlags.PEACE; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public GameHastener()
			: base()
		{

		}

		#region Examine/Interact Message
		protected const string MOVEMENT = "movement"; 

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a hastener.");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 5000);

			CastSpell(SpellMgr.HastenerSpell, SpellMgr.GuardSpellLine);
			return true;
		}


		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			if (source is GamePlayer == false)
				return true;
			GamePlayer player = (GamePlayer)source;

			switch (text)
			{
				case MOVEMENT:
					TurnTo(player, 5000);

					CastSpell(SpellMgr.HastenerSpell, SpellMgr.GuardSpellLine);
					break;
			}

			return true;
		} 
		#endregion Examine/Interact Message
	}
}