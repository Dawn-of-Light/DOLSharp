/*
 * Created by SharpDevelop.
 * User: Administrateur
 * Date: 09/08/2013
 * Time: 10:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using DOL.GS;
using DOL.GS.Items;
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.Quests.Catacombs.Obelisks;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Language;

using log4net;


namespace DOL.GS.Scripts
{
	/// <summary>
	/// Description of FreyadHelperNPC.
	/// </summary>
	[NPCGuildScript("Freyad Helper")]
	public class FreyadHelperNPC : BuffMerchant
	{

		//Capitals
		private static String[] m_hib_capitals = {
			"Tir na Nog",
			"Domnann",
			"Shar Labyrinth",
			"Scrios de Atlantis",
			"Cruachan Gorge",
			"Mag Mell",
		};
		
		private static String[] m_alb_capitals = {
			"Camelot City",
			"Caer Gothwaite",
			"Inconnu Crypt",
			"Ruins of Atlantis",
			"Forest Sauvage",
			"Cotswold",
		};
		
		private static String[] m_mid_capitals = {
			"Jordheim",
			"Aegirhamn",
			"Kobold Undercity",
			"Ruinrar av Atlantis",
			"Uppland",
			"Mularn",			
		};
		
		
		//Classic
		private static String[] m_alb_classic = {
			"Ludlow",
			"Caer Ulfwych",
			"West Downs",
			"Adribard's Retreat",
			"Dartmoor",
			"Savage Castle",
			"Snowdonia Fortress"
		};
		
		private static String[] m_alb_dungeon = {
			"Stonehenge Barrows",
			"Tomb of Mithra",
			"Keltoi Fogou",
			"Catacombs of Cardova",
			"Tepok's Mine",
			"Brimstone Caverns",
			"Darkness Falls"
		};
		
		private static String[] m_mid_classic = {
			"Vasudheim",
			"Galplen",
			"Fort Atla",
			"Uginfel",
			"Malmohus",
			"Svasud Faste",
			"Vindsaul Faste"
		};
		
		private static String[] m_mid_dungeon = {
			"Spindelhalla",
			"Vendo Caverns",
			"Varulvhamn",
			"Cursed Tomb",
			"Nisse's Lair",
			"Halls of Helgardh",
			"Darkness Falls"
		};
		
		private static String[] m_hib_classic = {
			"Ardee",
			"Tir na mBeo",
			"Howth",
			"Innis Carthaig",
			"Sheeroe Hills",
			"Druim Ligen",
			"Druim Cain"
		};
		
		private static String[] m_hib_dungeon = {
			"Muire Tomb",
			"Spraggon Den",
			"Treibh Caillte",
			"Koalinth Tribal Caverns",
			"Coruscating Mine",
			"Cave of Cruachan",
			"Darkness Falls"
		};
		
		// Shrouded Isles Cities
		private static String[] m_alb_si = {
			"Weary Village",
			"Fort Gwyntell",
			"Caer Diogel",
			"Krondon",
			"Caer Sidi",
			"Avalon City North",
			"Avalon City South",
		};
		
		private static String[] m_mid_si = {
			"Bjarken",
			"Hagall",
			"Knarr",
			"Trollheim",
			"Iarnvidiur's Lair",
			"Tuscaran Glacier",
		};
		
		private static String[] m_hib_si = {
			"Droighaid",
			"Aalid Feie",
			"Necht",
			"Fomor",
			"Tur Suil",
			"Galladoria",
		};

		//catacombs teleporter (only dungeon not instances)
		private static String[] m_alb_cata = {
			"Roman Aqueducts",
			"Lower Crypt",
			"Albion's Frontlines",
			"Albion's Underground Forest",
			"Albion's Deadlands",
			"Albion's Glashtin Forge",
			"Albion's Otherworld",
			"Albion's Abandoned Mines",
		};		

		private static String[] m_mid_cata = {
			"Burial Grounds",
			"Nyttheim",
			"Midgard's Frontlines",
			"Midgard's Underground Forest",
			"Midgard's Deadlands",
			"Midgard's Glashtin Forge",
			"Midgard's Otherworld",
			"Midgard's Abandoned Mines",
		};		

		private static String[] m_hib_cata = {
			"Veil Rift",
			"Queen's Labyrinth",
			"Hibernia's Frontlines",
			"Hibernia's Underground Forest",
			"Hibernia's Deadlands",
			"Hibernia's Glashtin Forge",
			"Hibernia's Otherworld",
			"Hibernia's Abandoned Mines",
		};		

		//toa teleporter (open and dungeon ?)
		private static String[] m_all_atlantis = {
			"Hesperos Haven",
			"Aerus Haven",
			"Volcanus Haven",
			"Stygia Haven",
			"Cetus' Pit",
			"Sobekite Eternal",
			"Template of Twilight",
			"Halls of Ma'ati",
			"The Great Pyramid of Stygia",
			"Deep Volcanus",
			"City of Aerus",
		};		
		
		//Frontier Teleporter
		private static String[] m_all_frontier = {
			"Hadrian's Wall",
			"Odin's Gate",
			"Emain Macha",
			"Passage of Conflict",
		};
		
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			// Teleporter Region
			List<String> playerAreaList = new List<String>();
			foreach (AbstractArea area in player.CurrentAreas)
				playerAreaList.Add(area.Description);

			String listTP = null; // List Buffer
			
			// Teleport Chooser
			String[] capitals = null;
			
			switch(player.Realm) 
			{
				case eRealm.Albion:
					
					capitals = m_alb_capitals;
					break;
				case eRealm.Midgard:
					
					capitals = m_mid_capitals;
					break;
				case eRealm.Hibernia:
					
					capitals = m_hib_capitals;
					break;
			}			

			//switch dependent of player zone
			switch(player.CurrentRegionID) {
					//classic alb
				case 1:
					//homeland + dungeon
					listTP = ListTeleportation(m_alb_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					listTP = ListTeleportation(m_alb_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					break;
					//capital alb
				case 10:
					listTP = ListTeleportation(m_alb_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);					
				break;
					//classic dungeon alb
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 339:
					listTP = ListTeleportation(m_alb_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//SI ALB Destination
				case 51:
				case 50:
				case 60:
				case 61:
				case 62:
					listTP = ListTeleportation(m_alb_si, playerAreaList);
					player.Out.SendMessage("Shrouded Isles Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;					
					//Cata ALB Destination
				case 59:
				case 63:
				case 66:
				case 67:
				case 68:
				case 109:
				case 196:
					listTP = ListTeleportation(m_alb_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//abandonned mines alb destination
				case 227:
					listTP = ListTeleportation(m_alb_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					listTP = ListTeleportation(m_alb_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//Atlantis alb destination
				case 73:
				case 78:
				case 79:
				case 80:
				case 83:
				case 88:
				case 89:
				case 90:
				case 70:
					listTP = ListTeleportation(m_all_atlantis, playerAreaList);
					player.Out.SendMessage("Atlantis Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

				break;

				
					//classic mid
				case 100:
					//homeland + dungeon
					listTP = ListTeleportation(m_mid_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					listTP = ListTeleportation(m_mid_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					break;
					//capital mid
				case 101:
					listTP = ListTeleportation(m_mid_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);					
				break;
					//classic mid dungeon
				case 125:
				case 126:
				case 127:
				case 128:
				case 129:
				case 340:
					listTP = ListTeleportation(m_mid_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//SI Mid Destination
				case 151:
				case 150:
				case 160:
				case 161:
					listTP = ListTeleportation(m_mid_si, playerAreaList);
					player.Out.SendMessage("Shrouded Isles Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;			
					//Cata Mid Destination
				case 58:
				case 148:
				case 149:
				case 162:
				case 189:
				case 195:
				case 229:
					listTP = ListTeleportation(m_mid_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//abandonned mines mid destination
				case 226:
					listTP = ListTeleportation(m_mid_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					listTP = ListTeleportation(m_mid_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//Atlantis mid destination
				case 35:
				case 36:
				case 37:
				case 40:
				case 45:
				case 46:
				case 47:
				case 30:
				case 71:
					listTP = ListTeleportation(m_all_atlantis, playerAreaList);
					player.Out.SendMessage("Atlantis Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

				break;
				
					//classic hib
				case 200:
					//homeland + dungeon
					listTP = ListTeleportation(m_hib_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					listTP = ListTeleportation(m_hib_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
				break;
					//capital hib
				case 201:
					//homeland + capital instances
					listTP = ListTeleportation(m_hib_classic, playerAreaList);
					player.Out.SendMessage("Homeland Teleports :\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				break;
					//classic dungeon hib
				case 220:
				case 221:
				case 222:
				case 223:
				case 224:
				case 341:
					//classic dungeon + classic instances
					listTP = ListTeleportation(m_hib_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
				break;
					//SI Hib Destination
				case 181:
				case 180:
				case 190:
				case 191:
					listTP = ListTeleportation(m_hib_si, playerAreaList);
					player.Out.SendMessage("Shrouded Isles Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//Cata HIB Destination
				case 92:
				case 94:
				case 95:
				case 96:
				case 97:
				case 99:
				case 197:
					listTP = ListTeleportation(m_hib_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//abandonned mines HIB destination
				case 228:
					listTP = ListTeleportation(m_hib_cata, playerAreaList);
					player.Out.SendMessage("Catacombs Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
					listTP = ListTeleportation(m_hib_dungeon, playerAreaList);
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);						
				break;
					//Atlantis hib destination
				case 130:
				case 135:
				case 136:
				case 137:
				case 140:
				case 145:
				case 146:
				case 147:
				case 72:
					listTP = ListTeleportation(m_all_atlantis, playerAreaList);
					player.Out.SendMessage("Atlantis Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

				break;
				
					//multirealm rvr
				case 163:
					listTP = ListTeleportation(m_all_frontier, playerAreaList);
					player.Out.SendMessage("Frontier Teleport:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				break;
				
					//multirealm dungeon				
				case 249:
					switch(player.Realm) 
					{
						case eRealm.Albion:
							listTP = ListTeleportation(m_alb_dungeon, playerAreaList);
							break;
						case eRealm.Midgard:
							listTP = ListTeleportation(m_mid_dungeon, playerAreaList);
							break;
							
						case eRealm.Hibernia:
							listTP = ListTeleportation(m_hib_dungeon, playerAreaList);
							break;
					}
					player.Out.SendMessage("Available Dungeons:\n" + listTP, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					
				break;

			}
			
			// Teleporter Capitals endlist
			
			listTP = ListTeleportation(capitals, playerAreaList);
			
			if(listTP.Length > 0) {
				player.Out.SendMessage("Here are the Capitals Teleportation:\n" + listTP,
						eChatType.CT_Say, eChatLoc.CL_PopupWindow);		
			}
			
			
			return true;
		}
		
		private String ListTeleportation(String[] listDestinations, List<String> playerArea) {
			
			String destList = "";
			
			foreach (String destination in listDestinations)
			{
				if (!playerArea.Contains(destination))
				{
					if(destList.Length > 0)
					{
						destList = destList + ", [" + destination + "]";
					}
					else 
					{
						destList = "[" + destination + "]";
					}
				}
			}

			return destList;			
		}
		
		public override bool WhisperReceive(GameLiving source, string str)
		{
			//Here we treat Gold and Bounty
			if (!base.WhisperReceive(source, str))
				return false;
			
			if(!(source is GamePlayer))
				return false;
			   
			
			Teleport destination = null;
		

			switch(source.Realm) 
			{
				case eRealm.Albion:
					switch(str) {
						case "Camelot City":
							destination = new Teleport();
							destination.X = 36433;
							destination.Y = 31177;
							destination.Z = 8011;
							destination.Heading = 1798;
							destination.RegionID = 10;
						break;
						case "Ruins of Atlantis":
							destination = new Teleport();
							destination.X = 578494;
							destination.Y = 533304;
							destination.Z = 7295;
							destination.Heading = 2664;
							destination.RegionID = 70;
						break;
						case "Caer Gothwaite":
							destination = new Teleport();
							destination.X = 526444;
							destination.Y = 542379;
							destination.Z = 3168;
							destination.Heading = 3534;
							destination.RegionID = 51;
						break;
						case "Cotswold":							
							destination = new Teleport();
							destination.X = 561533;
							destination.Y = 511887;
							destination.Z = 2459;
							destination.Heading = 1023;
							destination.RegionID = 1;
						break;
						case "Inconnu Crypt":
							destination = new Teleport();
							destination.X = 28052;
							destination.Y = 35807;
							destination.Z = 16399;
							destination.Heading = 3053;
							destination.RegionID = 65;
							
						break;
						case "Forest Sauvage":
							destination = new Teleport();
							destination.X = 654330;
							destination.Y = 617006;
							destination.Z = 9560;
							destination.Heading = 1876;
							destination.RegionID = 163;
							
						break;
						case "Ludlow":
							destination = new Teleport();
							destination.X = 531492;
							destination.Y = 479149;
							destination.Z = 2200;
							destination.Heading = 534;
							destination.RegionID = 1;
							
						break;
						case "Humberton":
							destination = new Teleport();
							destination.X = 508541;
							destination.Y = 477522;
							destination.Z = 2284;
							destination.Heading = 1596;
							destination.RegionID = 1;
							
						break;
						case "Caer Ulfwych":
							destination = new Teleport();
							destination.X = 520483;
							destination.Y = 616694;
							destination.Z = 1799;
							destination.Heading = 3750;
							destination.RegionID = 1;
							
						break;
						case "West Downs":
							destination = new Teleport();
							destination.X = 578936;
							destination.Y = 556937;
							destination.Z = 2187;
							destination.Heading = 1054;
							destination.RegionID = 1;
							
						break;
						case "Adribard's Retreat":
							destination = new Teleport();
							destination.X = 473114;
							destination.Y = 627701;
							destination.Z = 2153;
							destination.Heading = 171;
							destination.RegionID = 1;
							
						break;
						case "Dartmoor":
							destination = new Teleport();
							destination.X = 370369;
							destination.Y = 705169;
							destination.Z = 224;
							destination.Heading = 1205;
							destination.RegionID = 1;
						
						break;
						case "Savage Castle":
							destination = new Teleport();
							destination.X = 584616;
							destination.Y = 486318;
							destination.Z = 2184;
							destination.Heading = 862;
							destination.RegionID = 1;
							
						break;
						case "Snowdonia Fortress":
							destination = new Teleport();
							destination.X = 516570;
							destination.Y = 372880;
							destination.Z = 8208;
							destination.Heading = 2576;
							destination.RegionID = 1;
		
						break;
						case "Stonehenge Barrows":
							destination = new Teleport();
							destination.X = 31204;
							destination.Y = 33656;
							destination.Z = 16495;
							destination.Heading = 2646;
							destination.RegionID = 20;
							
						break;
						case "Tomb of Mithra":
							destination = new Teleport();
							destination.X = 33156;
							destination.Y = 32162;
							destination.Z = 16257;
							destination.Heading = 2050;
							destination.RegionID = 21;
							
						break;
						case "Keltoi Fogou":
							destination = new Teleport();
							destination.X = 30770;
							destination.Y = 31226;
							destination.Z = 16260;
							destination.Heading = 3507;
							destination.RegionID = 22;
							
						break;
						case "Catacombs of Cardova":
							destination = new Teleport();
							destination.X = 31746;
							destination.Y = 29942;
							destination.Z = 16000;
							destination.Heading = 3645;
							destination.RegionID = 23;
							
						break;
						case "Tepok's Mine":
							destination = new Teleport();
							destination.X = 32450;
							destination.Y = 34375;
							destination.Z = 15179;
							destination.Heading = 2736;
							destination.RegionID = 24;
							
						break;
						case "Brimstone Caverns":
							destination = new Teleport();
							destination.X = 33633;
							destination.Y = 40347;
							destination.Z = 16000;
							destination.Heading = 2615;
							destination.RegionID = 339;
						
						break;
						case "Darkness Falls":
							destination = new Teleport();
							destination.X = 31514;
							destination.Y = 27936;
							destination.Z = 22893;
							destination.Heading = 3103;
							destination.RegionID = 249;
							
						break;
						case "Weary Village":
							destination = new Teleport();
							destination.X = 435148;
							destination.Y = 493245;
							destination.Z = 3088;
							destination.Heading = 1091;
							destination.RegionID = 51;						
						break;
						case "Fort Gwyntell":
							destination = new Teleport();
							destination.X = 427374;
							destination.Y = 416461;
							destination.Z = 5712;
							destination.Heading = 664;
							destination.RegionID = 51;								
						break;
						case "Caer Diogel":
							destination = new Teleport();
							destination.X = 402986;
							destination.Y = 503100;
							destination.Z = 4680;
							destination.Heading = 2582;
							destination.RegionID = 51;								
						break;
						case "Krondon":
							destination = new Teleport();
							destination.X = 32853;
							destination.Y = 31362;
							destination.Z = 15721;
							destination.Heading = 964;
							destination.RegionID = 61;								
						break;
						case "Caer Sidi":
							destination = new Teleport();
							destination.X = 31656;
							destination.Y = 37526;
							destination.Z = 18382;
							destination.Heading = 6;
							destination.RegionID = 60;								
						break;
						case "Avalon City North":
							destination = new Teleport();
							destination.X = 32015;
							destination.Y = 22007;
							destination.Z = 8116;
							destination.Heading = 9;
							destination.RegionID = 50;
						break;
						case "Avalon City South":
							destination = new Teleport();
							destination.X = 31097;
							destination.Y = 47100;
							destination.Z = 8313;
							destination.Heading = 2050;
							destination.RegionID = 50;
						break;
						case "Roman Aqueducts":
							destination = new Teleport();
							destination.X = 32693;
							destination.Y = 17622;
							destination.Z = 17283;
							destination.Heading = 37;
							destination.RegionID = 63;
						break;
						case "Lower Crypt":
							destination = new Teleport();
							destination.X = 31553;
							destination.Y = 37896;
							destination.Z = 16465;
							destination.Heading = 2033;
							destination.RegionID = 68;
						break;
						case "Albion's Frontlines":
							destination = new Teleport();
							destination.X = 28454;
							destination.Y = 21778;
							destination.Z = 15851;
							destination.Heading = 2907;
							destination.RegionID = 109;
						break;
						case "Albion's Underground Forest":
							destination = new Teleport();
							destination.X = 15871;
							destination.Y = 40740;
							destination.Z = 16299;
							destination.Heading = 3059;
							destination.RegionID = 66;
						break;
						case "Albion's Deadlands":
							destination = new Teleport();
							destination.X = 40558;
							destination.Y = 16978;
							destination.Z = 15887;
							destination.Heading = 11;
							destination.RegionID = 67;
						break;
						case "Albion's Glashtin Forge":
							destination = new Teleport();
							destination.X = 35762;
							destination.Y = 22614;
							destination.Z = 16390;
							destination.Heading = 1013;
							destination.RegionID = 59;
						break;
						case "Albion's Otherworld":
							destination = new Teleport();
							destination.X = 25976;
							destination.Y = 21236;
							destination.Z = 15679;
							destination.Heading = 3218;
							destination.RegionID = 196;
						break;
						case "Albion's Abandoned Mines":
							destination = new Teleport();
							destination.X = 32802;
							destination.Y = 24119;
							destination.Z = 18430;
							destination.Heading = 1;
							destination.RegionID = 227;
						break;
						case "Hesperos Haven":
							destination = new Teleport();
							destination.X = 271904;
							destination.Y = 539894;
							destination.Z = 8344;
							destination.Heading = 3075;
							destination.RegionID = 73;
						break;
						case "Aerus Haven":
							destination = new Teleport();
							destination.X = 387800;
							destination.Y = 650545;
							destination.Z = 8104;
							destination.Heading = 1087;
							destination.RegionID = 73;
						break;
						case "Volcanus Haven":
							destination = new Teleport();
							destination.X = 448242;
							destination.Y = 548779;
							destination.Z = 8584;
							destination.Heading = 4055;
							destination.RegionID = 73;							
						break;
						case "Stygia Haven":
							destination = new Teleport();
							destination.X = 331268;
							destination.Y = 454183;
							destination.Z = 8112;
							destination.Heading = 2036;
							destination.RegionID = 73;							
						break;
						case "Cetus' Pit":
							destination = new Teleport();
							destination.X = 31556;
							destination.Y = 38706;
							destination.Z = 16254;
							destination.Heading = 1995;
							destination.RegionID = 78;							
						break;
						case "Sobekite Eternal":
							destination = new Teleport();
							destination.X = 22638;
							destination.Y = 45240;
							destination.Z = 8437;
							destination.Heading = 2180;
							destination.RegionID = 79;							
						break;
						case "Template of Twilight":
							destination = new Teleport();
							destination.X = 28984;
							destination.Y = 26669;
							destination.Z = 17391;
							destination.Heading = 4095;
							destination.RegionID = 80;							
						break;
						case "Halls of Ma'ati":
							destination = new Teleport();
							destination.X = 32551;
							destination.Y = 30052;
							destination.Z = 15975;
							destination.Heading = 3646;
							destination.RegionID = 83;							
						break;
						case "The Great Pyramid of Stygia":
							destination = new Teleport();
							destination.X = 32900;
							destination.Y = 28316;
							destination.Z = 16900;
							destination.Heading = 13;
							destination.RegionID = 88;							
						break;
						case "Deep Volcanus":
							destination = new Teleport();
							destination.X = 32468;
							destination.Y = 42960;
							destination.Z = 16445;
							destination.Heading = 1421;
							destination.RegionID = 89;							
						break;
						case "City of Aerus":
							destination = new Teleport();
							destination.X = 42314;
							destination.Y = 38013;
							destination.Z = 10291;
							destination.Heading = 615;
							destination.RegionID = 90;							
						break;						
							
						case "Emain Macha":
							destination = new Teleport();
							destination.X = 428561;
							destination.Y = 498780;
							destination.Z = 8295;
							destination.Heading = 3615;
							destination.RegionID = 163;
						break;
						case "Odin's Gate":
							destination = new Teleport();
							destination.X = 543053;
							destination.Y = 418532;
							destination.Z = 7991;
							destination.Heading = 2280;
							destination.RegionID = 163;
						break;
						case "Hadrian's Wall":
							destination = new Teleport();
							destination.X = 564566;
							destination.Y = 512100;
							destination.Z = 8163;
							destination.Heading = 2494;
							destination.RegionID = 163;
						break;
						case "Passage of Conflict":
							destination = new Teleport();
							destination.X = 27341;
							destination.Y = 51300;
							destination.Z = 17935;
							destination.Heading = 1662;
							destination.RegionID = 244;
						break;
						
						default:

						return false;
					}
					
					break;
				case eRealm.Midgard:
					switch(str) {
						case "Jordheim":
							destination = new Teleport();
							destination.X = 31755;
							destination.Y = 27427;
							destination.Z = 8800;
							destination.Heading = 11;
							destination.RegionID = 101;
						break;
						case "Aegirhamn":
							destination = new Teleport();
							destination.X = 293791;
							destination.Y = 356371;
							destination.Z = 3488;
							destination.Heading = 412;
							destination.RegionID = 151;
						
						break;
						case "Kobold Undercity":
							destination = new Teleport();
							destination.X = 33068;
							destination.Y = 28693;
							destination.Z = 16192;
							destination.Heading = 102;
							destination.RegionID = 243;

						break;
						case "Ruinrar av Atlantis":
							destination = new Teleport();
							destination.X = 565541;
							destination.Y = 569991;
							destination.Z = 7255;
							destination.Heading = 3766;
							destination.RegionID = 71;

						break;
						case "Uppland":
							destination = new Teleport();
							destination.X = 651441;
							destination.Y = 313287;
							destination.Z = 9432;
							destination.Heading = 973;
							destination.RegionID = 163;						
						break;
						case "Mularn":
							destination = new Teleport();
							destination.X = 803018;
							destination.Y = 727037;
							destination.Z = 4691;
							destination.Heading = 2752;
							destination.RegionID = 100;						
						break;
						case "Vasudheim":
							destination = new Teleport();
							destination.X = 774588;
							destination.Y = 755635;
							destination.Z = 4600;
							destination.Heading = 2044;
							destination.RegionID = 100;
						
						break;
						case "Galplen":
							destination = new Teleport();
							destination.X = 798788;
							destination.Y = 893543;
							destination.Z = 4744;
							destination.Heading = 1941;
							destination.RegionID = 100;
						
						break;
						case "Audliten":
							destination = new Teleport();
							destination.X = 724377;
							destination.Y = 760045;
							destination.Z = 4528;
							destination.Heading = 3286;
							destination.RegionID = 100;
						
						break;
						case "West Skona":
							destination = new Teleport();
							destination.X = 711755;
							destination.Y = 924063;
							destination.Z = 5069;
							destination.Heading = 3167;
							destination.RegionID = 100;
						
						break;
						case "Fort Atla":
							destination = new Teleport();
							destination.X = 749275;
							destination.Y = 815408;
							destination.Z = 4408;
							destination.Heading = 53;
							destination.RegionID = 100;
						
						break;
						case "Uginfel":
							destination = new Teleport();
							destination.X = 712796;
							destination.Y = 784009;
							destination.Z = 4672;
							destination.Heading = 1037;
							destination.RegionID = 100;						
						break;
						case "Malmohus":
							destination = new Teleport();
							destination.X = 742770;
							destination.Y = 978619;
							destination.Z = 3920;
							destination.Heading = 2675;
							destination.RegionID = 100;
							
						break;
						case "Svasud Faste":
							destination = new Teleport();
							destination.X = 764197;
							destination.Y = 672576;
							destination.Z = 5741;
							destination.Heading = 3195;
							destination.RegionID = 100;													
						break;
						case "Vindsaul Faste":
							destination = new Teleport();
							destination.X = 704687;
							destination.Y = 738350;
							destination.Z = 5704;
							destination.Heading = 2088;
							destination.RegionID = 100;
						
						break;
						case "Spindelhalla":
							destination = new Teleport();
							destination.X = 32222;
							destination.Y = 31861;
							destination.Z = 16371;
							destination.Heading = 3090;
							destination.RegionID = 125;
						
						break;
						case "Vendo Caverns":
							destination = new Teleport();
							destination.X = 32765;
							destination.Y = 33107;
							destination.Z = 16618;
							destination.Heading = 2055;
							destination.RegionID = 126;
							
						break;
						case "Varulvhamn":
							destination = new Teleport();
							destination.X = 35203;
							destination.Y = 30878;
							destination.Z = 15006;
							destination.Heading = 1073;
							destination.RegionID = 127;							
						break;
						case "Cursed Tomb":
							destination = new Teleport();
							destination.X = 30120;
							destination.Y = 31237;
							destination.Z = 16522;
							destination.Heading = 3079;
							destination.RegionID = 128;
							
						break;
						case "Nisse's Lair":
							destination = new Teleport();
							destination.X = 34716;
							destination.Y = 33180;
							destination.Z = 16463;
							destination.Heading = 1039;
							destination.RegionID = 129;
							
						break;
						case "Halls of Helgardh":
							destination = new Teleport();
							destination.X = 31955;
							destination.Y = 40717;
							destination.Z = 16000;
							destination.Heading = 3085;
							destination.RegionID = 340;
						
						break;
						case "Darkness Falls":
							destination = new Teleport();
							destination.X = 17569;
							destination.Y = 18965;
							destination.Z = 22892;
							destination.Heading = 1051;
							destination.RegionID = 249;
							
						break;
						case "Bjarken":
							destination = new Teleport();
							destination.X = 289376;
							destination.Y = 301645;
							destination.Z = 4160;
							destination.Heading = 2781;
							destination.RegionID = 151;
													
						break;
						case "Hagall":
							destination = new Teleport();
							destination.X = 380022;
							destination.Y = 384465;
							destination.Z = 7752;
							destination.Heading = 831;
							destination.RegionID = 151;
							
						break;
						case "Knarr":
							destination = new Teleport();
							destination.X = 302606;
							destination.Y = 433957;
							destination.Z = 3228;
							destination.Heading = 2055;
							destination.RegionID = 151;
							
						break;
						case "Trollheim":
							destination = new Teleport();
							destination.X = 28865;
							destination.Y = 47591;
							destination.Z = 15999;
							destination.Heading = 3077;
							destination.RegionID = 150;
							
						break;
						case "Iarnvidiur's Lair":
							destination = new Teleport();
							destination.X = 34728;
							destination.Y = 37407;
							destination.Z = 17081;
							destination.Heading = 2042;
							destination.RegionID = 161;
							
						break;
						case "Tuscaran Glacier":
							destination = new Teleport();
							destination.X = 34907;
							destination.Y = 18185;
							destination.Z = 18832;
							destination.Heading = 1311;
							destination.RegionID = 160;
							
						break;
						case "Burial Grounds":
							destination = new Teleport();
							destination.X = 29179;
							destination.Y = 12916;
							destination.Z = 13012;
							destination.Heading = 240;
							destination.RegionID = 229;
							
						break;
						case "Nyttheim":
							destination = new Teleport();
							destination.X = 39592;
							destination.Y = 33361;
							destination.Z = 15918;
							destination.Heading = 2230;
							destination.RegionID = 149;
							
						break;
						case "Midgard's Frontlines":
							destination = new Teleport();
							destination.X = 42555;
							destination.Y = 51022;
							destination.Z = 16129;
							destination.Heading = 2027;
							destination.RegionID = 148;
							
						break;
						case "Midgard's Underground Forest":
							destination = new Teleport();
							destination.X = 14950;
							destination.Y = 40681;
							destination.Z = 16371;
							destination.Heading = 3063;
							destination.RegionID = 58;
							
						break;
						case "Midgard's Deadlands":
							destination = new Teleport();
							destination.X = 36827;
							destination.Y = 29619;
							destination.Z = 15971;
							destination.Heading = 1124;
							destination.RegionID = 162;
							
						break;
						case "Midgard's Glashtin Forge":
							destination = new Teleport();
							destination.X = 37967;
							destination.Y = 24114;
							destination.Z = 16394;
							destination.Heading = 1003;
							destination.RegionID = 189;
							
						break;
						case "Midgard's Otherworld":
							destination = new Teleport();
							destination.X = 32624;
							destination.Y = 14966;
							destination.Z = 15651;
							destination.Heading = 64;
							destination.RegionID = 195;
							
						break;
						case "Midgard's Abandoned Mines":
							destination = new Teleport();
							destination.X = 32807;
							destination.Y = 25764;
							destination.Z = 18429;
							destination.Heading = 3965;
							destination.RegionID = 226;
							
						break;
						case "Hesperos Haven":
							destination = new Teleport();
							destination.X = 271904;
							destination.Y = 539894;
							destination.Z = 8344;
							destination.Heading = 3075;
							destination.RegionID = 30;
						break;
					case "Aerus Haven":
							destination = new Teleport();
							destination.X = 387800;
							destination.Y = 650545;
							destination.Z = 8104;
							destination.Heading = 1087;
							destination.RegionID = 30;
						break;
						case "Volcanus Haven":
							destination = new Teleport();
							destination.X = 448242;
							destination.Y = 548779;
							destination.Z = 8584;
							destination.Heading = 4055;
							destination.RegionID = 30;							
						break;
						case "Stygia Haven":
							destination = new Teleport();
							destination.X = 331268;
							destination.Y = 454183;
							destination.Z = 8112;
							destination.Heading = 2036;
							destination.RegionID = 30;							
						break;
						case "Cetus' Pit":
							destination = new Teleport();
							destination.X = 31556;
							destination.Y = 38706;
							destination.Z = 16254;
							destination.Heading = 1995;
							destination.RegionID = 35;							
						break;
						case "Sobekite Eternal":
							destination = new Teleport();
							destination.X = 22638;
							destination.Y = 45240;
							destination.Z = 8437;
							destination.Heading = 2180;
							destination.RegionID = 36;							
						break;
						case "Template of Twilight":
							destination = new Teleport();
							destination.X = 28984;
							destination.Y = 26669;
							destination.Z = 17391;
							destination.Heading = 4095;
							destination.RegionID = 37;							
						break;
						case "Halls of Ma'ati":
							destination = new Teleport();
							destination.X = 32551;
							destination.Y = 30052;
							destination.Z = 15975;
							destination.Heading = 3646;
							destination.RegionID = 40;							
						break;
						case "The Great Pyramid of Stygia":
							destination = new Teleport();
							destination.X = 32900;
							destination.Y = 28316;
							destination.Z = 16900;
							destination.Heading = 13;
							destination.RegionID = 45;							
						break;
						case "Deep Volcanus":
							destination = new Teleport();
							destination.X = 32468;
							destination.Y = 42960;
							destination.Z = 16445;
							destination.Heading = 1421;
							destination.RegionID = 46;							
						break;
						case "City of Aerus":
							destination = new Teleport();
							destination.X = 42314;
							destination.Y = 38013;
							destination.Z = 10291;
							destination.Heading = 615;
							destination.RegionID = 47;							
						break;

						case "Emain Macha":
							destination = new Teleport();
							destination.X = 429168;
							destination.Y = 519875;
							destination.Z = 8938;
							destination.Heading = 2700;
							destination.RegionID = 163;
						break;
						case "Odin's Gate":
							destination = new Teleport();
							destination.X = 554352;
							destination.Y = 389248;
							destination.Z = 8155;
							destination.Heading = 3939;
							destination.RegionID = 163;
						break;
						case "Hadrian's Wall":
							destination = new Teleport();
							destination.X = 585519;
							destination.Y = 525524;
							destination.Z = 8310;
							destination.Heading = 2341;
							destination.RegionID = 163;
						break;
						case "Passage of Conflict":
							destination = new Teleport();
							destination.X = 53404;
							destination.Y = 25048;
							destination.Z = 16513;
							destination.Heading = 890;
							destination.RegionID = 244;
						break;
						
						default:
							return false;
					}					
					
					break;
				case eRealm.Hibernia:
					switch(str) {
						case "Tir na Nog":
							destination = new Teleport();
							destination.X = 33785;
							destination.Y = 32054;
							destination.Z = 8047;
							destination.Heading = 1022;
							destination.RegionID = 201;
						break;							
						case "Domnann":
							destination = new Teleport();
							destination.X = 423482;
							destination.Y = 439732;
							destination.Z = 5949;
							destination.Heading = 4035;
							destination.RegionID = 181;
						break;							
						case "Shar Labyrinth":
							destination = new Teleport();
							destination.X = 24741;
							destination.Y = 27953;
							destination.Z = 17561;
							destination.Heading = 1436;
							destination.RegionID = 93;
						break;
						case "Scrios de Atlantis":
							destination = new Teleport();
							destination.X = 551805;
							destination.Y = 577070;
							destination.Z = 6767;
							destination.Heading = 17;
							destination.RegionID = 72;
						break;
						case "Cruachan Gorge":
							destination = new Teleport();
							destination.X = 396896;
							destination.Y = 618059;
							destination.Z = 9851;
							destination.Heading = 1751;
							destination.RegionID = 163;
						break;							
						case "Mag Mell":
							destination = new Teleport();
							destination.X = 347860;
							destination.Y = 490811;
							destination.Z = 5218;
							destination.Heading = 1145;
							destination.RegionID = 200;
						break;
						case "Ardee":
							destination = new Teleport();
							destination.X = 340289;
							destination.Y = 468241;
							destination.Z = 5200;
							destination.Heading = 3053;
							destination.RegionID = 200;
						
						break;
						case "Tir na mBeo":
							destination = new Teleport();
							destination.X = 344421;
							destination.Y = 528404;
							destination.Z = 5448;
							destination.Heading = 1038;
							destination.RegionID = 200;
						
						break;
						case "Howth":
							destination = new Teleport();
							destination.X = 343513;
							destination.Y = 592711;
							destination.Z = 5456;
							destination.Heading = 1263;
							destination.RegionID = 200;
						
						break;
						case "Innis Carthaig":
							destination = new Teleport();
							destination.X = 335174;
							destination.Y = 719988;
							destination.Z = 4296;
							destination.Heading = 1096;
							destination.RegionID = 200;
						
						break;
						case "Sheeroe Hills":
							destination = new Teleport();
							destination.X = 369776;
							destination.Y = 651406;
							destination.Z = 3696;
							destination.Heading = 2004;
							destination.RegionID = 200;
						break;
						case "Druim Ligen":
							destination = new Teleport();
							destination.X = 334430;
							destination.Y = 420046;
							destination.Z = 5184;
							destination.Heading = 886;
							destination.RegionID = 200;
						
						break;
						case "Druim Cain":
							destination = new Teleport();
							destination.X = 421281;
							destination.Y = 486595;
							destination.Z = 1824;
							destination.Heading = 2058;
							destination.RegionID = 200;
						
						break;
						case "Muire Tomb":
							destination = new Teleport();
							destination.X = 31086;
							destination.Y = 29874;
							destination.Z = 16239;
							destination.Heading = 3410;
							destination.RegionID = 221;
						break;
						case "Spraggon Den":
							destination = new Teleport();
							destination.X = 32552;
							destination.Y = 34307;
							destination.Z = 15181;
							destination.Heading = 2206;
							destination.RegionID = 222;

						break;
						case "Treibh Caillte":
							destination = new Teleport();
							destination.X = 35178;
							destination.Y = 30841;
							destination.Z = 15001;
							destination.Heading = 1034;
							destination.RegionID = 224;
						break;
						case "Koalinth Tribal Caverns":
							destination = new Teleport();
							destination.X = 27175;
							destination.Y = 32201;
							destination.Z = 17274;
							destination.Heading = 3425;
							destination.RegionID = 223;

						break;
						case "Coruscating Mine":
							destination = new Teleport();
							destination.X = 33439;
							destination.Y = 33654;
							destination.Z = 16040;
							destination.Heading = 1010;
							destination.RegionID = 220;
						break;
						case "Cave of Cruachan":
							destination = new Teleport();
							destination.X = 31536;
							destination.Y = 40938;
							destination.Z = 16000;
							destination.Heading = 2638;
							destination.RegionID = 341;
						break;
						case "Darkness Falls":
							destination = new Teleport();
							destination.X = 46344;
							destination.Y = 40261;
							destination.Z = 21357;
							destination.Heading = 2075;
							destination.RegionID = 249;
						break;
						case "Droighaid":
							destination = new Teleport();
							destination.X = 380240;
							destination.Y = 421087;
							destination.Z = 5528;
							destination.Heading = 1965;
							destination.RegionID = 181;
						
						break;
						case "Aalid Feie":
							destination = new Teleport();
							destination.X = 313544;
							destination.Y = 352491;
							destination.Z = 3589;
							destination.Heading = 595;
							destination.RegionID = 181;
							
						break;
						case "Necht":
							destination = new Teleport();
							destination.X = 428443;
							destination.Y = 319341;
							destination.Z = 3416;
							destination.Heading = 1766;
							destination.RegionID = 181;
						
						break;
						case "Fomor":
							destination = new Teleport();
							destination.X = 33209;
							destination.Y = 24763;
							destination.Z = 16032;
							destination.Heading = 575;
							destination.RegionID = 180;
						
						break;
						case "Tur Suil":
							destination = new Teleport();
							destination.X = 38023;
							destination.Y = 28479;
							destination.Z = 12991;
							destination.Heading = 4090;
							destination.RegionID = 190;
						
						break;
						case "Galladoria":
							destination = new Teleport();
							destination.X = 32118;
							destination.Y = 29822;
							destination.Z = 17033;
							destination.Heading = 22;
							destination.RegionID = 191;
						
						break;
						case "Veil Rift":
							destination = new Teleport();
							destination.X = 58071;
							destination.Y = 28789;
							destination.Z = 11286;
							destination.Heading = 1123;
							destination.RegionID = 92;										
						break;
						case "Queen's Labyrinth":
							destination = new Teleport();
							destination.X = 34715;
							destination.Y = 22729;
							destination.Z = 20365;
							destination.Heading = 3971;
							destination.RegionID = 94;										
						break;
						case "Hibernia's Frontlines":
							destination = new Teleport();
							destination.X = 28209;
							destination.Y = 21801;
							destination.Z = 15998;
							destination.Heading = 3157;
							destination.RegionID = 95;										
						break;
						case "Hibernia's Underground Forest":
							destination = new Teleport();
							destination.X = 15933;
							destination.Y = 40788;
							destination.Z = 16301;
							destination.Heading = 3074;
							destination.RegionID = 96;											
						break;
						case "Hibernia's Deadlands":
							destination = new Teleport();
							destination.X = 37005;
							destination.Y = 29651;
							destination.Z = 15995;
							destination.Heading = 1224;
							destination.RegionID = 97;											
						break;
						case "Hibernia's Glashtin Forge":
							destination = new Teleport();
							destination.X = 37930;
							destination.Y = 24135;
							destination.Z = 16394;
							destination.Heading = 1030;
							destination.RegionID = 99;
						break;						
						case "Hibernia's Otherworld":
							destination = new Teleport();
							destination.X = 19865;
							destination.Y = 24136;
							destination.Z = 15603;
							destination.Heading = 3220;
							destination.RegionID = 197;
						break;
						case "Hibernia's Abandoned Mines":
							destination = new Teleport();
							destination.X = 34388;
							destination.Y = 24314;
							destination.Z = 16825;
							destination.Heading = 1;
							destination.RegionID = 228;
						break;
						case "Hesperos Haven":
							destination = new Teleport();
							destination.X = 271904;
							destination.Y = 539894;
							destination.Z = 8344;
							destination.Heading = 3075;
							destination.RegionID = 130;
						break;
						case "Aerus Haven":
							destination = new Teleport();
							destination.X = 387800;
							destination.Y = 650545;
							destination.Z = 8104;
							destination.Heading = 1087;
							destination.RegionID = 130;
						break;
						case "Volcanus Haven":
							destination = new Teleport();
							destination.X = 448242;
							destination.Y = 548779;
							destination.Z = 8584;
							destination.Heading = 4055;
							destination.RegionID = 130;							
						break;
						case "Stygia Haven":
							destination = new Teleport();
							destination.X = 331268;
							destination.Y = 454183;
							destination.Z = 8112;
							destination.Heading = 2036;
							destination.RegionID = 130;							
						break;						
						case "Cetus' Pit":
							destination = new Teleport();
							destination.X = 31556;
							destination.Y = 38706;
							destination.Z = 16254;
							destination.Heading = 1995;
							destination.RegionID = 135;							
						break;
						case "Sobekite Eternal":
							destination = new Teleport();
							destination.X = 22638;
							destination.Y = 45240;
							destination.Z = 8437;
							destination.Heading = 2180;
							destination.RegionID = 136;							
						break;
						case "Template of Twilight":
							destination = new Teleport();
							destination.X = 28984;
							destination.Y = 26669;
							destination.Z = 17391;
							destination.Heading = 4095;
							destination.RegionID = 137;							
						break;
						case "Halls of Ma'ati":
							destination = new Teleport();
							destination.X = 32551;
							destination.Y = 30052;
							destination.Z = 15975;
							destination.Heading = 3646;
							destination.RegionID = 140;							
						break;
						case "The Great Pyramid of Stygia":
							destination = new Teleport();
							destination.X = 32900;
							destination.Y = 28316;
							destination.Z = 16900;
							destination.Heading = 13;
							destination.RegionID = 145;							
						break;
						case "Deep Volcanus":
							destination = new Teleport();
							destination.X = 32468;
							destination.Y = 42960;
							destination.Z = 16445;
							destination.Heading = 1421;
							destination.RegionID = 146;							
						break;
						case "City of Aerus":
							destination = new Teleport();
							destination.X = 42314;
							destination.Y = 38013;
							destination.Z = 10291;
							destination.Heading = 615;
							destination.RegionID = 147;
						break;

						case "Emain Macha":
							destination = new Teleport();
							destination.X = 457839;
							destination.Y = 518114;
							destination.Z = 7940;
							destination.Heading = 2115;
							destination.RegionID = 163;
						break;
						case "Odin's Gate":
							destination = new Teleport();
							destination.X = 553175;
							destination.Y = 403865;
							destination.Z = 8852;
							destination.Heading = 893;
							destination.RegionID = 163;
						break;
						case "Hadrian's Wall":
							destination = new Teleport();
							destination.X = 595346;
							destination.Y = 511480;
							destination.Z = 8697;
							destination.Heading = 2872;
							destination.RegionID = 163;
						break;
						case "Passage of Conflict":
							destination = new Teleport();
							destination.X = 47605;
							destination.Y = 48436;
							destination.Z = 17533;
							destination.Heading = 1034;
							destination.RegionID = 244;
						break;
						default:
							return false;
					}
					
					break;
			}
			
			if(destination != null) 
			{
				// don't add to database
				destination.AllowAdd = false;
				destination.AllowDelete = false;
	
				// Teleport
				this.OnTeleportSpell((GamePlayer)source, destination);
				
			}
			
			return true;
		}
		
		/// <summary>
		/// Teleport the player to the designated coordinates using the
		/// portal spell.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected virtual void OnTeleportSpell(GamePlayer player, Teleport destination)
		{
			SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
			List<Spell> spellList = SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells);
			Spell spell = SkillBase.GetSpellByID(5999);	// UniPortal spell.

			if (spell != null)
			{
				TargetObject = player;
				UniPortal portalHandler = new UniPortal(this, spell, spellLine, destination);
				m_runningSpellHandler = portalHandler;
				portalHandler.CastSpell();
				return;
			}

			// Spell not found in the database, fall back on default procedure.

			if (player.Client.Account.PrivLevel > 1)
				player.Out.SendMessage("Uni-Portal spell not found.",
					eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			
			this.OnTeleport(player, destination);
		}
		
		/// <summary>
		/// Teleport the player to the designated coordinates. 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected virtual void OnTeleport(GamePlayer player, Teleport destination)
		{
			if (player.InCombat == false && GameRelic.IsPlayerCarryingRelic(player) == false)
			{
				player.LeaveHouse();
				GameLocation currentLocation = new GameLocation("TeleportStart", player.CurrentRegionID, player.X, player.Y, player.Z);
				player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z, (ushort)destination.Heading);
				GameServer.ServerRules.OnPlayerTeleport(player, currentLocation, destination);
			}
		}
	}
	
	    #region Sojourner-5
    [SpellHandlerAttribute("PortableFreyadHelper")]
    public class PortableFreyadHelperSpellHandler : SpellHandler
    {
    	private static string m_forbiddenString;
    	private static List<ushort> m_forbiddenRegions;
    	
        private GameMerchant merchant;
        /// <summary>
        /// Execute Portable Helper summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner == null || !effect.Owner.IsAlive)
                return;
            
            merchant.AddToWorld();
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (merchant != null) merchant.Delete();
            return base.OnEffectExpires(effect, noMessages);
        }
        
		public override bool StartSpell(GameLiving target, InventoryItem item)
		{
			if(m_forbiddenRegions.Contains(Caster.CurrentRegionID))
            {
            	MessageToCaster("You can't summon Helper in this Region !", eChatType.CT_SpellResisted);
            	return false;
            }
			return base.StartSpell(target, item);
		}
        
        public PortableFreyadHelperSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        	
        	if(m_forbiddenString == null || m_forbiddenString != ServerProperties.Properties.PORTABLE_HELPER_FORBIDDEN_REGIONS) {
        		m_forbiddenRegions = new List<ushort>();
        		m_forbiddenString = ServerProperties.Properties.PORTABLE_HELPER_FORBIDDEN_REGIONS;
        		foreach(string region in Util.SplitCSV(ServerProperties.Properties.PORTABLE_HELPER_FORBIDDEN_REGIONS))
		        {
        			ushort regid;
        			if(ushort.TryParse(region, out regid))
        				m_forbiddenRegions.Add(regid);
        			
		        }
        	}
        	
            if (caster is GamePlayer)
            {
                GamePlayer casterPlayer = caster as GamePlayer;
                merchant = new FreyadHelperNPC();
                //Fill the object variables
                merchant.X = casterPlayer.X + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Y = casterPlayer.Y + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Z = casterPlayer.Z;
                merchant.CurrentRegion = casterPlayer.CurrentRegion;
                merchant.Heading = (ushort)((casterPlayer.Heading + 2048) % 4096);
                merchant.Level = 50;
                merchant.Realm = casterPlayer.Realm;
                merchant.Name = caster.Name+"'s Helper";
                merchant.GuildName = "Portable Helper";
                merchant.Model = 1647;
                merchant.CurrentSpeed = 0;
                merchant.MaxSpeedBase = 0;
                merchant.Size = 50;
                merchant.Flags |= GameNPC.eFlags.PEACE;
            }
        }
    }
    #endregion
}
