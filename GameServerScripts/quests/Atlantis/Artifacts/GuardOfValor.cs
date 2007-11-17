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
using DOL.Events;
using DOL.GS.Quests;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Guard of Valor artifact.
	/// </summary>
	/// <author>Aredhel</author>
	public class GuardOfValor : ArtifactQuest
	{
		public GuardOfValor()
			: base() { }

		public GuardOfValor(Artifact artifact, Type encounterType)
			: base(artifact, encounterType) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		public GuardOfValor(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Check if player is eligible for this quest.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			if (!base.CheckQuestQualification(player))
				return false;

			// TODO: Check if this is the correct level for the quest.
			return (player.Level >= 40);
		}

		/// <summary>
		/// Description for the current step.
		/// </summary>
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 2 :
						// TODO: Get correct description.
						return "[Step #2]: Turn in the complete Love Story.";
					default :
						return base.Description;
				}
			}
		}

		/// <summary>
		/// The name of the quest.
		/// </summary>
		public override string Name
		{
			get { return "A Gift of Love"; }
		}

		/// <summary>
		/// Decline the quest. This happens if the player cannot
		/// actually use the artifact.
		/// </summary>
		/// <param name="scholar"></param>
		public override void DeclineQuest(Scholar scholar)
		{
			base.DeclineQuest(scholar);
		}

		/// <summary>
		/// Start the quest.
		/// </summary>
		/// <param name="scholar"></param>
		public override void StartQuest(Scholar scholar)
		{
			if (scholar == null)
				return;

			base.StartQuest(scholar);
			String request = String.Format("Tell me, {0}, do you have any versions of the Love Story {1} {2} {3} {4}",
				QuestPlayer.CharacterClass.Name,
                "to go with the Guard of Valor? I have found a few copies, but I am always looking",
			    "for more. Each one has different information in them that helps me with",
			    "my research. Please give me the Love Story now while I finish up with",
			    "the Guard of Valor.");
			scholar.TurnTo(QuestPlayer);
			scholar.SayTo(QuestPlayer, eChatLoc.CL_PopupWindow, request);
		}

		/// <summary>
		/// End the quest.
		/// </summary>
		/// <param name="scholar"></param>
		public override void EndQuest(Scholar scholar)
		{
			if (scholar == null)
				return;

			String farewell = "Can you feel the magic of the Guard of Valor flowing once again? ";
			farewell += "It comes from Aloeus' love for his beautiful Nikolia. When Aloeus ";
			farewell += "presented the gift to Nikolia, the magic in it bound to her. And now as ";
			farewell += "I present it to you, the magic in it will bind itself to you, so that no ";
			farewell += String.Format("other may wear it. I beg you, {0}, take care not to destroy ",
				QuestPlayer.CharacterClass.Name);
			farewell += String.Format("such a gift! It cannot be replaced! I wish you well, {0}.",
				QuestPlayer.CharacterClass.Name);
			scholar.TurnTo(QuestPlayer);
			scholar.SayTo(QuestPlayer, eChatLoc.CL_PopupWindow, farewell);
			base.EndQuest(scholar);
		}
	}
}
