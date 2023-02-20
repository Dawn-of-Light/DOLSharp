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
using System.Collections.Generic;

namespace DOL.GS.Realm
{
	public class PlayerRace
	{
		public eRace ID { get; }
		public virtual eDAoCExpansion Expansion { get; }
		public eRealm Realm { get; }
		private eLivingModel FemaleModel { get; }
		private eLivingModel MaleModel { get; }

        private PlayerRace()
		{ 
			ID = eRace.Unknown;
		}

		private PlayerRace(eRace race, eRealm realm, eDAoCExpansion expansion, eLivingModel maleModel, eLivingModel femaleModel)
        {
			ID = race;
			Realm = realm;
			Expansion = expansion;
			MaleModel = maleModel;
			FemaleModel = femaleModel;
        }

        public static PlayerRace Unknown { get; } = new PlayerRace();

		private static Dictionary<eRace, PlayerRace> races = new Dictionary<eRace, PlayerRace>()
		{
			{ eRace.Briton, new PlayerRace( eRace.Briton, eRealm.Albion, eDAoCExpansion.Classic, eLivingModel.BritonMale, eLivingModel.BritonFemale) } ,
			{ eRace.Highlander, new PlayerRace(eRace.Highlander, eRealm.Albion, eDAoCExpansion.Classic, eLivingModel.HighlanderMale, eLivingModel.HighlanderFemale) } ,
			{ eRace.Saracen, new PlayerRace(eRace.Saracen, eRealm.Albion, eDAoCExpansion.Classic, eLivingModel.SaracenMale, eLivingModel.SaracenFemale) } ,
			{ eRace.Avalonian, new PlayerRace(eRace.Avalonian, eRealm.Albion, eDAoCExpansion.Classic, eLivingModel.AvalonianMale, eLivingModel.AvalonianFemale) } ,
			{ eRace.Inconnu, new PlayerRace(eRace.Inconnu, eRealm.Albion, eDAoCExpansion.ShroudedIsles, eLivingModel.InconnuMale, eLivingModel.InconnuFemale) } ,
			{ eRace.HalfOgre, new PlayerRace(eRace.HalfOgre, eRealm.Albion, eDAoCExpansion.Catacombs, eLivingModel.HalfOgreMale, eLivingModel.HalfOgreFemale) } ,
			{ eRace.Korazh, new PlayerRace(eRace.Korazh, eRealm.Albion, eDAoCExpansion.LabyrinthOfTheMinotaur, eLivingModel.MinotaurMaleAlb, eLivingModel.None) },
			{ eRace.Troll, new PlayerRace(eRace.Troll, eRealm.Midgard, eDAoCExpansion.Classic, eLivingModel.TrollMale, eLivingModel.TrollFemale) },
			{ eRace.Norseman, new PlayerRace(eRace.Norseman, eRealm.Midgard, eDAoCExpansion.Classic, eLivingModel.NorseMale, eLivingModel.NorseFemale) } ,
			{ eRace.Kobold, new PlayerRace(eRace.Kobold, eRealm.Midgard, eDAoCExpansion.Classic, eLivingModel.KoboldMale, eLivingModel.KoboldFemale) } ,
			{ eRace.Dwarf, new PlayerRace(eRace.Dwarf, eRealm.Midgard, eDAoCExpansion.Classic, eLivingModel.DwarfMale, eLivingModel.DwarfFemale) } ,
			{ eRace.Valkyn, new PlayerRace(eRace.Valkyn, eRealm.Midgard, eDAoCExpansion.ShroudedIsles, eLivingModel.ValkynMale, eLivingModel.ValkynFemale) } ,
			{ eRace.Frostalf, new PlayerRace(eRace.Frostalf, eRealm.Midgard, eDAoCExpansion.Catacombs, eLivingModel.FrostalfMale, eLivingModel.FrostalfFemale) } ,
			{ eRace.Deifrang, new PlayerRace(eRace.Deifrang, eRealm.Midgard, eDAoCExpansion.LabyrinthOfTheMinotaur, eLivingModel.MinotaurMaleMid, eLivingModel.None) } ,
			{ eRace.Firbolg, new PlayerRace(eRace.Firbolg, eRealm.Hibernia, eDAoCExpansion.Classic, eLivingModel.FirbolgMale, eLivingModel.FirbolgFemale) } ,
			{ eRace.Celt, new PlayerRace(eRace.Celt, eRealm.Hibernia, eDAoCExpansion.Classic, eLivingModel.CeltMale, eLivingModel.CeltFemale) } ,
			{ eRace.Lurikeen, new PlayerRace(eRace.Lurikeen, eRealm.Hibernia, eDAoCExpansion.Classic, eLivingModel.LurikeenMale, eLivingModel.LurikeenFemale) } ,
			{ eRace.Elf, new PlayerRace(eRace.Elf, eRealm.Hibernia, eDAoCExpansion.Classic, eLivingModel.ElfMale, eLivingModel.ElfFemale) } ,
			{ eRace.Sylvan, new PlayerRace(eRace.Sylvan, eRealm.Hibernia, eDAoCExpansion.ShroudedIsles, eLivingModel.SylvanMale, eLivingModel.SylvanFemale) } ,
			{ eRace.Shar, new PlayerRace(eRace.Shar, eRealm.Hibernia, eDAoCExpansion.Catacombs, eLivingModel.SharMale, eLivingModel.SharFemale) } ,
			{ eRace.Graoch, new PlayerRace(eRace.Graoch, eRealm.Hibernia, eDAoCExpansion.LabyrinthOfTheMinotaur, eLivingModel.MinotaurMaleHib, eLivingModel.None) } ,
		};

