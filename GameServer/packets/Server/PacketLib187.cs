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
using log4net;
using DOL.GS.Quests;
using System.Reflection;
using DOL.Database;
using System.Collections.Generic;

namespace DOL.GS.PacketHandler
{
	[PacketLib(187, GameClient.eClientVersion.Version187)]
	public class PacketLib187 : PacketLib186
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.87 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib187(GameClient client)
			: base(client)
		{
		}

		public override void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, true);
		}

		public override void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, false);
		}

		protected override void SendQuestWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest,
			bool offer)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.Dialog));
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
			pak.WritePascalString(quest.Summary);
			if (offer)
			{
				pak.WriteShort((ushort)quest.Story.Length);
				pak.WriteStringBytes(quest.Story);
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
			pak.WriteByte((byte)quest.Level);
			pak.WriteByte((byte)quest.Rewards.MoneyPercent);
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

		protected virtual void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			pak.WriteByte((byte)template.Level);

			int value1;
			int value2;

			switch (template.Object_Type)
			{
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
				case (int)eObjectType.GenericItem:
					value1 = count; // Count
					value2 = template.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = template.DPS_AF;
					value2 = count; // Count
					break;
				case (int)eObjectType.Instrument:
					value1 = (template.DPS_AF == 2 ? 0 : template.DPS_AF);
					value2 = 0;
					break;
				case (int)eObjectType.Shield:
					value1 = template.Type_Damage;
					value2 = template.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = template.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = template.DPS_AF;
					value2 = template.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (template.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(template.DPS_AF));
			else
				pak.WriteByte((byte)(template.Hand << 6));
			pak.WriteByte((byte)((template.Type_Damage > 3
				? 0
				: template.Type_Damage << 6) | template.Object_Type));
			pak.WriteShort((ushort)template.Weight);
			pak.WriteByte(template.BaseConditionPercent);
			pak.WriteByte(template.BaseDurabilityPercent);
			pak.WriteByte((byte)template.Quality);
			pak.WriteByte((byte)template.Bonus);
			pak.WriteShort((ushort)template.Model);
			pak.WriteByte((byte)template.Extension);
			if (template.Emblem != 0)
				pak.WriteShort((ushort)template.Emblem);
			else
				pak.WriteShort((ushort)template.Color);
			pak.WriteShort((ushort)template.Effect);
			if (count > 1)
				pak.WritePascalString(String.Format("{0} {1}", count, template.Name));
			else
				pak.WritePascalString(template.Name);
		}

		public override void SendQuestListUpdate()
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;

			SendTaskInfo();

			int questIndex = 1;
			lock (m_gameClient.Player.QuestList)
				foreach (AbstractQuest quest in m_gameClient.Player.QuestList)
					SendQuestPacket((quest.Step == -1) ? null : quest, questIndex++);
			while (questIndex <= 25)
				SendQuestPacket(null, questIndex++);
		}

		protected override void SendQuestPacket(AbstractQuest q, int index)
		{
			if (q == null || !(q is RewardQuest))
			{
				base.SendQuestPacket(q, index);
				return;
			}

			RewardQuest quest = q as RewardQuest;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));
			pak.WriteByte((byte)index);
			pak.WriteByte((byte)quest.Name.Length);
			pak.WriteShort(0x00); // unknown
			pak.WriteByte((byte)quest.Goals.Count);
			pak.WriteByte((byte)quest.Level);
			pak.WriteStringBytes(quest.Name);
			pak.WritePascalString(quest.Description);
			foreach (RewardQuest.QuestGoal goal in quest.Goals)
			{
				String goalDesc = String.Format("{0}\r", goal.Description);
				pak.WriteShortLowEndian((ushort)goalDesc.Length);
				pak.WriteStringBytes(goalDesc);
				pak.WriteShortLowEndian((ushort)goal.ZoneID2);
				pak.WriteShortLowEndian((ushort)goal.XOffset2);
				pak.WriteShortLowEndian((ushort)goal.YOffset2);
				pak.WriteShortLowEndian(0x00);	// unknown
				pak.WriteShortLowEndian((ushort)goal.Type);
				pak.WriteShortLowEndian(0x00);	// unknown
				pak.WriteShortLowEndian((ushort)goal.ZoneID1);
				pak.WriteShortLowEndian((ushort)goal.XOffset1);
				pak.WriteShortLowEndian((ushort)goal.YOffset1);
				pak.WriteByte((byte)((goal.IsAchieved) ? 0x01 : 0x00));
				if (goal.QuestItem == null)
					pak.WriteByte(0x00);
				else
				{
					pak.WriteByte(0x01);
					WriteTemplateData(pak, goal.QuestItem, 1);
				}
			}
			SendTCP(pak);
		}
	}
}
