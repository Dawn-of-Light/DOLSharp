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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&where", (uint) ePrivLevel.Player, "Ask where an NPC is from Guards", "/where")]
	public class WhereCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /where <Npc Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			GameNPC targetnpc = client.Player.TargetObject as GameNPC;
			if (targetnpc != null && CheckTargetIsGuard(targetnpc))
			{
				string name = String.Join(" ", args, 1, args.Length - 1);
				GameNPC[] npcs = WorldMgr.GetNPCsByNameFromRegion(name, (ushort)client.Player.RegionId, (eRealm) client.Player.Realm);
				if (npcs == null || npcs.Length <= 0)
				{
					targetnpc.SayTo(client.Player, "Sorry, i do not know this person.");
					return 0;
				}
				GameNPC npc = npcs[0];
				ushort heading = targetnpc.Position.GetHeadingTo(npc.Position);
				string directionstring = GetDirectionFromHeading(heading);
				targetnpc.SayTo(client.Player, eChatLoc.CL_SystemWindow, npc.Name + " is in the " + directionstring);
				targetnpc.TurnTo(npc, 10000);
				targetnpc.Emote(eEmote.Point);
				return 1;
			}
			return 1;
		}

		public bool CheckTargetIsGuard(GameLiving target)
		{
			if (target is GamePlayer)
				return false;

			if (target.Realm == 0)
				return false;

			String name = target.Name;

			if (name.IndexOf("Guard") >= 0)
			{
				if (name == "Guardian")
					return false;
				if (name == "Guardian Sergeant")
					return false;
				if (name.EndsWith("Guardian"))
					return false;
				if (name.StartsWith("Guardian of the"))
					return false;

				if (name == "Guard")
					return false;
				if (name.EndsWith("Guard"))
					return false;

				if (name == "Guardsman")
					return false;
				if (name.EndsWith("Guardsman"))
					return false;

				if (name == "Guard's Armorer")
					return false;

				return true;
			}

			if (name.StartsWith("Sir ") && (target.GuildName == null || target.GuildName == ""))
			{
				return true;
			}

			if (name.StartsWith("Captain ") && (target.GuildName == null || target.GuildName == ""))
			{
				return true;
			}

			if (name.StartsWith("Jarl "))
			{
				return true;
			}

			if (name.StartsWith("Lady ") && (target.GuildName == null || target.GuildName == ""))
			{
				return true;
			}
			if (name.StartsWith("Soldier ") || name.StartsWith("Soldat "))
				return true;

			if (name.StartsWith("Sentinel "))
			{
				if (name.EndsWith("Runes"))
					return false;
				if (name.EndsWith("Kynon"))
					return false;

				return true;
			}


			if (name.IndexOf("Viking") >= 0)
			{
				if (name.EndsWith("Archer"))
					return false;
				if (name.EndsWith("Dreng"))
					return false;
				if (name.EndsWith("Huscarl"))
					return false;
				if (name.EndsWith("Jarl"))
					return false;

				return true;
			}

			if (name.StartsWith("Huntress "))
			{
				return true;
			}
			return false;
		}

		public string GetDirectionFromHeading(ushort heading)
		{
			if (heading < 0)
				heading += 4096;
			if (heading >= 3840 || heading <= 256)
				return "South";
			else if (heading > 256 && heading < 768)
				return "South West";
			else if (heading >= 768 && heading <= 1280)
				return "West";
			else if (heading > 1280 && heading < 1792)
				return "North West";
			else if (heading >= 1792 && heading <= 2304)
				return "North";
			else if (heading > 2304 && heading < 2816)
				return "North East";
			else if (heading >= 2816 && heading <= 3328)
				return "East";
			else if (heading > 3328 && heading < 3840)
				return "South East";
			return "";
		}
	}
}