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
using System.Text;
using DOL.Events;
using DOL.Database;
using log4net;
using System.Reflection;
using DOL.GS.Scripts;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Behaviour;

namespace DOL.GS.Quests.Requirements
{

	/// <summary>
	/// Requirements describe what must be true to allow a QuestAction to fire.
	/// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
	/// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
	/// </summary>
    [RequirementAttribute(RequirementType=eRequirementType.Quest)]
	public class QuestCompletedRequirement : AbstractRequirement<Type,int>
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>		
		/// <param name="type">RequirementType</param>
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="comp">Comparator used if some values are veeing compared</param>
        public QuestCompletedRequirement(GameNPC defaultNPC, Object n, Object v, eComparator comp)
            : base(defaultNPC,eRequirementType.Quest, n, v, comp)
		{   			
		}

        /// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>		
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="comp">Comparator used if some values are veeing compared</param>
        public QuestCompletedRequirement(GameNPC defaultNPC, Type questType, int v, eComparator comp)
            : this(defaultNPC, (object)questType, (object)v, comp)
		{   			
		}

		/// <summary>
		/// Checks the added requirement whenever a trigger associated with this questpart fires.(returns true)
		/// </summary>        
		/// <param name="e">DolEvent of notify call</param>
		/// <param name="sender">Sender of notify call</param>
		/// <param name="args">EventArgs of notify call</param>
		/// <param name="player">GamePlayer this call is related to, can be null</param>
		/// <returns>true if all Requirements forQuestPart where fullfilled, else false</returns>
		public override bool Check(DOLEvent e, object sender, EventArgs args)
		{
			bool result = true;
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
            
            int finishedCount = player.HasFinishedQuest(N);            
            result = compare(finishedCount, V, Comparator);            

			return result;
		}

		
    }
}
