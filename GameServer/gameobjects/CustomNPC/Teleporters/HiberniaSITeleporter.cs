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
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.Spells;
using log4net;
using System.Reflection;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Hibernia SI teleporter.
	/// </summary>
	/// <author>Aredhel</author>
	public class HiberniaSITeleporter : GameTeleporter
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private String[] m_destination = { 
			"Grove of Domnann",
			"Droighaid",
			"Aalid Feie",
			"Necht" };

		/// <summary>
		/// Player right-clicked the teleporter.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			List<String> playerAreaList = new List<String>();
			foreach (AbstractArea area in player.CurrentAreas)
				playerAreaList.Add(area.Description);

			SayTo(player, "Greetings. Where can I send you?");
			foreach (String destination in m_destination)
				if (!playerAreaList.Contains(destination))
					player.Out.SendMessage(String.Format("[{0}]", destination),
						eChatType.CT_Say, eChatLoc.CL_PopupWindow);

			return true;
		}

		/// <summary>
		/// Player has picked a destination.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
		{
			// Not porting to where we already are.

			List<String> playerAreaList = new List<String>();
			foreach (AbstractArea area in player.CurrentAreas)
				playerAreaList.Add(area.Description);

			if (playerAreaList.Contains(destination.TeleportID))
				return;

			switch (destination.TeleportID.ToLower())
			{
				case "grove of domnann":
					break;
				case "droighaid":
					break;
				case "aalid feie":
					break;
				case "necht":
					break;
				default:
					return;
			}

			SayTo(player, "Have a safe journey!");
			base.OnDestinationPicked(player, destination);
		}

		/// <summary>
		/// Teleport the player to the designated coordinates.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnTeleport(GamePlayer player, Teleport destination)
		{
			SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
			IList spellList = SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells);
			foreach (Spell spell in spellList)
			{
				if (spell.SpellType == "UniPortal")
				{
					TargetObject = player;
					UniPortal portalHandler = new UniPortal(this, spell, spellLine, destination);
					portalHandler.CastSpell();
					return;
				}
			}

			// Spell not found in the database, fall back on default procedure.

			log.Warn("UniPortal spell not found");
			base.OnTeleport(player, destination);
		}
	}
}
