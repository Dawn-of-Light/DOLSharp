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
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Behaviour;

namespace DOL.GS.Behaviour.Requirements
{

	/// <summary>
	/// Requirements describe what must be true to allow a QuestAction to fire.
	/// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
	/// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
	/// </summary>
    [RequirementAttribute(RequirementType=eRequirementType.Region)]
	public class RegionRequirement : AbstractRequirement<int,int>
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="n"></param>
		/// <param name="v"></param>
		/// <param name="comp"></param>
        public RegionRequirement(GameNPC defaultNPC,  Object n, Object v, eComparator comp)
            : base(defaultNPC, eRequirementType.Region, n, v, comp)
		{   			
		}

        /// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="defaultNPC"></param>
        /// <param name="n"></param>
        /// <param name="v"></param>
        public RegionRequirement(GameNPC defaultNPC, int n, int v)
            : this(defaultNPC, (object)n, (object)v, eComparator.None)
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
    
            //Dinberg on Instances:
            //I've changed this to skin. This is so that instances can cause zone 
            //requirements on quests to be triggered too! It's unlikely they will
            //ever get the right ID otherwise.
            result = (player.CurrentRegion.Skin == V && player.CurrentZone.ZoneSkinID == N);

			return result;
		}

		
    }
}
