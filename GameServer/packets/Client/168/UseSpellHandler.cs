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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handles spell cast requests from client
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.UseSpell, ClientStatus.PlayerInGame)]
	public class UseSpellHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int flagSpeedData = packet.ReadShort();
			int heading = packet.ReadShort();

			if (client.Version > GameClient.eClientVersion.Version171)
			{
				int xOffsetInZone = packet.ReadShort();
				int yOffsetInZone = packet.ReadShort();
				int currentZoneID = packet.ReadShort();
				int realZ = packet.ReadShort();

				Zone newZone = WorldMgr.GetZone((ushort) currentZoneID);
				if (newZone == null)
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Unknown zone in UseSpellHandler: " + currentZoneID + " player: " + client.Player.Name);
				}
				else
				{
					client.Player.X = newZone.XOffset + xOffsetInZone;
					client.Player.Y = newZone.YOffset + yOffsetInZone;
					client.Player.Z = realZ;
					client.Player.MovementStartTick = Environment.TickCount;
				}
			}

			int spellLevel = packet.ReadByte();
			int spellLineIndex = packet.ReadByte();

			client.Player.Heading = (ushort) (heading & 0xfff);

			new UseSpellAction(client.Player, flagSpeedData, spellLevel, spellLineIndex).Start(1);
		}

		#endregion

		#region Nested type: UseSpellAction

		/// <summary>
		/// Handles player use spell actions
		/// </summary>
		protected class UseSpellAction : RegionAction
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// The speed and flags data
			/// </summary>
			protected readonly int m_flagSpeedData;

			/// <summary>
			/// The used spell level
			/// </summary>
			protected readonly int m_spellLevel;

			/// <summary>
			/// The used spell line index
			/// </summary>
			protected readonly int m_spellLineIndex;

			/// <summary>
			/// Constructs a new UseSpellAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="flagSpeedData">The speed and flags data</param>
			/// <param name="spellLevel">The used spell level</param>
			/// <param name="spellLineIndex">The used spell line index</param>
			public UseSpellAction(GamePlayer actionSource, int flagSpeedData, int spellLevel, int spellLineIndex)
				: base(actionSource)
			{
				m_flagSpeedData = flagSpeedData;
				m_spellLevel = spellLevel;
				m_spellLineIndex = spellLineIndex;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				if ((m_flagSpeedData & 0x200) != 0)
				{
					player.CurrentSpeed = (short)(-(m_flagSpeedData & 0x1ff)); // backward movement
				}
				else
				{
					player.CurrentSpeed = (short)(m_flagSpeedData & 0x1ff); // forward movement
				}
				player.IsStrafing = (m_flagSpeedData & 0x4000) != 0;
				player.TargetInView = (m_flagSpeedData & 0xa000) != 0; // why 2 bits? that has to be figured out
				player.GroundTargetInView = ((m_flagSpeedData & 0x1000) != 0);

				IList spelllines = player.GetSpellLines();
				Spell castSpell = null;
				SpellLine castLine = null;
				lock (spelllines.SyncRoot)
				{
					if (m_spellLineIndex < spelllines.Count)
					{
						castLine = (SpellLine) spelllines[m_spellLineIndex];
						List<Spell> spells = SkillBase.GetSpellList(castLine.KeyName);
						foreach (Spell spell in spells)
						{
							if (spell.Level == m_spellLevel)
							{
								castSpell = spell;
								break;
							}
						}
					}
				}
				if (castSpell != null)
				{
					player.CastSpell(castSpell, castLine);
					return;
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Client <" + player.Client.Account.Name + "> requested incorrect spell at level " + m_spellLevel +
						         " in spell-line " + castLine.Name);
				}
				if (castLine == null)
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Client <" + player.Client.Account.Name + "> requested incorrect spell-line index");
				}
			}
		}

		#endregion
	}
}