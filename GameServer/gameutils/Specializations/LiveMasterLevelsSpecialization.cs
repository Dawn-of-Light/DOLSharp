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
using System.Linq;

namespace DOL.GS
{
    public interface IMasterLevelsSpecialization
	{
	}
	
	/// <summary>
	/// Description of LiveMasterLevelsSpecialization.
	/// </summary>
	public class LiveMasterLevelsSpecialization : CareerSpecialization, IMasterLevelsSpecialization
	{		
		// Level of ML is always 50 should be based on Player.MaxLevel...
		public override int Level {
			get { return 50; }
			set { base.Level = value; }
		}
		
		public LiveMasterLevelsSpecialization(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{			
		}
		
		/// <summary>
		/// Don't send Abilities
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override List<Ability> GetAbilitiesForLiving(GameLiving living, int level)
		{
			return new List<Ability>();
		}
		
		/// <summary>
		/// Return the SpellLine(s) attributed to this ML
		/// This shouldn't be used as we set Hybrid Spell List
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override List<SpellLine> GetSpellLinesForLiving(GameLiving living)
		{
			var player = living as GamePlayer;
			return player != null ? GetSpellLinesForLiving(living, player.MLLevel) : new List<SpellLine>();
		}

		public override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living)
		{
			var player = living as GamePlayer;
			return player != null ? GetLinesSpellsForLiving(living, player.MLLevel) : new Dictionary<SpellLine, List<Skill>>();
		}
		
		/// <summary>
		/// Return all enabled spells in a single line
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living, int level)
		{
			var player = living as GamePlayer;
			if (player != null)
			{
				if (level < 1)
					return new Dictionary<SpellLine, List<Skill>>();
				
				var spells = base.GetLinesSpellsForLiving(player, level);
				
				// For Master Level : Ability and Styles are in List Spell Line
				if (spells.Keys.Count > 0)
				{
					var list = spells.First();
					
					foreach (Styles.Style st in base.GetStylesForLiving(player, level))
						list.Value.Add(st);
					
					foreach (Ability ab in base.GetAbilitiesForLiving(player, level))
						list.Value.Add(ab);
					
					list.Key.Level = player.Level;
				}
				
				return spells;
			}
			
			return new Dictionary<SpellLine, List<Skill>>();
		}

		/// <summary>
		/// Don't send Styles.
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override List<Styles.Style> GetStylesForLiving(GameLiving living, int level)
		{
			return new List<Styles.Style>();
		}
		
		/// <summary>
		/// Master level spec level depend on ML Level
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int GetSpecLevelForLiving(GameLiving living)
		{
			var player = living as GamePlayer;
			return player != null ? player.MLLevel : 0;
		}
	}
}
