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
using System.Collections.Specialized;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests
{
    public class AbstractMission
    {
        /// <summary>
        /// possible mission types
        /// </summary>
        public enum eMissionType
        {
            None = 0,
            Personal = 1,
            Group = 2,
            Realm = 3,
            Task = 4,
        }

        public eMissionType MissionType
        {
            get
            {
                if (Owner is GamePlayer)
                {
                    return eMissionType.Personal;
                }

                if (Owner is Group)
                {
                    return eMissionType.Group;
                }

                if (Owner is eRealm)
                {
                    return eMissionType.Realm;
                }

                return eMissionType.None;
            }
        }

        /// <summary>
        /// owner of the mission
        /// </summary>
        protected object Owner;

        /// <summary>
        /// Constructs a new Mission
        /// </summary>
        /// <param name="owner">The owner of the mission</param>
        public AbstractMission(object owner)
        {
            Owner = owner;
        }

        public virtual long RewardXp => 0;

        public virtual long RewardMoney => 50 * 100 * 100;

        public virtual long RewardRealmPoints => 1500;

        /// <summary>
        /// Retrieves the name of the mission
        /// </summary>
        public virtual string Name
        {
            get
            {
                switch (MissionType)
                {
                    case eMissionType.Personal: return "Personal Mission";
                    case eMissionType.Group: return "Group Mission";
                    case eMissionType.Realm: return "Realm Mission";
                    case eMissionType.Task: return "Task";
                    case eMissionType.None: return "Unknown Mission";
                    default: return "MISSION NAME UNDEFINED!";
                }
            }
        }

        /// <summary>
        /// Retrieves the description for the mission
        /// </summary>
        public virtual string Description => "MISSION DESCRIPTION UNDEFINED!";

        /// <summary>
        /// This HybridDictionary holds all the custom properties of this quest
        /// </summary>
        private readonly HybridDictionary _customProperties = new HybridDictionary();

        /// <summary>
        /// This method sets a custom Property to a specific value
        /// </summary>
        /// <param name="key">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public void SetCustomProperty(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Make the string safe
            key = key.Replace(';', ',');
            key = key.Replace('=', '-');
            value = value.Replace(';', ',');
            value = value.Replace('=', '-');
            lock (_customProperties)
            {
                _customProperties[key] = value;
            }
        }

        /// <summary>
        /// Removes a custom property from the database
        /// </summary>
        /// <param name="key">The key name of the property</param>
        public void RemoveCustomProperty(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_customProperties)
            {
                _customProperties.Remove(key);
            }
        }

        /// <summary>
        /// This method retrieves a custom property from the database
        /// </summary>
        /// <param name="key">The property key</param>
        /// <returns>The property value</returns>
        public string GetCustomProperty(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (string)_customProperties[key];
        }

        /// <summary>
        /// Called to finish the mission
        /// </summary>
        public virtual void FinishMission()
        {
            foreach (GamePlayer player in Targets)
            {
                if (Owner is Group group)
                {
                    if (!player.IsWithinRadius(group.Leader, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                    {
                        continue;
                    }
                }

                if (RewardXp > 0)
                {
                    player.GainExperience(GameLiving.eXPSource.Mission, RewardXp);
                }

                if (RewardMoney > 0)
                {
                    player.AddMoney(RewardMoney, "You recieve {0} for completing your task.");
                    InventoryLogging.LogInventoryAction($"(MISSION;{MissionType})", player, eInventoryActionType.Quest, RewardMoney);
                }

                if (RewardRealmPoints > 0)
                {
                    player.GainRealmPoints(RewardRealmPoints);
                }

                player.Out.SendMessage("You finish the " + Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            switch (MissionType)
            {
                case eMissionType.Personal: ((GamePlayer) Owner).Mission = null; break;
                case eMissionType.Group: ((Group) Owner).Mission = null; break;
            }

            _customProperties.Clear();
        }

        private List<GamePlayer> Targets
        {
            get
            {
                switch (MissionType)
                {
                    case eMissionType.Personal:
                    {
                        GamePlayer player = Owner as GamePlayer;
                        List<GamePlayer> list = new List<GamePlayer>
                        {
                            player
                        };

                        return list;
                    }

                    case eMissionType.Group:
                    {
                        Group group = (Group) Owner;
                        return new List<GamePlayer>(group.GetPlayersInTheGroup());
                    }

                    default: return new List<GamePlayer>();
                }
            }
        }

        /// <summary>
        /// A mission runs out of time
        /// </summary>
        public virtual void ExpireMission()
        {
            foreach (GamePlayer player in Targets)
            {
                player.Out.SendMessage($"Your {Name} has expired!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            switch (MissionType)
            {
                case eMissionType.Personal: ((GamePlayer) Owner).Mission = null; break;
                case eMissionType.Group: ((Group) Owner).Mission = null; break;

                // case eMissionType.Realm: m_owner.RealmMission = null; break;
            }

            _customProperties.Clear();
        }

        public virtual void UpdateMission()
        {
            foreach (GamePlayer player in Targets)
            {
                player.Out.SendQuestListUpdate();
            }
        }

        /// <summary>
        /// This method needs to be implemented in each quest.
        /// It is the core of the quest. The global event hook of the GamePlayer.
        /// This method will be called whenever a GamePlayer with this quest
        /// fires ANY event!
        /// </summary>
        /// <param name="e">The event type</param>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments</param>
        public virtual void Notify(DOLEvent e, object sender, EventArgs args)
        {
        }
    }
}