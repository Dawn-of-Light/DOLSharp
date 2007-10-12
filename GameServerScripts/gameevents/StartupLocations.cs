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
/*
 * Author:	code by Noret, locations by Avatarius
 * Date:	19.07.2004
 * This script should be put in /scripts/gameevents directory.
 * This event script changes starting location of created characters
 * based on region, class and race
 */

using System;
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using log4net;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Moves new created Characters to the starting location based on region, class and race
	/// </summary>
	public class StartupLocations
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static HybridDictionary[] ClassicLocations = null;
		private static HybridDictionary[] ShroudedIslesLocations = null;
		public static HybridDictionary MainTownStartingLocations = null;

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (ServerProperties.Properties.USE_CUSTOM_START_LOCATIONS)
				return;
			bool result = InitLocationTables();
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
			if (log.IsInfoEnabled)
				log.Info("StartupLocations initialized");
		}


		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (ServerProperties.Properties.USE_CUSTOM_START_LOCATIONS)
				return;
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
			ClassicLocations = null;
			ShroudedIslesLocations = null;
		}

		/// <summary>
		/// Change location on character creation
		/// </summary>
		public static void CharacterCreation(DOLEvent ev, object sender, EventArgs args)
		{
			CharacterEventArgs chArgs = args as CharacterEventArgs;
			if (chArgs == null)
				return;
			Character ch = chArgs.Character;
			try
			{
				StartLocation loc = null;
				GameClient client = chArgs.GameClient;
				if (ch.Region == 27) // tutorial all realms use the same region
				{
					switch (ch.Realm)
					{
						case 1: // alb
							loc = new StartLocation(95644, 101313, 5340, 1024);
							break;
						case 2: // mid
							loc = new StartLocation(226716, 232385, 5340, 1024);
							break;
						case 3: // hib
							loc = new StartLocation(357788, 363457, 5340, 1024);
							break;
					}
				}
				else if ((int)client.Version >= (int)GameClient.eClientVersion.Version180)
				{
					loc = (StartLocation)MainTownStartingLocations[ch.Class];
				}
				else if (ch.Region == 1 || ch.Region == 100 || ch.Region == 200) // all classic regions
				{
					loc = (StartLocation) ClassicLocations[ch.Race][ch.Class];
				}
				else if (ch.Region == 51 || ch.Region == 151 || ch.Region == 181) // all SI regions
				{
					loc = (StartLocation) ShroudedIslesLocations[ch.Race][ch.Class];
				}
				else
				{
					log.DebugFormat("tried to create char in unknown region {0}", ch.Region);
					switch (ch.Realm)
					{
						default: ch.Region = 1; break;
						case 2: ch.Region = 100; break;
						case 3: ch.Region = 200; break;
					}
					loc = (StartLocation)ClassicLocations[ch.Race][ch.Class];
				}

				if (loc == null)
				{
					log.Warn("startup location not found: account=" + ch.AccountName + "; char name=" + ch.Name + "; region=" + ch.Region + "; realm=" + ch.Realm + "; class=" + ch.Class + " (" + (eCharacterClass) ch.Class + "); race=" + ch.Race + " (" + (eRace)ch.Race + ")");
				}
				else
				{
					// can't change region on char creation, that is hardcoded in the client
					ch.Xpos = loc.X;
					ch.Ypos = loc.Y;
					ch.Zpos = loc.Z;
					ch.Direction = loc.Heading;
				}

				BindCharacter(ch);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("StartupLocations script: error changing location. account=" + ch.AccountName + "; char name=" + ch.Name + "; region=" + ch.Region + "; realm=" + ch.Realm + "; class=" + ch.Class + " (" + (eCharacterClass) ch.Class + "); race=" + ch.Race + " (" + (eRace)ch.Race + ")", e);
			}
		}

		/// <summary>
		/// Binds character to current location
		/// </summary>
		/// <param name="ch"></param>
		protected static void BindCharacter(Character ch)
		{
			ch.BindRegion = ch.Region;
			ch.BindHeading = ch.Direction;
			ch.BindXpos = ch.Xpos;
			ch.BindYpos = ch.Ypos;
			ch.BindZpos = ch.Zpos;
		}

		/// <summary>
		/// Converts direction given by /loc command to heading
		/// </summary>
		/// <param name="dir">/loc command direction</param>
		/// <returns>heading (0 .. 4095)</returns>
		protected static int LocDirectionToHeading(int dir)
		{
			return (int) ((dir + 180)%360*4096.0/360.0);
		}

		/// <summary>
		/// Converts zone coordinate to region
		/// </summary>
		/// <param name="zone">zone coordinate</param>
		/// <param name="zoneOff">zone offset in the region</param>
		/// <returns>region coordinate</returns>
		protected static int ZoneToRegion(int zone, int zoneOff)
		{
			return zone + zoneOff*8192;
		}

		/// <summary>
		/// Initializes location tables
		/// </summary>
		/// <returns>true if no errors</returns>
		protected static bool InitLocationTables()
		{
			try
			{
				int size = (int) eRace._Last + 1;

				ClassicLocations = new HybridDictionary[size];
				ShroudedIslesLocations = new HybridDictionary[size];

				for (int i = 0; i < size; i++)
				{
					ClassicLocations[i] = new HybridDictionary();
					ShroudedIslesLocations[i] = new HybridDictionary();
				}

				ClassicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(55636, 51), ZoneToRegion(13627, 75), 2048, LocDirectionToHeading(98));
				ClassicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(14191, 59), ZoneToRegion(5991, 71), 2308, LocDirectionToHeading(27));
				ClassicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(55220, 51), ZoneToRegion(13894, 75), 2048, LocDirectionToHeading(258));
				ClassicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(14241, 59), ZoneToRegion(6168, 71), 2308, LocDirectionToHeading(171));
				ClassicLocations[(int) eRace.Briton][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(25975, 67), ZoneToRegion(46836, 59), 2896, LocDirectionToHeading(116));
				ClassicLocations[(int) eRace.Briton][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(10486, 67), ZoneToRegion(27879, 59), 2488, LocDirectionToHeading(203));
				ClassicLocations[(int) eRace.Briton][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(12454, 67), ZoneToRegion(28714, 59), 2448, LocDirectionToHeading(135));
				ClassicLocations[(int) eRace.Briton][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(25319, 67), ZoneToRegion(46821, 59), 2896, LocDirectionToHeading(201));
				ClassicLocations[(int) eRace.Briton][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(55354, 51), ZoneToRegion(14075, 75), 2384, LocDirectionToHeading(174));
				ClassicLocations[(int) eRace.Celt][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(9821, 35), ZoneToRegion(26996, 75), 4848, LocDirectionToHeading(34));
				ClassicLocations[(int) eRace.Celt][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(22085, 39), ZoneToRegion(43343, 67), 5456, LocDirectionToHeading(67));
				ClassicLocations[(int) eRace.Celt][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(8040, 35), ZoneToRegion(27182, 75), 4848, LocDirectionToHeading(329));
				ClassicLocations[(int) eRace.Celt][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(22964, 39), ZoneToRegion(43842, 67), 5456, LocDirectionToHeading(169));
				ClassicLocations[(int) eRace.Dwarf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(52175, 92), ZoneToRegion(30137, 82), 4960, LocDirectionToHeading(184));
				ClassicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(49610, 92), ZoneToRegion(55293, 82), 4681, LocDirectionToHeading(341));
				ClassicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(52738, 88), ZoneToRegion(18129, 90), 4600, LocDirectionToHeading(5));
				ClassicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(52369, 92), ZoneToRegion(52579, 82), 4680, LocDirectionToHeading(347));
				ClassicLocations[(int) eRace.Elf][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(22568, 39), ZoneToRegion(42813, 67), 5456, LocDirectionToHeading(217));
				ClassicLocations[(int) eRace.Elf][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(26647, 39), ZoneToRegion(7524, 59), 5200, LocDirectionToHeading(112));
				ClassicLocations[(int) eRace.Elf][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(28167, 39), ZoneToRegion(7923, 59), 5239, LocDirectionToHeading(139));
				ClassicLocations[(int) eRace.Firbolg][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(54813, 35), ZoneToRegion(49963, 51), 5200, LocDirectionToHeading(352));
				ClassicLocations[(int) eRace.Firbolg][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(54154, 35), ZoneToRegion(49302, 51), 5200, LocDirectionToHeading(355));
				ClassicLocations[(int) eRace.HiberniaMinotaur][(int)eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(54154, 35), ZoneToRegion(49302, 51), 5200, LocDirectionToHeading(335));
				ClassicLocations[(int) eRace.HiberniaMinotaur][(int)eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(54154, 35), ZoneToRegion(49302, 51), 5200, LocDirectionToHeading(335));
				
				ClassicLocations[(int) eRace.Frostalf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(53306, 88), ZoneToRegion(19878, 90), 4600, LocDirectionToHeading(281));
				ClassicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				ClassicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(27540, 88), ZoneToRegion(13100, 98), 4408, LocDirectionToHeading(238));
				ClassicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				ClassicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(14191, 59), ZoneToRegion(5991, 71), 2308, LocDirectionToHeading(27));
				ClassicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(55220, 51), ZoneToRegion(13894, 75), 2048, LocDirectionToHeading(258));
				ClassicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(14241, 59), ZoneToRegion(6168, 71), 2308, LocDirectionToHeading(171));
				ClassicLocations[(int) eRace.Highlander][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(20285, 59), ZoneToRegion(40656, 53), 2828, LocDirectionToHeading(156));
				ClassicLocations[(int) eRace.Highlander][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(47287, 59), ZoneToRegion(46585, 53), 2200, LocDirectionToHeading(12));
				ClassicLocations[(int) eRace.Highlander][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(24883, 59), ZoneToRegion(42346, 53), 2284, LocDirectionToHeading(358));
				ClassicLocations[(int) eRace.Kobold][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(27876, 88), ZoneToRegion(13573, 98), 4408, LocDirectionToHeading(266));
				ClassicLocations[(int) eRace.Kobold][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(28094, 88), ZoneToRegion(11926, 98), 4408, LocDirectionToHeading(15));
				ClassicLocations[(int) eRace.Kobold][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(52176, 92), ZoneToRegion(29761, 82), 4960, LocDirectionToHeading(342));
				ClassicLocations[(int) eRace.Kobold][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(28975, 88), ZoneToRegion(14115, 98), 4414, LocDirectionToHeading(146));
				ClassicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(26924, 39), ZoneToRegion(7328, 59), 5330, LocDirectionToHeading(83));
				ClassicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(8969, 35), ZoneToRegion(27165, 75), 4848, LocDirectionToHeading(357));
				ClassicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(54713, 35), ZoneToRegion(51139, 51), 5200, LocDirectionToHeading(126));
				ClassicLocations[(int) eRace.Norseman][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(53306, 88), ZoneToRegion(19878, 90), 4600, LocDirectionToHeading(281));
				ClassicLocations[(int) eRace.Norseman][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				ClassicLocations[(int) eRace.Norseman][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(27540, 88), ZoneToRegion(13100, 98), 4408, LocDirectionToHeading(238));
				ClassicLocations[(int) eRace.Norseman][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				ClassicLocations[(int)eRace.MidgardMinotaur][(int)eCharacterClass.Viking] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				ClassicLocations[(int)eRace.MidgardMinotaur][(int)eCharacterClass.Seer] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				
				ClassicLocations[(int) eRace.Saracen][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(10096, 59), ZoneToRegion(11066, 71), 1948, LocDirectionToHeading(269));
				ClassicLocations[(int) eRace.Saracen][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(10177, 59), ZoneToRegion(11146, 71), 1948, LocDirectionToHeading(211));
				ClassicLocations[(int) eRace.Saracen][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(47648, 59), ZoneToRegion(46053, 53), 2200, LocDirectionToHeading(278));
				ClassicLocations[(int) eRace.AlbionMinotaur][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(47648, 59), ZoneToRegion(46043, 53), 2200, LocDirectionToHeading(278));
				ClassicLocations[(int) eRace.AlbionMinotaur][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(47648, 59), ZoneToRegion(46043, 53), 2200, LocDirectionToHeading(278));
				ClassicLocations[(int) eRace.Shar][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(9821, 35), ZoneToRegion(26996, 75), 4848, LocDirectionToHeading(34));
				ClassicLocations[(int) eRace.Shar][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(22085, 39), ZoneToRegion(43343, 67), 5456, LocDirectionToHeading(67));
				ClassicLocations[(int) eRace.Shar][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(22964, 39), ZoneToRegion(43842, 67), 5456, LocDirectionToHeading(169));
				ClassicLocations[(int) eRace.Troll][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				ClassicLocations[(int) eRace.Troll][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(44180, 92), ZoneToRegion(24390, 106), 4744, LocDirectionToHeading(3));
				ClassicLocations[(int) eRace.Troll][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(43935, 92), ZoneToRegion(25386, 106), 4744, LocDirectionToHeading(123));




				ShroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				ShroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				ShroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(41383, 60), ZoneToRegion(58253, 60), 4800, LocDirectionToHeading(312));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				ShroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				ShroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				ShroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				ShroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				ShroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				ShroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				ShroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				ShroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				ShroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				ShroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				ShroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				ShroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				ShroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				ShroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				ShroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				ShroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				ShroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(41383, 60), ZoneToRegion(58253, 60), 4800, LocDirectionToHeading(312));
				ShroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				ShroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				ShroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				ShroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				ShroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				ShroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				ShroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				ShroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				ShroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				ShroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int) eRace.MidgardMinotaur][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int)eRace.MidgardMinotaur][(int)eCharacterClass.Seer] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				ShroudedIslesLocations[(int)eRace.AlbionMinotaur][(int)eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int)eRace.AlbionMinotaur][(int)eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				ShroudedIslesLocations[(int)eRace.HiberniaMinotaur][(int)eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				ShroudedIslesLocations[(int)eRace.HiberniaMinotaur][(int)eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				
				
				//1.82

				MainTownStartingLocations = new HybridDictionary();

				MainTownStartingLocations[(int)eCharacterClass.Acolyte] =  new StartLocation(562418, 512268, 2500, 2980);
				MainTownStartingLocations[(int)eCharacterClass.AlbionRogue] =  new StartLocation(561956, 512226, 2516, 2116);
				MainTownStartingLocations[(int)eCharacterClass.Disciple] =  new StartLocation(562334, 512160, 2500, 2252);
				MainTownStartingLocations[(int)eCharacterClass.Elementalist] =  new StartLocation(561952, 512651, 2500, 3936);
				MainTownStartingLocations[(int)eCharacterClass.Fighter] =  new StartLocation(562099, 512472, 2500, 3606);
				MainTownStartingLocations[(int)eCharacterClass.Forester] =  new StartLocation(348494, 492021, 5176, 3572);
				MainTownStartingLocations[(int)eCharacterClass.Guardian] =  new StartLocation(347279, 489681, 5200, 2332);
				MainTownStartingLocations[(int)eCharacterClass.Mage] =  new StartLocation(561750, 512694, 2500, 1058);
				MainTownStartingLocations[(int)eCharacterClass.Magician] =  new StartLocation(348457, 491103, 5270, 3174);
				MainTownStartingLocations[(int)eCharacterClass.MidgardRogue] =  new StartLocation(802825, 726238, 4703, 1194);
				MainTownStartingLocations[(int)eCharacterClass.Mystic] =  new StartLocation(802726, 726512, 4694, 1103);
				MainTownStartingLocations[(int)eCharacterClass.Naturalist] =  new StartLocation(348877, 490997, 5414, 2863);
				MainTownStartingLocations[(int)eCharacterClass.Seer] =  new StartLocation(802671, 726752, 4690, 944);
				MainTownStartingLocations[(int)eCharacterClass.Stalker] =  new StartLocation(349404, 489469, 5282, 3003);
				MainTownStartingLocations[(int)eCharacterClass.Viking] = new StartLocation(802869, 726016, 4699, 1399);

			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("InitLocationTables", e);
				return false;
			}

			return true;
		}

		public class StartLocation
		{
			protected int m_x;
			protected int m_y;
			protected int m_z;
			protected int m_heading;

			public int X
			{
				get { return m_x; }
			}

			public int Y
			{
				get { return m_y; }
			}

			public int Z
			{
				get { return m_z; }
			}

			public int Heading
			{
				get { return m_heading; }
			}

			public StartLocation(int x, int y, int z, int heading)
			{
				m_x = x;
				m_y = y;
				m_z = z;
				m_heading = heading;
			}
		}
	}
}
