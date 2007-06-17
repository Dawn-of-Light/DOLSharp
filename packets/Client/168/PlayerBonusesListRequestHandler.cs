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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x62 ^ 168, "Handles player bonuses button clicks")]
	public class PlayerBonusesListRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static string ItemBonusDescription(int iBonus, int iBonusType)
		{
			/*Not needed.
			string BonusName;
			if (iBonusType == (int)eProperty.Stat_First)
				BonusName = "Strength";
			else if (iBonusType == (int)eProperty.Stat_Last)
				BonusName = "Charisma";
			else if (iBonusType == (int)eProperty.Resist_First)
				BonusName = "Resist Body";
			else if (iBonusType == (int)eProperty.Resist_Last)
				BonusName = "Resist Thrust";
			else if (iBonusType == (int)eProperty.Skill_First)
				BonusName = "Two Handed";
			else if (iBonusType == (int)eProperty.Skill_Last)
				BonusName = "Scythe";
			else if (!Enum.IsDefined(typeof(eProperty), (eProperty)iBonusType)) BonusName = iBonusType.ToString();
            
			string BonusName = ((eProperty)iBonusType).ToString();
			string str = BonusName.ToString().Replace("_", " ") + ": ";
			return str + iBonus + (((iBonusType < 20) && (iBonusType > 10)) ? "%" : "");
			*/
			//This displays the bonuses just like the Live servers. there is a check against the pts/% differences
			string str = ((iBonusType == 150) | (iBonusType == 151) | (iBonusType == 186)) ? "pts of " : "% to ";  //150(Health Regen) 151(PowerRegen) and 186(Style reductions) need the prefix of "pts of " to be correct
			return "+" + iBonus + str + SkillBase.GetPropertyName(((eProperty)iBonusType));
		}

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			if (code != 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("bonuses button: code is other than zero (" + code + ")");
			}

			/*
			<Begin Info: Bonuses (snapshot)>
			Resistances
			 Crush: +25%/+0%
			 Slash: +28%/+0%
			 Thrust: +28%/+0%
			 Heat: +25%/+0%
			 Cold: +25%/+0%
			 Matter: +26%/+0%
			 Body: +31%/+0%
			 Spirit: +21%/+0%
			 Energy: +26%/+0%

			Special Item Bonuses
			 +20% to Power Pool
			 +3% to all Melee Damage
			 +6% to all Stat Buff Spells
			 +23% to all Heal Spells
			 +3% to Melee Combat Speed
			 +10% to Casting Speed

			Realm Rank Bonuses
			 +7 to ALL Specs

			Relic Bonuses
			 +20% to all Melee Damage

			Outpost Bonuses
			 none

			<End Info>
			*/
			//AbilityBonus[(int)((eProperty)updateResists[i])]
			ArrayList info = new ArrayList();
			info.Add("Resistances");
			info.Add(string.Format(" Crush:   {0:+0;-0}%/\t{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Crush) - client.Player.AbilityBonus[(int)eProperty.Resist_Crush], client.Player.AbilityBonus[(int)eProperty.Resist_Crush]));
			info.Add(string.Format(" Slash:    {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Slash) - client.Player.AbilityBonus[(int)eProperty.Resist_Slash], client.Player.AbilityBonus[(int)eProperty.Resist_Slash]));
			info.Add(string.Format(" Thrust:  {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Thrust) - client.Player.AbilityBonus[(int)eProperty.Resist_Thrust], client.Player.AbilityBonus[(int)eProperty.Resist_Thrust]));
			info.Add(string.Format(" Heat:      {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Heat) - client.Player.AbilityBonus[(int)eProperty.Resist_Heat], client.Player.AbilityBonus[(int)eProperty.Resist_Heat]));
			info.Add(string.Format(" Cold:      {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Cold) - client.Player.AbilityBonus[(int)eProperty.Resist_Cold], client.Player.AbilityBonus[(int)eProperty.Resist_Cold]));
			info.Add(string.Format(" Matter:  {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Matter) - client.Player.AbilityBonus[(int)eProperty.Resist_Matter], client.Player.AbilityBonus[(int)eProperty.Resist_Matter]));
			info.Add(string.Format(" Body:     {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Body) - client.Player.AbilityBonus[(int)eProperty.Resist_Body], client.Player.AbilityBonus[(int)eProperty.Resist_Body]));
			info.Add(string.Format(" Spirit:    {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Spirit) - client.Player.AbilityBonus[(int)eProperty.Resist_Spirit], client.Player.AbilityBonus[(int)eProperty.Resist_Spirit]));
			info.Add(string.Format(" Energy: {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Energy) - client.Player.AbilityBonus[(int)eProperty.Resist_Energy], client.Player.AbilityBonus[(int)eProperty.Resist_Energy]));
			info.Add(" ");
			info.Add("Special Item Bonuses");
			GamePlayer player = client.Player;
			int[] bonusToBeDisplayed;//This is an Array of the bonuses that show up in the Bonuns Snapshot on Live, the only ones that really need to be there.
			bonusToBeDisplayed = new int[28] { 150, 151, 153, 154, 155, 173, 174, 179, 180, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200 };
			for (int i = 0; i < (int)eProperty.MaxProperty; i++)
			{
				if ((client.Player.ItemBonus[i] > 0) && ((Array.BinarySearch(bonusToBeDisplayed, i)) >= 0)) //Tiny edit here to add the binary serach to weed out the non essential bonuses
				{
					if (client.Player.ItemBonus[i] != 0) info.Add(ItemBonusDescription(client.Player.ItemBonus[i], i));
				}
			}
			info.Add(" ");
			info.Add("Realm Rank Bonuses");
			if (client.Player.RealmLevel > 10)
				info.Add(string.Format(" +{0} to ALL Specs", client.Player.RealmLevel / 10));
			else
				info.Add(" none");
			info.Add(" ");
			info.Add("Relic Bonuses");

			double meleeRelicBonus = RelicMgr.GetRelicBonusModifier(client.Player.Realm, eRelicType.Strength);
			double magicRelicBonus = RelicMgr.GetRelicBonusModifier(client.Player.Realm, eRelicType.Magic);

			if (meleeRelicBonus > 0.1) info.Add("Melee bonus: " + (magicRelicBonus * 100) + "%");
			if (magicRelicBonus > 0.1) info.Add("Magic bonus: " + (magicRelicBonus * 100) + "%");

			if (meleeRelicBonus == 0.1 && magicRelicBonus == 0.1)
				info.Add(" none");

			info.Add(" ");
			info.Add("Outpost Bonuses");
			info.Add("TODO, this needs to be written");
			client.Out.SendCustomTextWindow("Bonuses (snapshot)", info);

			return 1;
		}
	}
}
