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
/*
 * Author:      Gandulf Kohlweiss
 * Date:
 * Directory: /scripts/quests/
 *
 * Description:
 *  Brief Walkthrough:
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests
{

    /// <summary>
    /// BaseQuest provides some helper classes for writing quests and
    /// integrates a new QuestPart Based QuestSystem.
    /// </summary>
    public abstract class BaseQuest : AbstractQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// DO NOT USE, always true.
        /// Tolakram - this is not used anymore due to the fact items were saved based on the same setting
        /// With the new inventory system all items must be saved. To optionally not save NPC's use ServerProperties.Properties.SAVE_QUEST_MOBS_INTO_DATABASE
        /// </summary>
        public static bool SAVE_INTO_DATABASE = true;

        // Tolakram - this is not used anymore due to the fact items were saved based on the same setting
        // With the new inventory system all items must be saved.
        // To optionally not save NPC's use ServerProperties.Properties.SAVE_QUEST_MOBS_INTO_DATABASE
        public Queue AnimEmoteTeleportTimerQueue = new Queue();
        public Queue AnimEmoteObjectQueue = new Queue();
        public Queue AnimSpellTeleportTimerQueue = new Queue();
        public Queue AnimSpellObjectQueue = new Queue();

        private readonly Queue _portTeleportTimerQueue = new Queue();
        private readonly Queue _portObjectQueue = new Queue();
        private readonly Queue _portDestinationQueue = new Queue();

        // /// <summary>
        // /// List of all QuestParts that can be fired on interact Events.
        // /// </summary>
        // private static IDictionary interactQuestParts = new HybridDictionary();

        /// <summary>
        /// Create an empty Quest
        /// </summary>
        public BaseQuest()
        {
        }

        /// <summary>
        /// Constructs a new empty Quest
        /// </summary>
        public BaseQuest(GamePlayer questingPlayer)
            : base(questingPlayer)
        {
        }

        /// <summary>
        /// Constructs a new Quest
        /// </summary>
        /// <param name="questingPlayer">The player doing this quest</param>
        /// <param name="step">The current step the player is on</param>
        public BaseQuest(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        /// <summary>
        /// Constructs a new Quest from a database Object
        /// </summary>
        /// <param name="questingPlayer">The player doing the quest</param>
        /// <param name="dbQuest">The database object</param>
        public BaseQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloadedBase(DOLEvent e, object sender, EventArgs args)
        {
            if (QuestParts != null)
            {
                for (int i = QuestParts.Count - 1; i >= 0; i--)
                {
                    RemoveBehaviour((QuestBehaviour)QuestParts[i]);
                }
            }

            QuestParts = null;
        }

        // Base QuestPart methods

        /// <summary>
        /// Remove all registered handlers for this quest,
        /// this will not remove the questPart from the quest.
        /// </summary>
        /// <param name="questPart">QuestPart to remove handlers from</param>
        protected static void UnRegisterBehaviour(QuestBehaviour questPart)
        {
            if (questPart.Triggers == null)
            {
                return;
            }

            foreach (IBehaviourTrigger trigger in questPart.Triggers)
            {
                trigger.Unregister();
            }
        }

        /// <summary>
        /// Adds the given questpart to the quest depending on the added triggers it will either
        /// be added as InteractQuestPart as NotifyQuestPart or both and also register the needed event handler.
        /// </summary>
        /// <param name="questPart">QuestPart to be added</param>
        public static void AddBehaviour(QuestBehaviour questPart)
        {
            if (QuestParts == null)
            {
                QuestParts = new ArrayList();
            }

            if (!QuestParts.Contains(questPart))
            {
                QuestParts.Add(questPart);
            }

            questPart.ID = QuestParts.Count; // fake id but ids only have to be unique quest wide its enough to use the number in the list as id.
        }

        /// <summary>
        /// Remove the given questpart from the quest and also unregister the handlers
        /// </summary>
        /// <param name="questPart">QuestPart to be removed</param>
        public static void RemoveBehaviour(QuestBehaviour questPart)
        {
            if (QuestParts == null)
            {
                return;
            }

            UnRegisterBehaviour(questPart);
            QuestParts.Remove(questPart);
        }

        /// <summary>
        /// Quest internal Notify method only fires if player already has the quest assigned
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player && e == GameObjectEvent.InteractWith)
            {
                if (!(args is InteractWithEventArgs iArgs))
                {
                    return;
                }

                if (iArgs.Target is GameStaticItem item)
                {
                    InteractWithObject(player, item);
                    return;
                }
            }

            if (QuestParts == null)
            {
                return;
            }

            foreach (QuestBehaviour questPart in QuestParts)
            {
                questPart.Notify(e, sender, args);
            }
        }

        // timer callbacks
        protected virtual int MakeAnimSpellSequence(RegionTimer callingTimer)
        {
            if (AnimSpellTeleportTimerQueue.Count > 0)
            {
                AnimSpellTeleportTimerQueue.Dequeue();
                GameLiving animObject = (GameLiving)AnimSpellObjectQueue.Dequeue();
                foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendSpellCastAnimation(animObject, 1, 20);
                }
            }

            return 0;
        }

        protected virtual int MakeAnimEmoteSequence(RegionTimer callingTimer)
        {
            if (AnimEmoteTeleportTimerQueue.Count > 0)
            {
                AnimEmoteTeleportTimerQueue.Dequeue();
                GameLiving animObject = (GameLiving)AnimEmoteObjectQueue.Dequeue();
                foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendEmoteAnimation(animObject, eEmote.Bind);
                }
            }

            return 0;
        }

        protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location)
        {
            TeleportTo(target, caster, location, 0, 0);
        }

        protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay)
        {
            TeleportTo(target, caster, location, delay, 0);
        }

        protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay, int fuzzyLocation)
        {
            delay *= 100; // 1/10sec to milliseconds
            if (delay <= 0)
            {
                delay = 1;
            }

            AnimSpellObjectQueue.Enqueue(caster);
            AnimSpellTeleportTimerQueue.Enqueue(new RegionTimer(caster, new RegionTimerCallback(MakeAnimSpellSequence), (int)delay));

            AnimEmoteObjectQueue.Enqueue(target);
            AnimEmoteTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakeAnimEmoteSequence), (int)delay + 2000));

            _portObjectQueue.Enqueue(target);

            location.X += Util.Random(0 - fuzzyLocation, fuzzyLocation);
            location.Y += Util.Random(0 - fuzzyLocation, fuzzyLocation);

            _portDestinationQueue.Enqueue(location);
            _portTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakePortSequence), (int)delay + 3000));

            if (location.Name != null)
            {
                QuestPlayer.Out.SendMessage(LanguageMgr.GetTranslation(QuestPlayer.Client, "BaseQuest.TeleportTo.Text1", target.Name, location.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        protected virtual int MakePortSequence(RegionTimer callingTimer)
        {
            if (_portTeleportTimerQueue.Count > 0)
            {
                _portTeleportTimerQueue.Dequeue();
                GameObject gameObject = (GameObject)_portObjectQueue.Dequeue();
                GameLocation location = (GameLocation)_portDestinationQueue.Dequeue();
                gameObject.MoveTo(location.RegionID, location.X, location.Y, location.Z, location.Heading);
            }

            return 0;
        }

        protected struct QuestStepInteraction
        {
            public string ObjectName { get; set; }
            public int NumRequired { get; set; }
            public ItemTemplate ItemResult { get; set; }
            public string InteractText { get; set; }
        }

        private readonly Dictionary<int, QuestStepInteraction> _interactions = new Dictionary<int, QuestStepInteraction>();
        private const int InteractItemRespawnSeconds = 120;

        /// <summary>
        /// Add an interact item associated with a step for this quest
        /// </summary>
        /// <param name="step">What step is this item valid for</param>
        /// <param name="staticObjectName">the name of the static item to interact with</param>
        /// <param name="numRequired">How many times to interact before this step is complete</param>
        /// <param name="itemResult">What item is given to the player when interacting</param>
        /// <param name="interactText">Text presented to player when interacting with the object</param>
        protected void AddInteractStep(int step, string objectName, int numRequired, ItemTemplate itemResult, string interactText)
        {
            try
            {
                QuestStepInteraction info = new QuestStepInteraction
                {
                    ObjectName = objectName,
                    NumRequired = numRequired,
                    ItemResult = itemResult,
                    InteractText = interactText
                };

                _interactions.Add(step, info);
            }
            catch (Exception ex)
            {
                Log.Error("Error adding Interact Step, possible duplicate?", ex);
            }
        }

        /// <summary>
        /// We are interacting with an object, check to see if this quest and step needs to respond
        /// </summary>
        /// <param name="player"></param>
        /// <param name="staticItem"></param>
        protected void InteractWithObject(GamePlayer player, GameStaticItem staticItem)
        {
            if (_interactions.Count > 0)
            {
                if (_interactions.ContainsKey(Step))
                {
                    QuestStepInteraction info = _interactions[Step];

                    if (staticItem.Name == info.ObjectName)
                    {
                        if (GiveItem(player, info.ItemResult, false))
                        {
                            player.Out.SendMessage(info.InteractText, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            staticItem.RemoveFromWorld(InteractItemRespawnSeconds);
                            OnObjectInteract(info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When an object is interacted with this message is sent after world item is removed and inventory item added
        /// </summary>
        /// <param name="info"></param>
        protected virtual void OnObjectInteract(QuestStepInteraction info)
        {
            // this is needed in order to support both Base and Reward quests
            Log.Error("Override OnObjectInteract to advance goal progress");
        }
    }
}