        public static PlayerRace GetRace(int id)
        {
            races.TryGetValue((eRace)id, out var race);
            if (race == null) return Unknown;
            return race;
        }

		public eLivingModel GetModel(eGender gender)
        {
			if (gender == eGender.Male) return MaleModel;
			else if (gender == eGender.Female) return FemaleModel;
			else return eLivingModel.None;
		}

		public static List<PlayerRace> AllRaces
		{
			get
			{
				var allRaces = new List<PlayerRace>();
				foreach (var race in races)
				{
					allRaces.Add(race.Value);
				}
				return allRaces;
			}
		}

		public static PlayerRace Briton => races[eRace.Briton];
		public static PlayerRace Highlander => races[eRace.Highlander];
		public static PlayerRace Saracen => races[eRace.Saracen];
		public static PlayerRace Avalonian => races[eRace.Avalonian];
		public static PlayerRace Inconnu => races[eRace.Inconnu];
		public static PlayerRace HalfOgre => races[eRace.HalfOgre];
		public static PlayerRace Korazh => races[eRace.Korazh];
		public static PlayerRace Troll => races[eRace.Troll];
		public static PlayerRace Norseman => races[eRace.Norseman];
		public static PlayerRace Kobold => races[eRace.Kobold];
		public static PlayerRace Dwarf => races[eRace.Dwarf];
		public static PlayerRace Valkyn => races[eRace.Valkyn];
		public static PlayerRace Frostalf => races[eRace.Frostalf];
		public static PlayerRace Deifrang => races[eRace.Deifrang];
		public static PlayerRace Firbolg => races[eRace.Firbolg];
		public static PlayerRace Celt => races[eRace.Celt];
		public static PlayerRace Lurikeen => races[eRace.Lurikeen];
		public static PlayerRace Elf => races[eRace.Elf];
		public static PlayerRace Shar => races[eRace.Shar];
		public static PlayerRace Sylvan => races[eRace.Sylvan];
		public static PlayerRace Graoch => races[eRace.Graoch];

        public override bool Equals(object obj)
        {
            if(obj is PlayerRace compareRace)
            {
				return compareRace.ID == ID;
            }
			return false;
        }

        public override int GetHashCode()
        {
			return (int)ID;
        }
    }

	public enum eDAoCExpansion : byte
	{
		Classic = 1,
		ShroudedIsles = 2,
		TrialsOfAtlantis = 3,
		Catacombs = 4,
		DarknessRising = 5,
		LabyrinthOfTheMinotaur = 6
	}
}
