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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests
{
	public class QuestSearchArea : Area.Circle
	{
		public const int DEFAULT_SEARCH_SECONDS = 5;
		public const int DEFAULT_SEARCH_RADIUS = 150;

		Type m_questType;
		int m_validStep;
		int m_searchSeconds;
		string m_popupText = "";

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
			: base(text, x, y, z, DEFAULT_SEARCH_RADIUS)
		{
			CreateArea(questType, validStep, DEFAULT_SEARCH_SECONDS, text, regionId);
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

		protected void CreateArea(Type questType, int validStep, int searchSeconds, string text, ushort regionId)
		{
			m_questType = questType;
			m_validStep = validStep;
			m_searchSeconds = searchSeconds;
			m_popupText = text;
			DisplayMessage = false;

			if (WorldMgr.GetRegion(regionId) != null)
			{
				WorldMgr.GetRegion(regionId).AddArea(this);
			}
		}

		public override void OnPlayerEnter(GamePlayer player)
		{
			// popup a dialog telling the player they should search here
			if (player.IsDoingQuest(m_questType) != null && player.IsDoingQuest(m_questType).Step == m_validStep && m_popupText != string.Empty)
			{
				player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, m_popupText);
			}
		}

		public Type QuestType
		{
			get { return m_questType; }
		}

		public int Step
		{
			get { return m_validStep; }
		}

		public int SearchSeconds
		{
			get { return m_searchSeconds; }
		}
	}
}