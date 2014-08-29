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
 * created by Blues, based on works of Sumy and Vanatic
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&online",
		new string[] { "&on" },
		ePrivLevel.Player,
		"Shows all Players that are currently online",
		"Usage: /online")]
	public class OnlineCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		// ~~~~~~~~~~~~~~~~~~~~  CONFIG  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


		//set this to true or false to display additional info about connecting, disconnecting, playing clients.
		private static bool showAddOnlineInfo = true;

		//set this to true or false to show the realms population (eG: Albion: 13 34% 12Tanks | 1 Caster ... )
		private static bool showRealms = true;

		// set this to true or false to display players by zone (zone id must be added below)
		private static bool showByZone = true;
		//add the ID´s of the zones(!) you want the command to display here
		//get the ID´s from your Zone Table, keep in mind Zones != Regions.
		private static ushort[] zoneIDs = { 26, 120, 209 };

		// set this to true or false to display a displayed list of currently loged in classes.
		private static bool showDetailedClass = true;

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


		public class ClassToCount
		{
			public string name;
			public int count;

			public ClassToCount(string name, int count)
			{
				this.name = name;
				this.count = count;
			}
		}

		private List<ClassToCount> classcount = new List<ClassToCount>();



		public void OnCommand(GameClient client, string[] args)
		{
			bool bGM = client.Account.PrivLevel >= (uint)ePrivLevel.GM;
			bool sIP = false;
			
			if (bGM && args.Length == 2) {
				if (args[1].ToLower().Equals("sameip")) {
					sIP = true;
				}
			}
			List<string> textList = this.GetOnlineInfo(bGM, sIP);
			client.Out.SendCustomTextWindow("Currently Online", textList);
			return;
		}

		public List<string> GetOnlineInfo(bool bGM, bool sIP)
		{
			List<string> output = new List<string>();
			IList<GameClient> clients = WorldMgr.GetAllClients();
			
			Dictionary<string, List<GameClient>> dict = new Dictionary<string, List<GameClient>>();

			int gms = 0, connecting = 0, charscreen = 0, enterworld = 0,
				playing = 0, linkdeath = 0, disconnecting = 0;
			int midTanks = 0, hibTanks = 0, albTanks = 0, midCasters = 0,
				hibCasters = 0, albCasters = 0, midSupport = 0,
				hibSupport = 0, albSupport = 0, hibStealthers = 0,
				midStealthers = 0, albStealthers = 0;
			#region filling classcount list
			classcount.Clear();
			classcount.Add(new ClassToCount("Armsman", 0)); // 0
			classcount.Add(new ClassToCount("Mercenary", 0)); //1
			classcount.Add(new ClassToCount("Paladin", 0)); //2
			classcount.Add(new ClassToCount("Reaver", 0)); //3
			classcount.Add(new ClassToCount("Heretic", 0)); //4
			classcount.Add(new ClassToCount("Albion Mauler", 0)); //5
			classcount.Add(new ClassToCount("Cabalist", 0)); //6
			classcount.Add(new ClassToCount("Sorcerer", 0)); //7
			classcount.Add(new ClassToCount("Theurgist", 0)); //8
			classcount.Add(new ClassToCount("Wizard", 0)); //9
			classcount.Add(new ClassToCount("Necromancer", 0)); //10
			classcount.Add(new ClassToCount("Cleric", 0)); //11
			classcount.Add(new ClassToCount("Friar", 0)); //12
			classcount.Add(new ClassToCount("Minstrel", 0)); //13
			classcount.Add(new ClassToCount("Infiltrator", 0)); //14
			classcount.Add(new ClassToCount("Scout", 0)); //15
			classcount.Add(new ClassToCount("Berserker", 0)); //16
			classcount.Add(new ClassToCount("Savage", 0)); //17
			classcount.Add(new ClassToCount("Skald", 0)); //18
			classcount.Add(new ClassToCount("Thane", 0)); //19
			classcount.Add(new ClassToCount("Warrior", 0)); //20
			classcount.Add(new ClassToCount("Valkyrie", 0)); //21
			classcount.Add(new ClassToCount("Midgard Mauler", 0)); //22
			classcount.Add(new ClassToCount("Bonedancer", 0)); //23
			classcount.Add(new ClassToCount("Runemaster", 0)); //24
			classcount.Add(new ClassToCount("Spiritmaster", 0)); //25
			classcount.Add(new ClassToCount("Warlock", 0)); //26
			classcount.Add(new ClassToCount("Healer", 0)); //27
			classcount.Add(new ClassToCount("Shaman", 0)); //28
			classcount.Add(new ClassToCount("Hunter", 0)); //29
			classcount.Add(new ClassToCount("Shadowblade", 0)); //30
			classcount.Add(new ClassToCount("Blademaster", 0)); //31
			classcount.Add(new ClassToCount("Champion", 0)); //32
			classcount.Add(new ClassToCount("Hero", 0)); //33
			classcount.Add(new ClassToCount("Valewalker", 0)); //34
			classcount.Add(new ClassToCount("Hibernia Mauler", 0)); //35
			classcount.Add(new ClassToCount("Vampiir", 0)); //36
			classcount.Add(new ClassToCount("Eldritch", 0)); //37
			classcount.Add(new ClassToCount("Enchanter", 0)); //38
			classcount.Add(new ClassToCount("Mentalist", 0)); //39
			classcount.Add(new ClassToCount("Animist", 0)); //40
			classcount.Add(new ClassToCount("Bainshee", 0)); //41
			classcount.Add(new ClassToCount("Bard", 0)); //42
			classcount.Add(new ClassToCount("Druid", 0)); //43
			classcount.Add(new ClassToCount("Warden", 0)); //44
			classcount.Add(new ClassToCount("Nightshade", 0)); //45
			classcount.Add(new ClassToCount("Ranger", 0)); //46
			#endregion


			// Number of Alb, Mid and Hib tanks:
			foreach (GameClient c in clients)
			{
				if (c == null)
					continue;
				
				#region populate sameIP dictionary
				if (bGM && sIP) {
					if (!dict.ContainsKey(c.Account.LastLoginIP.ToString())) {
						dict.Add(c.Account.LastLoginIP.ToString(), new List<GameClient>());
					}
					
					dict[c.Account.LastLoginIP.ToString()].Add(c);
				}
				#endregion


				#region count GMs, and different client states
				if (c.ClientState == GameClient.eClientState.Connecting)
					++connecting;
				else if (c.ClientState == GameClient.eClientState.Disconnected)
					++disconnecting;
				else if (c.ClientState == GameClient.eClientState.CharScreen)
					++charscreen;
				else if (c.ClientState == GameClient.eClientState.Linkdead)
					++linkdeath;
				else if (c.ClientState == GameClient.eClientState.WorldEnter)
					++enterworld;
				else if (c.ClientState == GameClient.eClientState.Playing)
					++playing;
				else
					continue;

				// if a legal playing client, count some special things
				if (!c.IsPlaying
				    || c.Account == null
				    || c.Player == null
				    || c.Player.ObjectState != GameObject.eObjectState.Active)
					continue;

				if (c.Account.PrivLevel >= (uint)ePrivLevel.GM && !c.Player.IsAnonymous)
				{
					++gms;
					continue;
				}

				#endregion

				#region class specific counting
				switch (c.Player.CharacterClass.ID)
				{   // Alb tanks:
					case (int)eCharacterClass.Armsman:
						{ ++albTanks; classcount[0].count++; }
						break;
					case (int)eCharacterClass.Mercenary:
						{ ++albTanks; classcount[1].count++; }
						break;
					case (int)eCharacterClass.Paladin:
						{ ++albTanks; classcount[2].count++; }
						break;
					case (int)eCharacterClass.Reaver:
						{ ++albTanks; classcount[3].count++; }
						break;
					case (int)eCharacterClass.Heretic:
						{ ++albTanks; classcount[4].count++; }
						break;
					case (int)eCharacterClass.MaulerAlb:
						{ ++albTanks; classcount[5].count++; }
						break;

						// Alb casters:
					case (int)eCharacterClass.Cabalist:
						{ ++albCasters; classcount[6].count++; }
						break;
					case (int)eCharacterClass.Sorcerer:
						{ ++albCasters; classcount[7].count++; }
						break;
					case (int)eCharacterClass.Theurgist:
						{ ++albCasters; classcount[8].count++; }
						break;
					case (int)eCharacterClass.Wizard:
						{ ++albCasters; classcount[9].count++; }
						break;
					case (int)eCharacterClass.Necromancer:
						{ ++albCasters; classcount[10].count++; }
						break;
						// Alb support:
					case (int)eCharacterClass.Cleric:
						{ ++albSupport; classcount[11].count++; }
						break;
					case (int)eCharacterClass.Friar:
						{ ++albSupport; classcount[12].count++; }
						break;
					case (int)eCharacterClass.Minstrel:
						{ ++albSupport; classcount[13].count++; }
						break;
						// Alb stealthers:
					case (int)eCharacterClass.Infiltrator:
						{ ++albStealthers; classcount[14].count++; }
						break;
					case (int)eCharacterClass.Scout:
						{ ++albStealthers; classcount[15].count++; }
						break;
						// Mid tanks:
					case (int)eCharacterClass.Berserker:
						{ ++midTanks; classcount[16].count++; }
						break;
					case (int)eCharacterClass.Savage:
						{ ++midTanks; classcount[17].count++; }
						break;
					case (int)eCharacterClass.Skald:
						{ ++midTanks; classcount[18].count++; }
						break;
					case (int)eCharacterClass.Thane:
						{ ++midTanks; classcount[19].count++; }
						break;
					case (int)eCharacterClass.Warrior:
						{ ++midTanks; classcount[20].count++; }
						break;
					case (int)eCharacterClass.Valkyrie:
						{ ++midTanks; classcount[21].count++; }
						break;
					case (int)eCharacterClass.MaulerMid:
						{ ++midTanks; classcount[22].count++; }
						break;
						// Mid casters:
					case (int)eCharacterClass.Bonedancer:
						{ ++midCasters; classcount[23].count++; }
						break;
					case (int)eCharacterClass.Runemaster:
						{ ++midCasters; classcount[24].count++; }
						break;
					case (int)eCharacterClass.Spiritmaster:
						{ ++midCasters; classcount[25].count++; }
						break;
					case (int)eCharacterClass.Warlock:
						{ ++midCasters; classcount[26].count++; }
						break;
						// Mid support:
					case (int)eCharacterClass.Healer:
						{ ++midSupport; classcount[27].count++; }
						break;
					case (int)eCharacterClass.Shaman:
						{ ++midSupport; classcount[28].count++; }
						break;
						// Mid stealthers:
					case (int)eCharacterClass.Hunter:
						{ ++midStealthers; classcount[29].count++; }
						break;
					case (int)eCharacterClass.Shadowblade:
						{ ++midStealthers; classcount[30].count++; }
						break;
						// Hib tanks:
					case (int)eCharacterClass.Blademaster:
						{ ++hibTanks; classcount[31].count++; }
						break;
					case (int)eCharacterClass.Champion:
						{ ++hibTanks; classcount[32].count++; }
						break;
					case (int)eCharacterClass.Hero:
						{ ++hibTanks; classcount[33].count++; }
						break;
					case (int)eCharacterClass.Valewalker:
						{ ++hibTanks; classcount[34].count++; }
						break;
					case (int)eCharacterClass.MaulerHib:
						{ ++hibTanks; classcount[35].count++; }
						break;
					case (int)eCharacterClass.Vampiir:
						{ ++hibTanks; classcount[36].count++; }
						break;
						// Hib casters:
					case (int)eCharacterClass.Eldritch:
						{ ++hibCasters; classcount[37].count++; }
						break;
					case (int)eCharacterClass.Enchanter:
						{ ++hibCasters; classcount[38].count++; }
						break;
					case (int)eCharacterClass.Mentalist:
						{ ++hibCasters; classcount[39].count++; }
						break;
					case (int)eCharacterClass.Animist:
						{ ++hibCasters; classcount[40].count++; }
						break;
					case (int)eCharacterClass.Bainshee:
						{ ++hibCasters; classcount[41].count++; }
						break;
						// Hib support:
					case (int)eCharacterClass.Bard:
						{ ++hibSupport; classcount[42].count++; }
						break;
					case (int)eCharacterClass.Druid:
						{ ++hibSupport; classcount[43].count++; }
						break;
					case (int)eCharacterClass.Warden:
						{ ++hibSupport; classcount[44].count++; }
						break;

						// Hib stealthers:
					case (int)eCharacterClass.Nightshade:
						{ ++hibStealthers; classcount[45].count++; }
						break;
					case (int)eCharacterClass.Ranger:
						{ ++hibStealthers; classcount[46].count++; }
						break;
				}
				#endregion
			}
			
			#region output sameIP list
			if (bGM && sIP) {
				foreach (KeyValuePair<string, List<GameClient>> x in dict) {
					if (x.Value.Count >= 2) {
						if (output.Count > 0) {
							output.Add(" ");
						}
						output.Add(string.Format("Playing on IP {0}:", x.Key.ToString()));
						foreach (GameClient c in x.Value) {
							string realm;
							switch (c.ClientState) {
								case GameClient.eClientState.WorldEnter:
								case GameClient.eClientState.Playing:
									switch (c.Player.Realm) {
										case eRealm.Albion:
											realm = "Albion";
											break;
										case eRealm.Midgard:
											realm = "Midgard";
											break;
										case eRealm.Hibernia:
											realm = "Hibernia";
											break;
										default:
											realm = "Unknown realm";
											break;
									}
									break;
								default:
									realm = String.Empty;
									break;
							}
							
							switch (c.ClientState) {
								case GameClient.eClientState.Connecting:
									output.Add(string.Format("({0}) connecting.", c.Account.Name));
									break;
								case GameClient.eClientState.Disconnected:
									output.Add(string.Format("({0}) disconnected.", c.Account.Name));
									break;
								case GameClient.eClientState.CharScreen:
									output.Add(string.Format("({0}) in Character Screen", c.Account.Name));
									break;
								case GameClient.eClientState.Linkdead:
									output.Add(string.Format("{0} ({1}) went linkdead.", c.Player.Name, c.Account.Name));
									break;
								case GameClient.eClientState.WorldEnter:
									output.Add(string.Format("{0} ({1}), {2}, {3}, {4} is entering world.", c.Player.Name, c.Account.Name, c.Player.CharacterClass.Name, realm, c.Player.CurrentZone.Description));
									break;
								case GameClient.eClientState.Playing:
									output.Add(string.Format("{0} ({1}), {2}, {3}, {4}", c.Player.Name, c.Account.Name, c.Player.CharacterClass.Name, realm, c.Player.CurrentZone.Description));
									break;
								default:
									output.Add("Uhm. No ida what's going on.");
									break;
							}
						}
					}
				}
				if (output.Count == 0) {
					output.Add("No players online with the same IP.");
				}
				return output;
			}
			#endregion





			#region overview and class-specific
			int entering = connecting + enterworld + charscreen;
			int leaving = disconnecting + linkdeath;
			int albTotal = albTanks + albCasters + albSupport + albStealthers;
			int midTotal = midTanks + midCasters + midSupport + midStealthers;
			int hibTotal = hibTanks + hibCasters + hibSupport + hibStealthers;
			int total = entering + playing + leaving;
			output.Add(string.Format("Currently online:  {0}\n Playing:  {1} | Entering:  {2} | Leaving:  {3}",
			                         total, playing, entering, leaving));
			if (showAddOnlineInfo == true)
			{
				output.Add(string.Format("\n  (Connecting:  {0} | CharScreen:  {1} | EnterWorld:  {2} \nPlaying:  {3} | LinkDeath:  {4} | Disconnected:  {5} \nGMs:  {6})",
				                         connecting, enterworld, charscreen, playing, linkdeath, disconnecting, gms));
			}
			if (showRealms == true)
			{
				output.Add(string.Format("\nAlbion:  {4} ( {5}% )\n  Tanks:  {0} | Casters:  {1} \n  Supporters:  {2} | Stealthers:  {3}",
				                         albTanks, albCasters, albSupport, albStealthers, albTotal, (int)(albTotal * 100 / total)));
				output.Add(string.Format("\nMidgard:  {4} ( {5}% )\n  Tanks:  {0} | Casters:  {1} \n  Supporters:  {2} | Stealthers:  {3}",
				                         midTanks, midCasters, midSupport, midStealthers, midTotal, (int)(midTotal * 100 / total)));
				output.Add(string.Format("\nHibernia:  {4} ( {5}% )\n  Tanks:  {0} | Casters:  {1} \n  Supporters:  {2} | Stealthers:  {3}",
				                         hibTanks, hibCasters, hibSupport, hibStealthers, hibTotal, (int)(hibTotal * 100 / total)));
			}
			Zone zone = null;
			IList<GameClient> cls = new List<GameClient>();
			int albsinregion = 0;
			int midsinregion = 0;
			int hibsinregion = 0;
			int totalinregion = 0;
			if (zoneIDs.Length > 0 && showByZone == true)
			{
				for (int r = 0; r < zoneIDs.Length; r++)
				{
					albsinregion = 0;
					midsinregion = 0;
					hibsinregion = 0;
					totalinregion = 0;

					zone = WorldMgr.GetZone(zoneIDs[r]);

					cls = WorldMgr.GetClientsOfRegion(zone.ZoneRegion.ID);
					foreach (GameClient cir in cls)
					{
						if (!cir.IsPlaying
						    || cir.Account == null
						    || cir.Player == null
						    || cir.Player.ObjectState != GameObject.eObjectState.Active)
							continue;

						if (cir.Account.PrivLevel >= (uint)ePrivLevel.GM)
							continue;

						if (cir.Player.CurrentZone.Description != zone.Description)
							continue;



						if (cir.Player.Realm == eRealm.Albion)
							albsinregion++;
						else if (cir.Player.Realm == eRealm.Midgard)
							midsinregion++;
						else if (cir.Player.Realm == eRealm.Hibernia)
							hibsinregion++;

						totalinregion++;
					}


					output.Add(string.Format("\nPlayers in {0}: {1} \n Albs: {2} | Mids: {3} | Hibs: {4}", zone.Description, totalinregion, albsinregion, midsinregion, hibsinregion));

				}
			}


			if (showDetailedClass == true)
			{
				output.Add(string.Format("\n"));
				lock (classcount)
				{
					classcount.Sort(delegate(ClassToCount ctc1, ClassToCount ctc2) { return ctc1.count.CompareTo(ctc2.count); });
					classcount.Reverse();
					for (int c = 0; c < classcount.Count; c++)
					{
						if (classcount[c].count > 0)
							output.Add(string.Format("{0}: {1} ({2}%)", classcount[c].name, classcount[c].count.ToString(), (int)(classcount[c].count * 100 / total)));
					}
				}
			}


			#endregion


			return output;
		}
	}
}

