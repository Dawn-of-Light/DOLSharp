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
namespace DOL.GS.Realm
{
	public abstract class PlayerRace
	{
		public abstract eRace ID { get; }
		public virtual eDAoCExpansion Expansion => eDAoCExpansion.Classic;
		public abstract eRealm Realm { get; }
		protected abstract eLivingModel FemaleModel { get; }
		protected abstract eLivingModel MaleModel { get; }

		public eLivingModel GetModel(eGender gender)
        {
			if (gender == eGender.Male) return MaleModel;
			else if (gender == eGender.Female) return FemaleModel;
			else return eLivingModel.None;
		}

		public static PlayerRace Briton => new Briton();
		public static PlayerRace Highlander => new Highlander();
		public static PlayerRace Saracen => new Saracen();
		public static PlayerRace Avalonian => new Avalonian();
		public static PlayerRace Inconnu => new Inconnu();
		public static PlayerRace HalfOgre => new HalfOgre();
		public static PlayerRace Korazh => new Korazh();
		public static PlayerRace Troll => new Troll();
		public static PlayerRace Norseman => new Norseman();
		public static PlayerRace Kobold => new Kobold();
		public static PlayerRace Dwarf => new Dwarf();
		public static PlayerRace Valkyn => new Valkyn();
		public static PlayerRace Frostalf => new Frostalf();
		public static PlayerRace Deifrang => new Deifrang();
		public static PlayerRace Firbolg => new Firbolg();
		public static PlayerRace Celt => new Celt();
		public static PlayerRace Lurikeen => new Lurikeen();
		public static PlayerRace Elf => new Elf();
		public static PlayerRace Shar => new Shar();
		public static PlayerRace Sylvan => new Sylvan();
		public static PlayerRace Graoch => new Graoch();

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

	public class Briton : PlayerRace
	{
		public override eRace ID => eRace.Briton;
		public override eRealm Realm => eRealm.Albion;
		protected override eLivingModel MaleModel => eLivingModel.BritonMale;
		protected override eLivingModel FemaleModel => eLivingModel.BritonFemale;
	}

	public class Highlander : PlayerRace
	{
		public override eRace ID => eRace.Highlander;
		public override eRealm Realm => eRealm.Albion;
		protected override eLivingModel MaleModel => eLivingModel.HighlanderMale;
		protected override eLivingModel FemaleModel => eLivingModel.HighlanderFemale;
	}

	public class Saracen : PlayerRace
	{
		public override eRace ID => eRace.Saracen;
		public override eRealm Realm => eRealm.Albion;
		protected override eLivingModel MaleModel => eLivingModel.SaracenMale;
		protected override eLivingModel FemaleModel => eLivingModel.SaracenFemale;
	}

	public class Avalonian : PlayerRace
	{
		public override eRace ID => eRace.Avalonian;
		public override eRealm Realm => eRealm.Albion;
		protected override eLivingModel MaleModel => eLivingModel.AvalonianMale;
		protected override eLivingModel FemaleModel => eLivingModel.AvalonianFemale;
	}

	public class Inconnu : PlayerRace
	{
		public override eRace ID => eRace.Inconnu;
		public override eRealm Realm => eRealm.Albion;
		public override eDAoCExpansion Expansion => eDAoCExpansion.ShroudedIsles;
		protected override eLivingModel MaleModel => eLivingModel.InconnuMale;
		protected override eLivingModel FemaleModel => eLivingModel.InconnuFemale;
	}

	public class HalfOgre : PlayerRace
	{
		public override eRace ID => eRace.HalfOgre;
		public override eRealm Realm => eRealm.Albion;
		public override eDAoCExpansion Expansion => eDAoCExpansion.Catacombs;
		protected override eLivingModel MaleModel => eLivingModel.HalfOgreMale;
		protected override eLivingModel FemaleModel => eLivingModel.HalfOgreFemale;
	}

	public class Troll : PlayerRace
	{
		public override eRace ID => eRace.Troll;
		public override eRealm Realm => eRealm.Midgard;
		protected override eLivingModel MaleModel => eLivingModel.TrollMale;
		protected override eLivingModel FemaleModel => eLivingModel.TrollFemale;
	}

	public class Norseman : PlayerRace
	{
		public override eRace ID => eRace.Norseman;
		public override eRealm Realm => eRealm.Midgard;
		protected override eLivingModel MaleModel => eLivingModel.NorseMale;
		protected override eLivingModel FemaleModel => eLivingModel.NorseFemale;
	}

	public class Kobold : PlayerRace
	{
		public override eRace ID => eRace.Kobold;
		public override eRealm Realm => eRealm.Midgard;
		protected override eLivingModel MaleModel => eLivingModel.KoboldMale;
		protected override eLivingModel FemaleModel => eLivingModel.KoboldFemale;
	}

	public class Dwarf : PlayerRace
	{
		public override eRace ID => eRace.Dwarf;
		public override eRealm Realm => eRealm.Midgard;
		protected override eLivingModel MaleModel => eLivingModel.DwarfMale;
		protected override eLivingModel FemaleModel => eLivingModel.DwarfFemale;
	}

	public class Valkyn : PlayerRace
	{
		public override eRace ID => eRace.Valkyn;
		public override eRealm Realm => eRealm.Midgard;
		public override eDAoCExpansion Expansion => eDAoCExpansion.ShroudedIsles;
		protected override eLivingModel MaleModel => eLivingModel.ValkynMale;
		protected override eLivingModel FemaleModel => eLivingModel.ValkynFemale;
	}

