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
	/// Handler for Intercept ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Intercept)]
	public class InterceptAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The intercept distance
		/// </summary>
		public const int INTERCEPT_DISTANCE = 128;

		/// <summary>
		/// Intercept reuse timer in milliseconds
		/// </summary>
		public const int REUSE_TIMER = 60 * 1000;

		/// <summary>
		/// Executes the ability
		/// </summary>
		/// <param name="ab">The ability used</param>
		/// <param name="player">The player that used the ability</param>
		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in InterceptAbilityHandler.");
				return;
			}

			GameObject targetObject = player.TargetObject;
			if (targetObject == null)
			{
				foreach (InterceptEffect intercept in player.EffectList.GetAllOfType(typeof (InterceptEffect)))
				{
					if (intercept.InterceptSource != player)
						continue;
					intercept.Cancel(false);
				}
				player.Out.SendMessage("You are no longer attempting to intercept attacks for anyone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// Only attacks on other players may be intercepted. 
			// You cannot intercept attacks on yourself            
			PlayerGroup group = player.PlayerGroup;
			GamePlayer interceptTarget = targetObject as GamePlayer;
			if (interceptTarget == null || group == null || !group.IsInTheGroup(interceptTarget) || interceptTarget == player)
			{
				player.Out.SendMessage("You can only take intercept attacks for players in your group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// check if someone is already intercepting for that target
			foreach (InterceptEffect intercept in interceptTarget.EffectList.GetAllOfType(typeof (InterceptEffect)))
			{
				if (intercept.InterceptTarget != interceptTarget)
					continue;
				if (intercept.InterceptSource != player)
				{
					player.Out.SendMessage(string.Format("{0} is already intercepting for {1}.", intercept.InterceptSource.GetName(0, true), intercept.InterceptTarget.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			// cancel all intercepts by this player
			foreach (InterceptEffect intercept in player.EffectList.GetAllOfType(typeof (InterceptEffect)))
			{
				if (intercept.InterceptSource != player)
					continue;
				intercept.Cancel(false);
			}

			player.DisableSkill(ab, REUSE_TIMER);

			new InterceptEffect().Start(player, interceptTarget);
		}
	}
}