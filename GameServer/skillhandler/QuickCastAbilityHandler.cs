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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Sprint Ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Quickcast)]
	public class QuickCastAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// The ability disable duration in milliseconds
		/// </summary>
		public const int DISABLE_DURATION = 30000;

		/// <summary>
		/// Executes the ability
		/// </summary>
		/// <param name="ab">The used ability</param>
		/// <param name="player">The player that used the ability</param>
		public void Execute(Ability ab, GamePlayer player)
		{									
			// Cannot change QC state if already casting a spell (can't turn it off!)
			if(player.CurrentSpellHandler != null)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Skill.Ability.QuickCast.CannotUseIsCasting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			QuickCastEffect quickcast = player.EffectList.GetOfType<QuickCastEffect>();
			if (quickcast!=null)
			{
				quickcast.Cancel(false);
				return;
			}			

			// Dead can't quick cast
			if(!player.IsAlive)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Skill.Ability.QuickCast.CannotUseDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			// Can't quick cast if in attack mode
			if(player.AttackState)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Skill.Ability.QuickCast.CannotUseInMeleeCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			long quickcastChangeTick = player.TempProperties.getProperty<long>(GamePlayer.QUICK_CAST_CHANGE_TICK);
			long changeTime = player.CurrentRegion.Time - quickcastChangeTick;
			if(changeTime < DISABLE_DURATION)
			{
				player.Out.SendMessage("You must wait " + ((DISABLE_DURATION-changeTime)/1000) + " more second to attempt to quick cast!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Skill.Ability.QuickCast.CannotUseChangeTick", ((DISABLE_DURATION - changeTime) / 1000)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //30 sec is time between 2 quick cast 
				return;
			}

			//TODO: more checks in this order

			//player.DisableSkill(ab,DURATION / 10);

			new QuickCastEffect().Start(player);
		}
	}
}