	public class Frostalf : PlayerRace
	{
		public override eRace ID => eRace.Frostalf;
		public override eRealm Realm => eRealm.Midgard;
		public override eDAoCExpansion Expansion => eDAoCExpansion.Catacombs;
		protected override eLivingModel MaleModel => eLivingModel.FrostalfMale;
		protected override eLivingModel FemaleModel => eLivingModel.FrostalfFemale;
	}

	public class Firbolg : PlayerRace
	{
		public override eRace ID => eRace.Firbolg;
		public override eRealm Realm => eRealm.Hibernia;
		protected override eLivingModel MaleModel => eLivingModel.FirbolgMale;
		protected override eLivingModel FemaleModel => eLivingModel.FirbolgFemale;
	}

	public class Celt : PlayerRace
	{
		public override eRace ID => eRace.Celt;
		public override eRealm Realm => eRealm.Hibernia;
		protected override eLivingModel MaleModel => eLivingModel.CeltMale;
		protected override eLivingModel FemaleModel => eLivingModel.CeltFemale;
	}

	public class Lurikeen : PlayerRace
	{
		public override eRace ID => eRace.Lurikeen;
		public override eRealm Realm => eRealm.Hibernia;
		protected override eLivingModel MaleModel => eLivingModel.LurikeenMale;
		protected override eLivingModel FemaleModel => eLivingModel.LurikeenFemale;
	}

	public class Elf : PlayerRace
	{
		public override eRace ID => eRace.Elf;
		public override eRealm Realm => eRealm.Hibernia;
		protected override eLivingModel MaleModel => eLivingModel.ElfMale;
		protected override eLivingModel FemaleModel => eLivingModel.ElfFemale;
	}

	public class Shar : PlayerRace
	{
		public override eRace ID => eRace.Shar;
		public override eRealm Realm => eRealm.Hibernia;
		public override eDAoCExpansion Expansion => eDAoCExpansion.Catacombs;
		protected override eLivingModel MaleModel => eLivingModel.SharMale;
		protected override eLivingModel FemaleModel => eLivingModel.SharFemale;
	}

	public class Sylvan : PlayerRace
	{
		public override eRace ID => eRace.Sylvan;
		public override eRealm Realm => eRealm.Hibernia;
		public override eDAoCExpansion Expansion => eDAoCExpansion.ShroudedIsles;
		protected override eLivingModel MaleModel => eLivingModel.SylvanMale;
		protected override eLivingModel FemaleModel => eLivingModel.SylvanFemale;
	}

	public class Deifrang : PlayerRace
	{
		public override eRace ID => eRace.Deifrang;
		public override eRealm Realm => eRealm.Midgard;
		public override eDAoCExpansion Expansion => eDAoCExpansion.LabyrinthOfTheMinotaur;
		protected override eLivingModel MaleModel => eLivingModel.MinotaurMaleMid;
		protected override eLivingModel FemaleModel => eLivingModel.None;
	}

	public class Graoch : PlayerRace
	{
		public override eRace ID => eRace.Graoch;
		public override eRealm Realm => eRealm.Hibernia;
		public override eDAoCExpansion Expansion => eDAoCExpansion.LabyrinthOfTheMinotaur;
		protected override eLivingModel MaleModel => eLivingModel.MinotaurMaleHib;
		protected override eLivingModel FemaleModel => eLivingModel.None;
	}

	public class Korazh : PlayerRace
	{
		public override eRace ID => eRace.Korazh;
		public override eRealm Realm => eRealm.Albion;
		public override eDAoCExpansion Expansion => eDAoCExpansion.LabyrinthOfTheMinotaur;
		protected override eLivingModel MaleModel => eLivingModel.MinotaurMaleAlb;
		protected override eLivingModel FemaleModel => eLivingModel.None;
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

	public enum eLivingModel : ushort
	{
		None = 0,
		#region AlbionClassModels
		BritonMale = 32,
		BritonFemale = 35,
		HighlanderMale = 39,
		HighlanderFemale = 43,
		SaracenMale = 48,
		SaracenFemale = 52,
		AvalonianMale = 61,
		AvalonianFemale = 65,
		InconnuMale = 716,
		InconnuFemale = 724,
		HalfOgreMale = 1008,
		HalfOgreFemale = 1020,
		MinotaurMaleAlb = 1395,
		#endregion
		#region MidgardClassModels
		TrollMale = 137,
		TrollFemale = 145,
		NorseMale = 503,
		NorseFemale = 507,
		KoboldMale = 169,
		KoboldFemale = 177,
		DwarfMale = 185,
		DwarfFemale = 193,
		ValkynMale = 773,
		ValkynFemale = 781,
		FrostalfMale = 1051,
		FrostalfFemale = 1063,
		MinotaurMaleMid = 1407,
		#endregion
		#region HiberniaClassModels
		FirbolgMale = 286,
		FirbolgFemale = 294,
		CeltMale = 302,
		CeltFemale = 310,
		LurikeenMale = 318,
		LurikeenFemale = 326,
		ElfMale = 334,
		ElfFemale = 342,
		SharMale = 1075,
		SharFemale = 1087,
		SylvanMale = 700,
		SylvanFemale = 708,
		MinotaurMaleHib = 1419,
		#endregion
	}
}
