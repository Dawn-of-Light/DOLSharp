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
	/// This is used for a Hybrid Spec Spell Line (Not List Caster)
	/// </summary>
	public class LiveSpellHybridSpecialization : Specialization
	{
		public LiveSpellHybridSpecialization(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
		
		/// <summary>
		/// Is this Specialization Handling Hybrid lists ?
		/// This is always true for Hybrid Specs !
		/// </summary>
		public override bool HybridSpellList
		{
			get { return true; }
		}

		/// <summary>
		/// For Trainer Hybrid Skills aren't summarized !
		/// </summary>
		/// <param name="living"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		public override IDictionary<SpellLine, List<Skill>> PretendLinesSpellsForLiving(GameLiving living, int step)
		{
			return base.GetLinesSpellsForLiving(living, step);
		}
		
		/// <summary>
		/// Get Summarized "Hybrid" Spell Dictionary
		/// List Caster use basic Specialization Getter...
		/// This would have pretty much no reason to be used by GameLiving... (Maybe as a shortcut to force them to use their best spells...)
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		protected override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living, int level)
		{
			// An hybrid dictionary is composed of a spellline "base"
			// SpecLine and Baseline are mixed in spellline named "base"
			// baseline are displayed first (improvement are easier this way)
			// specline are displayed secondly (ordered in appareance order)
			// some class/spelline are allowed to display the "2 -best" spell
			// this is hardcoded in AllowMultipleSpellVersions...
			Dictionary<SpellLine, List<Skill>> buffer = new Dictionary<SpellLine, List<Skill>>();
			
			List<SpellLine> lines = GetSpellLinesForLiving(living, level);
			
			foreach (SpellLine ls in lines)
			{
				// buffer shouldn't contain duplicate lines
				if (buffer.ContainsKey(ls))
					continue;
				
				// Add to Dictionary
				buffer.Add(ls, new List<Skill>());
				
				IEnumerable<Spell> lib = SkillBase.GetSpellList(ls.KeyName).Where(item => item.Level <= ls.Level);
				
				int take = 1;
				if ((living is GamePlayer) && AllowMultipleSpellVersions(ls, (GamePlayer)living))
				{
					// Get 2-First Better Spell for each type
					take = 2;
				}
				
				IEnumerable<IEnumerable<Spell>> firstBetterSpellNoGroup = lib.Where(item => item.Group == 0)
					.GroupBy(item => new { item.SpellType, item.Target, item.IsAoE, item.IsInstantCast, item.HasSubSpell })
					.Select(ins => ins.OrderByDescending(it => it.Level).Take(take));
				
				IEnumerable<IEnumerable<Spell>> firstBetterSpellGroup = lib.Where(item => item.Group != 0)
					.GroupBy(item => item.Group)
					.Select(ins => ins.OrderByDescending(it => it.Level).Take(take));
		
				// sort by reverse level for multiple version
				List<Spell> firstBetterSpellFinal = firstBetterSpellGroup.SelectMany(el => el).Union(firstBetterSpellNoGroup.SelectMany(el => el))
					.Where(item => item != null).OrderByDescending(item => item.Level).ToList();
				
				// Get Appearance Order
				// not group base
				IEnumerable<IEnumerable<Spell>> baseOrderNoGroup = lib.Where(item => item.Group == 0)
					.GroupBy(item => new { item.SpellType, item.Target, item.IsAoE, item.IsInstantCast, item.HasSubSpell })
					.Select(ins => ins.OrderBy(it => it.Level).Take(take));
				// group based
				IEnumerable<IEnumerable<Spell>> baseOrderGroup = lib.Where(item => item.Group != 0)
					.GroupBy(item => item.Group)
					.Select(ins => ins.OrderBy(it => it.Level).Take(take));
				// Join and sort...
				IEnumerable<Spell> baseOrderFinal = baseOrderNoGroup.SelectMany(el => el).Union(baseOrderGroup.SelectMany(el => el))
					.Where(item => item != null).OrderBy(item => item.Level);
				
				foreach (Spell sp in baseOrderFinal)
				{
					// replace each spell of order with their best equivalent
					int index = firstBetterSpellFinal.FindIndex(od => ((od.Group == 0 && sp.Group == 0)
																	&& (od.SpellType == sp.SpellType
																	&& od.Target == sp.Target 
																	&& od.IsAoE == sp.IsAoE 
																	&& od.HasSubSpell == sp.HasSubSpell 
																	&& od.IsInstantCast == sp.IsInstantCast))
					                                         || (od.Group != 0 && od.Group == sp.Group));
					if (index > -1)
					{
						buffer[ls].Add(firstBetterSpellFinal[index]);
						firstBetterSpellFinal.RemoveAt(index);
					}
				}
			}
			
			return buffer;
		}
		
		/// <summary>		
		/// Should we allow multiple versions of each spell type in this spell line
		/// Used for hybrid classes
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		protected virtual bool AllowMultipleSpellVersions(SpellLine line, GamePlayer player)
		{
			bool allow = false;

			switch (line.Spec)
			{
				case Specs.Augmentation:
					allow = true;
					break;

				case Specs.Enhancement:
					if ((line.IsBaseLine || player.CharacterClass.ID == (int)eCharacterClass.Cleric) && player.CharacterClass.ID != (int)eCharacterClass.Heretic)
						allow = true;

					break;

				case Specs.Nurture:
					if (line.IsBaseLine || player.CharacterClass.ID == (int)eCharacterClass.Druid)
						allow = true;

					break;

				case Specs.Soulrending:
					allow = true;
					break;
			}

			return allow;
		}
	}
}
