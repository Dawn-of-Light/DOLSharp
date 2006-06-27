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
using DOL.GS;
using DOL.GS.Quests;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the Quest event of GamePlayer
	/// </summary>
	public class QuestEventArgs : SourceEventArgs
	{
		private AbstractQuestDescriptor desc;
        private GamePlayer player;

		/// <summary>
		/// Constrcuts a new QuesteventArgument
		/// </summary>
		/// <param name="source">Inviting NPC</param>
		/// <param name="player">Player associated with quest</param>
		/// <param name="desc">the quest descriptor</param>
		public QuestEventArgs(GameLiving source,GamePlayer player,AbstractQuestDescriptor desc) : base (source)
		{
			this.desc = desc;
            this.player = player;
		}

		/// <summary>
		/// Gets the Id of quest
		/// </summary>
		public AbstractQuestDescriptor QuestDescriptor
		{
			get { return desc; }
		}

        public GamePlayer Player
        {
            get { return player; }
        }
	}
}
