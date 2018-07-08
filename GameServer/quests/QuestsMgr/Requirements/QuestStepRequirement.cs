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
using DOL.Events;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Behaviour;

namespace DOL.GS.Quests.Requirements
{

    /// <summary>
    /// Requirements describe what must be true to allow a QuestAction to fire.
    /// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
    /// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.
    /// </summary>
    [Requirement(RequirementType=eRequirementType.QuestStep)]
    public class QuestStepRequirement : AbstractRequirement<Type,int>
    {
        /// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="defaultNpc"></param>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        public QuestStepRequirement(GameNPC defaultNpc, object n, object v, eComparator comp)
            : base(defaultNpc, eRequirementType.QuestStep, n, v, comp)
        {
        }

        /// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="defaultNpc"></param>
        /// <param name="questType"></param>
        /// <param name="v"></param>
        /// <param name="comp"></param>
        public QuestStepRequirement(GameNPC defaultNpc, Type questType,int v, eComparator comp)
            : this(defaultNpc, questType as object, v, comp)
        {
        }

        /// <summary>
        /// Checks the added requirement whenever a trigger associated with this questpart fires.(returns true)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override bool Check(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
            AbstractQuest playerQuest = player.IsDoingQuest(N);

            bool result = true;
            if (playerQuest != null)
            {
                result &= compare(playerQuest.Step, V, Comparator);
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
