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
using DOL.Database2;
using DOL.GS.PacketHandler;
using System.Collections;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Bruiser artifact.
	/// </summary>
	/// <author>Aredhel</author>
	public class Bruiser : ArtifactQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public Bruiser()
			: base() { }

		public Bruiser(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		public Bruiser(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		private static String m_artifactID = "Bruiser";

		/// <summary>
		/// Quest initialisation.
		/// </summary>
		public static void Init()
		{
			ArtifactQuest.Init(m_artifactID, typeof(Bruiser));
		}

		/// <summary>
		/// Handle an item given to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
		{
			if (base.ReceiveItem(source, target, item))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactID)
			{
				scholar.TurnTo(player);
				if (RemoveItem(player, item))
				{
					String reply = String.Format("Great! Now, I can make you a [single] handed or [double] handed version");

					switch (player.CharacterClass.ID)
					{
						case (int)eCharacterClass.Armsman:
							reply += " or I can make you a [polearm] version. Which would you prefer?";
							break;
						default:
							reply += ", which would you prefer?";
							break;
					}

					scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
					Step = 3;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Handle whispers to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
		{
			if (base.WhisperReceive(source, target, text))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 1 && text.ToLower() == ArtifactID.ToLower())
			{
				String reply = String.Format("Ah, Bruiser! Many smiths would love a hammer {0} {1} {2} {3}",
					"such as that! Well, for ",
					player.GetName(1, false),
					"like you, I am not sure what purpose it will serve! I hope you have better luck than",
					"its previous owner! Do you have any scrolls with it?");
				scholar.TurnTo(player);
				scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
				Step = 2;
				return true;
			}
			else if (Step == 3)
			{
				switch (text.ToLower())
				{
					case "single":
					case "double":
					case "polearm":
						{
							if (text.ToLower() == "polearm" && 
								player.CharacterClass.ID != (int)eCharacterClass.Armsman)
								return false;

							String versionID = String.Format(";{0};", text.ToLower());
							Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID,
								(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
							if (!versions.ContainsKey(versionID))
							{
								log.Warn(String.Format("Artifact version {0} not found", versionID));
								return false;
							}
							ItemTemplate template = versions[versionID];
							GiveItem(scholar, player, ArtifactID, template);
							String reply = String.Format("May Bruiser serve you well. Do not lose {0}",
								"this, for I can only unlock the artifact's powers once.");
							scholar.TurnTo(player);
							scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
							FinishQuest();
							return true;
						}
				}
				return false;
			}

			return false;
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
					case 1:
						return "Kill Taur Warlord.";
					case 2:
						return "Turn in the completed book.";
					case 3:
						return "Do you want a [single] or [double] handed version?";
					default:
						return base.Description;
				}
			}
		}

		/// <summary>
		/// The name of the quest (not necessarily the same as
		/// the name of the reward).
		/// </summary>
		public override string Name
		{
			get { return "Bruiser"; }
		}

		/// <summary>
		/// The artifact ID.
		/// </summary>
		public override String ArtifactID
		{
			get { return m_artifactID; }
		}
	}
}
