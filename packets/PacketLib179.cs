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
#define NOENCRYPTION
using System;
using System.Reflection;
using DOL.GS.PlayerTitles;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(179, GameClient.eClientVersion.Version179)]
	public class PacketLib179 : PacketLib178
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.79 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib179(GameClient client):base(client)
		{
		}

		public override void SendUpdatePlayer()
		{
			GamePlayer player = m_gameClient.Player;
			if (player == null)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VariousUpdate));
			pak.WriteByte(0x03); //subcode
			pak.WriteByte(0x0f); //number of entry
			pak.WriteByte(0x00); //subtype
			pak.WriteByte(0x00); //unk
			//entry :

			pak.WriteByte(player.Level); //level
			pak.WritePascalString(player.Name);

			pak.WriteByte((byte) (player.MaxHealth >> 8)); // maxhealth high byte ?
			pak.WritePascalString(player.CharacterClass.Name); // class name
			pak.WriteByte((byte) (player.MaxHealth & 0xFF)); // maxhealth low byte ?

			pak.WritePascalString( /*"The "+*/player.CharacterClass.Profession); // Profession

			pak.WriteByte(0x00); //unk

			pak.WritePascalString(player.CharacterClass.GetTitle(player.Level));

			//todo make function to calcule realm rank
			//client.Player.RealmPoints
			//todo i think it s realmpoint percent not realrank
			pak.WriteByte((byte) player.RealmLevel); //urealm rank
			pak.WritePascalString(player.RealmTitle);
			pak.WriteByte((byte) player.RealmSpecialtyPoints); // realm skill points

			pak.WritePascalString(player.CharacterClass.BaseName); // base class

			pak.WriteByte((byte) (player.PlayerCharacter.LotNumber >> 8)); // personal house high byte
			pak.WritePascalString(player.GuildName);
			pak.WriteByte((byte) (player.PlayerCharacter.LotNumber & 0xFF)); // personal house low byte

			pak.WritePascalString(player.LastName);

			pak.WriteByte(0x0); // ML Level
			pak.WritePascalString(player.RaceName);

			pak.WriteByte(0x0);
			if (player.GuildRank != null)
				pak.WritePascalString(player.GuildRank.Title);
			else
				pak.WritePascalString("");
			pak.WriteByte(0x0);

			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(player.CraftingPrimarySkill);
			if (skill != null)
				pak.WritePascalString(skill.Name); //crafter guilde: alchemist
			else
				pak.WritePascalString("None"); //no craft skill at start

			pak.WriteByte(0x0);
			pak.WritePascalString(player.CraftTitle); //crafter title: legendary alchemist

			pak.WriteByte(0x0);
			pak.WritePascalString("None"); //ML title

			// new in 1.75
			pak.WriteByte(0x0);
			string title = "None";
			if (player.CurrentTitle != PlayerTitleMgr.ClearTitle)
				title = player.CurrentTitle.GetValue(player);
			pak.WritePascalString(title); // new in 1.74

			// new in 1.79
			pak.WriteByte(0x0); // Champion Level
			pak.WritePascalString("None"); // Champion Title
			SendTCP(pak);
		}
	}
}
