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

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Sprint Ability clicks
	/// </summary>
	[SkillHandlerAttribute(Abilities.Berserk)]
	public class BerserkAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The reuse time in milliseconds for berserk ability
		/// </summary>
		protected const int REUSE_TIMER = 60000 * 7; // 7 minutes

		/// <summary>
		/// The effect duration in milliseconds
		/// </summary>
		public const int DURATION = 20000;

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
					log.Warn("Could not retrieve player in BerserkAbilityHandler.");
				return;
			}

            if (!player.Alive)
            {
                player.Out.SendMessage("You cannot use this while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (player.Mez)
            {
                player.Out.SendMessage("You cannot use this while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (player.Stun)
            {
                player.Out.SendMessage("You cannot use this while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
			if (player.Sitting)
			{
				player.Out.SendMessage("You must be standing to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			//Cancel old berserk effects on player
			BerserkEffect berserk = (BerserkEffect) player.EffectList.GetOfType(typeof(BerserkEffect));
			if (berserk!=null)
			{
				berserk.Cancel(false);
				return;
			}

			player.DisableSkill(ab, REUSE_TIMER);

			new BerserkEffect().Start(player);
		}                       
    }
}
