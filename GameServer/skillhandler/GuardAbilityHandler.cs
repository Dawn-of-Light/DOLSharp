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
using DOL.GS;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Guard ability clicks
	/// </summary>
	[SkillHandler(Abilities.Guard)]
	public class GuardAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The guard distance
		/// </summary>
		public const int GUARD_DISTANCE = 256;

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in GuardAbilityHandler.");
				return;
			}

			GameObject targetObject = player.TargetObject;
			if (targetObject == null)
			{
				foreach (GuardEffect guard in player.EffectList.GetAllOfType(typeof(GuardEffect)))
				{
					if (guard.GuardSource == player)
						guard.Cancel(false);
				}
				player.Out.SendMessage("You are no longer guarding anyone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// You cannot guard attacks on yourself            
			GamePlayer guardTarget = player.TargetObject as GamePlayer;
			if (guardTarget == player)
			{
				player.Out.SendMessage("You can't guard yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// Only attacks on other players may be guarded. 
			// guard may only be used on other players in group
			Group group = player.Group;
			if (guardTarget == null || group == null || !group.IsInTheGroup(guardTarget))
			{
				player.Out.SendMessage("You can only guard players in your group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// check if someone is guarding the target
			foreach (GuardEffect guard in guardTarget.EffectList.GetAllOfType(typeof(GuardEffect)))
			{
				if (guard.GuardTarget != guardTarget) continue;
				if (guard.GuardSource == player)
				{
					guard.Cancel(false);
					return;
				}
				player.Out.SendMessage(string.Format("{0} is already guarding {1}!", guard.GuardSource.GetName(0, true), guard.GuardTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}


			// cancel all guard effects by this player before adding a new one
			foreach (GuardEffect guard in player.EffectList.GetAllOfType(typeof(GuardEffect)))
			{
				if (guard.GuardSource == player)
					guard.Cancel(false);
			}

			new GuardEffect().Start(player, guardTarget);
		}
	}
}
