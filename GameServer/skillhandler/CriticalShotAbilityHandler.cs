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
	/// Handler for Critical Shot ability
	/// </summary>
	[SkillHandlerAttribute(Abilities.Critical_Shot)]
	public class CriticalShotAbilityHandler : IAbilityActionHandler
	{
		public void Execute(Ability ab, GamePlayer player)
		{
			if (player.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUse.CriticalShot.NoRangedWeapons"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
			}
			if (player.IsSitting)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUse.CriticalShot.MustBeStanding"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
			}

			// cancel rapid fire effect
			RapidFireEffect rapidFire = player.EffectList.GetOfType<RapidFireEffect>();
			if (rapidFire != null)
				rapidFire.Cancel(false);

			// cancel sure shot effect
			SureShotEffect sureShot = player.EffectList.GetOfType<SureShotEffect>();
			if (sureShot != null)
				sureShot.Cancel(false);

			TrueshotEffect trueshot = player.EffectList.GetOfType<TrueshotEffect>();
			if (trueshot != null)
				trueshot.Cancel(false);

			if (player.AttackState)
			{
				if (player.RangedAttackType == GameLiving.eRangedAttackType.Critical)
				{
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CriticalShot.SwitchToRegular"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.RangedAttackType = GameLiving.eRangedAttackType.Normal;
				}
				else
				{
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CriticalShot.AlreadyFiring"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
				return;
			}
			player.RangedAttackType = GameLiving.eRangedAttackType.Critical;
			player.StartAttack(player.TargetObject);
		}
	}
}
