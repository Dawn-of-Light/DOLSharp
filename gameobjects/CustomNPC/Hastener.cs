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
using DOL.Database;
using DOL.Database.Attributes;
using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.GS
{
	public class GameHastener : GameNPC
	{
		public GameHastener() : base() { }
		public GameHastener(INpcTemplate template) : base(template) { }

		public const int SPEEDOFTHEREALMID = 2430;
		private const int STROFTHEREALMID = 2431;

		public override bool Interact(GamePlayer player)
		{
			if (player != null)
			{
				string message2 = "";
				string message3 = "  I can also grant you extra [strength] to help you carry your wares throughout the city.";
				string message4 = "";
				switch (player.CurrentZone.Description)
				{
					case "City of Camelot":
					case "Tir Na Nog":
					case "Jordheim":
					case "Isle of Glass":
					case "Domnann":
					case "Aegir's Landing":
						message2 = "  I am here to assist with travel.  Would you like to increase your rate of [movement] for traveling within our city?";
						message4 = "  If you have not achieved your tenth season I can transport you to the [borderkeep] so that you may assist in battleground combat.";
						break;
					default:
						message2 = "  I am here to assist with travel across our fair lands.  Just say the word and I will gladly increase your rate of [movement] to aid your adventures!";
						break;
				}
				SayTo(player, string.Format("Greetings, {0}.{1}{2}{3}", player.CharacterClass.Name, message2, player.CurrentRegion.IsCapitalCity ? message3 : "", message4));
				return true;
			}
			return base.Interact(player);
		}

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (source != null && source is GamePlayer && ((source as GamePlayer).Realm == this.Realm || !CurrentRegion.IsRvR))
			{
				GamePlayer player = source as GamePlayer;
				switch (str.ToLower())
				{
					case "movement":
						TargetObject = this;
						CastSpell(SkillBase.GetSpellByID(SPEEDOFTHEREALMID), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
						break;
					case "strength":
						if (player.CurrentRegion.IsCapitalCity)
						{
							TargetObject = this;
							CastSpell(SkillBase.GetSpellByID(STROFTHEREALMID), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
						}
						break;
					case "borderkeep":
						if ((player.CurrentRegion.IsCapitalCity || IsSICity(player.CurrentZone.ID) && player.Level < 10))
						{
							AbstractGameKeep portalKeep = KeepMgr.GetBGPK(player);
							if (portalKeep != null)
							{
								player.MoveTo((ushort)portalKeep.Region, portalKeep.X, portalKeep.Y, portalKeep.Z, (ushort)portalKeep.Heading);
								return true;
							}
						}
						break;
				}
				return true;
			}
			return base.WhisperReceive(source, str);
		}

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add(string.Format("You examine {0}. {1} is {2}.", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
			return list;
		}

		private bool IsSICity(int zoneID)
		{
			switch (zoneID)
			{
				case 51: //Isle of Glass
				case 151: //Aegir's Landing
				case 181: //Domnann
					return true;
			}
			return false;
		}
	}
}