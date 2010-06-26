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

// Eden - Darwin 06/10/2008 - Complete /stats

using System;
using System.Reflection;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL;
using System.Collections;
using DOL.Database;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&stats",
		ePrivLevel.Player,
		"Displays player statistics",
		"/stats [ top | rp | kills | deathblows | irs | heal | rez | <name> ]")]

	public class StatsCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static bool hasbeenrun = false;
		private static long LastUpdatedTime = 0;
		private static long TimeToChange = 0;
		private static string staff = "48c93337-e37b-49dc-97a1-c110ece96f8b"; //don't show player stats from staff guild ID

		public void OnCommand(GameClient client, string[] args)
		{
			if (client == null) return;
			if (LastUpdatedTime == 0) { LastUpdatedTime = client.Player.CurrentRegion.Time; }
			TimeToChange = (LastUpdatedTime + 60000); //1minutes between list refreshing
			if (!(client.Player.CurrentRegion.Time <= TimeToChange && hasbeenrun == true)) CreateStats(client);

			if (args.Length > 1)
			{
				switch (args[1].ToString())
				{
					case "top":
						client.Out.SendCustomTextWindow("Top 20 Players", toplist);
						client.Out.SendCustomTextWindow("Top 20 Players", toplist); //dono why send 2 times avoid blank window sometimes
						break;
					case "rp":
						client.Player.Out.SendMessage("Top 20 for Realm Points\n" + statsrp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "lrp":
						client.Player.Out.SendMessage("Top 20 for RP / Hour\n" + statslrp, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "kills":
						client.Player.Out.SendMessage("Top 20 Killer\n" + statskills, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "deathblows":
						client.Player.Out.SendMessage("Top 20 Deathblower\n" + statsdeath, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "irs":
						client.Player.Out.SendMessage("Top 20 \"I Remain Standing\"\n" + statsirs, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "heal":
						client.Player.Out.SendMessage("Top 20 Healer\n" + statsheal, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "rez":
						client.Player.Out.SendMessage("Top 20 Resurrectors\n" + statsres, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					default:
						GameClient clientc = WorldMgr.GetClientByPlayerName(args[1].ToString(), false, true);
						if (clientc == null)
						{ 
							client.Player.Out.SendMessage("No player with name " + args[1].ToString() + " found!", eChatType.CT_System, eChatLoc.CL_SystemWindow); 
							return; 
						}

						if (clientc.Player.StatsAnonFlag) 
						{ 
							client.Player.Out.SendMessage(args[1].ToString() + " doesn't want you to view his stats.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						DisplayMessage(client, PlayerStatistic.GetStatsMessage(clientc.Player));
						break;
				}
			}
			else
				DisplayMessage(client, PlayerStatistic.GetStatsMessage(client.Player));
		}

		private static IList<string> toplist = new List<string>();
		public class StatToCount
		{
			public string name; public uint count;
			public StatToCount(string name, uint count) { this.name = name; this.count = count; }
		}
		private static string statsrp;
		private static string statslrp;
		private static string statskills;
		private static string statsdeath;
		private static string statsirs;
		private static string statsheal;
		private static string statsres;

		private void CreateStats(GameClient client)
		{
			if (client == null) return;
			LastUpdatedTime = client.Player.CurrentRegion.Time;

			#region /stats top
			var chars = GameServer.Database.SelectObjects<DOLCharacters>("RealmPoints > 213881 AND GuildID!='" + staff + "' ORDER BY RealmPoints DESC LIMIT 30");
			if (toplist != null) toplist.Clear();
			int count = 1;
			foreach (DOLCharacters chr in chars)
			{
				toplist.Add( "\n" + count.ToString() + " - [ " + chr.Name + " ] with " + String.Format( "{0:0,0}", chr.RealmPoints ) + " RP - [ " + ( ( ( chr.RealmLevel + 10 ) / 10 ) + "L" + ( ( chr.RealmLevel + 10 ) % 10 ) ) + " ]" );
				count++; if (count > 20) break;
			}
			#endregion /stats top

			List<StatToCount> allstatsrp = new List<StatToCount>();
			List<StatToCount> allstatslrp = new List<StatToCount>();
			List<StatToCount> allstatskills = new List<StatToCount>();
			List<StatToCount> allstatsdeath = new List<StatToCount>();
			List<StatToCount> allstatsirs = new List<StatToCount>();
			List<StatToCount> allstatsheal = new List<StatToCount>();
			List<StatToCount> allstatsres = new List<StatToCount>();

			foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
			{
				if (clients == null) continue;
				PlayerStatistic stats = PlayerStatistic.GetStatistic(clients.Player);
				if (clients.Player.RealmLevel > 31)
				{
					allstatsrp.Add(new StatToCount(clients.Player.Name, stats.TotalRP));
					TimeSpan onlineTime = DateTime.Now.Subtract(stats.LoginTime);
					allstatslrp.Add(new StatToCount(clients.Player.Name, (uint)Math.Round(stats.RPsPerHour(stats.TotalRP, onlineTime))));
					uint deaths = stats.Deaths; if (deaths < 1) deaths = 1;
					allstatsirs.Add(new StatToCount(clients.Player.Name, (uint)Math.Round((double)stats.TotalRP / (double)deaths)));
				}
				allstatskills.Add(new StatToCount(clients.Player.Name, stats.KillsThatHaveEarnedRPs));
				allstatsdeath.Add(new StatToCount(clients.Player.Name, stats.Deathblows));
				allstatsheal.Add(new StatToCount(clients.Player.Name, stats.HitPointsHealed));
				allstatsres.Add(new StatToCount(clients.Player.Name, stats.RessurectionsPerformed));
			}
			allstatsrp.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsrp.Reverse();
			allstatslrp.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatslrp.Reverse();
			allstatskills.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatskills.Reverse();
			allstatsdeath.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsdeath.Reverse();
			allstatsirs.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsirs.Reverse();
			allstatsheal.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsheal.Reverse();
			allstatsres.Sort((ctc1, ctc2) => ctc1.count.CompareTo(ctc2.count)); allstatsres.Reverse();

			statsrp = ""; statslrp = ""; statskills = ""; statsdeath = ""; statsirs = ""; statsheal = ""; statsres = "";
			for (int c = 0; c < allstatsrp.Count; c++) { if (c > 19 || allstatsrp[c].count < 1) break; statsrp += (c + 1) + ". " + allstatsrp[c].name + " with " + allstatsrp[c].count.ToString() + " RP\n"; }
			for (int c = 0; c < allstatslrp.Count; c++) { if (c > 19 || allstatslrp[c].count < 1) break; statslrp += (c + 1) + ". " + allstatslrp[c].name + " with " + allstatslrp[c].count.ToString() + " RP/hour\n"; }
			for (int c = 0; c < allstatskills.Count; c++) { if (c > 19 || allstatskills[c].count < 1) break; statskills += (c + 1) + ". " + allstatskills[c].name + " with " + allstatskills[c].count.ToString() + " kills\n"; }
			for (int c = 0; c < allstatsdeath.Count; c++) { if (c > 19 || allstatsdeath[c].count < 1) break; statsdeath += (c + 1) + ". " + allstatsdeath[c].name + " with " + allstatsdeath[c].count.ToString() + " deathblows\n"; }
			for (int c = 0; c < allstatsirs.Count; c++) { if (c > 19 || allstatsirs[c].count < 1) break; statsirs += (c + 1) + ". " + allstatsirs[c].name + " with " + allstatsirs[c].count.ToString() + " RP/death\n"; }
			for (int c = 0; c < allstatsheal.Count; c++) { if (c > 19 || allstatsheal[c].count < 1) break; statsheal += (c + 1) + ". " + allstatsheal[c].name + " with " + allstatsheal[c].count.ToString() + " HP\n"; }
			for (int c = 0; c < allstatsres.Count; c++) { if (c > 19 || allstatsres[c].count < 1) break; statsres += (c + 1) + ". " + allstatsres[c].name + " with " + allstatsres[c].count.ToString() + " res\n"; }

			hasbeenrun = true;
		}

	}
}