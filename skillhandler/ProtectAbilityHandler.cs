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
using System.Reflection;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for protect ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Protect)]
	public class ProtectAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The Protect Distance
		/// </summary>
		public const int PROTECT_DISTANCE = 1000;

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in ProtectAbilityHandler.");
				return;
			}

			GameObject targetObject = player.TargetObject;
			if (targetObject == null)
			{
				foreach (ProtectEffect protect in player.EffectList.GetAllOfType(typeof (ProtectEffect)))
				{
					if (protect.ProtectSource == player)
						protect.Cancel(false);
				}
				player.Out.SendMessage("You are no longer protecting anyone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// You cannot protect attacks on yourself            
			GamePlayer protectTarget = player.TargetObject as GamePlayer;
			if (protectTarget == player)
			{
				player.Out.SendMessage("You can't protect yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// Only attacks on other players may be protected. 
			// protect may only be used on other players in group
			Group group = player.Group;
			if (protectTarget == null || group == null || !group.IsInTheGroup(protectTarget))
			{
				player.Out.SendMessage("You can only protect players in your group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// check if someone is protecting the target
			foreach (ProtectEffect protect in protectTarget.EffectList.GetAllOfType(typeof (ProtectEffect)))
			{
				if (protect.ProtectTarget != protectTarget)
					continue;
				if (protect.ProtectSource == player)
				{
					protect.Cancel(false);
					return;
				}
				player.Out.SendMessage(string.Format("{0} is already protecting {1}!", protect.ProtectSource.GetName(0, true), protect.ProtectTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// cancel all guard effects by this player before adding a new one
			foreach (ProtectEffect protect in player.EffectList.GetAllOfType(typeof (ProtectEffect)))
			{
				if (protect.ProtectSource == player)
					protect.Cancel(false);
			}

			new ProtectEffect().Start(player, protectTarget);
		}
	}
}