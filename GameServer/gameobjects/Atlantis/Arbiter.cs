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
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    /// <summary>
    /// The Atlantis arbiter.
    /// </summary>
    /// <author>Aredhel</author>
    public class Arbiter : Researcher
    {
        public Arbiter()
            : base() { }

        /// <summary>
        /// Address the arbiter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) 
                return false;

			String realm = GlobalConstants.RealmToName((eRealm)Realm);

			SayTo(player, eChatLoc.CL_PopupWindow, LanguageMgr.GetTranslation(player.Client, 
				String.Format("{0}.Arbiter.Interact.Welcome", realm), player.CharacterClass.Name));

            // TODO: This appears to be level-dependent. Get the proper message
            // for all the other cases (high enough level when starting the trials
            // high enough level and trials already started).

			SayTo(player, eChatLoc.CL_PopupWindow, LanguageMgr.GetTranslation(player.Client, 
				String.Format("{0}.Arbiter.Interact.BeginTrials", realm), player.Name));
            return true;
        }

        /// <summary>
		/// Talk to the arbiter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns>True, if string needs further processing.</returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text)) return false;
            GamePlayer player = source as GamePlayer;
			String realm = GlobalConstants.RealmToName((eRealm)Realm);
			String lowerCase = text.ToLower();

			if (lowerCase == LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Case1", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text1", realm), player.CharacterClass.Name));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Case2", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text2", realm)));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case3", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text3", realm)));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case4", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text4", realm), player.CharacterClass.Name));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case5", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text5", realm)));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case6", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text6", realm), player.CharacterClass.Name));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case7", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text7", realm), player.CharacterClass.Name));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
			   String.Format("{0}.Arbiter.WhisperReceive.Case8", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text8", realm), player.CharacterClass.Name));
				return false;
			}
			else if (lowerCase == LanguageMgr.GetTranslation(player.Client,
				String.Format("{0}.Arbiter.WhisperReceive.Case9", realm)))
			{
				SayTo(player, eChatLoc.CL_PopupWindow,
					LanguageMgr.GetTranslation(player.Client,
					String.Format("{0}.Arbiter.WhisperReceive.Text9", realm), player.CharacterClass.Name));
				return false;
			}

            return true;
        }
    }
}
