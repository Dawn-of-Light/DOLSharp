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
using System.Reflection;
using DOL.Database;
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
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The maximum number of steps that we can offer options in
        /// </summary>
        private const int MaxNumOfSteps = 3;

        /// <summary>
        /// Used to get the right versions without incrementing the actual step number
        /// </summary>
        private int _virtualStep;
        private static ArtifactScholar _scholarJarron;
        private static ArtifactScholar _scholarAlaria;
        private static ArtifactScholar _scholarElmer;
        private string _scholarName = string.Empty;

        public string ChosenTypes { get; set; }

        private string _artifactId = string.Empty;
        /// <summary>
        /// The artifact ID.
        /// </summary>
        public override string ArtifactId => _artifactId;

        private string _name = "Artifact Turn-in Quest";

        public override string Name => _name;

        /// <summary>
        /// The current types to pick from
        /// </summary>
        public List<string> CurrentTypes { get; private set; }

        public ArtifactTurnInQuest()
        { }

        public ArtifactTurnInQuest(GamePlayer questingPlayer, string scholarName)
            : base(questingPlayer)
        {
            _name = $"{GlobalConstants.RealmToName(questingPlayer.Realm)} {_name}";
            _scholarName = scholarName;
            CurrentTypes = new List<string>(2);
            SetCustomProperty("Name", _name);
            SetCustomProperty("SchName", scholarName);
        }

        public ArtifactTurnInQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {

            LoadProperties();
        }

        public static string QuestTitle => "Artifact Turn-in Quest";

        public override string Description
        {
            get
            {
                switch (Step)
                {
                    case 0:
                        return $" Give {_scholarName} the artifact to begin.";
                    case 1:
                        return $" Tell {_scholarName} if you would like {GetOptions()} of {ArtifactId}.";
                    case 2:
                        return $" Tell {_scholarName} if you would like {GetOptions()} of {ArtifactId}.";
                }

                return base.Description;
            }
        }

        private string GetOptions()
        {
            string options = string.Empty;
            for (int i = 0; i < CurrentTypes.Count; i++)
            {
                if (i == CurrentTypes.Count - 1 && CurrentTypes.Count > 1)
                {
                    options = $"{options} or the [{CurrentTypes[i]}] version";
                }
                else
                {
                    string comma = string.Empty;
                    if (CurrentTypes.Count > 2)
                    {
                        comma = ",";
                    }

                    options = $"{options} the [{CurrentTypes[i]}] version{comma}";
                }
            }

            return options;
        }

        public static void Init()
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
            {
                return;
            }

            if (Log.IsInfoEnabled)
            {
                Log.Info("Quest \"" + QuestTitle + "\" initializing ...");
            }

            GameNPC[] npcs = WorldMgr.GetObjectsByName<GameNPC>($"{LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarFemale")} Alaria", eRealm.Midgard);
            if (npcs.Length == 0)
            {
                _scholarAlaria = new ArtifactScholar
                {
                    Model = 226,
                    Name = "Artifact Scholar Alaria",
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 71,
                    Size = 50,
                    Level = 45,
                    X = 565733,
                    Y = 569502,
                    Z = 7255,
                    Heading = 708,
                    MaxSpeedBase = 200
                };

                if (Log.IsWarnEnabled)
                {
                    Log.Warn($"Could not find {_scholarAlaria.Name}, creating her ...");
                }

                _scholarAlaria.SaveIntoDatabase();
                _scholarAlaria.AddToWorld();
            }
            else
            {
                _scholarAlaria = npcs[0] as ArtifactScholar;
            }

            npcs = WorldMgr.GetObjectsByName<GameNPC>($"{LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarMale")} Jarron", eRealm.Albion);
            if (npcs.Length == 0)
            {
                _scholarJarron = new ArtifactScholar
                {
                    Model = 50,
                    Name = "Artifact Scholar Jarron",
                    Realm = eRealm.Albion,
                    CurrentRegionID = 70,
                    Size = 50,
                    Level = 45,
                    X = 577936,
                    Y = 533228,
                    Z = 7295,
                    Heading = 3731,
                    MaxSpeedBase = 200
                };

                if (Log.IsWarnEnabled)
                {
                    Log.Warn("Could not find " + _scholarJarron.Name + ", creating him ...");
                }

                _scholarJarron.SaveIntoDatabase();
                _scholarJarron.AddToWorld();
            }
            else
            {
                _scholarJarron = npcs[0] as ArtifactScholar;
            }

            npcs = WorldMgr.GetObjectsByName<GameNPC>(
                $"{LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "ArtifactTurnInQuest.Init.ArtifactScholarMale")} Elmer", eRealm.Hibernia);
            if (npcs.Length == 0)
            {
                _scholarElmer = new ArtifactScholar
                {
                    Model = 374,
                    Name = "Artifact Scholar Elmer",
                    Realm = eRealm.Hibernia,
                    CurrentRegionID = 72,
                    Size = 50,
                    Level = 45,
                    X = 552291,
                    Y = 576366,
                    Z = 6767,
                    Heading = 1074,
                    MaxSpeedBase = 200
                };

                if (Log.IsWarnEnabled)
                {
                    Log.Warn("Could not find " + _scholarElmer.Name + ", creating him ...");
                }

                _scholarElmer.SaveIntoDatabase();
                _scholarElmer.AddToWorld();
            }
            else
            {
                _scholarElmer = npcs[0] as ArtifactScholar;
            }

            _scholarAlaria?.AddQuestToGive(typeof(ArtifactTurnInQuest));
            _scholarJarron?.AddQuestToGive(typeof(ArtifactTurnInQuest));
            _scholarElmer?.AddQuestToGive(typeof(ArtifactTurnInQuest));

            if (Log.IsInfoEnabled)
            {
                Log.Info($"Quest \"{QuestTitle}\" initialized");
            }
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
                Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(art, (eCharacterClass)player.CharacterClass.ID, player.Realm);
                if (versions.Count > 1)
                {
                    artCheck = true;
                    break;
                }
            }

            return player != null &&
                    player.IsDoingQuest(GetType()) == null &&
                    artCheck;
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
            if (!(source is GamePlayer player) || !(target is ArtifactScholar scholar))
            {
                return false;
            }

            // If the player is on the right step (the give me an art step)
            if (Step == 0)
            {
                // Lets see if they gave us a valid artifact
                string artId = ArtifactMgr.GetArtifactIDFromItemID(item.Id_nb);
                Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(artId, (eCharacterClass)player.CharacterClass.ID, player.Realm);

                // If this artifact has more than one option for them, give them the quest
                if (versions != null && versions.Count > 1 && RemoveItem(player, item))
                {
                    _artifactId = artId;
                    SetCustomProperty("Art", ArtifactId);
                    SetCustomProperty("Id_nb", item.Id_nb);
                    SetCustomProperty("AXP", (item as InventoryArtifact)?.Experience.ToString());
                    SetCustomProperty("ALevel", (item as InventoryArtifact)?.ArtifactLevel.ToString());

                    GetNextOptions(versions);

                    SaveProperties();

                    scholar.TurnTo(player);
                    scholar.SayTo(player, $"Would you prefer {GetOptions()} of {ArtifactId}?");
                    Step = 1;
                    return true;
                }
            }
            else
            {
                // Why are they giving this to us!
                player.Out.SendMessage($"{scholar.Name} doesn't want that item.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
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
            if (!(source is GamePlayer player) || !(target is ArtifactScholar scholar))
            {
                return false;
            }

            // Did they send a valid string?
            if (CurrentTypes.Contains(text))
            {
                // Apend their choice to the chosen types
                ChosenTypes = $"{ChosenTypes}{text};";

                // Lets get the next set of options
                // Get the versions of this art
                Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactId, (eCharacterClass)player.CharacterClass.ID, player.Realm);

                // If we still have more options, give it to them
                if (GetNextOptions(versions) && _virtualStep < MaxNumOfSteps)
                {
                    SaveProperties();

                    scholar.TurnTo(player);
                    scholar.SayTo(player, $"Would you prefer {GetOptions()} of {ArtifactId}?");
                    Step++;
                }

                // Else lets hand them their finished artifact!
                else
                {
                    scholar.TurnTo(player);

                    // Attempt to get the right version of the artifact
                    ChosenTypes = ChosenTypes.Replace(";;", ";");

                    if (!versions.ContainsKey(ChosenTypes))
                    {
                        Log.Warn($"Artifact version {ChosenTypes} not found");
                        scholar.SayTo(player, eChatLoc.CL_PopupWindow, "I can't find your chosen replacement, it may not be available for your class. Please try again.");
                        ReturnArtifact(player);
                        return true;
                    }

                    ItemTemplate template = versions[ChosenTypes];

                    if (GiveItem(player, template))
                    {
                        FinishQuest();
                        scholar.SayTo(player, eChatLoc.CL_PopupWindow, $"Here is your {ArtifactId}, {player.CharacterClass.Name}. May it serve you well!");
                        return true;
                    }

                    return false;
                }
            }

            return base.WhisperReceive(source, target, text);
        }

        protected void ReturnArtifact(GamePlayer player)
        {
            ItemTemplate itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(GetCustomProperty("Id_nb"));
            InventoryArtifact artifact = new InventoryArtifact(itemTemplate)
            {
                ArtifactLevel = Convert.ToInt32(GetCustomProperty("ALevel")),
                Experience = Convert.ToInt64(GetCustomProperty("AXP"))
            };

            artifact.UpdateAbilities(itemTemplate);

            if (!player.ReceiveItem(null, artifact))
            {
                player.Out.SendMessage("Your backpack is full, please make some room and try again.  You may have to relog to get to complete this quest.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
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
                player.Out.SendMessage("Your backpack is full, please make some room and try again.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
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
            if (!(source is GamePlayer player) || !(target is ArtifactScholar scholar))
            {
                return false;
            }

            // If the player is already doing the quest, we look if he has the items:
            if (Step == 0)
            {
                scholar.SayTo(player, "Since you are still interested, simply hand me your Artifact and I shall begin.");
            }
            else
            {
                string options = string.Empty;
                foreach (string str in CurrentTypes)
                {
                    options += $"[{str}] ";
                }

                scholar.SayTo(player, "Choose your options I mentioned earlier and I shall begin.\n\nYour options are: " + options);
            }

            return true;
        }

        private bool GetNextOptions(Dictionary<string, ItemTemplate> versions)
        {
            // Clear the current types since we are going to be offering more
            CurrentTypes.Clear();

            // Loop until we find the next set of options
            while (CurrentTypes.Count <= 1 && _virtualStep < MaxNumOfSteps)
            {
                // Go through each set of options in our list
                foreach (string str in versions.Keys)
                {
                    // Used as the key to store the option in the database
                    string[] splitVersion = str.Split(';');

                    // multiple versions may not available
                    if (splitVersion.Length <= _virtualStep)
                    {
                        return false;
                    }

                    // Get the current option using our virtual step.  This gets the right option in the DT;WT;STAT; list
                    string type = splitVersion[_virtualStep];

                    // If we got a valid type and we don't already have it, put/save it
                    if (type != string.Empty && !CurrentTypes.Contains(type))
                    {
                        CurrentTypes.Add(type);
                    }
                }

                // If there was only one option added, obviously that is the only thing they can pick
                if (CurrentTypes.Count == 1)
                {
                    ChosenTypes = $"{ChosenTypes}{CurrentTypes[0]};";
                    CurrentTypes.Clear();
                }
                else if (CurrentTypes.Count == 0)
                {
                    // We need to add the end semi-colon even if we don't get an option!
                    ChosenTypes = $"{ChosenTypes};";
                }

                // Increment our virtual step, if this loops again it will proceed to the next set of options
                _virtualStep++;
            }

            return true;
        }

        private void ClearCustomOptions()
        {
            RemoveCustomProperty("Types");
            RemoveCustomProperty(_virtualStep.ToString());
        }

        /// <summary>
        /// Handles the saving of the types and virtualstep.  Clears them first!
        /// </summary>
        private void SaveProperties()
        {
            // Clear any remaining options
            ClearCustomOptions();

            // Save the options in the database!
            string options = string.Empty;
            foreach (string str in CurrentTypes)
            {
                options = $"{options}|{str}";
            }

            SetCustomProperty("Types", options);

            // Save the virtual step so we can reload the quest
            SetCustomProperty("VS", _virtualStep.ToString());
        }

        /// <summary>
        /// Handles the loading of the name, artID, any options, and the VS
        /// </summary>
        private void LoadProperties()
        {
            _scholarName = GetCustomProperty("SN");
            _artifactId = GetCustomProperty("Art");
            _name = GetCustomProperty("Name");

            CurrentTypes = new List<string>(2);
            string combinedOptions = GetCustomProperty("Types");
            if (combinedOptions != null)
            {
                string[] options = combinedOptions.TrimStart().TrimEnd().Split('|');
                foreach (string str in options)
                {
                    if (str != " " && str != string.Empty)
                    {
                        CurrentTypes.Add(str);
                    }
                }
            }

            // If some how the VS got messed up, reset the quest
            if (!int.TryParse(GetCustomProperty("VS"), out _virtualStep) || _virtualStep > MaxNumOfSteps || _virtualStep < 0)
            {
                // This would be a strange error - but for some reason it doesn't like Step = 0 o.O
                // this.Step = 0;
                _virtualStep = 0;
                CurrentTypes.Clear();
                ChosenTypes = string.Empty;
            }
        }

        public override void FinishQuest()
        {
            Step = -1; // -1 indicates finished or aborted quests etc, they won't show up in the list
            QuestPlayer.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(QuestPlayer.Client, "ArtifactTurnInQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);

            // move quest from active list to finished list...
            QuestPlayer.QuestList.Remove(this);

            if (QuestPlayer.HasFinishedQuest(GetType()) == 0)
            {
                QuestPlayer.QuestListFinished.Add(this);
            }

            DeleteFromDatabase();

            QuestPlayer.Out.SendQuestListUpdate();
        }
    }
}
