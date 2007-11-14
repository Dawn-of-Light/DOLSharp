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
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Quests
{
    /// <summary>
    /// Base class for all artifact quests.
    /// </summary>
    /// <author>Aredhel</author>
    public class ArtifactQuest : AbstractQuest
    {
        public ArtifactQuest(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        public ArtifactQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        /// <summary>
        /// Level 40 requirement to get any artifact credit at all, where
        /// not applicable, override.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            return (player == null || player.Level < 40);
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            // Need to do anything here?
        }
    }
}
