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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using log4net;
using DOL.GS.Keeps;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Flurry Ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Flurry)]
	public class FlurryAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The reuse time in milliseconds for flurry ability
		/// </summary>
		protected const int REUSE_TIMER = 60 * 2000; // 2 minutes


		/// <summary>
		/// Execute the ability
		/// </summary>
		/// <param name="ab">The ability executed</param>
		/// <param name="player">The player that used the ability</param>
		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in FlurryAbilityHandler.");
				return;
			}

			#region precheck
			if (!player.IsAlive)
			{
				player.Out.SendMessage("You cannot use this while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage("You cannot use this while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage("You cannot use this while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage("You must be standing to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.TargetObject == null)
			{
				player.Out.SendMessage("You need a target to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!(player.TargetObject is GamePlayer || player.TargetObject is GameKeepGuard))
			{
				player.Out.SendMessage("You can only attack players or guards of the enemy realms with this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!GameServer.ServerRules.IsAllowedToAttack(player, (GameLiving)player.TargetObject, true))
			{
				player.Out.SendMessage("You cannot attack " + player.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!player.IsObjectInFront(player.TargetObject, 180) || !player.TargetInView)
			{
				player.Out.SendMessage("You cannot see " + player.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!WorldMgr.CheckDistance(player, player.TargetObject, 135)) //Didn't use AttackRange cause of the fact that player could use a Bow
			{
				player.Out.SendMessage("Your target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			#endregion

			GameLiving target = (GameLiving)player.TargetObject;
			int damage = 0;
			int specc = (player.CharacterClass is PlayerClass.ClassBlademaster) ?
				player.GetModifiedSpecLevel(Specs.Celtic_Dual) : player.GetModifiedSpecLevel(Specs.Dual_Wield);

			//damage = base HP / 100 * DWspec / 2.7 that would be the original calculation
			if (target is GamePlayer)
			{
				damage = (int)(target.MaxHealth / 100 * specc / 3.5);   //works fine      
			}
			else
			{ damage = (int)(target.MaxHealth / 100 * specc / 3.6); } //works fine

			#region Resists
			int primaryResistModifier = target.GetResist(eDamageType.Slash);

			//Using the resist BuffBonusCategory2 - its unused in ResistCalculator
			int secondaryResistModifier = target.BuffBonusCategory2[(int)eProperty.Resist_Slash];

			int resistModifier = 0;
			//primary resists
			resistModifier += (int)(damage * (double)primaryResistModifier * -0.01);
			//secondary resists
			resistModifier += (int)((damage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);
			//apply resists
			damage += resistModifier;

			#endregion

			//flurry is slash damage
			target.TakeDamage(player, eDamageType.Slash, damage, 0);

			//sending spell effect
			foreach (GamePlayer effPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				effPlayer.Out.SendSpellEffectAnimation(player, target, 7103, 0, false, 0x01);

			player.Out.SendMessage("You hit " + target.GetName(0, false) + " for " + damage + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			if (target is GamePlayer)
				(target as GamePlayer).Out.SendMessage(player.Name + " flurry hits you for " + damage + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

			player.LastAttackTickPvP = player.CurrentRegion.Time;
			target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;

			player.DisableSkill(ab, REUSE_TIMER);

		}
	}
}