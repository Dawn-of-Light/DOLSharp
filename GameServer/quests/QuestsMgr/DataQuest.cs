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

// Tolakram, July 2010 - This represents a data driven quest that can be added and removed at runtime.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Behaviour;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests
{

    /// <summary>
    /// This represents a data driven quest
    /// DataQuests are defined in the database instead of a script.
    ///
    /// Each Quest should have a complete set of startup parameters.  All of these are non serialized.
    ///
    /// Name, StartType, StartName, StartRegionID, AcceptText and Description.  These determine who offers the quest and what text is displayed
    /// to the player considering accepting the quest.  StartRegion of 0 indicates every GameObject with the given name will have this quest.
    ///
    /// Once a quest is started each step behaves in a set order.
    ///
    /// Source -> Target -> Advance to Next Step
    ///
    /// Step 1 is considered the first step, and for each step
    /// Source is considered who started the Step and Target is considered who ends the Step.   For each Step the following columns must
    /// have serialized values, separated by | for each Step, including Step 1.  If the last value for any step is empty then the string should
    /// end with a double pipe |
    ///
    /// SourceName - Various uses as defined below:
    ///
    ///         NO_INDICATOR - Placing the text NO_INDICATOR in the source name field will disable any quest indicator that would normally
    ///                        display above the NPC's head.  This can be combined with the options below, just make sure to include a | character
    ///                        to separate it from the other options.  Ex: NO_INDICATOR|SEARCH;2;Search for ring here;12;5000;77665;500;20|SEARCH;3;Search for necklace here;12;8000;74665;500;20
    ///         StartTypes:
    ///         Search
    ///         SearchFinish -  Defines a complete quest search area and any associated messages. Format: SEARCH;Step #;Popup Text;Region ID;X;Y;RADIUS;TIME
    ///                         TIME = amount of time search takes, in seconds
    ///                         Ex: SEARCH;2;Search here for the ring;12;5000;77665;500;20
    ///                         The Text entry can be blank for no popup display.  Ex:  SEARCH;2;;12;5000;77665;500;20
    ///                         Multiple search entries can also be created:
    ///                             SEARCH;2;Search for ring here;12;5000;77665;500;20|SEARCH;3;Search for necklace here;12;8000;74665;500;20
    ///                         You only need to make entries for each search area, not for every step. Search areas must start with SEARCH
    ///                         For Search steps, if Searching succeeds the Step is advanced as normal, using StepitemTemplate to give any item to the player.
    ///                         You can make it so searching does not always succeed by adding a chance to the StepitemTemplate as described below.
    ///                         SearchFinish uses FinalRewardsItemTemplate to give items to a player and finish the quest.
    ///
    ///         SearchStart -   Similar to above but removes Required Step and adds an item template to give to player on startup
    ///                         ex: SEARCHSTART;Some_Ite_Template;You see some disturbed soil, you might want to search here.;12;5000;77665;500;20
    ///                         You must assign all SearchStart quests to a mob or object in order for the quest to load and allow refreshes.  Any mob or object
    ///                         will work, and the mob or object will not display any indications that it holds one of these quests.
    ///
    ///
    /// SourceText - What is said to the player when beginning a step.  If a target starting the next step has no text then an empty
    /// string can be provided using || with nothing between the pipes.
    ///
    /// StepType - The type of step from eStepType
    ///
    /// StepText - The text for the step that appears in the players quest journal
    ///
    /// StepItemTemplates - Any items that need to be given to the player for a step.  Every step can give an item to a player. All
    /// steps give an item at the completion of the step except Delivery and DeliveryFinish.  If StepItemTemplates are defined for a
    /// Delivery step then the item is given at the beginning of the step and accepted by a target to end the step.
    /// For Kill and Search steps, StepItemTemplates can include a drop chance behind the template name.  Ex: |some_template_name;50|
    /// If the item does not drop then the step is not advanced.
    /// If no items are given to a player at any of the steps then this can be null, otherwise it must have values for each step.
    /// Empty values || are ok.
    ///
    /// AdvanceText - The text needed, if any, to advance this step.  If no step requires advance text then this can be null, otherwise
    /// text must be provided for every step.  Empty values || are ok.
    ///
    /// TargetName - Must be in the format Name;RegionID|Name;RegionID.... RegionID can be 0 to indicate any Target of the correct Name can advance the quest
    ///
    /// TargetText - Text shown to player when current step ends.
    ///
    /// CollectItemTemplate - Item that needs to be collected to end the current step.  If no items are ever collected this can be kept null,
    /// otherwise it needs an entry for each step.  Empty values || are ok.
    ///
    /// MaxCount, MinLevel, MaxLevel - Single values to determine who can do quest.  All must be provided.  MaxCount == 0 for no limit
    ///
    /// RewardMoney - Serialized list of money rewarded for each step.  All steps must have a value, 0 is ok.
    ///
    /// RewardXP - Serialized list of XP rewarded each step.  All steps must have a value, 0 is ok.
    ///
    /// RewardCLXP - Serialized list of CLXP rewarded each step.  All steps must have a value, 0 is ok.
    ///
    /// RewardRP - Serialized list of RP rewarded each step.  All steps must have a value, 0 is ok.
    ///
    /// RewardBP - Serialized list of XP rewarded each step.  All steps must have a value, 0 is ok.
    ///
    /// OptionalRewardItemTemplates - A serialized list of optional rewards to be presented to the player at the end of a Reward quest.
    /// The first value must be a number from 0 to 8 followed by the item list.  ex: 2id_nb|id_nb  For quests without optional rewards
    /// this field can be null.
    ///
    /// FinalRewardItemTemplates - A serialized list of rewards to be given to the player at the end of a quest.  For quests without
    /// rewards this field can be null.
    ///
    /// FinishText - The text to show the player once the quest has completed.
    ///
    /// QuestDependency - If this quest is dependent on other quests being done first then the name(s) of those quests should be here.
    /// This can be null if no dependencies.
    ///
    /// AllowedClasses - Player classes that can get this quest
    ///
    /// ClassType - Any class of type IDataQuestStep which is called on each quest step and when the quest is finished.  You can optionally include
    /// additional data to be used by the custom step.  Example:  DOL.Storm.MyCustomStep|some_additonal_data
    ///
    /// </summary>
    public class DataQuest : AbstractQuest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly GameNPC _startNpc;
        private IDataQuestStep _customQuestStep;

        /// <summary>
        /// In order to avoid conflicts with scripted quests data quest ID's are added to this number when sending a quest ID to the client
        /// </summary>
        public const ushort DataquestClientoffset = 32767;

        /// <summary>
        /// A string containing the last error message generated by this quest
        /// </summary>
        public string LastErrorText { get; set; } = string.Empty;

        private bool _showIndicator = true;

        /// <summary>
        /// Should the available quest indicator be shown for this quest?  Use NO_INDICATOR in SourceName
        /// </summary>
        public bool ShowIndicator
        {
            get
            {
                if (StartType != eStartType.Collection &&
                    StartType != eStartType.KillComplete &&
                    StartType != eStartType.InteractComplete &&
                    StartType != eStartType.SearchStart)
                {
                    return _showIndicator;
                }

                return false;
            }

            set => _showIndicator = value; // maybe someone wants to change this for some reason?
        }

        /// <summary>
        /// How does this quest start
        /// </summary>
        public enum eStartType : byte
        {
            Standard = 0,           // Talk to npc, accept quest, go through steps
            Collection = 1,         // Player turns drops into npc for xp, quest not added to player quest log, has no steps
            AutoStart = 2,          // Standard quest is auto started simply by interacting with start object
            KillComplete = 3,       // Killing the Start living grants and finished the quest, similar to One Time Drops
            InteractComplete = 4,   // Interacting with start object grants and finishes the quest
            SearchStart = 5,        // Quest is started by searching in the designated QuestSearchArea
            RewardQuest = 200,      // A reward quest, where reward dialog is given to player on quest offer and complete.
            Unknown = 255
        }

        /// <summary>
        /// The type of each quest step
        /// All quests with steps must end in a Finish step
        /// </summary>
        public enum eStepType : byte
        {
            Kill = 0,               // Kill the target to advance the quest.  Can set chance to drop on StepItemTemplate.
            KillFinish = 1,         // Killing the target finishes the quest and gives the reward.  Can set chance to drop on StepItemTemplate.
            Deliver = 2,            // Deliver an item to the target to advance the quest
            DeliverFinish = 3,      // Deliver an item to the target to finish the quest
            Interact = 4,           // Interact with the target to advance the step
            InteractFinish = 5,     // Interact with the target to finish the quest.  This is required to end a RewardQuest
            Whisper = 6,            // Whisper to the target to advance the quest
            WhisperFinish = 7,      // Whisper to the target to finish the quest
            Search = 8,             // Search in a specified location. Can set chance to drop on StepItemTemplate.
            SearchFinish = 9,       // Search in a specified location to finish the quest. Can set chance to drop on StepItemTemplate.
            Collect = 10,           // Player must give the target an item to advance the step
            CollectFinish = 11,     // Player must give the target an item to finish the quest
            Unknown = 255
        }

        /// <summary>
        /// A static list of every search area for all data quests
        /// </summary>
        private static readonly List<KeyValuePair<int, QuestSearchArea>> AllQuestSearchAreas = new List<KeyValuePair<int, QuestSearchArea>>();

        /// <summary>
        /// How many search areas are part of this quest
        /// </summary>
        private int _numSearchAreas;

        /// <summary>
        /// An item given to a player when starting with a search.
        /// </summary>
        private string _searchStartItemTemplate = string.Empty;

        private readonly List<string> _sourceTexts = new List<string>();
        private readonly List<string> _targetNames = new List<string>();
        private readonly List<string> _targetTexts = new List<string>();
        private readonly List<eStepType> _stepTypes = new List<eStepType>();
        private readonly List<string> _stepItemTemplates = new List<string>();
        private readonly List<string> _advanceTexts = new List<string>();
        private readonly List<string> _collectItems = new List<string>();
        private readonly List<long> _rewardXPs = new List<long>();

        private string _classType = string.Empty;
        private readonly List<long> _rewardClxps = new List<long>();
        private readonly List<long> _rewardRPs = new List<long>();
        private readonly List<long> _rewardBPs = new List<long>();
        private readonly List<long> _rewardMoneys = new List<long>();
        private readonly List<string> _questDependencies = new List<string>();
        private readonly List<byte> _allowedClasses = new List<byte>();

        /// <summary>
        /// Create an empty Quest
        /// </summary>
        public DataQuest()
        { }

        /// <summary>
        /// DataQuest object used for delving RewardItems or other information
        /// </summary>
        /// <param name="dataQuest"></param>
        public DataQuest(DBDataQuest dataQuest)
        {
            QuestPlayer = null;
            DbDataQuest = dataQuest;
            ParseQuestData();
        }

        /// <summary>
        /// DataQuest object assigned to an object or NPC that is used to start or offer the quest
        /// </summary>
        /// <param name="dbQuest"></param>
        public DataQuest(DBDataQuest dataQuest, GameObject startingObject)
        {
            QuestPlayer = null;
            DbDataQuest = dataQuest;
            StartObject = startingObject;
            LastErrorText = string.Empty;
            ParseSearchAreas();
            ParseQuestData();
        }

        /// <summary>
        /// Dataquest that belongs to a player
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dataQuest"></param>
        /// <param name="charQuest"></param>
        public DataQuest(GamePlayer questingPlayer, DBDataQuest dataQuest, CharacterXDataQuest charQuest)
            : this(questingPlayer, null, dataQuest, charQuest)
        {
        }

        /// <summary>
        /// This is a dataquest that belongs to a player
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        /// <param name="charQuest"></param>
        public DataQuest(GamePlayer questingPlayer, GameObject sourceObject, DBDataQuest dataQuest, CharacterXDataQuest charQuest)
        {
            QuestPlayer = questingPlayer;
            DbDataQuest = dataQuest;
            CharDataQuest = charQuest;

            if (sourceObject != null)
            {
                if (sourceObject is GameNPC npc)
                {
                    _startNpc = npc;
                }

                StartObject = sourceObject;
            }

            ParseQuestData();
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(RewardQuestNotify));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(RewardQuestNotify));
        }

        /// <summary>
        /// Split the quest strings into individual step data
        /// It's important to remember that there must be an entry, even if empty, for each column for each step.
        /// For example; something|||something for a 4 part quest
        /// </summary>
        protected void ParseQuestData()
        {
            if (DbDataQuest == null)
            {
                return;
            }

            string lastParse = string.Empty;

            try
            {
                foreach (KeyValuePair<int, QuestSearchArea> entry in AllQuestSearchAreas)
                {
                    if (entry.Key == Id)
                    {
                        _numSearchAreas++;
                    }
                }

                LastErrorText += $" ::{_numSearchAreas} search areas defined for data quest ID:{Id}";

                string[] parse1;

                // check for NO_INDICATOR option
                lastParse = DbDataQuest.SourceName;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    if (lastParse.ToUpper().Contains("NO_INDICATOR"))
                    {
                        _showIndicator = false;
                    }
                }

                lastParse = DbDataQuest.SourceText;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _sourceTexts.Add(str);
                    }
                }

                lastParse = DbDataQuest.TargetName;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        if (str == string.Empty)
                        {
                            // if there's not npc for this step then empty is ok
                            _targetNames.Add(string.Empty);
                            TargetRegions.Add(0);
                        }
                        else
                        {
                            string[] parse2 = str.Split(';');
                            _targetNames.Add(parse2[0]);
                            TargetRegions.Add(Convert.ToUInt16(parse2[1]));
                        }
                    }
                }

                lastParse = DbDataQuest.TargetText;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _targetTexts.Add(str);
                    }
                }

                lastParse = DbDataQuest.StepType;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _stepTypes.Add((eStepType)Convert.ToByte(str));
                    }
                }

                lastParse = DbDataQuest.StepText;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        StepTexts.Add(str);
                    }
                }

                lastParse = DbDataQuest.StepItemTemplates;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _stepItemTemplates.Add(str);
                    }
                }

                lastParse = DbDataQuest.AdvanceText;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _advanceTexts.Add(str);
                    }
                }

                lastParse = DbDataQuest.CollectItemTemplate;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _collectItems.Add(str);
                    }
                }

                lastParse = DbDataQuest.RewardMoney;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _rewardMoneys.Add(Convert.ToInt64(str));
                    }
                }

                lastParse = DbDataQuest.RewardXP;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _rewardXPs.Add(Convert.ToInt64(str));
                    }
                }

                lastParse = DbDataQuest.RewardCLXP;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _rewardClxps.Add(Convert.ToInt64(str));
                    }
                }

                lastParse = DbDataQuest.RewardRP;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _rewardRPs.Add(Convert.ToInt64(str));
                    }
                }

                lastParse = DbDataQuest.RewardBP;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _rewardBPs.Add(Convert.ToInt64(str));
                    }
                }

                lastParse = DbDataQuest.OptionalRewardItemTemplates;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    NumOptionalRewardsChoice = Convert.ToByte(lastParse.Substring(0, 1));
                    parse1 = lastParse.Substring(1).Split('|');
                    foreach (string str in parse1)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(str);
                            if (item != null)
                            {
                                OptionalRewards.Add(item);
                            }
                            else
                            {
                                string errorText = $"DataQuest: Optional reward ItemTemplate not found: {str}";
                                Log.Error(errorText);
                                LastErrorText += " " + errorText;
                            }
                        }
                    }
                }

                lastParse = DbDataQuest.FinalRewardItemTemplates;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(str);
                        if (item != null)
                        {
                            FinalRewards.Add(item);
                        }
                        else
                        {
                            string errorText = $"DataQuest: Final reward ItemTemplate not found: {str}";
                            Log.Error(errorText);
                            LastErrorText += " " + errorText;
                        }
                    }
                }

                lastParse = DbDataQuest.QuestDependency;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        if (str != string.Empty)
                        {
                            _questDependencies.Add(str);
                        }
                    }
                }

                lastParse = DbDataQuest.AllowedClasses;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        _allowedClasses.Add(Convert.ToByte(str));
                    }
                }

                lastParse = DbDataQuest.ClassType;
                if (!string.IsNullOrEmpty(lastParse))
                {
                    parse1 = lastParse.Split('|');
                    _classType = parse1[0];
                    if (parse1.Length > 1)
                    {
                        AdditionalData = parse1[1];
                    }
                }
            }

            catch (Exception ex)
            {
                string errorText = $"Error parsing quest data for {DbDataQuest.Name} ({DbDataQuest.ID}), last string to parse = \'{lastParse}\'.";
                Log.Error(errorText, ex);
                LastErrorText += $" {errorText} {ex.Message}";
            }
        }

        /// <summary>
        /// Parse or re-parse all the search areas for this quest and add to the static list of all dataquest search areas
        /// </summary>
        protected void ParseSearchAreas()
        {
            if (DbDataQuest == null)
            {
                return;
            }

            string lastParse = string.Empty;

            try
            {
                // If we have any search areas created we delete them first, then re-create if needed
                List<KeyValuePair<int, QuestSearchArea>> areasToDelete = new List<KeyValuePair<int, QuestSearchArea>>();

                foreach (KeyValuePair<int, QuestSearchArea> entry in AllQuestSearchAreas)
                {
                    if (entry.Key == Id)
                    {
                        areasToDelete.Add(entry);
                    }
                }

                foreach (KeyValuePair<int, QuestSearchArea> entry in areasToDelete)
                {
                    LastErrorText += $" ::Removing QuestSearchArea for DataQuest ID:{Id}, Step {entry.Value.Step}";
                    entry.Value.RemoveArea();
                    AllQuestSearchAreas.Remove(entry);
                }

                lastParse = DbDataQuest.SourceName;

                if (!string.IsNullOrEmpty(lastParse))
                {
                    var parse1 = lastParse.Split('|');
                    foreach (string str in parse1)
                    {
                        if (str.ToUpper().StartsWith("SEARCH"))
                        {
                            CreateSearchArea(str);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing quest data for {DbDataQuest.Name} ({DbDataQuest.ID}), last string to parse = \'{lastParse}\'.", ex);
                LastErrorText += $" {lastParse} {ex.Message}";
            }
        }

        /// <summary>
        /// Add a search area to the static list of all DataQuest search areas
        /// </summary>
        /// <param name="str"></param>
        protected void CreateSearchArea(string areaStr)
        {
            try
            {
                string[] parse = areaStr.Split(';');

                int requiredStep;

                if (parse[0] == "SEARCHSTART")
                {
                    requiredStep = 0;
                    _searchStartItemTemplate = parse[1];
                }
                else
                {
                    requiredStep = Convert.ToInt32(parse[1]);
                }

                // 0       1 2                        3  4    5     6   7
                // COMMAND;3;Search for necklace here;12;8000;74665;500;20
                QuestSearchArea questArea = new QuestSearchArea(this, requiredStep, parse[2], Convert.ToUInt16(parse[3]), Convert.ToInt32(parse[4]), Convert.ToInt32(parse[5]), Convert.ToInt32(parse[6]), Convert.ToInt32(parse[7]));
                AllQuestSearchAreas.Add(new KeyValuePair<int,QuestSearchArea>(Id, questArea));

                LastErrorText += $" ::Created Search Area for quest {Name}, step {requiredStep} in region {parse[3]} at X:{parse[4]}, Y:{parse[5]}, Radius:{parse[6]}, Text:{parse[2]}, Seconds:{parse[7]}.";
            }
            catch
            {
                string error = $"Error creating search area for {DbDataQuest.Name} ({DbDataQuest.ID}), area str = \'{areaStr}\'";
                Log.Error(error);
                LastErrorText += error;
            }
        }

        /// <summary>
        /// Name of this quest to show in quest log
        /// </summary>
        public override string Name => DbDataQuest.Name;

        /// <summary>
        /// How does this quest start?
        /// </summary>
        public virtual eStartType StartType => (eStartType)DbDataQuest.StartType;

        /// <summary>
        /// What object started this quest
        /// </summary>
        public virtual GameObject StartObject { get; set; }

        /// <summary>
        /// List of final rewards for this quest
        /// </summary>
        public virtual List<ItemTemplate> FinalRewards { get; } = new List<ItemTemplate>();

        /// <summary>
        /// How many optional items can the player choose
        /// </summary>
        public virtual byte NumOptionalRewardsChoice { get; set; }

        /// <summary>
        /// List of optional rewards for this quest
        /// </summary>
        public virtual List<ItemTemplate> OptionalRewards { get; set; } = new List<ItemTemplate>();

        /// <summary>
        /// List of all the items the player has chosen
        /// </summary>
        public virtual List<ItemTemplate> OptionalRewardsChoice { get; } = new List<ItemTemplate>();

        /// <summary>
        /// Array of each optional reward item choice (0-7)
        /// </summary>
        public virtual int[] RewardItemsChosen { get; private set; }

        /// <summary>
        /// Final text to display to player when quest is finished
        /// </summary>
        public virtual string FinishText => BehaviourUtils.GetPersonalizedMessage(DbDataQuest.FinishText, QuestPlayer);

        /// <summary>
        /// The DBDataQuest for this quest
        /// </summary>
        public virtual DBDataQuest DbDataQuest { get; }

        /// <summary>
        /// The CharacterXDataQuest entry for the player doing this quest
        /// </summary>
        public virtual CharacterXDataQuest CharDataQuest { get; }

        /// <summary>
        /// The unique ID for this quest
        /// </summary>
        public virtual int Id => DbDataQuest.ID;

        /// <summary>
        /// Unique quest ID to send to the client
        /// </summary>
        public virtual ushort ClientQuestId => (ushort)(DbDataQuest.ID + DataquestClientoffset);

        /// <summary>
        /// Minimum level this quest can be done
        /// </summary>
        public override int Level => DbDataQuest.MinLevel;

        /// <summary>
        /// Max level that this quest can be done
        /// </summary>
        public virtual int MaxLevel => DbDataQuest.MaxLevel;

        /// <summary>
        /// Text of every step in this quest
        /// </summary>
        public virtual List<string> StepTexts { get; } = new List<string>();

        public virtual short Count
        {
            get
            {
                if (CharDataQuest != null)
                {
                    return CharDataQuest.Count;
                }

                return 0;
            }

            set
            {
                short oldCount = CharDataQuest.Count;
                CharDataQuest.Count = value;
                if (CharDataQuest.Count != oldCount)
                {
                    GameServer.Database.SaveObject(CharDataQuest);
                }
            }
        }

        /// <summary>
        /// Maximum number of times this quest can be done
        /// </summary>
        public override int MaxQuestCount
        {
            get
            {
                if (DbDataQuest.MaxCount == 0)
                {
                    return int.MaxValue;
                }

                return DbDataQuest.MaxCount;
            }
        }

        /// <summary>
        /// Description of this quest to show in quest log
        /// </summary>
        public override string Description
        {
            get
            {
                if (Step == 0)
                {
                    if (DbDataQuest.Description == null)
                    {
                        return string.Empty;
                    }

                    return BehaviourUtils.GetPersonalizedMessage(DbDataQuest.Description, QuestPlayer);
                }

                return BehaviourUtils.GetPersonalizedMessage(StepText, QuestPlayer);
            }
        }

        /// <summary>
        /// The Story to display if this is a Reward Quest
        /// </summary>
        public virtual string Story
        {
            get
            {
                if (_sourceTexts.Count > 0)
                {
                    // BehaviorUtils will personalize this message in the packet handlers
                    return _sourceTexts[0];
                }

                return "SourceTexts[0] undefined!";
            }
        }

        /// <summary>
        /// What quest step is the player on
        /// Generic Quests only support a single step
        /// </summary>
        public override int Step
        {
            get
            {
                if (CharDataQuest == null)
                {
                    return 0;
                }

                return CharDataQuest.Step;
            }

            set
            {
                if (CharDataQuest != null)
                {
                    int oldStep = CharDataQuest.Step;
                    CharDataQuest.Step = (short)value;
                    if (CharDataQuest.Step != oldStep)
                    {
                        GameServer.Database.SaveObject(CharDataQuest);
                    }
                }
            }
        }

        /// <summary>
        /// Additional data following ClassType
        /// </summary>
        public string AdditionalData { get; private set; } = string.Empty;

        /// <summary>
        /// Get or create the CharacterXDataQuest for this player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static CharacterXDataQuest GetCharacterQuest(GamePlayer player, int id, bool create)
        {
            CharacterXDataQuest charQuest = GameServer.Database.SelectObjects<CharacterXDataQuest>("`Character_ID` = @Character_ID AND `DataQuestID` = @DataQuestID", new[] { new QueryParameter("@Character_ID", player.QuestPlayerID), new QueryParameter("@DataQuestID", id) }).FirstOrDefault();

            if (charQuest == null && create)
            {
                charQuest = new CharacterXDataQuest(player.QuestPlayerID, id)
                {
                    Count = 0,
                    Step = 0
                };

                GameServer.Database.AddObject(charQuest);
            }

            return charQuest;
        }

        /// <summary>
        /// Can this player do this quest
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (player.Level < DbDataQuest.MinLevel || player.Level > DbDataQuest.MaxLevel)
            {
                return false;
            }

            if (_allowedClasses.Count > 0)
            {
                if (_allowedClasses.Contains((byte)player.CharacterClass.ID) == false)
                {
                    return false;
                }
            }

            if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Qualification) == false)
            {
                return false;
            }

            if (StartType == eStartType.Collection)
            {
                CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, false);
                if (charQuest != null && charQuest.Count >= MaxQuestCount)
                {
                    return false;
                }

                return true;
            }

            lock (player.QuestList)
            {
                foreach (AbstractQuest q in player.QuestList)
                {
                    if (q is DataQuest && (q as DataQuest).Id == Id)
                    {
                        return false;  // player is currently doing this quest
                    }
                }
            }

            lock (player.QuestListFinished)
            {
                foreach (AbstractQuest q in player.QuestListFinished)
                {
                    if (q is DataQuest && (q as DataQuest).Id == Id)
                    {
                        if (q.IsDoingQuest(q) || (q as DataQuest).Count >= MaxQuestCount)
                        {
                            return false; // player has done this quest the max number of times
                        }
                    }
                }

                // check to see if this quest requires another to be done first
                if (_questDependencies.Count > 0)
                {
                    int numFound = 0;

                    foreach (string str in _questDependencies)
                    {
                        foreach (AbstractQuest q in player.QuestListFinished)
                        {
                            if (q is DataQuest && (q as DataQuest).Name.ToLower() == str.ToLower())
                            {
                                numFound++;
                                break;
                            }
                        }
                    }

                    if (numFound < _questDependencies.Count)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Is the player currently doing this quest
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool IsDoingQuest(AbstractQuest checkQuest)
        {
            if (checkQuest is DataQuest quest && quest.Id == Id)
            {
                return Step > 0;
            }

            return false;
        }

        /// <summary>
        /// Update the quest indicator
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public virtual void UpdateQuestIndicator(GameNPC npc, GamePlayer player)
        {
            player.Out.SendNPCsQuestEffect(npc, npc.GetQuestIndicator(player));
        }

        /// <summary>
        /// Source text for the current step
        /// </summary>
        private string SourceText
        {
            get
            {
                try
                {
                    return _sourceTexts[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] SourceText error for Step {Step}", ex);
                }

                return $"Error retrieving source text for step {Step}";
            }
        }

        /// <summary>
        /// Target name for the current step
        /// </summary>
        public string TargetName
        {
            get
            {
                try
                {
                    if (_targetNames.Count > 0)
                    {
                        return _targetNames[Step - 1];
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] TargetName error for Step {Step}", ex);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Target region for the current step
        /// </summary>
        public ushort TargetRegion
        {
            get
            {
                try
                {
                    if (TargetRegions.Count > 0)
                    {
                        return TargetRegions[Step - 1];
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] TargetRegion error for Step {Step}", ex);
                }

                return 0;
            }
        }

        /// <summary>
        /// Target text for the current step
        /// </summary>
        private string TargetText
        {
            get
            {
                try
                {
                    if (_targetTexts.Count > 0)
                    {
                        if (Step < 1)
                        {
                            return _targetTexts[0];
                        }

                        return _targetTexts[Step - 1];
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] TargetText error for Step {Step}", ex);
                }

                return $"Error retrieving target text for step {Step}";
            }
        }

        /// <summary>
        /// Current step type
        /// </summary>
        public eStepType StepType
        {
            get
            {
                try
                {
                    return _stepTypes[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] StepType error for Step {Step}", ex);
                }

                return eStepType.Unknown;
            }
        }

        /// <summary>
        /// Step description to show in quest log for the current step
        /// </summary>
        private string StepText
        {
            get
            {
                try
                {
                    if (QuestPlayer != null && QuestPlayer.Client.Account.PrivLevel > 1)
                    {
                        string text = StepTexts[Step - 1];
                        text += $" [DEBUG] SType = {StepType}";
                        if (StepType == eStepType.Collect || StepType == eStepType.CollectFinish)
                        {
                            text += $": cit: {CollectItemTemplate}";
                            text += $", Trg: {TargetName}";
                        }
                        else if (StepType == eStepType.Deliver || StepType == eStepType.DeliverFinish || StepType == eStepType.Search)
                        {
                            text += $": sit: {StepItemTemplate}";
                            text += $" Trg: {TargetName}";
                        }
                        else if (StepType == eStepType.SearchFinish)
                        {
                            text += $": frit: {FinalRewards}";
                        }
                        else
                        {
                            if (StepType == eStepType.Whisper || StepType == eStepType.WhisperFinish)
                            {
                                text += $": [{AdvanceText}]";
                            }

                            text += $", Trg: {TargetName}";
                        }

                        return text;
                    }

                    return StepTexts[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] StepText error for Step {Step}", ex);
                }

                return $"Error retrieving step text for step {Step}";
            }
        }

        /// <summary>
        /// An item template to give to the player for this step
        /// </summary>
        private string StepItemTemplate
        {
            get
            {
                try
                {
                    if (_stepItemTemplates.Count > 0)
                    {
                        return _stepItemTemplates[Step - 1].Trim();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] StepItemTemplate error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] StepItemTemplate error for Step {Step}");
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// The item template player needs to turn in to advance this quest.
        /// </summary>
        private string CollectItemTemplate
        {
            get
            {
                try
                {
                    if (_collectItems.Count > 0)
                    {
                        return _collectItems[Step - 1];
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] CollectItemTemplate error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] CollectItemTemplate error for Step {Step}");
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Text needed to advance the step or end the quest for the current step
        /// </summary>
        private string AdvanceText
        {
            get
            {
                try
                {
                    if (_advanceTexts.Count > 0)
                    {
                        return _advanceTexts[Step - 1];
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] AdvanceText error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] AdvanceText error for Step {Step}");
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Any money reward for the current step
        /// </summary>
        private long RewardMoney
        {
            get
            {
                try
                {
                    if (_rewardMoneys.Count == 0)
                    {
                        return 0;
                    }

                    return _rewardMoneys[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] RewardMoney error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] RewardMoney error for Step {Step}");
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Any xp reward for the current step
        /// </summary>
        private long RewardXp
        {
            get
            {
                try
                {
                    if (_rewardXPs.Count == 0)
                    {
                        return 0;
                    }

                    return _rewardXPs[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] RewardXP error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] RewardXP error for Step {Step}");
                    }
                }

                return 0;
            }
        }

        private long RewardClxp
        {
            get
            {
                try
                {
                    if (_rewardClxps.Count == 0)
                    {
                        return 0;
                    }

                    return _rewardClxps[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] RewardCLXP error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] RewardCLXP error for Step {Step}");
                    }
                }

                return 0;
            }
        }

        private long RewardRp
        {
            get
            {
                try
                {
                    if (_rewardRPs.Count == 0)
                    {
                        return 0;
                    }

                    return _rewardRPs[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] RewardRP error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] RewardRP error for Step {Step}");
                    }
                }

                return 0;
            }
        }

        private long RewardBp
        {
            get
            {
                try
                {
                    if (_rewardBPs.Count == 0)
                    {
                        return 0;
                    }

                    return _rewardBPs[Step - 1];
                }
                catch (Exception ex)
                {
                    Log.Error($"DataQuest [{Id}] RewardBP error for Step {Step}", ex);
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] RewardBP error for Step {Step}");
                    }
                }

                return 0;
            }
        }

        public List<ushort> TargetRegions { get; set; } = new List<ushort>();

        /// <summary>
        /// Gets money reward for reward quests. Used for sending packet info to dialog popup window.
        /// </summary>
        /// <returns></returns>
        public long MoneyReward()
        {
            return _rewardMoneys[0];
        }

        /// <summary>
        /// Gets experience reward for reward quests. Used for sending packet info to dialog popup window.
        /// </summary>
        /// <returns></returns>
        public int ExperiencePercent(GamePlayer player)
        {
            int currentLevel = player.Level;
            if (currentLevel > player.MaxLevel)
            {
                return 0;
            }

            long experienceToLevel = player.GetExperienceNeededForLevel(currentLevel + 1) -
                player.GetExperienceNeededForLevel(currentLevel);

            return (int)((_rewardXPs[0] * 100) / experienceToLevel);
        }

        protected virtual bool ExecuteCustomQuestStep(GamePlayer player, int step, eStepCheckType stepCheckType)
        {
            bool canContinue = true;

            if (!string.IsNullOrEmpty(_classType))
            {
                if (_customQuestStep == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.GetType(_classType) != null)
                        {
                            try
                            {
                                _customQuestStep = assembly.CreateInstance(_classType, false, BindingFlags.CreateInstance, null, new object[] { }, null, null) as IDataQuestStep;
                            }
                            catch (Exception)
                            {
                            }

                            break;
                        }
                    }

                    if (_customQuestStep == null)
                    {
                        foreach (Assembly assembly in ScriptMgr.Scripts)
                        {
                            if (assembly.GetType(_classType) != null)
                            {
                                try
                                {
                                    _customQuestStep = assembly.CreateInstance(_classType, false, BindingFlags.CreateInstance, null, new object[] { }, null, null) as IDataQuestStep;
                                }
                                catch (Exception)
                                {
                                }

                                break;
                            }
                        }
                    }
                }

                if (_customQuestStep == null)
                {
                    Log.Error($"Failed to construct custom DataQuest step of ClassType {_classType}!  Quest will continue anyway.");
                    if (QuestPlayer != null)
                    {
                        ChatUtil.SendDebugMessage(QuestPlayer, $"Failed to construct custom DataQuest step of ClassType {_classType}!  Quest will continue anyway.");
                    }
                }
            }

            if (_customQuestStep != null)
            {
                canContinue = _customQuestStep.Execute(this, player, step, stepCheckType);
            }

            return canContinue;
        }

        /// <summary>
        /// Try to advance the quest step, doing any actions required to start the next step
        /// </summary>
        /// <param name="obj">The object that is advancing the step</param>
        /// <returns></returns>
        protected virtual bool AdvanceQuestStep(GameObject obj = null)
        {
            try
            {
                eStepType nextStepType = _stepTypes[Step];
                bool advance = false;

                if (ExecuteCustomQuestStep(QuestPlayer, Step, eStepCheckType.Step))
                {
                    if (RewardXp > 0 && QuestPlayer.GainXP == false)
                    {
                        QuestPlayer.Out.SendMessage("Your XP is turned off, you must turn it on to complete this quest step!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    if (RewardRp > 0 && QuestPlayer.GainRP == false)
                    {
                        QuestPlayer.Out.SendMessage("Your RP is turned off, you must turn it on to complete this quest step!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    advance = true;
                    List<string> stepTemplates = new List<string>();

                    // If completing this step or starting the next step requires giving the player an item then
                    // we need to check to make sure player has enough inventory space to accept the item, otherwise do not advance the step

                    // NOTE: Original plan was to support more than one template per step, but only a single template is supported at this time
                    if (!string.IsNullOrEmpty(StepItemTemplate))
                    {
                        stepTemplates.Add(StepItemTemplate);
                    }

                    // If this is a kill or search step with a drop then check for chance to drop an item
                    if (stepTemplates.Count == 1 && (StepType == eStepType.Kill || StepType == eStepType.KillFinish || StepType == eStepType.Search || StepType == eStepType.SearchFinish))
                    {
                        string[] template = stepTemplates[0].Split(';');

                        if (template.Length > 1)
                        {
                            int.TryParse(template[1], out var chance);

                            if (chance > 0)
                            {
                                if (Util.Chance(chance) == false)
                                {
                                    // failed to drop, ignore step advance
                                    return false;
                                }
                            }
                            else
                            {
                                ChatUtil.SendDebugMessage(QuestPlayer, $"[DEBUG] AdvanceQuestStep error; chance to drop StepTemplate is 0 when advancing from Step {Step}");
                                return false;
                            }
                        }

                        stepTemplates[0] = template[0];
                    }

                    if (nextStepType == eStepType.Deliver || nextStepType == eStepType.DeliverFinish)
                    {
                        // Allow StepItemTemplate to be empty, assume quest player received item in a previous step or outside of the quest
                        if (!string.IsNullOrEmpty(_stepItemTemplates[Step].Trim()))
                        {
                            stepTemplates.Add(_stepItemTemplates[Step].Trim());
                        }
                    }

                    if (stepTemplates.Count > 0)
                    {
                        // check for inventory space
                        lock (QuestPlayer.Inventory)
                        {
                            if (QuestPlayer.Inventory.IsSlotsFree(stepTemplates.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                            {
                                foreach (string template in stepTemplates)
                                {
                                    ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(template);
                                    if (item == null)
                                    {
                                        string errorMsg = $"StepItemTemplate {template} not found in DB!";
                                        QuestPlayer.Out.SendMessage(errorMsg, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                        throw new Exception(errorMsg);
                                    }

                                    if (obj is GameLiving living)
                                    {
                                        GiveItem(living, QuestPlayer, item, false);
                                    }
                                    else
                                    {
                                        GiveItem(QuestPlayer, item, false);
                                    }
                                }
                            }
                            else
                            {
                                QuestPlayer.Out.SendMessage($"You don\'t have enough inventory space to advance this quest.  You need {stepTemplates.Count} free slot(s)!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                advance = false;
                            }
                        }
                    }
                }

                if (advance)
                {
                    // Since we can advance first give any rewards for the current step
                    if (StartType != eStartType.RewardQuest) // Reward quests receive rewards upon completing quest.
                    {
                        if (RewardXp > 0)
                        {
                            QuestPlayer.GainExperience(GameLiving.eXPSource.Quest, RewardXp);
                        }

                        if (RewardRp > 0)
                        {
                            QuestPlayer.GainRealmPoints(RewardRp);
                        }

                        if (RewardMoney > 0)
                        {
                            QuestPlayer.AddMoney(RewardMoney, "You are awarded {0}!");
                            InventoryLogging.LogInventoryAction($"(QUEST;{Name})", QuestPlayer, eInventoryActionType.Quest, RewardMoney);
                        }

                        if (RewardClxp > 0)
                        {
                            QuestPlayer.GainChampionExperience(RewardClxp, GameLiving.eXPSource.Quest);
                        }

                        if (RewardBp > 0)
                        {
                            QuestPlayer.GainBountyPoints(RewardBp);
                        }
                    }

                    // Then advance step

                    // Then advance step
                    Step++;
                    QuestPlayer.Out.SendQuestListUpdate();

                    // Try to update Icon
                    switch (StepType)
                    {
                        case eStepType.DeliverFinish:
                        case eStepType.InteractFinish:
                        case eStepType.KillFinish:
                        case eStepType.WhisperFinish:
                        case eStepType.CollectFinish:
                            foreach (GameNPC n in QuestPlayer.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                GameNPC npc = n;
                                if (npc != null && TargetName == npc.Name && (TargetRegion == 0 || TargetRegion == npc.CurrentRegionID))
                                {
                                    UpdateQuestIndicator(npc, QuestPlayer);
                                }
                            }

                        break;
                    }

                    // Then say any source text for the new step
                    if (!string.IsNullOrEmpty(SourceText))
                    {
                        TryTurnTo(obj, QuestPlayer);

                        if (obj != null)
                        {
                            if (obj.Realm == eRealm.None)
                            {
                                SendMessage(QuestPlayer, SourceText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                            }
                            else
                            {
                                SendMessage(QuestPlayer, SourceText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DataQuest [{Id}] AdvanceQuestStep error when advancing from Step {Step}", ex);
                if (QuestPlayer != null)
                {
                    ChatUtil.SendDebugMessage(QuestPlayer, $"[DEBUG] AdvanceQuestStep error when advancing from Step {Step}: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Notify is sent to all quests in the players active quest list
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            // log.DebugFormat("DataQuest: Notify {0}, mQuestPlayer {1}", e.Name, mQuestPlayer == null ? "null" : mQuestPlayer.Name);
            try
            {
                // Interact to check quest offer
                if (e == GameObjectEvent.Interact && StartType != eStartType.SearchStart)
                {
                    if (!(args is InteractEventArgs a))
                    {
                        return;
                    }

                    if (a.Source is GamePlayer p && sender is GameObject o)
                    {
                        // log.DebugFormat("DataQuest CheckOffer: Player {0} is interacting with {1}", p.Name, o.Name);
                        CheckOfferQuest(p, o);
                    }

                    return;
                }

                // Interact when already doing quest
                if (e == GameObjectEvent.InteractWith)
                {
                    GamePlayer p = sender as GamePlayer;
                    if (!(args is InteractWithEventArgs a))
                    {
                        return;
                    }

                    // log.DebugFormat("DataQuest Interact: Player {0} is interacting with {1}", p.Name, a.Target.Name);
                    OnPlayerInteract(p, a.Target);
                    return;
                }

                // Player is giving an item to something
                if (e == GamePlayerEvent.GiveItem)
                {
                    if (!(args is GiveItemEventArgs a))
                    {
                        return;
                    }

                    // log.DebugFormat("DataQuest: GiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
                    OnPlayerGiveItem(a.Source, a.Target, a.Item);
                    return;
                }

                // Living is receiving an item, should a quest react to this
                if (e == GameObjectEvent.ReceiveItem)
                {
                    if (!(args is ReceiveItemEventArgs a))
                    {
                        return;
                    }

                    if (a.Source is GamePlayer p)
                    {
                        // log.DebugFormat("DataQuest: ReceiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
                        OnNPCReceiveItem(p, a.Target, a.Item);
                    }

                    return;
                }

                // Whisper
                if (e == GameLivingEvent.WhisperReceive)
                {
                    if (!(args is WhisperReceiveEventArgs a))
                    {
                        return;
                    }

                    if (a.Source is GamePlayer p)
                    {
                        // log.DebugFormat("DataQuest: WhisperReceived {0} receives whisper {1} from {2}", a.Target.Name, a.Text, a.Source.Name);
                        OnNPCReceiveWhisper(p, a.Target, a.Text);
                    }

                    return;
                }

                if (e == GameLivingEvent.Whisper)
                {
                    if (!(args is WhisperEventArgs a))
                    {
                        return;
                    }

                    if (sender is GamePlayer p)
                    {
                        OnPlayerWhisper(p, a.Target, a.Text);
                    }
                }

                // NPC is dying, check for KillComplete quests
                if (e == GameLivingEvent.Dying)
                {
                    if (!(args is DyingEventArgs a))
                    {
                        return;
                    }

                    GameLiving dying = sender as GameLiving;
                    GameObject killer = a.Killer;
                    List<GamePlayer> playerKillers = a.PlayerKillers;

                    OnLivingIsDying(dying, killer, playerKillers);

                    return;
                }

                // Enemy of player with quest was killed, check quests and steps
                if (e == GameLivingEvent.EnemyKilled)
                {
                    if (!(args is EnemyKilledEventArgs a))
                    {
                        return;
                    }

                    GamePlayer player = sender as GamePlayer;
                    GameLiving killed = a.Target;

                    OnEnemyKilled(player, killed);

                    return;
                }

                // Player is trying to finish a Reward Quest
                if (e == GamePlayerEvent.QuestRewardChosen)
                {
                    if (!(args is QuestRewardChosenEventArgs rewardArgs))
                    {
                        return;
                    }

                    // Check if this particular quest has been finished.
                    if (ClientQuestId != rewardArgs.QuestID)
                    {
                        return;
                    }

                    OptionalRewardsChoice.Clear();
                    RewardItemsChosen = rewardArgs.ItemsChosen;

                    if (ExecuteCustomQuestStep(QuestPlayer, 0, eStepCheckType.RewardsChosen))
                    {
                        if (OptionalRewards.Count > 0)
                        {
                            for (int reward = 0; reward < rewardArgs.CountChosen; ++reward)
                            {
                                OptionalRewardsChoice.Add(OptionalRewards[rewardArgs.ItemsChosen[reward]]);
                            }

                            if (NumOptionalRewardsChoice > 0 && rewardArgs.CountChosen <= 0)
                            {
                                QuestPlayer.Out.SendMessage(LanguageMgr.GetTranslation(QuestPlayer.Client, "RewardQuest.Notify"), eChatType.CT_System, eChatLoc.CL_ChatWindow);
                                return;
                            }
                        }

                        FinishQuest(null, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DataQuest [{Id}] Notify Error for {e.Name}", ex);
                if (QuestPlayer != null)
                {
                    ChatUtil.SendDebugMessage(QuestPlayer, $"DataQuest [{Id}] Notify Error for {e.Name}");
                }
            }
        }

        public static void RewardQuestNotify(DOLEvent e, object sender, EventArgs args)
        {
            // Reward Quest accept
            if (e == GamePlayerEvent.AcceptQuest)
            {
                if (!(args is QuestEventArgs qargs))
                {
                    return;
                }

                GamePlayer player = qargs.Player;
                GameLiving giver = qargs.Source;

                foreach (DBDataQuest quest in GameObject.DataQuestCache)
                {
                    if ((quest.ID + DataquestClientoffset) == qargs.QuestID)
                    {
                        CharacterXDataQuest charQuest = GetCharacterQuest(player, quest.ID, true);
                        DataQuest dq = new DataQuest(player, giver, quest, charQuest)
                        {
                            Step = 1
                        };

                        player.AddQuest(dq);
                        if (giver is GameNPC npc)
                        {
                            player.Out.SendNPCsQuestEffect(npc, npc.GetQuestIndicator(player));
                        }

                        player.Out.SendSoundEffect(7, 0, 0, 0, 0, 0);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// A player has interacted with an object that has a DataQuest.
        /// Check to see if we can offer this quest to the player and display the text
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        protected virtual void CheckOfferQuest(GamePlayer player, GameObject obj)
        {
            // Can we offer this quest to the player?
            if (CheckQuestQualification(player))
            {
                if (StartType == eStartType.InteractComplete)
                {
                    // This quest finishes with the interaction
                    CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);

                    if (charQuest.Count < MaxQuestCount)
                    {
                        TryTurnTo(obj, player);

                        if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
                        {
                            if (Description.Trim() != string.Empty)
                            {
                                SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            }

                            if (FinalRewards.Count > 0)
                            {
                                lock (player.Inventory)
                                {
                                    if (player.Inventory.IsSlotsFree(FinalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                                    {
                                        foreach (ItemTemplate item in FinalRewards)
                                        {
                                            if (item != null)
                                            {
                                                GiveItem(obj as GameLiving, player, item, false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SendMessage(player, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                        return;
                                    }
                                }
                            }

                            if (_rewardXPs.Count > 0 && _rewardXPs[0] > 0)
                            {
                                player.GainExperience(GameLiving.eXPSource.Quest, _rewardXPs[0]);
                            }

                            if (_rewardClxps.Count > 0 && _rewardClxps[0] > 0)
                            {
                                player.GainChampionExperience(_rewardClxps[0], GameLiving.eXPSource.Quest);
                            }

                            if (_rewardRPs.Count > 0 && _rewardRPs[0] > 0)
                            {
                                player.GainRealmPoints(_rewardRPs[0]);
                            }

                            if (_rewardBPs.Count > 0 && _rewardBPs[0] > 0)
                            {
                                player.GainBountyPoints(_rewardBPs[0]);
                            }

                            if (_rewardMoneys.Count > 0 && _rewardMoneys[0] > 0)
                            {
                                player.AddMoney(_rewardMoneys[0], "You are awarded {0}!");
                                InventoryLogging.LogInventoryAction($"(QUEST;{Name})", player, eInventoryActionType.Quest, _rewardMoneys[0]);
                            }

                            charQuest.Count++;
                            GameServer.Database.SaveObject(charQuest);

                            bool add = true;
                            lock (player.QuestListFinished)
                            {
                                foreach (AbstractQuest q in player.QuestListFinished)
                                {
                                    if (q is DataQuest && (q as DataQuest).Id == Id)
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                            }

                            if (add)
                            {
                                player.QuestListFinished.Add(this);
                            }

                            player.Out.SendQuestListUpdate();

                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                    }

                    return;
                }

                if (StartType == eStartType.AutoStart)
                {
                    CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);
                    DataQuest dq = new DataQuest(player, obj, DbDataQuest, charQuest)
                    {
                        Step = 1
                    };

                    player.AddQuest(dq);
                    if (_sourceTexts.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(_sourceTexts[0]))
                        {
                            TryTurnTo(obj, player);
                            SendMessage(player, _sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                    }
                    else
                    {
                        ChatUtil.SendDebugMessage(player, "Source Text missing on AutoStart quest.");
                    }

                    if (obj is GameNPC npc)
                    {
                        UpdateQuestIndicator(npc, player);
                    }

                    return;
                }

                if (StartType == eStartType.SearchStart)
                {
                    if (_searchStartItemTemplate != string.Empty)
                    {
                        lock (player.Inventory)
                        {
                            if (player.Inventory.IsSlotsFree(1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                            {
                                ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(_searchStartItemTemplate.Trim());
                                if (item == null)
                                {
                                    string errorMsg = $"SearchStart Item Template {_searchStartItemTemplate} not found in DB!";
                                    ChatUtil.SendDebugMessage(player, errorMsg);
                                    Log.Error(errorMsg);
                                    return;
                                }

                                GiveItem(player, item, false);
                            }
                            else
                            {
                                player.Out.SendMessage("Your backpack is full, you can't start this quest!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }
                    }

                    CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);
                    DataQuest dq = new DataQuest(player, obj, DbDataQuest, charQuest)
                    {
                        Step = 1
                    };

                    player.AddQuest(dq);
                    if (_sourceTexts.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(_sourceTexts[0]))
                        {
                            TryTurnTo(obj, player);
                            SendMessage(player, _sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                    }
                    else
                    {
                        ChatUtil.SendDebugMessage(player, "Source Text missing on SearchStart quest.");
                    }

                    if (obj is GameNPC npc)
                    {
                        UpdateQuestIndicator(npc, player);
                    }

                    return;
                }

                if (StartType == eStartType.RewardQuest)
                {
                    // Send offer quest dialog
                    if (obj is GameNPC offerNpc)
                    {
                        TryTurnTo(obj, player);

                        // Note: If the offer is handled by the custom step then it should return false to prevent a double offer
                        if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Offer))
                        {
                            player.Out.SendQuestOfferWindow(offerNpc, player, this);
                        }
                    }

                    return; // Return here so we dont send 'Description' in a separate popup window
                }

                if (StartType == eStartType.Collection)
                {
                    CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, false);

                    if (charQuest != null && charQuest.Count >= 1 && charQuest.Count < MaxQuestCount)
                    {
                        if (!string.IsNullOrEmpty(TargetText))
                        {
                            TryTurnTo(obj, player);
                            SendMessage(player, _targetTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                    }
                    else if (!string.IsNullOrEmpty(Description))
                    {
                        TryTurnTo(obj, player);
                        SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                }
                else if (!string.IsNullOrEmpty(Description))
                {
                    TryTurnTo(obj, player);
                    SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
            }
        }

        protected virtual void TryTurnTo(GameObject obj, GamePlayer player)
        {
            if (obj is GameNPC npc)
            {
                npc.TurnTo(player, 10000);
            }
        }

        /// <summary>
        /// Check quests offered to see if receiving an item should be processed
        /// Used for Collection and Item Start quest types
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="item"></param>
        protected virtual void CheckOfferedQuestReceiveItem(GamePlayer player, GameObject obj, InventoryItem item)
        {
            // checking the quests we can offer to see if this is a collection quest or if the item starts a quest
            // log.DebugFormat("Checking collection quests: '{0}' of type '{1}', wants item '{2}'", Name, (eStartType)DBDataQuest.StartType, DBDataQuest.CollectItemTemplate == null ? "" : DBDataQuest.CollectItemTemplate);

            // check to see if this object has a collection quest and if so accept the item and generate the reward
            // collection quests do not go into the GamePlayer quest lists
            if (StartType == eStartType.Collection && item.Id_nb == DbDataQuest.CollectItemTemplate)
            {
                CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);

                if (charQuest.Count < MaxQuestCount && player.Level <= MaxLevel && player.Level >= Level)
                {
                    TryTurnTo(obj, player);

                    if (item.Count == 1)
                    {
                        RemoveItem(obj, player, item, false);
                        charQuest.Count++;
                        charQuest.Step = 0;
                        GameServer.Database.SaveObject(charQuest);
                        if (long.TryParse(DbDataQuest.RewardXP, out var rewardXp))
                        {
                            player.GainExperience(GameLiving.eXPSource.Quest, rewardXp);
                        }

                        if (_sourceTexts.Count > 0)
                        {
                            SendMessage(player, _sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                        else
                        {
                            ChatUtil.SendDebugMessage(player, "Source Text missing on Collection Quest receive item.");
                        }
                    }
                    else
                    {
                        SendMessage(player, "You need to unstack these first.", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                }

                if (charQuest.Count >= MaxQuestCount)
                {
                    if (!string.IsNullOrEmpty(FinishText))
                    {
                            TryTurnTo(obj, player);
                            SendMessage(player, FinishText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                }

                if (player.Level < Level)
                {
                    if (StepTexts.Count != 0)
                    {
                        TryTurnTo(obj, player);
                        SendMessage(player, StepTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    }
                }
            }
        }

        /// <summary>
        /// Check offered quests to see if whisper should be processed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="living"></param>
        /// <param name="text"></param>
        protected virtual void CheckOfferedQuestWhisper(GamePlayer player, GameLiving living, string text)
        {
            // log.DebugFormat("Checking accept quest: '{0}' ID: {1} of type '{2}', key word '{3}', is qualified {4}", Name, ID, (eStartType)DBDataQuest.StartType, DBDataQuest.AcceptText, CheckQuestQualification(player));
            if (CheckQuestQualification(player) && DbDataQuest.StartType == (byte)eStartType.Standard && DbDataQuest.AcceptText == text)
            {
                TryTurnTo(living, player);

                CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);
                DataQuest dq = new DataQuest(player, living, DbDataQuest, charQuest)
                {
                    Step = 1
                };

                player.AddQuest(dq);
                if (_sourceTexts.Count > 0)
                {
                    SendMessage(player, _sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    ChatUtil.SendDebugMessage(player, "Source Text missing on accept quest.");
                }

                if (living is GameNPC npc)
                {
                    UpdateQuestIndicator(npc, player);
                }
            }
        }

        /// <summary>
        /// A player with this quest has interacted with an object.
        /// See if this object is part of the quest and respond accordingly
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        protected virtual void OnPlayerInteract(GamePlayer player, GameObject obj)
        {
            if (TargetName == obj.Name && (TargetRegion == obj.CurrentRegionID || TargetRegion == 0))
            {
                switch (StepType)
                {
                    case eStepType.Interact:
                        {
                            TryTurnTo(obj, player);

                            if (!string.IsNullOrEmpty(TargetText))
                            {
                                SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            }

                            AdvanceQuestStep(obj);
                        }

                        break;

                    case eStepType.InteractFinish:
                        {
                            if (StartType == eStartType.RewardQuest)
                            {
                                if (obj is GameNPC finishNpc)
                                {
                                    TryTurnTo(obj, player);

                                    // Custom step can modify rewards here.  Should return false if it sends the reward window
                                    if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
                                    {
                                        player.Out.SendQuestRewardWindow(finishNpc, player, this);
                                    }
                                }
                                else
                                {
                                    Log.Error($"DataQuest Finish is RewardQuest but object {obj.Name} is not an NPC!");
                                }
                            }
                            else
                            {
                                FinishQuest(obj, true);
                            }
                        }

                        break;

                    default:
                        {
                            if (!string.IsNullOrEmpty(TargetText))
                            {
                                TryTurnTo(obj, player);
                                SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            }
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// A player doing this quest has given an item to something.  All active quests check to see if they need to respond to this.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="item"></param>
        protected virtual void OnPlayerGiveItem(GamePlayer player, GameObject obj, InventoryItem item)
        {
            if (item?.OwnerID == null || _collectItems.Count == 0)
            {
                return;
            }

            if (TargetName == obj.Name && (TargetRegion == obj.CurrentRegionID || TargetRegion == 0)
               && player.Level >= Level && player.Level <= MaxLevel)
            {
                if (_collectItems.Count >= Step &&
                    !string.IsNullOrEmpty(_collectItems[Step - 1]) &&
                    item.Id_nb.ToLower().Contains(_collectItems[Step - 1].ToLower()) &&
                    ExecuteCustomQuestStep(player, Step, eStepCheckType.GiveItem))
                {
                    switch (StepType)
                    {
                        case eStepType.Deliver:
                        case eStepType.Collect:
                            {
                                TryTurnTo(obj, player);

                                if (!string.IsNullOrEmpty(TargetText))
                                {
                                    if (obj.Realm == eRealm.None)
                                    {
                                        // mobs and other non realm objects send chat text and not popup text.
                                        SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                                    }
                                    else
                                    {
                                        SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                    }
                                }

                                if (AdvanceQuestStep(obj))
                                {
                                    RemoveItem(obj, player, item, true);
                                }
                            }

                            break;

                        case eStepType.DeliverFinish:
                        case eStepType.CollectFinish:
                            {
                                if (FinishQuest(obj, true))
                                {
                                    RemoveItem(obj, player, item, true);
                                }
                            }

                            break;
                    }
                }
                else if (_stepItemTemplates.Count >= Step &&
                    !string.IsNullOrEmpty(_stepItemTemplates[Step - 1]) &&
                    item.Id_nb.ToLower().Contains(_stepItemTemplates[Step - 1].ToLower()) &&
                    ExecuteCustomQuestStep(player, Step, eStepCheckType.GiveItem))
                {
                    // Current step must be a delivery so take the item and advance the quest
                    if (StepType == eStepType.Deliver)
                    {
                        TryTurnTo(obj, player);

                        if (!string.IsNullOrEmpty(TargetText))
                        {
                            if (obj.Realm == eRealm.None)
                            {
                                // mobs and other non realm objects send chat text and not popup text.
                                SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                            }
                            else
                            {
                                SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            }
                        }

                        if (AdvanceQuestStep(obj))
                        {
                            RemoveItem(obj, player, item, true);
                        }
                    }
                    else if (StepType == eStepType.DeliverFinish)
                    {
                        if (FinishQuest(obj, true))
                        {
                            RemoveItem(obj, player, item, true);
                        }
                    }
                    else
                    {
                        ChatUtil.SendDebugMessage(player, "Received item in StepItemTemplates but current step is not deliver or deliver finish.");
                    }
                }
                else
                {
                    ChatUtil.SendDebugMessage(player, "Received item not in Collect or Step item list.");
                }
            }
        }

        /// <summary>
        /// A player has given an item to an object
        /// See if this object is part of the quest and respond accordingly
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="item"></param>
        protected virtual void OnNPCReceiveItem(GamePlayer player, GameObject obj, InventoryItem item)
        {
            if (QuestPlayer == null)
            {
                // Player may want to start this quest
                CheckOfferedQuestReceiveItem(player, obj, item);
            }
        }

        /// <summary>
        /// A player has whispered to a GameLiving
        /// The player is either starting this quest, or doing this quest
        /// </summary>
        /// <param name="player"></param>
        /// <param name="living"></param>
        /// <param name="text"></param>
        protected virtual void OnNPCReceiveWhisper(GamePlayer player, GameLiving living, string text)
        {
            if (QuestPlayer == null)
            {
                // Player may want to start this quest
                CheckOfferedQuestWhisper(player, living, text);
            }
        }

        /// <summary>
        /// A player doing this quest whispers something to a living
        /// </summary>
        /// <param name="p"></param>
        /// <param name="living"></param>
        /// <param name="text"></param>
        public virtual void OnPlayerWhisper(GamePlayer p, GameObject obj, string text)
        {
            // log.DebugFormat("Whisper {0}, listening for {1}, on step type {2}", text, AdvanceText, _stepTypes[Step - 1]);
            if (TargetName == obj.Name && (TargetRegion == obj.CurrentRegionID || TargetRegion == 0) && AdvanceText == text)
            {
                switch (StepType)
                {
                    case eStepType.Whisper:
                        {
                            AdvanceQuestStep(obj);
                        }

                        break;

                    case eStepType.WhisperFinish:
                        {
                            FinishQuest(obj, true);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// The living offering a dataquest is dying.  Do we have any kill quests we need to activate?
        /// </summary>
        /// <param name="dying"></param>
        /// <param name="killer"></param>
        /// <param name="playerKillers"></param>
        protected virtual void OnLivingIsDying(GameLiving dying, GameObject killer, List<GamePlayer> playerKillers)
        {
            if (StartType == eStartType.KillComplete)
            {
                if (playerKillers == null)
                {
                    GamePlayer player = killer as GamePlayer;

                    if (player == null)
                    {
                        if (killer is GameNPC npc)
                        {
                            if (npc.Brain is IControlledBrain brain)
                            {
                                player = brain.GetPlayerOwner();
                            }
                        }
                    }

                    if (player == null)
                    {
                        return;
                    }

                    playerKillers = new List<GamePlayer>
                    {
                        player
                    };
                }

                if (killer is GamePlayer gamePlayer)
                {
                    if (playerKillers.Contains(gamePlayer) == false)
                    {
                        playerKillers.Add(gamePlayer);
                    }
                }

                foreach (GamePlayer player in playerKillers)
                {
                    if (CheckQuestQualification(player))
                    {
                        CharacterXDataQuest charQuest = GetCharacterQuest(player, Id, true);

                        if (charQuest.Count < MaxQuestCount)
                        {
                            if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
                            {
                                if (Description.Trim() != string.Empty)
                                {
                                    SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                }

                                if (FinalRewards.Count > 0)
                                {
                                    lock (player.Inventory)
                                    {
                                        if (player.Inventory.IsSlotsFree(FinalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                                        {
                                            foreach (ItemTemplate item in FinalRewards)
                                            {
                                                if (item != null)
                                                {
                                                    GiveItem(dying, player, item, false);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            SendMessage(player, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                            return;
                                        }
                                    }
                                }

                                if (_rewardXPs.Count > 0 && _rewardXPs[0] > 0)
                                {
                                    player.GainExperience(GameLiving.eXPSource.Quest, _rewardXPs[0]);
                                }

                                if (_rewardClxps.Count > 0 && _rewardClxps[0] > 0)
                                {
                                    player.GainChampionExperience(_rewardClxps[0], GameLiving.eXPSource.Quest);
                                }

                                if (_rewardRPs.Count > 0 && _rewardRPs[0] > 0)
                                {
                                    player.GainRealmPoints(_rewardRPs[0]);
                                }

                                if (_rewardBPs.Count > 0 && _rewardBPs[0] > 0)
                                {
                                    player.GainBountyPoints(_rewardBPs[0]);
                                }

                                if (_rewardMoneys.Count > 0 && _rewardMoneys[0] > 0)
                                {
                                    player.AddMoney(_rewardMoneys[0], "You are awarded {0}!");
                                    InventoryLogging.LogInventoryAction($"(QUEST;{Name})", player, eInventoryActionType.Quest, _rewardMoneys[0]);
                                }

                                charQuest.Count++;
                                GameServer.Database.SaveObject(charQuest);

                                bool add = true;
                                lock (player.QuestListFinished)
                                {
                                    foreach (AbstractQuest q in player.QuestListFinished)
                                    {
                                        if (q is DataQuest && (q as DataQuest).Id == Id)
                                        {
                                            add = false;
                                            break;
                                        }
                                    }
                                }

                                if (add)
                                {
                                    player.QuestListFinished.Add(this);
                                }

                                player.Out.SendQuestListUpdate();

                                player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                                player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enemy of a player with a dataquest is killed, check for quest advancement
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        protected virtual void OnEnemyKilled(GamePlayer player, GameLiving living)
        {
            if (TargetName == living.Name && (TargetRegion == living.CurrentRegionID || TargetRegion == 0))
            {
                switch (StepType)
                {
                    case eStepType.Kill:
                        {
                            if (!string.IsNullOrEmpty(TargetText))
                            {
                                if (living.Realm == eRealm.None)
                                {
                                    // mobs and other non realm objects send chat text and not popup text.
                                    SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                                }
                                else
                                {
                                    SendMessage(QuestPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                }
                            }

                            AdvanceQuestStep(living);
                        }

                        break;

                    case eStepType.KillFinish:
                        {
                            FinishQuest(living, true);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Triggered from quest commands like /search
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public override bool Command(GamePlayer player, eQuestCommand command, AbstractArea area = null)
        {
            if (player == null || command == eQuestCommand.None)
            {
                return false;
            }

            if (command == eQuestCommand.Search)
            {
                // every active quest in the players quest list is sent this command.  Respond if we have an active search
                if (_numSearchAreas > 0 && player == QuestPlayer)
                {
                    // see if the player is in our search area
                    foreach (var area1 in player.CurrentAreas)
                    {
                        if (area1 is QuestSearchArea playerArea && playerArea.DataQuest != null && playerArea.DataQuest.Id == Id)
                        {
                            if (playerArea.Step == Step)
                            {
                                StartQuestActionTimer(player, command, playerArea.SearchSeconds, "Searching ...");
                                return true; // only allow one active search at a time
                            }
                        }
                    }
                }
            }

            if (command == eQuestCommand.SearchStart && area != null)
            {
                // If player can start this quest then do search action
                if (CheckQuestQualification(player))
                {
                    StartQuestActionTimer(player, command, ((QuestSearchArea) area).SearchSeconds, "Searching ...");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A quest command like /search is completed, so do something
        /// </summary>
        /// <param name="command"></param>
        protected override void QuestCommandCompleted(eQuestCommand command, GamePlayer player)
        {
            if (command == eQuestCommand.Search && QuestPlayer == player)
            {
                if (StepType == eStepType.Search)
                {
                    if (AdvanceQuestStep() == false)
                    {
                        SendMessage(QuestPlayer, "You fail to find anything!", 0, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
                else if (StepType == eStepType.SearchFinish)
                {
                    FinishQuest();
                }
            }

            if (command == eQuestCommand.SearchStart)
            {
                CheckOfferQuest(player, null);
            }
        }

        public override void FinishQuest()
        {
            FinishQuest(null, true);
        }

        /// <summary>
        /// Finish the quest and update the player quest list
        /// </summary>
        public virtual bool FinishQuest(GameObject obj, bool checkCustomStep)
        {
            if (QuestPlayer == null || CharDataQuest == null || CharDataQuest.IsPersisted == false)
            {
                return false;
            }

            int lastStep = Step;

            TryTurnTo(obj, QuestPlayer);

            if (checkCustomStep && ExecuteCustomQuestStep(QuestPlayer, Step, eStepCheckType.Finish) == false)
            {
                return false;
            }

            // try rewards first
            lock (QuestPlayer.Inventory)
            {
                if (QuestPlayer.Inventory.IsSlotsFree(FinalRewards.Count + OptionalRewardsChoice.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                {
                    long rewardXp = 0;
                    long rewardRp = 0;
                    long rewardClxp;
                    long rewardBp;
                    long rewardMoney;

                    const string xpError = "Your XP is turned off, you must turn it on to complete this quest!";
                    const string rpError = "Your RP is turned off, you must turn it on to complete this quest!";

                    if (StartType != eStartType.RewardQuest)
                    {
                        if (_rewardXPs.Count > 0)
                        {
                            rewardXp = _rewardXPs[lastStep - 1];
                        }

                        if (_rewardRPs.Count > 0)
                        {
                            rewardRp = _rewardRPs[lastStep - 1];
                        }

                        if (rewardXp > 0)
                        {
                            if (!QuestPlayer.GainXP)
                            {
                                QuestPlayer.Out.SendMessage(xpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            if (rewardRp > 0 && !QuestPlayer.GainRP)
                            {
                                QuestPlayer.Out.SendMessage(rpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            QuestPlayer.GainExperience(GameLiving.eXPSource.Quest, rewardXp);
                        }

                        if (rewardRp > 0)
                        {
                            if (!QuestPlayer.GainRP)
                            {
                                QuestPlayer.Out.SendMessage(rpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            QuestPlayer.GainRealmPoints(rewardRp);
                        }

                        foreach (ItemTemplate item in FinalRewards)
                        {
                            if (item != null)
                            {
                                GiveItem(QuestPlayer, item);
                            }
                        }

                        foreach (ItemTemplate item in OptionalRewardsChoice)
                        {
                            if (item != null)
                            {
                                GiveItem(QuestPlayer, item);
                            }
                        }

                        if (_rewardClxps.Count > 0)
                        {
                            rewardClxp = _rewardClxps[lastStep - 1];
                            if (rewardClxp > 0)
                            {
                                QuestPlayer.GainChampionExperience(rewardClxp, GameLiving.eXPSource.Quest);
                            }
                        }

                        if (_rewardBPs.Count > 0)
                        {
                            rewardBp = _rewardBPs[lastStep - 1];
                            if (rewardBp > 0)
                            {
                                QuestPlayer.GainBountyPoints(rewardBp);
                            }
                        }

                        if (_rewardMoneys.Count > 0)
                        {
                            rewardMoney = _rewardMoneys[lastStep - 1];
                            if (rewardMoney > 0)
                            {
                                QuestPlayer.AddMoney(rewardMoney, "You are awarded {0}!");
                                InventoryLogging.LogInventoryAction($"(QUEST;{Name})", QuestPlayer, eInventoryActionType.Quest, rewardMoney);
                            }
                        }
                    }

                    // Reward quest receives everything on last interactFinish step.
                    if (StartType == eStartType.RewardQuest)
                    {
                        if (_rewardXPs.Count > 0)
                        {
                            rewardXp = _rewardXPs[0];
                        }

                        if (_rewardRPs.Count > 0)
                        {
                            rewardRp = _rewardRPs[0];
                        }

                        if (rewardXp > 0)
                        {
                            if (!QuestPlayer.GainXP)
                            {
                                QuestPlayer.Out.SendMessage(xpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            if (rewardRp > 0 && !QuestPlayer.GainRP)
                            {
                                QuestPlayer.Out.SendMessage(rpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            QuestPlayer.GainExperience(GameLiving.eXPSource.Quest, rewardXp);
                        }

                        if (rewardRp > 0)
                        {
                            if (!QuestPlayer.GainRP)
                            {
                                QuestPlayer.Out.SendMessage(rpError, eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                                return false;
                            }

                            QuestPlayer.GainRealmPoints(rewardRp);
                        }

                        foreach (ItemTemplate item in FinalRewards)
                        {
                            if (item != null)
                            {
                                GiveItem(QuestPlayer, item);
                            }
                        }

                        foreach (ItemTemplate item in OptionalRewardsChoice)
                        {
                            if (item != null)
                            {
                                GiveItem(QuestPlayer, item);
                            }
                        }

                        if (_rewardClxps.Count > 0)
                        {
                            rewardClxp = _rewardClxps[0];
                            if (rewardClxp > 0)
                            {
                                QuestPlayer.GainChampionExperience(rewardClxp, GameLiving.eXPSource.Quest);
                            }
                        }

                        if (_rewardBPs.Count > 0)
                        {
                            rewardBp = _rewardBPs[0];
                            if (rewardBp > 0)
                            {
                                QuestPlayer.GainBountyPoints(rewardBp);
                            }
                        }

                        if (_rewardMoneys.Count > 0)
                        {
                            rewardMoney = _rewardMoneys[0];
                            if (rewardMoney > 0)
                            {
                                QuestPlayer.AddMoney(rewardMoney, "You are awarded {0}!");
                                InventoryLogging.LogInventoryAction($"(QUEST;{Name})", QuestPlayer, eInventoryActionType.Quest, rewardMoney);
                            }
                        }
                    }
                }
                else
                {
                    SendMessage(QuestPlayer, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return false;
                }
            }

            CharDataQuest.Step = 0;
            CharDataQuest.Count++;
            GameServer.Database.SaveObject(CharDataQuest);

            // Now that quest is finished do any post finished custom steps
            ExecuteCustomQuestStep(QuestPlayer, Step, eStepCheckType.PostFinish);

            QuestPlayer.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(QuestPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
            QuestPlayer.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(QuestPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            // Remove this quest from the players active quest list and either
            // Add or update the quest in the players finished list
            QuestPlayer.QuestList.Remove(this);

            bool add = true;
            lock (QuestPlayer.QuestListFinished)
            {
                foreach (AbstractQuest q in QuestPlayer.QuestListFinished)
                {
                    if (q is DataQuest && (q as DataQuest).Id == Id)
                    {
                        (q as DataQuest).CharDataQuest.Step = 0;
                        (q as DataQuest).CharDataQuest.Count++;
                        add = false;
                        break;
                    }
                }
            }

            if (add)
            {
                QuestPlayer.QuestListFinished.Add(this);
            }

            QuestPlayer.Out.SendQuestListUpdate();

            if (StartType == eStartType.RewardQuest)
            {
                QuestPlayer.Out.SendSoundEffect(11, 0, 0, 0, 0, 0);
            }

            if (!string.IsNullOrEmpty(DbDataQuest.FinishText)) // Give users option to have 'finish' text with rewardquest too
            {
                if (obj != null && obj.Realm == eRealm.None)
                {
                    // mobs and other non realm objects send chat text and not popup text.
                    SendMessage(QuestPlayer, DbDataQuest.FinishText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                }
                else
                {
                    SendMessage(QuestPlayer, DbDataQuest.FinishText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
            }

            if (obj is GameNPC gameNpc)
            {
                UpdateQuestIndicator(gameNpc, QuestPlayer);
            }

            if (_startNpc != null)
            {
                UpdateQuestIndicator(_startNpc, QuestPlayer);
            }

            foreach (GameNPC npc in QuestPlayer.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                UpdateQuestIndicator(npc, QuestPlayer);
            }

            return true;
        }

        /// <summary>
        /// Replace special characters in an item string
        /// Supported parsing:
        /// %c = character class
        /// </summary>
        /// <param name="idnb"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        protected virtual string ParseItemString(string idnb, GamePlayer player)
        {
            string parsed = idnb;

            parsed = parsed.Replace("%c", ((eCharacterClass)player.CharacterClass.ID).ToString());

            return parsed;
        }

        /// <summary>
        /// Called to abort the quest and remove it from the database!
        /// </summary>
        public override void AbortQuest()
        {
            if (QuestPlayer == null || CharDataQuest == null || CharDataQuest.IsPersisted == false)
            {
                return;
            }

            if (QuestPlayer.QuestList.Contains(this))
            {
                QuestPlayer.QuestList.Remove(this);
            }

            if (CharDataQuest.Count == 0)
            {
                if (QuestPlayer.QuestListFinished.Contains(this))
                {
                    QuestPlayer.QuestListFinished.Remove(this);
                }

                DeleteFromDatabase();
            }

            QuestPlayer.Out.SendQuestListUpdate();
            QuestPlayer.Out.SendMessage(LanguageMgr.GetTranslation(QuestPlayer.Client, "AbstractQuest.AbortQuest"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            if (_startNpc != null)
            {
                UpdateQuestIndicator(_startNpc, QuestPlayer);
            }
        }

        /// <summary>
        /// Saves this quest into the database
        /// </summary>
        public override void SaveIntoDatabase()
        {
            // Not applicable for data quests
        }

        /// <summary>
        /// Quest aborted, deleting from player
        /// </summary>
        public override void DeleteFromDatabase()
        {
            if (CharDataQuest == null || CharDataQuest.IsPersisted == false)
            {
                return;
            }

            CharacterXDataQuest charQuest = GameServer.Database.FindObjectByKey<CharacterXDataQuest>(CharDataQuest.ID);
            if (charQuest != null)
            {
                GameServer.Database.DeleteObject(charQuest);
            }
        }
    }
}