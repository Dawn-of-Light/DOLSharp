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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.UseSpell, "Handles Player Use Spell Request.", eClientStatus.PlayerInGame)]
	public class UseSpellHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int flagSpeedData;
			int spellLevel;
			int spellLineIndex;
			if (client.Version >= GameClient.eClientVersion.Version1124)
			{
				client.Player.X = (int)packet.ReadFloatLowEndian();
				client.Player.Y = (int)packet.ReadFloatLowEndian();
				client.Player.Z = (int)packet.ReadFloatLowEndian();
				client.Player.CurrentSpeed = (short)packet.ReadFloatLowEndian();
				client.Player.Heading = packet.ReadShort();
				flagSpeedData = packet.ReadShort(); // target visible ? 0xA000 : 0x0000
				spellLevel = packet.ReadByte();
				spellLineIndex = packet.ReadByte();
				// two bytes at end, not sure what for
			}
			else
			{
				flagSpeedData = packet.ReadShort();
				int heading = packet.ReadShort();

				if (client.Version > GameClient.eClientVersion.Version171)
				{
					int xOffsetInZone = packet.ReadShort();
					int yOffsetInZone = packet.ReadShort();
					int currentZoneID = packet.ReadShort();
					int realZ = packet.ReadShort();

					Zone newZone = WorldMgr.GetZone((ushort)currentZoneID);
					if (newZone == null)
					{
						Log.Warn($"Unknown zone in UseSpellHandler: {currentZoneID} player: {client.Player.Name}");
					}
					else
					{
						client.Player.X = newZone.XOffset + xOffsetInZone;
						client.Player.Y = newZone.YOffset + yOffsetInZone;
						client.Player.Z = realZ;
						client.Player.MovementStartTick = Environment.TickCount;
					}
				}

				spellLevel = packet.ReadByte();
				spellLineIndex = packet.ReadByte();

				client.Player.Heading = (ushort)(heading & 0xfff);
			}

			new UseSpellAction(client.Player, flagSpeedData, spellLevel, spellLineIndex).Start(1);
		}

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
				GamePlayer player = (GamePlayer)m_actionSource;

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

				List<Tuple<SpellLine, List<Skill>>> snap = player.GetAllUsableListSpells();
				Skill sk = null;
				SpellLine sl = null;
				
				// is spelline in index ?
				if (m_spellLineIndex < snap.Count)
				{
					int index = snap[m_spellLineIndex].Item2.FindIndex(s => s is Spell ? 
					                                                   s.Level == m_spellLevel 
					                                                   : (s is Styles.Style ? ((Styles.Style)s).SpecLevelRequirement == m_spellLevel
					                                                      : (s is Ability ? ((Ability)s).SpecLevelRequirement == m_spellLevel : false)));
					
					if (index > -1)
					{
						sk = snap[m_spellLineIndex].Item2[index];
					}
					
					sl = snap[m_spellLineIndex].Item1;
				}
				
				if (sk is Spell && sl != null)
				{
					player.CastSpell((Spell)sk, sl);
				}
				else if (sk is Styles.Style)
				{
					player.ExecuteWeaponStyle((Styles.Style)sk);
				}
				else if (sk is Ability)
				{
					Ability ab = (Ability)sk;
					IAbilityActionHandler handler = SkillBase.GetAbilityActionHandler(ab.KeyName);
					if (handler != null)
					{
						handler.Execute(ab, player);
					}
					
					ab.Execute(player);
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Client <" + player.Client.Account.Name + "> requested incorrect spell at level " + m_spellLevel +
							" in spell-line " + ((sl == null || sl.Name == null) ? "unkown" : sl.Name));
					
					player.Out.SendMessage(string.Format("Error : Spell (Line {0}, Level {1}) can't be resolved...", m_spellLineIndex, m_spellLevel), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				}
				
			}
		}
	}
}