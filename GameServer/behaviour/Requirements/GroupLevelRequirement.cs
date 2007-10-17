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
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;

namespace DOL.GS.Behaviour.Requirements
{

	/// <summary>
	/// Requirements describe what must be true to allow a QuestAction to fire.
	/// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
	/// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
	/// </summary>
    [RequirementAttribute(RequirementType=eRequirementType.GroupLevel)]
	public class GroupLevelRequirement : AbstractRequirement<int,Unused>
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="n"></param>
		/// <param name="v"></param>
		/// <param name="comp"></param>
        public GroupLevelRequirement(GameNPC defaultNPC,  Object n, Object v, eComparator comp)
            : base(defaultNPC, eRequirementType.GroupLevel, n, v, comp)
		{   			
		}

        /// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="defaultNPC">Parent defaultNPC of this Requirement</param>		
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>		
		/// <param name="comp">Comparator used if some values are veeing compared</param>
        public GroupLevelRequirement(GameNPC defaultNPC, int n,  eComparator comp)
            : this(defaultNPC,  (object)n, (object)null, comp)
		{   			
		}

		/// <summary>
        /// Checks the added requirement whenever a trigger associated with this defaultNPC fires.(returns true)
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public override bool Check(DOLEvent e, object sender, EventArgs args)
		{
			bool result = true;
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);

            PlayerGroup group = player.PlayerGroup;
            int grouplevel = 0;
            if (group != null)
            {
                foreach (GamePlayer member in group.GetPlayersInTheGroup())
                {
                    grouplevel += member.Level;
                }
            }
            else
            {
                grouplevel += player.Level;
            }
            result = compare(grouplevel, N, Comparator);

			return result;
		}

		
    }
}
