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
using System.Collections;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	public abstract class CraftNPC : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public abstract string GUILD_ORDER { get; }

		public abstract string GUILD_CRAFTERS { get; }

		public abstract eCraftingSkill[] TrainedSkills { get; }

		public abstract eCraftingSkill TheCraftingSkill { get; }

		public abstract string InitialEntersentence { get; }

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;
			if (player.CharacterClass == null)
				return false;

			TurnTo(player, 5000);

            // Luhz Crafting Update:
            // Players can join any, and all, crafting professions.
            SayTo(player, eChatLoc.CL_PopupWindow, InitialEntersentence);
            /*			
			if(player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
			{
				SayTo(player, eChatLoc.CL_PopupWindow, InitialEntersentence);
				return true;
			}
			
			if (player.CraftingPrimarySkill != TheCraftingSkill)
			{
				SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client, "CraftNPC.Interact.NotMaster"));
				return true;
			}

            if (player.GetCraftingSkillValue(TheCraftingSkill)%100 == 99)
			{
				player.GainCraftingSkill(TheCraftingSkill, 1);
				SayTo(player, eChatLoc.CL_PopupWindow, LanguageMgr.GetTranslation(player.Client, "CraftNPC.Interact.Promoted", player.CraftTitle));
				player.Out.SendUpdateCraftingSkills();
			}
			else
			{
                SayTo(player, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(player.Client, "CraftNPC.Interact.Examine", Name));
            }
            */
            		
			return true;
		}

		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			if (source is GamePlayer == false)
				return true;

			GamePlayer player = (GamePlayer) source;

            // Luhz Crafting Update:
            // Players may now join any, and all, crafting professions.
			// if (text == GUILD_ORDER && player.CraftingPrimarySkill == 0)
            // && player.GetCraftingSkillValue(TheCraftingSkill) < 1
            if(text == GUILD_ORDER)
			{
				player.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client, "CraftNPC.WhisperReceive.WishToJoin", GUILD_ORDER), new CustomDialogResponse(CraftNpcDialogResponse));
			}
			return true;
		}

		protected virtual void CraftNpcDialogResponse(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return; //declined

			player.CraftingPrimarySkill = TheCraftingSkill;
		
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "CraftNPC.CraftNpcDialogResponse.Accepted", GUILD_CRAFTERS), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				
			foreach (eCraftingSkill skill in TrainedSkills)
			{
				player.AddCraftingSkill(skill, 1);
			}
			player.Out.SendUpdateCraftingSkills();
		}
	}
}