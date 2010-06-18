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
using DOL.GS.Quests;
using DOL.Database;
using log4net;
using DOL.GS.Behaviour;

namespace DOL.GS.PacketHandler
{
	[PacketLib(194, GameClient.eClientVersion.Version194)]
	public class PacketLib194 : PacketLib193
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected override void SendQuestWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest,	bool offer)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog));
			ushort QuestID = QuestMgr.GetIDForQuestType(quest.GetType());
			pak.WriteShort((offer) ? (byte)0x22 : (byte)0x21); // Dialog
			pak.WriteShort(QuestID);
			pak.WriteShort((ushort)questNPC.ObjectID);
			pak.WriteByte(0x00); // unknown
			pak.WriteByte(0x00); // unknown
			pak.WriteByte(0x00); // unknown
			pak.WriteByte(0x00); // unknown
			pak.WriteByte((offer) ? (byte)0x02 : (byte)0x01); // Accept/Decline or Finish/Not Yet
			pak.WriteByte(0x01); // Wrap
			pak.WritePascalString(quest.Name);

			String personalizedSummary = BehaviourUtils.GetPersonalizedMessage(quest.Summary, player);
			if (personalizedSummary.Length > 255)
				pak.WritePascalString(personalizedSummary.Substring(0, 255)); // Summary is max 255 bytes !
			else
				pak.WritePascalString(personalizedSummary);
			if (offer)
			{
				String personalizedStory = BehaviourUtils.GetPersonalizedMessage(quest.Story, player);
				pak.WriteShort((ushort)personalizedStory.Length);
				pak.WriteStringBytes(personalizedStory);
			}
			else
			{
				pak.WriteShort((ushort)quest.Conclusion.Length);
				pak.WriteStringBytes(quest.Conclusion);
			}
			pak.WriteShort(QuestID);
			pak.WriteByte((byte)quest.Goals.Count); // #goals count
			foreach (RewardQuest.QuestGoal goal in quest.Goals)
			{
				pak.WritePascalString(String.Format("{0}\r", goal.Description));
			}
			pak.WriteInt((uint)(quest.Rewards.Money)); // unknown, new in 1.94
		//	pak.WriteByte((byte)quest.Level);
		//	pak.WriteByte((byte)quest.Rewards.MoneyPercent);
			pak.WriteByte((byte)quest.Rewards.ExperiencePercent(player));
			pak.WriteByte((byte)quest.Rewards.BasicItems.Count);
			foreach (ItemTemplate reward in quest.Rewards.BasicItems)
				WriteTemplateData(pak, reward, 1);
			pak.WriteByte((byte)quest.Rewards.ChoiceOf);
			pak.WriteByte((byte)quest.Rewards.OptionalItems.Count);
			foreach (ItemTemplate reward in quest.Rewards.OptionalItems)
				WriteTemplateData(pak, reward, 1);
			SendTCP(pak);
		}

		/// <summary>
		/// Constructs a new PacketLib for Version 1.94 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib194(GameClient client)
			: base(client)
		{
		}
	}
}
