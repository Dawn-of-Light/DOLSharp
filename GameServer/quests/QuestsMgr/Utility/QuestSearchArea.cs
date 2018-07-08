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
using System.Reflection;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests
{
    public class QuestSearchArea : Area.Circle
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int DefaultSearchSeconds = 5;
        private const int DefaultSearchRadius = 150;

        private ushort _regionId;

        /// <summary>
        /// Create an area used for /search.  Area will only be active when player is doing associated quest
        /// and is on the correct step.
        /// </summary>
        /// <param name="questType">The quest associated with this area.</param>
        /// <param name="validStep">The quest step that uses this area.</param>
        /// <param name="text">Popup text to show when player enters area.  Leave blank to suppress popup.</param>
        /// <param name="regionId">The region this ares is in.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public QuestSearchArea(Type questType, int validStep, string text, ushort regionId, int x, int y, int z)
            : base(text, x, y, z, DefaultSearchRadius)
        {
            CreateArea(questType, validStep, DefaultSearchSeconds, text, regionId);
        }

        /// <summary>
        /// Create an area used for /search.  Area will only be active when player is doing associated quest
        /// and is on the correct step.
        /// </summary>
        /// <param name="questType">The quest associated with this area.</param>
        /// <param name="validStep">The quest step that uses this area.</param>
        /// <param name="searchSeconds">How long the search progress takes till completion.</param>
        /// <param name="text">Popup text to show when player enters area.  Leave blank to suppress popup.</param>
        /// <param name="regionId">The region this ares is in.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius">The size of the search area.</param>
        public QuestSearchArea(Type questType, int validStep, int searchSeconds, string text, ushort regionId, int x, int y, int z, int radius)
            : base(text, x, y, z, radius)
        {
            CreateArea(questType, validStep, searchSeconds, text, regionId);
        }

        /// <summary>
        /// Search area for data quests.  Z is not checked, user must specify radius and time in data quest
        /// </summary>
        /// <param name="dataQuest"></param>
        /// <param name="validStep"></param>
        /// <param name="text"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="searchSeconds"></param>
        public QuestSearchArea(DataQuest dataQuest, int validStep, string text, ushort regionId, int x, int y, int radius = DefaultSearchRadius, int searchSeconds = DefaultSearchSeconds)
            : base(text, x, y, 0, radius)
        {
            CreateArea(dataQuest, validStep, searchSeconds, text, regionId);
        }

        protected void CreateArea(Type questType, int validStep, int searchSeconds, string text, ushort regionId)
        {
            _regionId = regionId;
            QuestType = questType;
            Step = validStep;
            SearchSeconds = searchSeconds;
            PopupText = text;
            DisplayMessage = false;

            if (WorldMgr.GetRegion(regionId) != null)
            {
                WorldMgr.GetRegion(regionId).AddArea(this);
            }
            else
            {
                Log.Error(
                    $"Could not find region {regionId} when trying to create QuestSearchArea for quest {QuestType}");
            }
        }

        protected void CreateArea(DataQuest dataQuest, int validStep, int searchSeconds, string text, ushort regionId)
        {
            _regionId = regionId;
            QuestType = typeof(DataQuest);
            DataQuest = dataQuest;
            Step = validStep;
            SearchSeconds = searchSeconds;
            PopupText = text;
            DisplayMessage = false;

            if (WorldMgr.GetRegion(regionId) != null)
            {
                WorldMgr.GetRegion(regionId).AddArea(this);
            }
            else
            {
                string errorText = $"Could not find region {regionId} when trying to create QuestSearchArea! ";
                dataQuest.LastErrorText += errorText;
                Log.Error(errorText);
            }
        }

        public virtual void RemoveArea()
        {
            if (WorldMgr.GetRegion(_regionId) != null)
            {
                WorldMgr.GetRegion(_regionId).RemoveArea(this);
            }
        }

        public override void OnPlayerEnter(GamePlayer player)
        {
            bool showText = false;

            if (DataQuest != null)
            {
                ChatUtil.SendDebugMessage(player, $"Entered QuestSearchArea for DataQuest ID:{DataQuest.Id}, Step {Step}");

                // first check active data quests
                foreach (AbstractQuest quest in player.QuestList)
                {
                    if ((quest as DataQuest)?.Id == DataQuest.Id && quest.Step == Step && PopupText != string.Empty)
                    {
                        showText = true;
                    }
                }

                // next check for searches that start a dataquest
                if (Step == 0 && DataQuest.CheckQuestQualification(player))
                {
                    showText = true;
                }
            }
            else
            {
                ChatUtil.SendDebugMessage(player, $"Entered QuestSearchArea for {QuestType.Name}, Step {Step}");

                // popup a dialog telling the player they should search here
                if (player.IsDoingQuest(QuestType) != null && player.IsDoingQuest(QuestType).Step == Step && PopupText != string.Empty)
                {
                    showText = true;
                }
            }

            if (showText)
            {
                player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, PopupText);
            }
        }

        public Type QuestType { get; private set; }

        public DataQuest DataQuest { get; private set; }

        public int Step { get; private set; }

        public int SearchSeconds { get; private set; }

        public string PopupText { get; private set; } = string.Empty;
    }
}