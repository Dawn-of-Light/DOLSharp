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
using DOL.Events;
using log4net;
using DOL.Database;

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

		protected static HybridDictionary[] m_classicLocations = null;
		protected static HybridDictionary[] m_shroudedIslesLocations = null;

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			bool result = InitLocationTables();
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
			if (log.IsInfoEnabled)
				log.Info("StartupLocations initialized");
		}


		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
			m_classicLocations = null;
			m_shroudedIslesLocations = null;
		}

		/// <summary>
		/// Change location on character creation
		/// </summary>
		public static void CharacterCreation(DOLEvent ev, object sender, EventArgs args)
		{
			CharacterEventArgs chArgs = args as CharacterEventArgs;
			if (chArgs == null)
				return;
			GamePlayer ch = chArgs.Character;
			try
			{
				StartLocation loc = null;

				if (ch.Region.RegionID == 1 || ch.Region.RegionID == 100 || ch.Region.RegionID == 200) // all classic regions
				{
					loc = (StartLocation) m_classicLocations[(int)ch.Race][ch.CharacterClassID];
				}
				else if (ch.Region.RegionID == 51 || ch.Region.RegionID == 151 || ch.Region.RegionID == 181) // all SI regions
				{
					loc = (StartLocation) m_shroudedIslesLocations[(int)ch.Race][ch.CharacterClassID];
				}
				else if (ch.Region.RegionID == 27) // tutorial all realms use the same region
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
				else
				{
					log.DebugFormat("tried to create char in unknown region {0}", ch.Region);
					switch (ch.Realm)
					{
						default: ch.Region = WorldMgr.GetRegion(1); break;
						case (byte)eRealm.Midgard: ch.Region = WorldMgr.GetRegion(100); break;
						case (byte)eRealm.Hibernia: ch.Region = WorldMgr.GetRegion(200); break;
					}
					loc = (StartLocation) m_classicLocations[(int)ch.Race][ch.CharacterClassID];
				}

				if (loc == null)
				{
					log.Warn("startup location not found: char name=" + ch.Name + "; region=" + ch.Region + "; realm=" + ch.Realm + "; class=" + ch.CharacterClassID + " (" + (eCharacterClass) ch.CharacterClassID + "); race=" + ch.Race + " (" + (eRace)ch.Race + ")");
				}
				else
				{
					// can't change region on char creation, that is hardcoded in the client
					ch.Position = new Point(loc.X, loc.Y,  loc.Z);
					ch.Heading = loc.Heading;
				}

				BindCharacter(ch);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("StartupLocations script: error changing location. char name=" + ch.Name + "; region=" + ch.Region + "; realm=" + ch.Realm + "; class=" + ch.CharacterClassID + " (" + (eCharacterClass) ch.CharacterClassID + "); race=" + ch.Race + " (" + (eRace)ch.Race + ")", e);
			}
		}

		/// <summary>
		/// Binds character to current location
		/// </summary>
		/// <param name="ch"></param>
		protected static void BindCharacter(GamePlayer ch)
		{
			ch.BindRegion = ch.Region;
			ch.BindHeading = ch.Heading;
			ch.BindPosition = ch.Position;
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

				m_classicLocations = new HybridDictionary[size];
				m_shroudedIslesLocations = new HybridDictionary[size];

				for (int i = 0; i < size; i++)
				{
					m_classicLocations[i] = new HybridDictionary();
					m_shroudedIslesLocations[i] = new HybridDictionary();
				}

				m_classicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(55636, 51), ZoneToRegion(13627, 75), 2048, LocDirectionToHeading(98));
				m_classicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(14191, 59), ZoneToRegion(5991, 71), 2308, LocDirectionToHeading(27));
				m_classicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(55220, 51), ZoneToRegion(13894, 75), 2048, LocDirectionToHeading(258));
				m_classicLocations[(int) eRace.Avalonian][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(14241, 59), ZoneToRegion(6168, 71), 2308, LocDirectionToHeading(171));
				m_classicLocations[(int) eRace.Briton][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(25975, 67), ZoneToRegion(46836, 59), 2896, LocDirectionToHeading(116));
				m_classicLocations[(int) eRace.Briton][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(10486, 67), ZoneToRegion(27879, 59), 2488, LocDirectionToHeading(203));
				m_classicLocations[(int) eRace.Briton][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(12454, 67), ZoneToRegion(28714, 59), 2448, LocDirectionToHeading(135));
				m_classicLocations[(int) eRace.Briton][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(25319, 67), ZoneToRegion(46821, 59), 2896, LocDirectionToHeading(201));
				m_classicLocations[(int) eRace.Briton][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(55354, 51), ZoneToRegion(14075, 75), 2384, LocDirectionToHeading(174));
				m_classicLocations[(int) eRace.Celt][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(9821, 35), ZoneToRegion(26996, 75), 4848, LocDirectionToHeading(34));
				m_classicLocations[(int) eRace.Celt][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(22085, 39), ZoneToRegion(43343, 67), 5456, LocDirectionToHeading(67));
				m_classicLocations[(int) eRace.Celt][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(8040, 35), ZoneToRegion(27182, 75), 4848, LocDirectionToHeading(329));
				m_classicLocations[(int) eRace.Celt][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(22964, 39), ZoneToRegion(43842, 67), 5456, LocDirectionToHeading(169));
				m_classicLocations[(int) eRace.Dwarf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(52175, 92), ZoneToRegion(30137, 82), 4960, LocDirectionToHeading(184));
				m_classicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(49610, 92), ZoneToRegion(55293, 82), 4681, LocDirectionToHeading(341));
				m_classicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(52738, 88), ZoneToRegion(18129, 90), 4600, LocDirectionToHeading(5));
				m_classicLocations[(int) eRace.Dwarf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(52369, 92), ZoneToRegion(52579, 82), 4680, LocDirectionToHeading(347));
				m_classicLocations[(int) eRace.Elf][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(22568, 39), ZoneToRegion(42813, 67), 5456, LocDirectionToHeading(217));
				m_classicLocations[(int) eRace.Elf][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(26647, 39), ZoneToRegion(7524, 59), 5200, LocDirectionToHeading(112));
				m_classicLocations[(int) eRace.Elf][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(28167, 39), ZoneToRegion(7923, 59), 5239, LocDirectionToHeading(139));
				m_classicLocations[(int) eRace.Firbolg][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(54813, 35), ZoneToRegion(49963, 51), 5200, LocDirectionToHeading(352));
				m_classicLocations[(int) eRace.Firbolg][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(54154, 35), ZoneToRegion(49302, 51), 5200, LocDirectionToHeading(355));
				m_classicLocations[(int) eRace.Frostalf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(53306, 88), ZoneToRegion(19878, 90), 4600, LocDirectionToHeading(281));
				m_classicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				m_classicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(27540, 88), ZoneToRegion(13100, 98), 4408, LocDirectionToHeading(238));
				m_classicLocations[(int) eRace.Frostalf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				m_classicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(14191, 59), ZoneToRegion(5991, 71), 2308, LocDirectionToHeading(27));
				m_classicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(55220, 51), ZoneToRegion(13894, 75), 2048, LocDirectionToHeading(258));
				m_classicLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(14241, 59), ZoneToRegion(6168, 71), 2308, LocDirectionToHeading(171));
				m_classicLocations[(int) eRace.Highlander][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(20285, 59), ZoneToRegion(40656, 53), 2828, LocDirectionToHeading(156));
				m_classicLocations[(int) eRace.Highlander][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(47287, 59), ZoneToRegion(46585, 53), 2200, LocDirectionToHeading(12));
				m_classicLocations[(int) eRace.Highlander][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(24883, 59), ZoneToRegion(42346, 53), 2284, LocDirectionToHeading(358));
				m_classicLocations[(int) eRace.Kobold][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(27876, 88), ZoneToRegion(13573, 98), 4408, LocDirectionToHeading(266));
				m_classicLocations[(int) eRace.Kobold][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(28094, 88), ZoneToRegion(11926, 98), 4408, LocDirectionToHeading(15));
				m_classicLocations[(int) eRace.Kobold][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(52176, 92), ZoneToRegion(29761, 82), 4960, LocDirectionToHeading(342));
				m_classicLocations[(int) eRace.Kobold][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(28975, 88), ZoneToRegion(14115, 98), 4414, LocDirectionToHeading(146));
				m_classicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(26924, 39), ZoneToRegion(7328, 59), 5330, LocDirectionToHeading(83));
				m_classicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(8969, 35), ZoneToRegion(27165, 75), 4848, LocDirectionToHeading(357));
				m_classicLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(54713, 35), ZoneToRegion(51139, 51), 5200, LocDirectionToHeading(126));
				m_classicLocations[(int) eRace.Norseman][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(53306, 88), ZoneToRegion(19878, 90), 4600, LocDirectionToHeading(281));
				m_classicLocations[(int) eRace.Norseman][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				m_classicLocations[(int) eRace.Norseman][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(27540, 88), ZoneToRegion(13100, 98), 4408, LocDirectionToHeading(238));
				m_classicLocations[(int) eRace.Norseman][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(53803, 88), ZoneToRegion(16475, 90), 4600, LocDirectionToHeading(25));
				m_classicLocations[(int) eRace.Saracen][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(10096, 59), ZoneToRegion(11066, 71), 1948, LocDirectionToHeading(269));
				m_classicLocations[(int) eRace.Saracen][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(10177, 59), ZoneToRegion(11146, 71), 1948, LocDirectionToHeading(211));
				m_classicLocations[(int) eRace.Saracen][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(47648, 59), ZoneToRegion(46053, 53), 2200, LocDirectionToHeading(278));
				m_classicLocations[(int) eRace.Shar][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(9821, 35), ZoneToRegion(26996, 75), 4848, LocDirectionToHeading(34));
				m_classicLocations[(int) eRace.Shar][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(22085, 39), ZoneToRegion(43343, 67), 5456, LocDirectionToHeading(67));
				m_classicLocations[(int) eRace.Shar][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(22964, 39), ZoneToRegion(43842, 67), 5456, LocDirectionToHeading(169));
				m_classicLocations[(int) eRace.Troll][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(54582, 88), ZoneToRegion(16626, 90), 4600, LocDirectionToHeading(69));
				m_classicLocations[(int) eRace.Troll][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(44180, 92), ZoneToRegion(24390, 106), 4744, LocDirectionToHeading(3));
				m_classicLocations[(int) eRace.Troll][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(43935, 92), ZoneToRegion(25386, 106), 4744, LocDirectionToHeading(123));

				m_shroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				m_shroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				m_shroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.Avalonian][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(41383, 60), ZoneToRegion(58253, 60), 4800, LocDirectionToHeading(312));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.Briton][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				m_shroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				m_shroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				m_shroudedIslesLocations[(int) eRace.Celt][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				m_shroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				m_shroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				m_shroudedIslesLocations[(int) eRace.Dwarf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				m_shroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Elf][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				m_shroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				m_shroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Firbolg][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				m_shroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				m_shroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				m_shroudedIslesLocations[(int) eRace.Frostalf][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				m_shroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Elementalist] = new StartLocation(ZoneToRegion(45096, 60), ZoneToRegion(57536, 60), 4800, LocDirectionToHeading(186));
				m_shroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.HalfOgre][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				m_shroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.Acolyte] = new StartLocation(ZoneToRegion(43421, 60), ZoneToRegion(58659, 60), 4827, LocDirectionToHeading(176));
				m_shroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Highlander][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(41383, 60), ZoneToRegion(58253, 60), 4800, LocDirectionToHeading(312));
				m_shroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.Inconnu][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				m_shroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				m_shroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				m_shroudedIslesLocations[(int) eRace.Kobold][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				m_shroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Lurikeen][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				m_shroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				m_shroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				m_shroudedIslesLocations[(int) eRace.Norseman][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				m_shroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.AlbionRogue] = new StartLocation(ZoneToRegion(42034, 60), ZoneToRegion(55725, 60), 4800, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Disciple] = new StartLocation(ZoneToRegion(41383, 60), ZoneToRegion(58253, 60), 4800, LocDirectionToHeading(312));
				m_shroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Fighter] = new StartLocation(ZoneToRegion(45234, 60), ZoneToRegion(56900, 60), 4800, LocDirectionToHeading(79));
				m_shroudedIslesLocations[(int) eRace.Saracen][(int) eCharacterClass.Mage] = new StartLocation(ZoneToRegion(43786, 60), ZoneToRegion(57553, 60), 4800, LocDirectionToHeading(95));
				m_shroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Magician] = new StartLocation(ZoneToRegion(29052, 48), ZoneToRegion(49605, 48), 5952, LocDirectionToHeading(236));
				m_shroudedIslesLocations[(int) eRace.Shar][(int) eCharacterClass.Stalker] = new StartLocation(ZoneToRegion(27375, 48), ZoneToRegion(51307, 48), 5952, LocDirectionToHeading(323));
				m_shroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Forester] = new StartLocation(ZoneToRegion(30768, 48), ZoneToRegion(53079, 48), 5952, LocDirectionToHeading(100));
				m_shroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Guardian] = new StartLocation(ZoneToRegion(31470, 48), ZoneToRegion(51847, 48), 5952, LocDirectionToHeading(167));
				m_shroudedIslesLocations[(int) eRace.Sylvan][(int) eCharacterClass.Naturalist] = new StartLocation(ZoneToRegion(29769, 48), ZoneToRegion(52637, 48), 5952, LocDirectionToHeading(207));
				m_shroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Seer] = new StartLocation(ZoneToRegion(42531, 30), ZoneToRegion(44266, 38), 3488, LocDirectionToHeading(17));
				m_shroudedIslesLocations[(int) eRace.Troll][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
				m_shroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.MidgardRogue] = new StartLocation(ZoneToRegion(43183, 30), ZoneToRegion(44381, 38), 3866, LocDirectionToHeading(232));
				m_shroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.Mystic] = new StartLocation(ZoneToRegion(44075, 30), ZoneToRegion(44629, 38), 3866, LocDirectionToHeading(144));
				m_shroudedIslesLocations[(int) eRace.Valkyn][(int) eCharacterClass.Viking] = new StartLocation(ZoneToRegion(41922, 30), ZoneToRegion(44001, 38), 3488, LocDirectionToHeading(315));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("InitLocationTables", e);
				return false;
			}

			return true;
		}

		protected class StartLocation
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