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

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Stealth Spec clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Camouflage)]
	public class CamouflageSpecHandler : IAbilityActionHandler
	{
		public const int DISABLE_DURATION = 420000;

		/// <summary>
		/// Executes the stealth ability
		/// </summary>
		/// <param name="ab"></param>
		/// <param name="player"></param>
		public void Execute(Ability ab, GamePlayer player)
		{
			#region Check
			if (!player.IsStealthed)
			{
				if (player.IsMezzed)
				{
					player.Out.SendMessage("You can't use camouflage while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (player.IsStunned)
				{
					player.Out.SendMessage("You can't use camouflage while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				player.Out.SendMessage("You can't use camouflage while not stealthed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!player.IsAlive)
			{
				player.Out.SendMessage("You can't use camouflage when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			#endregion
			CamouflageEffect camouflage = (CamouflageEffect)player.EffectList.GetOfType(typeof(CamouflageEffect));
			if (camouflage != null)
			{
				camouflage.Cancel(false);
				return;
			}
			long changeTime = player.CurrentRegion.Time - player.LastAttackTickPvP;
			if (player.CurrentRegion.IsRvR && changeTime < DISABLE_DURATION)
			{
				player.Out.SendMessage("You must wait " + ((DISABLE_DURATION - changeTime) / 1000) + " more second to attempt to use camouflage!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			new CamouflageEffect().Start(player);
		}

	}
}
