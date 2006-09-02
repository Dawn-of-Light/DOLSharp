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
using log4net;

namespace DOL.GS
{
	public abstract class CraftNPC : GameMob
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public abstract string GUILD_ORDER { get; }

		public abstract string GUILD_CRAFTERS { get; }

		public abstract eCharacterClass[] AllowedClass { get; }

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

			bool eligible = false;
			foreach(eCharacterClass currentClass in AllowedClass)
			{
				if(currentClass == (eCharacterClass) player.CharacterClass.ID)
				{
					eligible = true;
					break;
				}
			}

			if (!eligible)
			{
				SayTo(player, eChatLoc.CL_ChatWindow, "I'm sorry, those in your profession are not eligible for the Order of " + GUILD_ORDER + ". Perhaps the other trade orders would be interested in your services.");
				return true;
			}
			
			if(player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
			{
				SayTo(player, eChatLoc.CL_PopupWindow, InitialEntersentence);
				return true;
			}
			
			if (player.CraftingPrimarySkill != TheCraftingSkill)
			{
				SayTo(player, eChatLoc.CL_ChatWindow, "I am not your master.");
				return true;
			}

			if (player.GetCraftingSkillValue(TheCraftingSkill)%100 == 99)
			{
				player.IncreaseCraftingSkill(TheCraftingSkill, 1);
				SayTo(player, eChatLoc.CL_PopupWindow, "You have been promoted to "+player.CraftTitle+".");
				player.Out.SendUpdateCraftingSkills();
			}
			else
			{
				SayTo(player, eChatLoc.CL_SystemWindow, "You examine " + Name + ". He is friendly ");
			}
		
			return true;
		}

		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			if (source is GamePlayer == false)
				return true;

			GamePlayer player = (GamePlayer) source;

			if (text == GUILD_ORDER && player.CraftingPrimarySkill == 0)
			{
				player.Out.SendCustomDialog("Are you sure you wish to join this order?\nOnce you select a primary trade skill,\nyou can't switch.", new CustomDialogResponse(CraftNpcDialogResponse));
			}
			return true;
		}

		protected virtual void CraftNpcDialogResponse(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return; //declined

			player.CraftingPrimarySkill = TheCraftingSkill;
		
			player.Out.SendMessage("You've been accepted by the "+GUILD_CRAFTERS+"!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				
			foreach (eCraftingSkill skill in TrainedSkills)
			{
				player.AddCraftingSkill(skill, 1);
			}
			player.Out.SendUpdateCraftingSkills();
		}
	}
}