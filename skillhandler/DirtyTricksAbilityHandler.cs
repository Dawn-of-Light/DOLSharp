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
using System.Collections;
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
	[SkillHandlerAttribute(Abilities.DirtyTricks)]
	public class DirtyTricksAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
					log.Warn("Could not retrieve player in DirtyTricksAbilityHandler.");
				return;
			}

			if (!player.Alive)
			{
				player.Out.SendMessage("You are dead and can't use that ability!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			SpellLine abilitiesLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
			if (abilitiesLine != null)
			{
				IList spells = SkillBase.GetSpellList(abilitiesLine.KeyName);
				if (spells != null)
				{
					foreach (Spell spell in spells) 
					{
						if (spell.ID == ab.Icon)
						{
							player.CastSpell(spell, abilitiesLine);
							player.DisableSkill(ab, spell.RecastDelay);
							break;
						}
					}
				}
			}
		}                       
	}
}
