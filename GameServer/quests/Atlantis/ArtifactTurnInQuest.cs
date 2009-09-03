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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Atlantis
{
	/// <summary>
	/// Turn in Quest for changing artifact types
	/// </summary>
	// Serialize string: <dmgtype>;<weptype>;<stat>
	public class ArtifactTurnInQuest : ArtifactQuest
	{
		#region Member/Static variables

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The maximum number of steps that we can offer options in
		/// </summary>
		private const int MAXNUMOFSTEPS = 3;

		/// <summary>
		/// Used to get the right versions without incrementing the actual step number
		/// </summary>
		private int virtualStep = 0;

		private static ArtifactScholar m_scholarJarron = null;
		private static ArtifactScholar m_scholarAlaria = null;
		private static ArtifactScholar m_scholarElmer = null;
		private string m_scholarName = "";
		
		private string m_chosenTypes = null;
		public string ChosenTypes
		{
			get { return m_chosenTypes; }
			set { m_chosenTypes = value; }
		}

		private string m_artifactID = "";
		/// <summary>
		/// The artifact ID.
		/// </summary>
		public override String ArtifactID
		{
			get {return m_artifactID; }
		}

		private string m_name = "Artifact Turn-in Quest";
		public override string Name
		{
			get
			{
				return m_name;
			}
		}
		
		private List<string> m_curTypes = null;
		/// <summary>
		/// The current types to pick from
		/// </summary>
		public List<string> CurrentTypes
		{
			get { return m_curTypes; }
		}

		#endregion

		public ArtifactTurnInQuest() : base() { }

		public ArtifactTurnInQuest(GamePlayer questingPlayer, string scholarName)
			: base(questingPlayer)
		{
			m_name = string.Format("{0} {1}", GlobalConstants.RealmToName((eRealm)questingPlayer.Realm), m_name);
			m_scholarName = scholarName;
			m_curTypes = new List<string>(2);
			SetCustomProperty("Name", m_name);
			SetCustomProperty("SchName", scholarName);
		}

		public ArtifactTurnInQuest(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{

			LoadProperties();
		}

		public static string QuestTitle
		{
			get
			{
				return "Artifact Turn-in Quest";
			}
		}

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 0:
						return string.Format(" Give {0} the artifact to begin.", m_scholarName);
					case 1:
						return string.Format(" Tell {0} if you would like {2} of {1}.", m_scholarName, ArtifactID, GetOptions());
					case 2:
						return string.Format(" Tell {0} if you would like {2} of {1}.", m_scholarName, ArtifactID, GetOptions());
				}
				return base.Description;
			}
		}

		private string GetOptions()
		{
			string options = "";
			for (int i = 0; i < m_curTypes.Count; i++)
			{
				if (i == m_curTypes.Count - 1 && m_curTypes.Count > 1)
				{
					options = string.Format("{0} or the [{1}] version", options, m_curTypes[i]);
				}
				else
				{
					string comma = "";
					if (m_curTypes.Count > 2)
						comma = ",";
					options = string.Format("{0} the [{1}] version{2}", options, m_curTypes[i], comma);
				}
			}
			return options;
		}

		#region Load/Check

		public static void Init()
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + QuestTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarFemale") + " Alaria", eRealm.Midgard);
			if (npcs.Length == 0)
			{
				m_scholarAlaria = new ArtifactScholar();
				m_scholarAlaria.Model = 226;
				m_scholarAlaria.Name = "Artifact Scholar Alaria";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + m_scholarAlaria.Name + ", creating her ...");
				m_scholarAlaria.Realm = eRealm.Midgard;
				m_scholarAlaria.CurrentRegionID = 71;
				m_scholarAlaria.Size = 50;
				m_scholarAlaria.Level = 45;
				m_scholarAlaria.X = 565733;
				m_scholarAlaria.Y = 569502;
				m_scholarAlaria.Z = 7255;
				m_scholarAlaria.Heading = 708;
				m_scholarAlaria.MaxSpeedBase = 200;

				m_scholarAlaria.SaveIntoDatabase();

				m_scholarAlaria.AddToWorld();
			}
			else
				m_scholarAlaria = npcs[0] as ArtifactScholar;

			npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarMale") + " Jarron", eRealm.Albion);
			if (npcs.Length == 0)
			{
				m_scholarJarron = new ArtifactScholar();
				m_scholarJarron.Model = 50;
				m_scholarJarron.Name = "Artifact Scholar Jarron";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + m_scholarJarron.Name + ", creating him ...");
				m_scholarJarron.Realm = eRealm.Albion;
				m_scholarJarron.CurrentRegionID = 70;
				m_scholarJarron.Size = 50;
				m_scholarJarron.Level = 45;
				m_scholarJarron.X = 577936;
				m_scholarJarron.Y = 533228;
				m_scholarJarron.Z = 7295;
				m_scholarJarron.Heading = 3731;
				m_scholarJarron.MaxSpeedBase = 200;

				m_scholarJarron.SaveIntoDatabase();

				m_scholarJarron.AddToWorld();
			}
			else
				m_scholarJarron = npcs[0] as ArtifactScholar;

			npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarMale") + " Elmer", eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				m_scholarElmer = new ArtifactScholar();
				m_scholarElmer.Model = 374;
				m_scholarElmer.Name = "Artifact Scholar Elmer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + m_scholarElmer.Name + ", creating him ...");
				m_scholarElmer.Realm = eRealm.Hibernia;
				m_scholarElmer.CurrentRegionID = 72;
				m_scholarElmer.Size = 50;
				m_scholarElmer.Level = 45;
				m_scholarElmer.X = 552291;
				m_scholarElmer.Y = 576366;
				m_scholarElmer.Z = 6767;
				m_scholarElmer.Heading = 1074;
				m_scholarElmer.MaxSpeedBase = 200;

				m_scholarElmer.SaveIntoDatabase();

				m_scholarElmer.AddToWorld();
			}
			else
				m_scholarElmer = npcs[0] as ArtifactScholar;

			#endregion

			m_scholarAlaria.AddQuestToGive(typeof(ArtifactTurnInQuest));
			m_scholarJarron.AddQuestToGive(typeof(ArtifactTurnInQuest));
			m_scholarElmer.AddQuestToGive(typeof(ArtifactTurnInQuest));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + QuestTitle + "\" initialized");
		}

		/// <summary>
		/// Check if player is eligible for this quest.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			bool artCheck = false;
			List<string> arts = ArtifactMgr.GetArtifacts(player);

			foreach (string art in arts)
			{
				Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(art, 
					(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
				if (versions.Count > 1)
				{
					artCheck = true;
					break;
				}
			}

			return (player != null &&
				player.IsDoingQuest(this.GetType()) == null &&
				artCheck);
		}

		#endregion

		#region TalkTo/Receive Item

		/// <summary>
	    /// Handle an item given to the scholar.
	    /// </summary>
	    /// <param name="source"></param>
	    /// <param name="item"></param>
	    /// <param name="target"></param>
	    /// <returns></returns>
	    public override bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
	    {
	        GamePlayer player = source as GamePlayer;
	        ArtifactScholar scholar = target as ArtifactScholar;
	        if (player == null || scholar == null)
	            return false;

			//If the player is on the right step (the give me an art step)
	        if (Step == 0)
	        {
				//Lets see if they gave us a valid artifact
	            string ArtID = ArtifactMgr.GetArtifactIDFromItemID(item.Id_nb);
				Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtID, (eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
				//If this artifact has more than one option for them, give them the quest
				if (versions.Count > 1 && RemoveItem(player, item))
				{
					m_artifactID = ArtID;
					SetCustomProperty("Art", ArtifactID);
					SetCustomProperty("Id_nb", item.Id_nb);
					SetCustomProperty("AXP", (item as InventoryArtifact).Experience.ToString());
					SetCustomProperty("ALevel", (item as InventoryArtifact).ArtifactLevel.ToString());

					GetNextOptions(versions);

					SaveProperties();

					scholar.TurnTo(player);
					scholar.SayTo(player, string.Format("Would you prefer {0} of {1}?", GetOptions(), ArtifactID));
					Step = 1;
					return true;
				}
	        }
	        else
	        {
				//Why are they giving this to us!
	            player.Out.SendMessage(string.Format("{0} doesn't want that item.", scholar.Name), 
					eChatType.CT_Say, eChatLoc.CL_SystemWindow);
	        }

	        return base.ReceiveItem(source, target, item);
		}

		/// <summary>
		/// Called from ArtifactScholar when he receives a whisper
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
		{
			GamePlayer player = source as GamePlayer;
			ArtifactScholar scholar = target as ArtifactScholar;
			if (player == null || scholar == null)
				return false;

			//Did they send a valid string?
			if (m_curTypes.Contains(text))
			{
				//Apend their choice to the chosen types
				m_chosenTypes = string.Format("{0}{1};", m_chosenTypes, text);

				//Lets get the next set of options
				//Get the versions of this art
				Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID, (eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
				GetNextOptions(versions);

				//If we still have more options, give it to them
				if (virtualStep < MAXNUMOFSTEPS)
				{
					SaveProperties();

					scholar.TurnTo(player);
					scholar.SayTo(player, string.Format("Would you prefer {0} of {1}?", GetOptions(), ArtifactID));
					Step++;
				}
				//Else lets hand them their finished artifact!
				else
				{
					scholar.TurnTo(player);
					//Attempt to get the right version of the artifact

					m_chosenTypes = m_chosenTypes.Replace(";;", ";");

					if (!versions.ContainsKey(m_chosenTypes))
					{
						log.Warn(String.Format("Artifact version {0} not found", m_chosenTypes));
						scholar.SayTo(player, eChatLoc.CL_PopupWindow, "I can't find your chosen replacement, it may not be available for your class. Please try again.");
						ReturnArtifact(player);
						return true;
					}

					ItemTemplate template = versions[m_chosenTypes] as ItemTemplate;

					if (GiveItem(player, template))
					{
						FinishQuest();
						scholar.SayTo(player, eChatLoc.CL_PopupWindow, string.Format("Here is your {0}, {1}. May it serve you well!", ArtifactID, player.CharacterClass.Name));
						return true;
					}

					return false;
				}
			}
			return base.WhisperReceive(source, target, text);
		}


		protected void ReturnArtifact(GamePlayer player)
		{
            ItemTemplate itemTemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), GetCustomProperty("Id_nb"));
			InventoryArtifact artifact = new InventoryArtifact(itemTemplate);
			artifact.ArtifactLevel = Convert.ToInt32(GetCustomProperty("ALevel"));
			artifact.Experience = Convert.ToInt64(GetCustomProperty("AXP"));
			artifact.CheckAbilities();

			if (!player.ReceiveItem(null, artifact))
			{
				player.Out.SendMessage(String.Format("Your backpack is full, please make some room and try again.  You may have to relog to get to complete this quest."), eChatType.CT_Important, eChatLoc.CL_PopupWindow);
				return;
			}

			FinishQuest();
		}


		/// <summary>
		/// Hand out an artifact.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="player"></param>
		/// <param name="artifactID"></param>
		/// <param name="itemTemplate"></param>
		protected new static bool GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			InventoryArtifact item = new InventoryArtifact(itemTemplate);
			if (!player.ReceiveItem(null, item))
			{
				player.Out.SendMessage(String.Format("Your backpack is full, please make some room and try again."), eChatType.CT_Important, eChatLoc.CL_PopupWindow);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Called from ArtifactScholar when he is interacted on
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool Interact(GameLiving source, GameLiving target)
		{
			GamePlayer player = source as GamePlayer;
			ArtifactScholar scholar = target as ArtifactScholar;

			if (player == null || scholar == null)
				return false;

			//If the player is already doing the quest, we look if he has the items:
			if (Step == 0)
			{
				scholar.SayTo(player, "Since you are still interested, simply hand me your Artifact and I shall begin.");
			}
			else
			{
				string options = "";
				foreach (string str in m_curTypes)
					options += "[" + str + "] ";

				scholar.SayTo(player, "Choose your options I mentioned earlier and I shall begin.\n\nYour options are: " + options);
			}

			return true;
		}

		private void GetNextOptions(Dictionary<String, ItemTemplate> versions)
		{
			//Clear the current types since we are going to be offering more
			m_curTypes.Clear();

			//Loop until we find the next set of options
			while (m_curTypes.Count <= 1 && virtualStep < MAXNUMOFSTEPS)
			{
				//Go through each set of options in our list
				foreach (string str in versions.Keys)
				{
					//Used as the key to store the option in the database
					string[] splitVersion = str.Split(';');

					//Get the current option using our virtual step.  This gets the right option in the DT;WT;STAT; list
					string type = splitVersion[virtualStep];

					//If we got a valid type and we don't already have it, put/save it
					if (type != "" && !m_curTypes.Contains(type))
					{
						m_curTypes.Add(type);
					}
				}

				//If there was only one option added, obviously that is the only thing they can pick
				if (m_curTypes.Count == 1)
				{
					m_chosenTypes = string.Format("{0}{1};", m_chosenTypes, m_curTypes[0]);
					m_curTypes.Clear();
				}
				else if (m_curTypes.Count == 0)
				{
					//We need to add the end semi-colon even if we don't get an option!
					m_chosenTypes = string.Format("{0};", m_chosenTypes);
				}

				//Increment our virtual step, if this loops again it will proceed to the next set of options
				virtualStep++;
			}
		}

		private void ClearCustomOptions()
		{
			RemoveCustomProperty("Types");
			RemoveCustomProperty(virtualStep.ToString());
		}

		/// <summary>
		/// Handles the saving of the types and virtualstep.  Clears them first!
		/// </summary>
		private void SaveProperties()
		{
			//Clear any remaining options
			ClearCustomOptions();

			//Save the options in the database!
			string options = "";
			foreach (string str in m_curTypes)
			{
				options = string.Format("{0}|{1}", options, str);
			}
			SetCustomProperty("Types", options);

			//Save the virtual step so we can reload the quest
			SetCustomProperty("VS", virtualStep.ToString());
		}

		/// <summary>
		/// Handles the loading of the name, artID, any options, and the VS
		/// </summary>
		private void LoadProperties()
		{
			this.m_scholarName = GetCustomProperty("SN");
			this.m_artifactID = GetCustomProperty("Art");
			this.m_name = GetCustomProperty("Name");

			m_curTypes = new List<string>(2);
			string combinedOptions = GetCustomProperty("Types");
			if (combinedOptions != null)
			{
				string[] options = combinedOptions.TrimStart().TrimEnd().Split('|');
				foreach (string str in options)
					if (str != " " && str != "")
						m_curTypes.Add(str);
			}

			//If some how the VS got messed up, reset the quest
			if (!int.TryParse(GetCustomProperty("VS"), out this.virtualStep) || this.virtualStep > MAXNUMOFSTEPS || this.virtualStep < 0)
			{
				//This would be a strange error - but for some reason it doesn't like Step = 0 o.O
				//this.Step = 0;
				this.virtualStep = 0;
				m_curTypes.Clear();
				m_chosenTypes = "";
			}
		}

		#endregion

		public override void FinishQuest()
		{
			Step = -1; // -1 indicates finished or aborted quests etc, they won't show up in the list
			m_questPlayer.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(m_questPlayer.Client, "ArtifactTurnInQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);

			// move quest from active list to finished list...
			m_questPlayer.QuestList.Remove(this);

			if (m_questPlayer.HasFinishedQuest(this.GetType()) == 0)
				m_questPlayer.QuestListFinished.Add(this);

			DeleteFromDatabase();

			m_questPlayer.Out.SendQuestListUpdate();
		}
	}
}
