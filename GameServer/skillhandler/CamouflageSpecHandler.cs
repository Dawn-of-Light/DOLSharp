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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Stealth Spec clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Camouflage)]
	public class CamouflageSpecHandler : IAbilityActionHandler
	{
		public const int DISABLE_DURATION = 600000;

		/// <summary>
		/// Executes the stealth ability
		/// </summary>
		/// <param name="ab"></param>
		/// <param name="player"></param>
		public void Execute(Ability ab, GamePlayer player)
		{
			if (!player.IsStealthed)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUse.Camouflage.NotStealthed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}
			 
			CamouflageEffect camouflage = player.EffectList.GetOfType<CamouflageEffect>();
			
			if (camouflage != null)
			{				
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.Camouflage.UseCamo"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			new CamouflageEffect().Start(player);
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.Camouflage.UseCamo"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}