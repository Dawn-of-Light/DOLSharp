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
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests.Atlantis
{
    /// <summary>
    /// Base class for all artifact quests.
    /// </summary>
    /// <author>Aredhel</author>
    public class ArtifactQuest : AbstractQuest
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public ArtifactQuest()
			: base() { }

        private Artifact m_artifact;
		private Type m_encounterType;

		public ArtifactQuest(Artifact artifact, Type encounterType) 
			: base()
		{
            m_artifact = artifact;
			m_encounterType = encounterType;
		}

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		public ArtifactQuest(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Check if player is eligible for this quest.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// Must have the encounter, must have the book; must not be on the quest
			// and must not have the quest finished either.

			return (player != null &&
				m_encounterType != null &&
                ArtifactMgr.HasBookForArtifact(player, m_artifact) &&
				player.HasFinishedQuest(m_encounterType) > 0 &&
				player.IsDoingQuest(this.GetType()) == null &&
				player.HasFinishedQuest(this.GetType()) == 0);
		}

		/// <summary>
		/// Handles events.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
			if (e == GamePlayerEvent.AcceptQuest)
			{
				GamePlayer player = sender as GamePlayer;
				if (player != null)
					player.Out.SendMessage(String.Format("You have been given the {0} quest.",
						Name), eChatType.CT_Group, eChatLoc.CL_ChatWindow);
				Step = 2;
			}
        }

		/// <summary>
		/// Decline the quest. This happens if the player cannot
		/// actually use the artifact.
		/// </summary>
		/// <param name="scholar"></param>
		public virtual void DeclineQuest(Scholar scholar)
		{
			if (scholar == null || QuestPlayer == null)
				return;
		}

		/// <summary>
		/// Start the quest.
		/// </summary>
		/// <param name="scholar"></param>
		public virtual void StartQuest(Scholar scholar)
		{
		}

		/// <summary>
		/// End the quest.
		/// </summary>
		/// <param name="scholar"></param>
		public virtual void EndQuest(Scholar scholar)
		{
			FinishQuest();
		}
    }
}
