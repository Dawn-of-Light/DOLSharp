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

namespace DOL.GS.Keeps
{
	public class TemplateMgr
	{
		public static void RefreshTemplate(GameKeepGuard guard)
		{
			guard.RefreshTemplate();
		}

		#region Hastener Models
		public static ushort AlbionHastener = 244;
		public static ushort MidgardHastener = 22;
		public static ushort HiberniaHastener = 1910;
		#endregion

		#region AlbionClassModels
		public static ushort BritonMale = 32;
		public static ushort BritonFemale = 35;
		public static ushort HighlanderMale = 39;
		public static ushort HighlanderFemale = 43;
		public static ushort SaracenMale = 48;
		public static ushort SaracenFemale = 52;
		public static ushort AvalonianMale = 61;
		public static ushort AvalonianFemale = 65;
		public static ushort IcconuMale = 716;
		public static ushort IcconuFemale = 724;
		public static ushort HalfOgreMale = 1008;
		public static ushort HalfOgreFemale = 1020;
        public static ushort MinotaurMaleAlb = 1395;
		#endregion
		#region MidgardClassModels
		public static ushort TrollMale = 137;
		public static ushort TrollFemale = 145;
		public static ushort NorseMale = 503;
		public static ushort NorseFemale = 507;
		public static ushort KoboldMale = 169;
		public static ushort KoboldFemale = 177;
		public static ushort DwarfMale = 185;
		public static ushort DwarfFemale = 193;
		public static ushort ValkynMale = 773;
		public static ushort ValkynFemale = 781;
		public static ushort FrostalfMale = 1051;
		public static ushort FrostalfFemale = 1063;
        public static ushort MinotaurMaleMid = 1407;
		#endregion
		#region HiberniaClassModels
		public static ushort FirbolgMale = 286;
		public static ushort FirbolgFemale = 294;
		public static ushort CeltMale = 302;
		public static ushort CeltFemale = 310;
		public static ushort LurikeenMale = 318;
		public static ushort LurikeenFemale = 326;
		public static ushort ElfMale = 334;
		public static ushort ElfFemale = 342;
		public static ushort SharMale = 1075;
		public static ushort SharFemale = 1087;
		public static ushort SylvianMale = 700;
		public static ushort SylvianFemale = 708;
        public static ushort MinotaurMaleHib = 1419;
		#endregion

	}
}
