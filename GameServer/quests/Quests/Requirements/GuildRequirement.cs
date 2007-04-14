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
using DOL.GS.Quests.Attributes;

namespace DOL.GS.Quests.Requirements
{

	/// <summary>
	/// Requirements describe what must be true to allow a QuestAction to fire.
	/// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
	/// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
	/// </summary>
    [QuestRequirementAttribute(RequirementType=eRequirementType.Guild,DefaultValueN=eDefaultValueConstants.NPC)]
	public class GuildRequirement : AbstractQuestRequirement<GameLiving,string>
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="questPart">Parent QuestPart of this Requirement</param>
		/// <param name="type">RequirementType</param>
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="comp">Comparator used if some values are veeing compared</param>
        public GuildRequirement(BaseQuestPart questPart, eRequirementType type, Object n, Object v, eComparator comp)
            : base(questPart, type, n, v, comp)
		{   			
		}

        /// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="questPart">Parent QuestPart of this Requirement</param>		
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>		
        public GuildRequirement(BaseQuestPart questPart,  GameLiving n, string v)
            : this(questPart, eRequirementType.Guild, (object)n, (object)v, eComparator.None)
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
		public override bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player)
		{
			bool result = true;            
            
            result = N.GuildName == V;

			return result;
		}

		
    }
}
