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
using System.Collections;
using DOL.GS.Spells;
using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Represents an in-game Game Hastener NPC
	/// </summary>
	[Subclass(NameType=typeof(GameHastener), ExtendsType=typeof(GameMob))] 
	public class GameHastener : GameMob
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public GameHastener() : base()
		{
			//load spell handler at start
			SpellLine reservedSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
			if (reservedSpellLine != null)
			{
				IList spells = SkillBase.GetSpellList(reservedSpellLine.KeyName);
				if (spells != null)
				{
					foreach (Spell spell in spells) 
					{
						if (spell.ID == 2430) //Speed of the Realm
						{
							if(spell.Level <= Level)
							{
								m_hasteSpellHandler = ScriptMgr.CreateSpellHandler(this, spell, reservedSpellLine);
							}
							break;
						}
					}
				}
			}
		}

		private ISpellHandler m_hasteSpellHandler;

		#region Examine/Interact Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a hastener.");
			return list;
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			TurnTo(player, 10000);

			foreach(GamePlayer players in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE)) 
			{
				players.Out.SendSpellCastAnimation(this, 1, 20);
			}
			m_hasteSpellHandler.StartSpell(player);
			return true;
		}
		#endregion Examine/Interact Message
	}
}