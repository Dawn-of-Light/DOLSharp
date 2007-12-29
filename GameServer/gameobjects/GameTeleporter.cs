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
using System.Collections;
using DOL.AI.Brain;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.Spells;
using log4net;
using System.Reflection;

namespace DOL.GS
{
	/// <summary>
	/// Base class for all teleporter type NPCs.
	/// </summary>
	/// <author>Aredhel</author>
	public class GameTeleporter : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GameTeleporter()
			: base() { }

		/// <summary>
		/// Turn the teleporter to face the player.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 10000);
			return true;
		}

		/// <summary>
		/// Talk to the teleporter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			// Battlegrounds is special, as the teleport location depends on
			// the level of the player, so let's deal with that first.

			if (text.ToLower() == "battlegrounds")
			{
				AbstractGameKeep portalKeep = KeepMgr.GetBGPK(player);
				if (portalKeep != null)
				{
					Teleport teleport = new Teleport();
					teleport.TeleportID = "battlegrounds";
					teleport.Realm = (byte)portalKeep.Realm;
					teleport.RegionID = portalKeep.Region;
					teleport.X = portalKeep.X;
					teleport.Y = portalKeep.Y;
					teleport.Z = portalKeep.Z;
					teleport.Heading = 0;
					OnDestinationPicked(player, teleport);
					return true;
				}
				else
				{
					if (player.Client.Account.PrivLevel > 1)
						player.Out.SendMessage("No portal keep found.",
							eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
					return true;
				}
			}

			// Find the teleport location in the database.

			List<Teleport> teleports = WorldMgr.GetTeleportLocations((eRealm)Realm);
			foreach (Teleport teleport in teleports)
			{
				if (teleport.TeleportID.ToLower() == text.ToLower())
				{
					if (teleport.RegionID == 0 &&
						teleport.X == 0 &&
						teleport.Y == 0 &&
						teleport.Z == 0)
						OnSubSelectionPicked(player, teleport);
					else
						OnDestinationPicked(player, teleport);
					return false;
				}
			}

			return true;	// Needs further processing.
		}

		/// <summary>
		/// Player has picked a destination.
		/// Override if you need the teleporter to say something to the player
		/// before porting him.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected virtual void OnDestinationPicked(GamePlayer player, Teleport destination)
		{
			OnTeleport(player, destination);
		}

		/// <summary>
		/// Player has picked a subselection.
		/// Override to pass teleport options on to the player.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="subSelection"></param>
		protected virtual void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
		{
		}

		/// <summary>
		/// Teleport the player to the designated coordinates using the
		/// portal spell.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected virtual void OnTeleportSpell(GamePlayer player, Teleport destination)
		{
			SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
			IList spellList = SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells);
			Spell spell = SkillBase.GetSpellByID(5999);	// UniPortal spell.

			if (spell != null)
			{
				TargetObject = player;
				UniPortal portalHandler = new UniPortal(this, spell, spellLine, destination);
				m_runningSpellHandler = portalHandler;
				portalHandler.CastSpell();
				return;
			}

			// Spell not found in the database, fall back on default procedure.

			if (player.Client.Account.PrivLevel > 1)
				player.Out.SendMessage("Uni-Portal spell not found.",
					eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

			player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z,
				(ushort)destination.Heading);
		}

		/// <summary>
		/// Teleport the player to the designated coordinates. 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected virtual void OnTeleport(GamePlayer player, Teleport destination)
		{
			player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z,
				(ushort)destination.Heading);
		}
	}
}