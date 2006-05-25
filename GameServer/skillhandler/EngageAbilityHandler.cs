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
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Sprint Ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Engage)]
	public class EngageAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// wait 5 sec to engage after attack
		/// </summary>
		public const int ENGAGE_ATTACK_DELAY_TICK = 5000;

		/// <summary>
		/// Endurance lost on every attack
		/// </summary>
		public const int ENGAGE_DURATION_LOST = 15;

		/// <summary>
		/// Execute engage ability
		/// </summary>
		/// <param name="ab">The used ability</param>
		/// <param name="player">The player that used the ability</param>
		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in EngageAbilityHandler.");
				return;
			}

			//Cancel old engage effects on player
			EngageEffect engage = (EngageEffect) player.EffectList.GetOfType(typeof(EngageEffect));
			if (engage!=null)
			{
				engage.Cancel(false);
				return;
			}

			if (!player.Alive)
			{
				player.Out.SendMessage("You can't enter combat mode while lying down!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
			{
				player.Out.SendMessage("You can't enter melee combat mode with a fired weapon!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			GameLiving target = player.TargetObject as GameLiving;
			if (target == null)
			{				
				player.Out.SendMessage("You can only engage mobs hostile to you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			// You cannot engage a mob that was attacked within the last 5 seconds...
			if (target.LastAttackedByEnemyTick > target.Region.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK) 
			{
				player.Out.SendMessage(target.GetName(0,true)+" has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);	
				return;
			}

			if (!GameServer.ServerRules.IsAllowedToAttack(player,target,true))
			{
				player.Out.SendMessage("You are not allowed to engage "+target.GetName(0,false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}	

			new EngageEffect().Start(player, target);
		}                       
    }
}
