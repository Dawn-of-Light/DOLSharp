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
	/// Description of LiveChampionsSpecialization.
	/// </summary>
	public class LiveChampionsSpecialization : CareerSpecialization
	{
		/// <summary>
		/// Champion Skills Spells List are Hybrid Style
		/// </summary>
		public override bool HybridSpellList {
			get { return true; }
		}
		
		public virtual byte MaxChampionSpecLevel {
			get { return 5; }
		}
		
		public LiveChampionsSpecialization(string keyname, string displayname, ushort icon, int ID)
			: base(keyname, displayname, icon, ID)
		{
		}
				
		/// <summary>
		/// Current Champion Specialization doesn't support Abilities (Hybrid Spell List doesn't allow ability displaying)
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override List<Ability> GetAbilitiesForLiving(GameLiving living, int level)
		{
			if (living is GamePlayer)
			{
				base.GetAbilitiesForLiving(living, ((GamePlayer)living).ChampionLevel);
			}
			
			return new List<Ability>();
		}
		
		/// <summary>
		/// Retrieve all Champion Subline Spec's Spells to an Hybrid List
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living, int level)
		{
			if (living is GamePlayer)
			{
				GamePlayer player = (GamePlayer)living;
				
				var specs = player.GetSpecList().Where(sp => sp is LiveChampionsLineSpec).Cast<LiveChampionsLineSpec>();
				
				var result = new Dictionary<SpellLine, List<Skill>>();
				
				foreach (var spec in specs)
				{
					var skills = spec.GetMiniLineSkillsForLiving(living, spec.GetSpecLevelForLiving(living)).Where(e => e.Item1 is Spell);
					
					foreach (var elem in skills)
					{
						elem.Item2.Level = player.MaxLevel;
						if (!result.ContainsKey((SpellLine)elem.Item2))
							result.Add((SpellLine)elem.Item2, new List<Skill>());
						
						elem.Item1.Level = player.MaxLevel;
						result[(SpellLine)elem.Item2].Add(elem.Item1);
					}
				}
				
				return result;
			}
			else
			{
				// Unsupported specs for livings...
				return new Dictionary<SpellLine, List<Skill>>();
			}
		}
		
		/// <summary>
		/// Retrieve all Champion Subline Spec's Styles for this living.
		/// </summary>
		/// <param name="living"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		protected override List<DOL.GS.Styles.Style> GetStylesForLiving(GameLiving living, int level)
		{
			if (living is GamePlayer)
			{
				GamePlayer player = (GamePlayer)living;
				
				var specs = player.GetSpecList().Where(sp => sp is LiveChampionsLineSpec).Cast<LiveChampionsLineSpec>();
				
				var result = new List<DOL.GS.Styles.Style>();
				
				foreach (var spec in specs)
				{
					var skills = spec.GetMiniLineSkillsForLiving(living, spec.GetSpecLevelForLiving(living)).Where(e => e.Item1 is DOL.GS.Styles.Style);
					
					foreach (var elem in skills)
					{
						elem.Item1.Level = player.MaxLevel;
						result.Add((DOL.GS.Styles.Style)elem.Item1);
					}
				}
				
				return result;
			}
			else
			{
				// Unsupported specs for livings...
				return new List<DOL.GS.Styles.Style>();
			}
		}
		
		/// <summary>
		/// Retrieve Trainer Tree
		/// List of MiniLines Containing Skill and attached Skill count (2 and 3 attachments supported)
		/// </summary>
		/// <param name="living"></param>
		public virtual List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>> GetTrainerTreeDisplay(GameLiving living, Type type)
		{
			if (typeof(MiniLineSpecialization).IsAssignableFrom(type))
			{
				var minispecs = SkillBase.GetSpecializationByType(type).OrderBy(e => e.KeyName);
				
				List<Tuple<MiniLineSpecialization, List<Skill>>> allLines = new List<Tuple<MiniLineSpecialization, List<Skill>>>();
				
				foreach (var spec in minispecs)
				{
					MiniLineSpecialization minispec = (MiniLineSpecialization)spec;
					minispec.Level = living.GetBaseSpecLevel(minispec.KeyName);
					allLines.Add(new Tuple<MiniLineSpecialization, List<Skill>>(minispec, minispec.GetMiniLineSkillsForLiving(living, MaxChampionSpecLevel).Select(t => t.Item1).ToList()));
				}
				
				var final = new List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>>();
				
				// Find intersecting Lines
				for (int i = 0 ; i < MaxChampionSpecLevel ; i++)
				{
					var grouped = allLines.Where(l => l.Item2.Count > i).GroupBy(l => new { l.Item2[i].Name, l.Item2[i].ID }).Where(grp => grp.Count() > 1 && grp.Count() < 4).Select(grp => grp.OrderBy(msp => msp.Item1.KeyName));
					
					foreach (var groups in grouped)
					{
						// Champion display only handle these tree merge
						if (groups.Count() == 3)
						{
							byte index = 0;
							foreach(var tsp in groups)
							{
								final.Add(new Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>(tsp.Item1,
								                                                                            tsp.Item2.Take(index == 1 ? tsp.Item2.Count : i)
								                                                                            .Select(e => new Tuple<Skill, byte>(e, 0)).ToList()));
								allLines.Remove(tsp);
								index++;
							}
							
							// Set group
							int ins = final.Count - 2;
							var skill = final[ins].Item2[i];
							final[ins].Item2[i] = new Tuple<Skill, byte>(skill.Item1, 3);
						}
						else if (groups.Count() == 2)
						{
							// firsts lines
							foreach(var tsp in groups)
							{
								final.Add(new Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>(tsp.Item1,
								                                                                            tsp.Item2.Take(i)
								                                                                            .Select(e => new Tuple<Skill, byte>(e, 0)).ToList()));
								allLines.Remove(tsp);
							}
							
							// Remaining
							int ins = final.Count - 1;
							final.Insert(ins, new Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>(
								groups.First().Item1, groups.First().Item2.Skip(i).Select(e => new Tuple<Skill, byte>(e, 0)).ToList()));
							// Set Group and add "nulls"
							for (int n = 0 ; n < i ; n++)
								final[ins].Item2.Insert(0, new Tuple<Skill, byte>(null, 0));
							var skill = final[ins].Item2[i];
							final[ins].Item2[i] = new Tuple<Skill, byte>(skill.Item1, 2);
						}
					}
				}
				
				Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>> pivot = final.Count > 0 ? final[0] : null;
				
				// Insert not intersecting lines (try keeping order)
				foreach (var elem in allLines)
				{
					var item = new Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>(elem.Item1, elem.Item2
					                                                                       .Select(e => new Tuple<Skill, byte>(e, 0))
					                                                                       .ToList());
					if (pivot != null && elem.Item1.KeyName.CompareTo(pivot.Item1.KeyName) <= 0)
					{
						// insert
						int ins = final.FindIndex(el => el == pivot);
						if (ins > -1)
						{
							final.Insert(ins, item);
						}
						else
						{
							final.Insert(0, item);
						}
					}
					else
					{
						// add
						final.Add(item);
					}
				}
				
				return final;
			}
			
			// default
			return new List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>>();
		}
		
		/// <summary>
		/// Retrieve the given Champ Mini Line Type for "index"
		/// </summary>
		/// <param name="index">Hardcoded index</param>
		/// <returns></returns>
		public virtual Type RetrieveTypeForIndex(int index)
		{
			var enumName = Enum.GetName(typeof(GameTrainer.eChampionTrainerType), index);
			return Type.GetType(string.Format("DOL.GS.LiveCL{0}Spec", enumName));
		}
		
		/// <summary>
		/// Retrieve the status of a Skill in the Champion Tree
		/// </summary>
		/// <param name="tree">Champion Skill Tree</param>
		/// <param name="skillIndex">Line Index (0 based)</param>
		/// <param name="itemIndex">Skill Index (0 based)</param>
		/// <returns>1 = Have It, 2 = Can Train, 0 = disable</returns>
		public virtual Tuple<byte, MiniLineSpecialization> GetSkillStatus(List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>> tree, int skillIndex, int itemIndex)
		{
			try
			{
				// Test if Available For Training
				int findlink = tree[skillIndex].Item2.FindIndex(it => it.Item2 > 1);
				
				// Attached skills ?
				if (findlink > -1 && findlink <= itemIndex)
				{
					// previous line
					if (tree[skillIndex-1].Item1.Level >= itemIndex)
					{
						return new Tuple<byte, MiniLineSpecialization>((byte)((tree[skillIndex-1].Item1.Level > itemIndex) ? 1 : 2), tree[skillIndex-1].Item1);
					}
					// next line
					else if (tree[skillIndex+1].Item1.Level >= itemIndex)
					{
						return new Tuple<byte, MiniLineSpecialization>((byte)((tree[skillIndex+1].Item1.Level > itemIndex) ? 1 : 2), tree[skillIndex+1].Item1);
					}
					// If 3-attachments mainline too !
					else if (tree[skillIndex].Item2[findlink].Item2 == 3 && tree[skillIndex].Item1.Level >= itemIndex)
					{
						return new Tuple<byte, MiniLineSpecialization>((byte)((tree[skillIndex].Item1.Level > itemIndex) ? 1 : 2), tree[skillIndex].Item1);
					}
				}
				else if (tree[skillIndex].Item1.Level >= itemIndex)
				{
					return new Tuple<byte, MiniLineSpecialization>((byte)((tree[skillIndex].Item1.Level > itemIndex) ? 1 : 2), tree[skillIndex].Item1);
				}
			}
			catch
			{
			}
			
			return new Tuple<byte, MiniLineSpecialization>(0, null);
		}
	}
}
