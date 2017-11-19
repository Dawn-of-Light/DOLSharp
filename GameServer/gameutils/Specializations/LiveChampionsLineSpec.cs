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
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    /// <summary>
    /// MiniLineSpecialization are "Mini-Spec" Used to match Sub-Spec (CL ~ Subclass) Skills
    /// They shouldn't be attached to a career, Global Champion or other Custom Career will handle them and display skills.
    /// </summary>
    public class MiniLineSpecialization : UntrainableSpecialization
	{		
		public MiniLineSpecialization(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
		
		/// <summary>
		/// Always Empty Collection
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public override List<Ability> GetAbilitiesForLiving(GameLiving living)
		{
			return new List<Ability>();
		}
		
		/// <summary>
		/// Always Empty Collection
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public override List<SpellLine> GetSpellLinesForLiving(GameLiving living)
		{
			return new List<SpellLine>();
		}
		
		/// <summary>
		/// Always Empty Collection
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living)
		{
			return new Dictionary<SpellLine, List<Skill>>();
		}
		
		/// <summary>
		/// Always Empty Collection
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public override List<DOL.GS.Styles.Style> GetStylesForLiving(GameLiving living)
		{
			return new List<DOL.GS.Styles.Style>();
		}
		
		/// <summary>
		/// Retrieve the Mini Spec Skill List, it will be used as a 0-index skill line
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public virtual List<Tuple<Skill, Skill>> GetMiniLineSkillsForLiving(GameLiving living, int level)
		{
			var abilities = base.PretendAbilitiesForLiving(living, level);
			var styles = base.PretendStylesForLiving(living, level);
			var spells = base.PretendLinesSpellsForLiving(living, level);

			return abilities.Select(a => new Tuple<Skill, Skill, int>(a, null, a.SpecLevelRequirement))
				.Union(styles.Select(s => new Tuple<Skill, Skill, int>(s, null, s.SpecLevelRequirement)))
				.Union(spells.Select(kv => kv.Value.Select(e => new Tuple<Skill, Skill, int>(e, kv.Key, e.Level))).SelectMany(e => e))
				.OrderBy(t => t.Item3).Select(sk => new Tuple<Skill, Skill>(sk.Item1, sk.Item2)).Take(level).ToList();
		}
	}
	
	/// <summary>
	/// LiveChampionLineSpec are Mini-Lines that match a base class type
	/// Each "Grouped" Spec that are all displayed by the same trainer should use the same class
	/// Subclass is only used for grouping in Database, removing the need for a dedicated table
	/// Each Spec will be save to the according level to keep track of choosen paths.
	/// </summary>
	public class LiveChampionsLineSpec : MiniLineSpecialization
	{
		public LiveChampionsLineSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLAcolyteSpec : LiveChampionsLineSpec
	{
		public LiveCLAcolyteSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLAlbionRogueSpec : LiveChampionsLineSpec
	{
		public LiveCLAlbionRogueSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLDiscipleSpec : LiveChampionsLineSpec
	{
		public LiveCLDiscipleSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLElementalistSpec : LiveChampionsLineSpec
	{
		public LiveCLElementalistSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLFighterSpec : LiveChampionsLineSpec
	{
		public LiveCLFighterSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLForesterSpec : LiveChampionsLineSpec
	{
		public LiveCLForesterSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLGuardianSpec : LiveChampionsLineSpec
	{
		public LiveCLGuardianSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLMageSpec : LiveChampionsLineSpec
	{
		public LiveCLMageSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLMagicianSpec : LiveChampionsLineSpec
	{
		public LiveCLMagicianSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLMidgardRogueSpec : LiveChampionsLineSpec
	{
		public LiveCLMidgardRogueSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLMysticSpec : LiveChampionsLineSpec
	{
		public LiveCLMysticSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLNaturalistSpec : LiveChampionsLineSpec
	{
		public LiveCLNaturalistSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLSeerSpec : LiveChampionsLineSpec
	{
		public LiveCLSeerSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLStalkerSpec : LiveChampionsLineSpec
	{
		public LiveCLStalkerSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
	public class LiveCLVikingSpec : LiveChampionsLineSpec
	{
		public LiveCLVikingSpec(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
	}
}
