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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading;

using DOL.Database;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	/// <summary>
	///
	/// </summary>
	public class SkillBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Flag to Check if SkillBase has been pre-loaded.
		/// </summary>
		private static bool m_loaded = false;

		private static ReaderWriterLockSlim m_syncLockUpdates = new ReaderWriterLockSlim();
		private static object m_loadingLock = new object();
		
		#region caches and static indexes
		
		// Career Dictionary, Spec Attached to Character class ID, auto loaded on char creation !!
		protected static readonly Dictionary<int, IDictionary<string, int>> m_specsByClass = new Dictionary<int, IDictionary<string, int>>();
		

		// Specialization dict KeyName => Spec Tuple to instanciate.
		protected static readonly Dictionary<string, Tuple<Type, string, ushort, int>> m_specsByName = new Dictionary<string, Tuple<Type, string, ushort, int>>();
		
		// Specialization X SpellLines Dict<"string spec keyname", "List<"Tuple<"SpellLine line", "int classid"> line constraint"> list of lines">
		protected static readonly Dictionary<string, IList<Tuple<SpellLine, int>>> m_specsSpellLines = new Dictionary<string, IList<Tuple<SpellLine, int>>>();
		
		// global table for spec => List of styles, Dict <"string spec keyname", "Dict <"int classid", "List<"Tuple<"Style style", "byte requiredLevel"> Style Constraint" StyleByClass">
		protected static readonly Dictionary<string, IDictionary<int, List<Tuple<Style, byte>>>> m_specsStyles = new Dictionary<string, IDictionary<int, List<Tuple<Style, byte>>>>();
		
		// Specialization X Ability Cache Dict<"string spec keyname", "List<"Tuple<"string abilitykey", "byte speclevel", "int ab Level", "int class hint"> ab constraint"> list ab's>">
		protected static readonly Dictionary<string, List<Tuple<string, byte, int, int>>> m_specsAbilities = new Dictionary<string, List<Tuple<string, byte, int, int>>>();

		
		// Ability Cache Dict KeyName => DBAbility Object (to instanciate)
		protected static readonly Dictionary<string, DBAbility> m_abilityIndex = new Dictionary<string, DBAbility>();
		
		// class id => realm abilitykey list
		protected static readonly Dictionary<int, IList<string>> m_classRealmAbilities = new Dictionary<int, IList<string>>();

		
		// SpellLine Cache Dict KeyName => SpellLine Object
		protected static readonly Dictionary<string, SpellLine> m_spellLineIndex = new Dictionary<string, SpellLine>();

		// SpellLine X Spells Dict<"string spellline", "IList<"Spell spell"> spell list">
		protected static readonly Dictionary<string, List<Spell>> m_lineSpells = new Dictionary<string, List<Spell>>();
		
		// Spells Cache Dict SpellID => Spell
		protected static readonly Dictionary<int, Spell> m_spellIndex = new Dictionary<int, Spell>();
		// Spells Tooltip Dict ToolTipID => SpellID
		protected static readonly Dictionary<ushort, int> m_spellToolTipIndex = new Dictionary<ushort, int>();
		
		
		// lookup table for styles, faster access when invoking a char styleID with classID
		protected static readonly Dictionary<KeyValuePair<int, int>, Style> m_styleIndex = new Dictionary<KeyValuePair<int, int>, Style>();
		
		// Style X Spell Cache (Procs) Dict<"int StyleID", "Dict<"int classID", "Tuple<"Spell spell", "int Chance"> Proc Constraints"> list of procs">
		protected static readonly Dictionary<int, IDictionary<int, Tuple<Spell, int>>> m_stylesProcs = new Dictionary<int, IDictionary<int, Tuple<Spell, int>>>();

		
		// Ability Action Handler Dictionary Index, typename to instanciate ondemande
		protected static readonly Dictionary<string, Type> m_abilityActionHandler = new Dictionary<string, Type>();
		
		// Spec Action Handler Dictionary Index, typename to instanciate ondemande
		protected static readonly Dictionary<string, Type> m_specActionHandler = new Dictionary<string, Type>();

		#endregion

		#region class initialize

		static SkillBase()
		{
			RegisterPropertyNames();
			InitArmorResists();
			InitPropertyTypes();
			InitializeObjectTypeToSpec();
			InitializeSpecToSkill();
			InitializeSpecToFocus();
			InitializeRaceResists();
		}


		public static void LoadSkills()
		{
			lock (m_loadingLock)
			{
				if (!m_loaded)
				{
					LoadSpells();
					LoadSpellLines();
					LoadAbilities();
					LoadClassRealmAbilities();
					// Load Spec, SpecXAbility, SpecXSpellLine, SpecXStyle, Styles, StylesProcs...
					// Need Spell, SpellLines, Abilities Loaded (including RealmAbilities...) !
					LoadSpecializations();
					LoadClassSpecializations();
					LoadAbilityHandlers();
					LoadSkillHandlers();
					m_loaded = true;
				}
			}
		}

		/// <summary>
		/// Load Spells From Database
		/// This will wipe any scripted spells !
		/// </summary>
		public static void LoadSpells()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				//load all spells
				if (log.IsInfoEnabled)
					log.Info("Loading spells...");
	
				IList<DBSpell> spelldb = GameServer.Database.SelectAllObjects<DBSpell>();

				if (spelldb != null)
				{
		
					// clean cache
					m_spellIndex.Clear();
					m_spellToolTipIndex.Clear();
		
					foreach (DBSpell spell in spelldb)
					{
						try
						{
							m_spellIndex.Add(spell.SpellID, new Spell(spell, 1));
							// Update tooltip index.
							if (spell.TooltipId != 0 && !m_spellToolTipIndex.ContainsKey(spell.TooltipId))
								m_spellToolTipIndex.Add(spell.TooltipId, spell.SpellID);
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("{0} with spellid = {1} spell.TS= {2}", e.Message, spell.SpellID, spell.ToString());
						}
					}
					
					if (log.IsInfoEnabled)
						log.InfoFormat("Spells loaded: {0}", m_spellIndex.Count);
					
					spelldb = null;
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Load SpellLines and Line X Spell relation from Database
		/// This will wipe any Script Loaded Lines or LineXSpell Relation !
		/// </summary>
		public static void LoadSpellLines()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				//load all spellline
				if (log.IsInfoEnabled)
					log.Info("Loading Spell Lines...");
	
				// load all spell lines
				IList<DBSpellLine> dbo = GameServer.Database.SelectAllObjects<DBSpellLine>();
				
				if (dbo != null)
				{
					// clean cache
					m_spellLineIndex.Clear();
					
					foreach(DBSpellLine line in dbo)
					{
						try
						{
							m_spellLineIndex.Add(line.KeyName, new SpellLine(line.KeyName, line.Name, line.Spec, line.IsBaseLine));
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("{0} with Spell Line = {1} line.TS= {2}", e.Message, line.KeyName, line.ToString());					
						}
						
					}
					
					dbo = null;
				}
	
				if (log.IsInfoEnabled)
					log.InfoFormat("Spell Lines loaded: {0}", m_spellLineIndex.Count);
	
				
				//load spell relation
				if (log.IsInfoEnabled)
					log.Info("Loading Spell Lines X Spells Relation...");
				
				IList<DBLineXSpell> dbox = GameServer.Database.SelectAllObjects<DBLineXSpell>();

				int count = 0;

				if (dbox != null)
				{
					// Clean cache
					m_lineSpells.Clear();
										
					foreach (DBLineXSpell lxs in dbox)
					{
						try
						{
							if (!m_lineSpells.ContainsKey(lxs.LineName))
								m_lineSpells.Add(lxs.LineName, new List<Spell>());
													
							Spell spl = (Spell)m_spellIndex[lxs.SpellID].Clone();
							
							spl.Level = Math.Max(1, lxs.Level);
							
							m_lineSpells[lxs.LineName].Add(spl);
							count++;
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("LineXSpell Spell Adding Error : {0}, Line {1}, Spell {2}, Level {3}", e.Message, lxs.LineName, lxs.SpellID, lxs.Level);
								
						}
					}
					
					dbox = null;
				}
				
				// sort spells
				foreach (string sps in m_lineSpells.Keys.ToList())
					m_lineSpells[sps] = m_lineSpells[sps].OrderBy(e => e.Level).ThenBy(e => e.ID).ToList();
	
				if (log.IsInfoEnabled)
					log.InfoFormat("Total spell lines X Spell loaded: {0}", count);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Reload all the DB spells from the database. 
		/// Useful to load new spells added in preperation for ReloadSpellLine(linename) to update a spell line live
		/// We want to add any new spells in the DB to the global spell list, m_spells, but not remove any added by scripts
		/// </summary>
		public static void ReloadDBSpells()
		{
			// lock skillbase for writes
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				//load all spells
				if (log.IsInfoEnabled)
					log.Info("Reloading DB spells...");
	
				IList<DBSpell> spelldb = GameServer.Database.SelectAllObjects<DBSpell>();
				
				if (spelldb != null)
				{
		
					int count = 0;
		
					foreach (DBSpell spell in spelldb)
					{
						if (m_spellIndex.ContainsKey(spell.SpellID) == false)
						{
							// Add new spell
							m_spellIndex.Add(spell.SpellID, new Spell(spell, 1));
							count++;
						}
						else
						{
							// Replace Spell
							m_spellIndex[spell.SpellID] = new Spell(spell, 1);
						}
						
						// Update tooltip index
						if (spell.TooltipId != 0)
						{
							if (m_spellToolTipIndex.ContainsKey(spell.TooltipId))
								m_spellToolTipIndex[spell.TooltipId] = spell.SpellID;
							else
							{
								m_spellToolTipIndex.Add(spell.TooltipId, spell.SpellID);
								count++;
							}
						}
						
					}
		
					if (log.IsInfoEnabled)
					{
						log.Info("Spells loaded from DB: " + spelldb.Count);
						log.Info("Spells added to global spell list: " + count);
					}
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Reload Spell Line Spells from Database without wiping Line collection
		/// This allow to reload from database without wiping scripted spells.
		/// </summary>
		/// <param name="lineName"></param>
		/// <returns></returns>
		[RefreshCommandAttribute]
		public static int ReloadSpellLines()
		{
			int count = 0;
			// lock skillbase for writes
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				foreach (string lineName in m_spellLineIndex.Keys)
				{
					// Get SpellLine X Spell relation
					IList<DBLineXSpell> spells = GameServer.Database.SelectObjects<DBLineXSpell>("LineName LIKE '" + GameServer.Database.Escape(lineName) + "'");
					
					// Load them if any records.
					if (spells != null)
					{
						if (!m_lineSpells.ContainsKey(lineName))
							m_lineSpells.Add(lineName, new List<Spell>());
	
						
						foreach (DBLineXSpell lxs in spells)
						{
							try
							{
								// Clone Spell to change Level to relation Level's
								Spell spl = (Spell)m_spellIndex[lxs.SpellID].Clone();
								
								spl.Level = Math.Max(1, lxs.Level);
								
								// Look for existing spell for replacement
								bool added = false;
								
								for (int r = 0; r < m_lineSpells[lineName].Count; r++)
								{
									if (m_lineSpells[lineName][r] != null && m_lineSpells[lineName][r].ID == lxs.SpellID)
									{
										m_lineSpells[lineName][r] = spl;
										added = true;
										break;
									}
								}
								
								// no replacement then add this
								if (!added)
								{
									m_lineSpells[lineName].Add(spl);
									count++;
								}
								
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("LineXSpell Adding Error : {0}, Line {1}, Spell {2}, Level {3}", e.Message, lxs.LineName, lxs.SpellID, lxs.Level);
									
							}
						}
						
						// Line can need a sort...
						m_lineSpells[lineName] = m_lineSpells[lineName].OrderBy(e => e.Level).ThenBy(e => e.ID).ToList();
					}
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
			
			return count;
		}

		/// <summary>
		/// This Load Ability from Database
		/// Will wipe any registered "Scripted" Abilities
		/// </summary>
		private static void LoadAbilities()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// load Abilities
				if (log.IsInfoEnabled)
					log.Info("Loading Abilities...");
	
				IList<DBAbility> abilities = GameServer.Database.SelectAllObjects<DBAbility>();
								
				if (abilities != null)
				{
					// Clean Cache
					m_abilityIndex.Clear();

					foreach (DBAbility dba in abilities)
					{
						try
						{
							// test only...
							Ability ability = GetNewAbilityInstance(dba);

							m_abilityIndex.Add(ability.KeyName, dba);
	
							if (log.IsDebugEnabled)
								log.DebugFormat("Ability {0} successfuly instanciated from {1} (expeted {2})", dba.KeyName, dba.Implementation, ability.GetType());

						}
						catch (Exception e)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Error while Loading Ability {0} with Class {1} : {2}", dba.KeyName, dba.Implementation, e);
						}
					}
					
					abilities = null;
				}
				
				if (log.IsInfoEnabled)
				{
					log.InfoFormat("Total abilities loaded: {0}", m_abilityIndex.Count);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Load Class Realm Abilities Relations
		/// Wipes any script loaded Relation
		/// </summary>
		private static void LoadClassRealmAbilities()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// load class RA
				m_classRealmAbilities.Clear();
				
				if (log.IsInfoEnabled)
					log.Info("Loading class to realm ability associations...");
				
				IList<ClassXRealmAbility> classxra = GameServer.Database.SelectAllObjects<ClassXRealmAbility>();
	
				if (classxra != null)
				{
					foreach (ClassXRealmAbility cxra in classxra)
					{
						if (!m_classRealmAbilities.ContainsKey(cxra.CharClass))
							m_classRealmAbilities.Add(cxra.CharClass, new List<string>());
						
						try
						{
							DBAbility dba = m_abilityIndex[cxra.AbilityKey];
							
							if (!m_classRealmAbilities[cxra.CharClass].Contains(dba.KeyName))
								m_classRealmAbilities[cxra.CharClass].Add(dba.KeyName);
						}
						catch (Exception e)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Error while Adding RealmAbility {0} to Class {1} : {2}", cxra.AbilityKey, cxra.CharClass, e);

						}
					}
					
					classxra = null;
				}
	
				log.Info("Realm Abilities assigned to classes!");
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Load Specialization from Database
		/// Also load Relation SpecXAbility, SpecXSpellLine, SpecXStyle
		/// This is Master Loader for Styles, Styles can't work without an existing Database Spec !
		/// Wipe Specs Index, SpecXAbility, SpecXSpellLine, StyleDict, StyleIndex
		/// Anything loaded in this from scripted behavior can be lost... (try to not use Scripted Career !!)
		/// </summary>
		/// <returns>number of specs loaded.</returns>
		[RefreshCommandAttribute]
		public static int LoadSpecializations()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				IList<DBSpecialization> specs = GameServer.Database.SelectAllObjects<DBSpecialization>();
				
				int count = 0;
				
				if (specs != null)
				{
					// Clear Spec Cache
					m_specsByName.Clear();
					
					// Clear SpecXAbility Cache (Ability Career)
					m_specsAbilities.Clear();
					
					// Clear SpecXSpellLine Cache (Spell Career)
					m_specsSpellLines.Clear();
					
					// Clear Style Cache (Style Career)
					m_specsStyles.Clear();
					
					// Clear Style ID Cache (Utils...)
					m_styleIndex.Clear();
					
					// Clear Style X Spell Cache (Style Procs...)
					m_stylesProcs.Clear();
	
					foreach (DBSpecialization spec in specs)
					{
						StringBuilder str = new StringBuilder("Specialization ");
						str.AppendFormat("{0} - ", spec.KeyName);
						
						Specialization gameSpec = null;
						if (Util.IsEmpty(spec.Implementation, true) == false)
						{
							gameSpec = GetNewSpecializationInstance(spec.KeyName, spec.Implementation, spec.Name, spec.Icon, spec.SpecializationID);
						}
						else
						{
							gameSpec = new Specialization(spec.KeyName, spec.Name, spec.Icon, spec.SpecializationID);
						}
						
						if (log.IsDebugEnabled)
							log.DebugFormat("Specialization {0} successfuly instanciated from {1} (expected {2})", spec.KeyName, gameSpec.GetType().FullName, spec.Implementation);
						
						Tuple<Type, string, ushort, int> entry = new Tuple<Type, string, ushort, int>(gameSpec.GetType(), spec.Name, spec.Icon, spec.SpecializationID);
						
						// Now we have an instanciated Specialization, Cache their properties in Skillbase to prevent using too much memory
						// As most skill objects are duplicated for every game object use...
						
						// Load SpecXAbility
						count = 0;
						if (spec.AbilityConstraints != null)
						{
							if (!m_specsAbilities.ContainsKey(spec.KeyName))
								m_specsAbilities.Add(spec.KeyName, new List<Tuple<string, byte, int, int>>());

							foreach (DBSpecXAbility specx in spec.AbilityConstraints)
							{
								
								try
								{
									m_specsAbilities[spec.KeyName].Add(new Tuple<string, byte, int, int>(m_abilityIndex[specx.AbilityKey].KeyName, (byte)specx.SpecLevel, specx.AbilityLevel, specx.ClassId));
									count++;
								}
								catch (Exception e)
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Specialization : {0} while adding Spec X Ability {1}, from Spec {2}({3}), Level {4}", e.Message, specx.AbilityKey, specx.Spec, specx.SpecLevel, specx.AbilityLevel);
								}
								
							}
							
							// sort them according to required levels
							m_specsAbilities[spec.KeyName].Sort((i, j) => i.Item2.CompareTo(j.Item2));
						}
						
						str.AppendFormat("{0} Ability Constraint, ", count);
	
						
						// Load SpecXSpellLine
						count = 0;
						if (spec.SpellLines != null)
						{
							foreach (DBSpellLine line in spec.SpellLines)
							{
								if (!m_specsSpellLines.ContainsKey(spec.KeyName))
									m_specsSpellLines.Add(spec.KeyName, new List<Tuple<SpellLine, int>>());
								
								try
								{
									m_specsSpellLines[spec.KeyName].Add(new Tuple<SpellLine, int>(m_spellLineIndex[line.KeyName], line.ClassIDHint));
									count++;
								}
								catch (Exception e)
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Specialization : {0} while adding Spec X SpellLine {1} from Spec {2}, ClassID {3}", e.Message, line.KeyName, line.Spec, line.ClassIDHint);
								}
							}
						}
	
						str.AppendFormat("{0} Spell Line, ", count);
						
						// Load DBStyle
						count = 0;
						if (spec.Styles != null)
						{
							foreach (DBStyle specStyle in spec.Styles)
							{
								// Update Style Career
								if (!m_specsStyles.ContainsKey(spec.KeyName))
								{
									m_specsStyles.Add(spec.KeyName, new Dictionary<int, List<Tuple<Style, byte>>>());
								}
								
								if (!m_specsStyles[spec.KeyName].ContainsKey(specStyle.ClassId))
								{
									m_specsStyles[spec.KeyName].Add(specStyle.ClassId, new List<Tuple<Style, byte>>());
								}
								
								Style newStyle = new Style(specStyle);
								
								m_specsStyles[spec.KeyName][specStyle.ClassId].Add(new Tuple<Style, byte>(newStyle, (byte)specStyle.SpecLevelRequirement));
								
								// Update Style Index.
								
								KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(newStyle.ID, specStyle.ClassId);
								
								if (!m_styleIndex.ContainsKey(styleKey))
								{
									m_styleIndex.Add(styleKey, newStyle);
									count++;
								}
								else
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Specialization {0} - Duplicate Style Key, StyleID {1} : ClassID {2}, Ignored...", spec.KeyName, newStyle.ID, specStyle.ClassId);
								}
								
								// load Procs
								if (specStyle.AttachedProcs != null)
								{
									foreach (DBStyleXSpell styleProc in specStyle.AttachedProcs)
									{
										if (m_spellIndex.ContainsKey(styleProc.SpellID))
										{
											if (!m_stylesProcs.ContainsKey(specStyle.ID))
											{
												m_stylesProcs.Add(specStyle.ID, new Dictionary<int, Tuple<Spell, int>>());
											}
											
											if (!m_stylesProcs[specStyle.ID].ContainsKey(styleProc.ClassID))
												m_stylesProcs[specStyle.ID].Add(styleProc.ClassID, new Tuple<Spell, int>(m_spellIndex[styleProc.SpellID], styleProc.Chance));
										}
									}
								}
								
							}
						}
	
						// We've added all the styles to their respective lists.  Now lets go through and sort them by their level
						foreach (string keyname in m_specsStyles.Keys)
						{
							foreach (int classid in m_specsStyles[keyname].Keys)
								m_specsStyles[keyname][classid].Sort((i, j) => i.Item2.CompareTo(j.Item2));
						}
						
						str.AppendFormat("{0} Styles", count);
						
						if (log.IsDebugEnabled)
							log.Debug(str.ToString());
	
						// Add spec to global Spec Index Cache
						if (!m_specsByName.ContainsKey(spec.KeyName))
						{
							m_specsByName.Add(spec.KeyName, entry);
						}
						else
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Specialization {0} is duplicated ignoring...", spec.KeyName);
						}
						
					}
					
					specs = null;
	
				}
				
				if (log.IsInfoEnabled)
					log.InfoFormat("Total specializations loaded: {0}", m_specsByName.Count);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
	
			return m_specsByName.Count;
		}

		/// <summary>
		/// Load (or Reload) Class Career, Appending each Class a Specialization !
		/// </summary>
		public static void LoadClassSpecializations()
		{
			// lock skillbase for write
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				if (log.IsInfoEnabled)
					log.Info("Loading Class Specialization's Career...");
				
				//Retrieve from DB
				IList<ClassXSpecialization> dbo = GameServer.Database.SelectAllObjects<ClassXSpecialization>();
				Dictionary<int, StringBuilder> summary = new Dictionary<int, StringBuilder>();
				
				if (dbo != null)
				{
					// clear
					m_specsByClass.Clear();
					
					foreach (ClassXSpecialization career in dbo)
					{
						if (!m_specsByClass.ContainsKey(career.ClassID))
						{
							m_specsByClass.Add(career.ClassID, new Dictionary<string, int>());
							summary.Add(career.ClassID, new StringBuilder());
							summary[career.ClassID].AppendFormat("Career for Class {0} - ", career.ClassID);
						}
						
						if (!m_specsByClass[career.ClassID].ContainsKey(career.SpecKeyName))
						{
							m_specsByClass[career.ClassID].Add(career.SpecKeyName, career.LevelAcquired);
							summary[career.ClassID].AppendFormat("{0}({1}), ", career.SpecKeyName, career.LevelAcquired);
						}
						else
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Duplicate Sepcialization Key {0} for Class Career : {1}", career.SpecKeyName, career.ClassID);
						}
					}
				}
				
				if (log.IsInfoEnabled)
					log.Info("Finished loading Class Specialization's Career !");
				
				if (log.IsDebugEnabled)
				{
					// print summary
					foreach (KeyValuePair<int, StringBuilder> entry in summary)
						log.Debug(entry.Value.ToString());
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}
		
		/// <summary>
		/// Load Ability Handler for Action Ability.
		/// </summary>
		private static void LoadAbilityHandlers()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// load Ability actions handlers
				m_abilityActionHandler.Clear();
				
				//Search for ability handlers in the gameserver first
				if (log.IsInfoEnabled)
					log.Info("Searching ability handlers in GameServer");
				
				IList<KeyValuePair<string, Type>> ht = ScriptMgr.FindAllAbilityActionHandler(Assembly.GetExecutingAssembly());
								
				foreach (KeyValuePair<string, Type> entry in ht)
				{
					if (log.IsDebugEnabled)
						log.DebugFormat("\tFound ability handler for {0}", entry.Key);

					if (m_abilityActionHandler.ContainsKey(entry.Key))
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Duplicate type handler for: ", entry.Key);
					}
					else
					{
						try
						{
							// test only...
							IAbilityActionHandler handler = GetNewAbilityActionHandler(entry.Value);
							string test = handler.ToString();
							
							m_abilityActionHandler.Add(entry.Key, entry.Value);
						}
						catch (Exception ex)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("Error While instantiacting IAbilityHandler {0} using {1} in GameServer : {2}", entry.Key, entry.Value, ex);
						}
					}
				}
	
				//Now search ability handlers in the scripts directory and overwrite the ones
				//found from gameserver
				if (log.IsInfoEnabled)
					log.Info("Searching AbilityHandlers in Scripts");
				
				foreach (Assembly asm in ScriptMgr.Scripts)
				{
					ht = ScriptMgr.FindAllAbilityActionHandler(asm);
					foreach (KeyValuePair<string, Type> entry in ht)
					{
						string message = "";	
						try
						{
							// test only...
							IAbilityActionHandler handler = GetNewAbilityActionHandler(entry.Value);
							string test = handler.ToString();

							if (m_abilityActionHandler.ContainsKey(entry.Key))
							{
								message = "\tFound new ability handler for " + entry.Key;
								m_abilityActionHandler[entry.Key] = entry.Value;
							}
							else
							{
								message = "\tFound ability handler for " + entry.Key;
								m_abilityActionHandler.Add(entry.Key, entry.Value);
							}
						}
						catch (Exception ex)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("Error While instantiacting IAbilityHandler {0} using {1} in GameServerScripts : {2}", entry.Key, entry.Value, ex);
						}
	
						if (log.IsDebugEnabled)
							log.Debug(message);
					}
				}
								
				if (log.IsInfoEnabled)
					log.Info("Total ability handlers loaded: " + m_abilityActionHandler.Count);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Load Skill Handler for Action Spec Icon
		/// </summary>
		private static void LoadSkillHandlers()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				//load skill action handlers
				m_specActionHandler.Clear();
				
				//Search for skill handlers in gameserver first
				if (log.IsInfoEnabled)
					log.Info("Searching skill handlers in GameServer.");
				
				IList<KeyValuePair<string, Type>> ht = ScriptMgr.FindAllSpecActionHandler(Assembly.GetExecutingAssembly());
				
				foreach (KeyValuePair<string, Type> entry in ht)
				{
					if (log.IsDebugEnabled)
						log.Debug("\tFound skill handler for " + entry.Key);

					if (m_specActionHandler.ContainsKey(entry.Key))
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Duplicate type Skill handler for: ", entry.Key);
					}
					else
					{
						try
						{
							// test only...
							ISpecActionHandler handler = GetNewSpecActionHandler(entry.Value);
							string test = handler.ToString();
							
							m_specActionHandler.Add(entry.Key, entry.Value);
						}
						catch (Exception ex)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Error While instantiacting ISpecActionHandler {0} using {1} in GameServer : {2}", entry.Key, entry.Value, ex);
						}
					}
				}
				
				//Now search skill handlers in the scripts directory and overwrite the ones
				//found from the gameserver
				
				if (log.IsInfoEnabled)
					log.Info("Searching skill handlers in Scripts.");
				
				foreach (Assembly asm in ScriptMgr.Scripts)
				{
					ht = ScriptMgr.FindAllSpecActionHandler(asm);
					
					foreach (KeyValuePair<string, Type> entry in ht)
					{
						string message = "";
						
						try
						{
							// test only
							ISpecActionHandler handler = GetNewSpecActionHandler(entry.Value);
							string test = handler.ToString();

							if (m_specActionHandler.ContainsKey(entry.Key))
							{
								message = "\tFound new spec handler for " + entry.Key;
								m_specActionHandler[entry.Key] = entry.Value;
							}
							else
							{
								message = "\tFound spec handler for " + entry.Key;
								m_specActionHandler.Add(entry.Key, entry.Value);
							}
						}
						catch (Exception ex)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Error While instantiacting ISpecActionHandler {0} using {1} in GameServerScripts : {2}", entry.Key, entry.Value, ex);
						}
							
						if (log.IsDebugEnabled)
							log.Debug(message);
					}
				}
				
				if (log.IsInfoEnabled)
					log.Info("Total skill handlers loaded: " + m_specActionHandler.Count);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}
		#endregion
		
		#region Initialization Tables

		/// <summary>
		/// Holds object type to spec convertion table
		/// </summary>
		protected static readonly Dictionary<eObjectType, string> m_objectTypeToSpec = new Dictionary<eObjectType, string>();

		/// <summary>
		/// Holds spec to skill table
		/// </summary>
		protected static readonly Dictionary<string, eProperty> m_specToSkill = new Dictionary<string, eProperty>();

		/// <summary>
		/// Holds spec to focus table
		/// </summary>
		protected static readonly Dictionary<string, eProperty> m_specToFocus = new Dictionary<string, eProperty>();

		/// <summary>
		/// Holds all property types
		/// </summary>
		private static readonly ePropertyType[] m_propertyTypes = new ePropertyType[(int)eProperty.MaxProperty+1];

		/// <summary>
		/// table for property names
		/// </summary>
		protected static readonly Dictionary<eProperty, string> m_propertyNames = new Dictionary<eProperty, string>();

		/// <summary>
		/// Table to hold the race resists
		/// </summary>
		protected static readonly Dictionary<int, int[]> m_raceResists = new Dictionary<int, int[]>();

		/// <summary>
		/// Initialize the object type hashtable
		/// </summary>
		private static void InitializeObjectTypeToSpec()
		{
			m_objectTypeToSpec.Add(eObjectType.Staff, Specs.Staff);
			m_objectTypeToSpec.Add(eObjectType.Fired, Specs.ShortBow);

			m_objectTypeToSpec.Add(eObjectType.FistWraps, Specs.Fist_Wraps);
			m_objectTypeToSpec.Add(eObjectType.MaulerStaff, Specs.Mauler_Staff);

			//alb
			m_objectTypeToSpec.Add(eObjectType.CrushingWeapon, Specs.Crush);
			m_objectTypeToSpec.Add(eObjectType.SlashingWeapon, Specs.Slash);
			m_objectTypeToSpec.Add(eObjectType.ThrustWeapon, Specs.Thrust);
			m_objectTypeToSpec.Add(eObjectType.TwoHandedWeapon, Specs.Two_Handed);
			m_objectTypeToSpec.Add(eObjectType.PolearmWeapon, Specs.Polearms);
			m_objectTypeToSpec.Add(eObjectType.Flexible, Specs.Flexible);
			m_objectTypeToSpec.Add(eObjectType.Crossbow, Specs.Crossbow);
			
			// RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
			if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
			{
				m_objectTypeToSpec.Add(eObjectType.Longbow, Specs.Longbow);
			}
			// RDSandersJR: If we are NOT using old archery it should be SpellDamage
			else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
			{
				m_objectTypeToSpec.Add(eObjectType.Longbow, Specs.Archery);
			}
			
			//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown); break);

			//mid
			m_objectTypeToSpec.Add(eObjectType.Hammer, Specs.Hammer);
			m_objectTypeToSpec.Add(eObjectType.Sword, Specs.Sword);
			m_objectTypeToSpec.Add(eObjectType.LeftAxe, Specs.Left_Axe);
			m_objectTypeToSpec.Add(eObjectType.Axe, Specs.Axe);
			m_objectTypeToSpec.Add(eObjectType.HandToHand, Specs.HandToHand);
			m_objectTypeToSpec.Add(eObjectType.Spear, Specs.Spear);
			m_objectTypeToSpec.Add(eObjectType.Thrown, Specs.Thrown_Weapons);
			
			// RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
			if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
			{
				m_objectTypeToSpec.Add(eObjectType.CompositeBow, Specs.CompositeBow);
			}
			// RDSandersJR: If we are NOT using old archery it should be SpellDamage
			else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
			{
				m_objectTypeToSpec.Add(eObjectType.CompositeBow, Specs.Archery);
			}

			//hib
			m_objectTypeToSpec.Add(eObjectType.Blunt, Specs.Blunt);
			m_objectTypeToSpec.Add(eObjectType.Blades, Specs.Blades);
			m_objectTypeToSpec.Add(eObjectType.Piercing, Specs.Piercing);
			m_objectTypeToSpec.Add(eObjectType.LargeWeapons, Specs.Large_Weapons);
			m_objectTypeToSpec.Add(eObjectType.CelticSpear, Specs.Celtic_Spear);
			m_objectTypeToSpec.Add(eObjectType.Scythe, Specs.Scythe);
			m_objectTypeToSpec.Add(eObjectType.Shield, Specs.Shields);
			m_objectTypeToSpec.Add(eObjectType.Poison, Specs.Envenom);
			
			// RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
			if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
			{
				m_objectTypeToSpec.Add(eObjectType.RecurvedBow, Specs.RecurveBow);
			}
			// RDSandersJR: If we are NOT using old archery it should be SpellDamage
			else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
			{
				m_objectTypeToSpec.Add(eObjectType.RecurvedBow, Specs.Archery);
			}
		}

		/// <summary>
		/// Initialize the spec to skill table
		/// </summary>
		private static void InitializeSpecToSkill()
		{
			#region Weapon Specs

			//Weapon specs
			//Alb
			m_specToSkill.Add(Specs.Thrust, eProperty.Skill_Thrusting);
			m_specToSkill.Add(Specs.Slash, eProperty.Skill_Slashing);
			m_specToSkill.Add(Specs.Crush, eProperty.Skill_Crushing);
			m_specToSkill.Add(Specs.Polearms, eProperty.Skill_Polearms);
			m_specToSkill.Add(Specs.Two_Handed, eProperty.Skill_Two_Handed);
			m_specToSkill.Add(Specs.Staff, eProperty.Skill_Staff);
			m_specToSkill.Add(Specs.Dual_Wield, eProperty.Skill_Dual_Wield);
			m_specToSkill.Add(Specs.Flexible, eProperty.Skill_Flexible_Weapon);
			m_specToSkill.Add(Specs.Longbow, eProperty.Skill_Long_bows);
			m_specToSkill.Add(Specs.Crossbow, eProperty.Skill_Cross_Bows);
			//Mid
			m_specToSkill.Add(Specs.Sword, eProperty.Skill_Sword);
			m_specToSkill.Add(Specs.Axe, eProperty.Skill_Axe);
			m_specToSkill.Add(Specs.Hammer, eProperty.Skill_Hammer);
			m_specToSkill.Add(Specs.Left_Axe, eProperty.Skill_Left_Axe);
			m_specToSkill.Add(Specs.Spear, eProperty.Skill_Spear);
			m_specToSkill.Add(Specs.CompositeBow, eProperty.Skill_Composite);
			m_specToSkill.Add(Specs.Thrown_Weapons, eProperty.Skill_Thrown_Weapons);
			m_specToSkill.Add(Specs.HandToHand, eProperty.Skill_HandToHand);
			//Hib
			m_specToSkill.Add(Specs.Blades, eProperty.Skill_Blades);
			m_specToSkill.Add(Specs.Blunt, eProperty.Skill_Blunt);
			m_specToSkill.Add(Specs.Piercing, eProperty.Skill_Piercing);
			m_specToSkill.Add(Specs.Large_Weapons, eProperty.Skill_Large_Weapon);
			m_specToSkill.Add(Specs.Celtic_Dual, eProperty.Skill_Celtic_Dual);
			m_specToSkill.Add(Specs.Celtic_Spear, eProperty.Skill_Celtic_Spear);
			m_specToSkill.Add(Specs.RecurveBow, eProperty.Skill_RecurvedBow);
			m_specToSkill.Add(Specs.Scythe, eProperty.Skill_Scythe);

			#endregion

			#region Magic Specs

			//Magic specs
			//Alb
			m_specToSkill.Add(Specs.Matter_Magic, eProperty.Skill_Matter);
			m_specToSkill.Add(Specs.Body_Magic, eProperty.Skill_Body);
			m_specToSkill.Add(Specs.Spirit_Magic, eProperty.Skill_Spirit);
			m_specToSkill.Add(Specs.Rejuvenation, eProperty.Skill_Rejuvenation);
			m_specToSkill.Add(Specs.Enhancement, eProperty.Skill_Enhancement);
			m_specToSkill.Add(Specs.Smite, eProperty.Skill_Smiting);
			m_specToSkill.Add(Specs.Instruments, eProperty.Skill_Instruments);
			m_specToSkill.Add(Specs.Deathsight, eProperty.Skill_DeathSight);
			m_specToSkill.Add(Specs.Painworking, eProperty.Skill_Pain_working);
			m_specToSkill.Add(Specs.Death_Servant, eProperty.Skill_Death_Servant);
			m_specToSkill.Add(Specs.Chants, eProperty.Skill_Chants);
			m_specToSkill.Add(Specs.Mind_Magic, eProperty.Skill_Mind);
			m_specToSkill.Add(Specs.Earth_Magic, eProperty.Skill_Earth);
			m_specToSkill.Add(Specs.Cold_Magic, eProperty.Skill_Cold);
			m_specToSkill.Add(Specs.Fire_Magic, eProperty.Skill_Fire);
			m_specToSkill.Add(Specs.Wind_Magic, eProperty.Skill_Wind);
			m_specToSkill.Add(Specs.Soulrending, eProperty.Skill_SoulRending);
			//Mid
			m_specToSkill.Add(Specs.Darkness, eProperty.Skill_Darkness);
			m_specToSkill.Add(Specs.Suppression, eProperty.Skill_Suppression);
			m_specToSkill.Add(Specs.Runecarving, eProperty.Skill_Runecarving);
			m_specToSkill.Add(Specs.Summoning, eProperty.Skill_Summoning);
			m_specToSkill.Add(Specs.BoneArmy, eProperty.Skill_BoneArmy);
			m_specToSkill.Add(Specs.Mending, eProperty.Skill_Mending);
			m_specToSkill.Add(Specs.Augmentation, eProperty.Skill_Augmentation);
			m_specToSkill.Add(Specs.Pacification, eProperty.Skill_Pacification);
			m_specToSkill.Add(Specs.Subterranean, eProperty.Skill_Subterranean);
			m_specToSkill.Add(Specs.Beastcraft, eProperty.Skill_BeastCraft);
			m_specToSkill.Add(Specs.Stormcalling, eProperty.Skill_Stormcalling);
			m_specToSkill.Add(Specs.Battlesongs, eProperty.Skill_Battlesongs);
			m_specToSkill.Add(Specs.Savagery, eProperty.Skill_Savagery);
			m_specToSkill.Add(Specs.OdinsWill, eProperty.Skill_OdinsWill);
			m_specToSkill.Add(Specs.Cursing, eProperty.Skill_Cursing);
			m_specToSkill.Add(Specs.Hexing, eProperty.Skill_Hexing);
			m_specToSkill.Add(Specs.Witchcraft, eProperty.Skill_Witchcraft);

			//Hib
			m_specToSkill.Add(Specs.Arboreal_Path, eProperty.Skill_Arboreal);
			m_specToSkill.Add(Specs.Creeping_Path, eProperty.Skill_Creeping);
			m_specToSkill.Add(Specs.Verdant_Path, eProperty.Skill_Verdant);
			m_specToSkill.Add(Specs.Regrowth, eProperty.Skill_Regrowth);
			m_specToSkill.Add(Specs.Nurture, eProperty.Skill_Nurture);
			m_specToSkill.Add(Specs.Music, eProperty.Skill_Music);
			m_specToSkill.Add(Specs.Valor, eProperty.Skill_Valor);
			m_specToSkill.Add(Specs.Nature, eProperty.Skill_Nature);
			m_specToSkill.Add(Specs.Light, eProperty.Skill_Light);
			m_specToSkill.Add(Specs.Void, eProperty.Skill_Void);
			m_specToSkill.Add(Specs.Mana, eProperty.Skill_Mana);
			m_specToSkill.Add(Specs.Enchantments, eProperty.Skill_Enchantments);
			m_specToSkill.Add(Specs.Mentalism, eProperty.Skill_Mentalism);
			m_specToSkill.Add(Specs.Nightshade_Magic, eProperty.Skill_Nightshade);
			m_specToSkill.Add(Specs.Pathfinding, eProperty.Skill_Pathfinding);
			m_specToSkill.Add(Specs.Dementia, eProperty.Skill_Dementia);
			m_specToSkill.Add(Specs.ShadowMastery, eProperty.Skill_ShadowMastery);
			m_specToSkill.Add(Specs.VampiiricEmbrace, eProperty.Skill_VampiiricEmbrace);
			m_specToSkill.Add(Specs.EtherealShriek, eProperty.Skill_EtherealShriek);
			m_specToSkill.Add(Specs.PhantasmalWail, eProperty.Skill_PhantasmalWail);
			m_specToSkill.Add(Specs.SpectralForce, eProperty.Skill_SpectralForce);
			m_specToSkill.Add(Specs.SpectralGuard, eProperty.Skill_SpectralGuard);

			#endregion

			#region Other

			//Other
			m_specToSkill.Add(Specs.Critical_Strike, eProperty.Skill_Critical_Strike);
			m_specToSkill.Add(Specs.Stealth, eProperty.Skill_Stealth);
			m_specToSkill.Add(Specs.Shields, eProperty.Skill_Shields);
			m_specToSkill.Add(Specs.Envenom, eProperty.Skill_Envenom);
			m_specToSkill.Add(Specs.Parry, eProperty.Skill_Parry);
			m_specToSkill.Add(Specs.ShortBow, eProperty.Skill_ShortBow);
			m_specToSkill.Add(Specs.Mauler_Staff, eProperty.Skill_MaulerStaff);
			m_specToSkill.Add(Specs.Fist_Wraps, eProperty.Skill_FistWraps);
			m_specToSkill.Add(Specs.Aura_Manipulation, eProperty.Skill_Aura_Manipulation);
			m_specToSkill.Add(Specs.Magnetism, eProperty.Skill_Magnetism);
			m_specToSkill.Add(Specs.Power_Strikes, eProperty.Skill_Power_Strikes);

			m_specToSkill.Add(Specs.Archery, eProperty.Skill_Archery);

			#endregion
		}

		/// <summary>
		/// Initialize the spec to focus tables
		/// </summary>
		private static void InitializeSpecToFocus()
		{
			m_specToFocus.Add(Specs.Darkness, eProperty.Focus_Darkness);
			m_specToFocus.Add(Specs.Suppression, eProperty.Focus_Suppression);
			m_specToFocus.Add(Specs.Runecarving, eProperty.Focus_Runecarving);
			m_specToFocus.Add(Specs.Spirit_Magic, eProperty.Focus_Spirit);
			m_specToFocus.Add(Specs.Fire_Magic, eProperty.Focus_Fire);
			m_specToFocus.Add(Specs.Wind_Magic, eProperty.Focus_Air);
			m_specToFocus.Add(Specs.Cold_Magic, eProperty.Focus_Cold);
			m_specToFocus.Add(Specs.Earth_Magic, eProperty.Focus_Earth);
			m_specToFocus.Add(Specs.Light, eProperty.Focus_Light);
			m_specToFocus.Add(Specs.Body_Magic, eProperty.Focus_Body);
			m_specToFocus.Add(Specs.Mind_Magic, eProperty.Focus_Mind);
			m_specToFocus.Add(Specs.Matter_Magic, eProperty.Focus_Matter);
			m_specToFocus.Add(Specs.Void, eProperty.Focus_Void);
			m_specToFocus.Add(Specs.Mana, eProperty.Focus_Mana);
			m_specToFocus.Add(Specs.Enchantments, eProperty.Focus_Enchantments);
			m_specToFocus.Add(Specs.Mentalism, eProperty.Focus_Mentalism);
			m_specToFocus.Add(Specs.Summoning, eProperty.Focus_Summoning);
			// SI
			m_specToFocus.Add(Specs.BoneArmy, eProperty.Focus_BoneArmy);
			m_specToFocus.Add(Specs.Painworking, eProperty.Focus_PainWorking);
			m_specToFocus.Add(Specs.Deathsight, eProperty.Focus_DeathSight);
			m_specToFocus.Add(Specs.Death_Servant, eProperty.Focus_DeathServant);
			m_specToFocus.Add(Specs.Verdant_Path, eProperty.Focus_Verdant);
			m_specToFocus.Add(Specs.Creeping_Path, eProperty.Focus_CreepingPath);
			m_specToFocus.Add(Specs.Arboreal_Path, eProperty.Focus_Arboreal);
			// Catacombs
			m_specToFocus.Add(Specs.EtherealShriek, eProperty.Focus_EtherealShriek);
			m_specToFocus.Add(Specs.PhantasmalWail, eProperty.Focus_PhantasmalWail);
			m_specToFocus.Add(Specs.SpectralForce, eProperty.Focus_SpectralForce);
			m_specToFocus.Add(Specs.Cursing, eProperty.Focus_Cursing);
			m_specToFocus.Add(Specs.Hexing, eProperty.Focus_Hexing);
			m_specToFocus.Add(Specs.Witchcraft, eProperty.Focus_Witchcraft);
		}

		/// <summary>
		/// Init property types table
		/// </summary>
		private static void InitPropertyTypes()
		{
			#region Resist

			// resists
			m_propertyTypes[(int)eProperty.Resist_Natural] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Body] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Cold] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Crush] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Energy] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Heat] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Matter] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Slash] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Spirit] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Thrust] = ePropertyType.Resist;

			#endregion

			#region Focus

			// focuses
			m_propertyTypes[(int)eProperty.Focus_Darkness] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Suppression] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Runecarving] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Spirit] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Fire] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Air] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Cold] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Earth] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Light] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Body] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Matter] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mind] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Void] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mana] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Enchantments] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mentalism] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Summoning] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_BoneArmy] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_PainWorking] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_DeathSight] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_DeathServant] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Verdant] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_CreepingPath] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Arboreal] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_EtherealShriek] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_PhantasmalWail] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_SpectralForce] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Cursing] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Hexing] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Witchcraft] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.AllFocusLevels] = ePropertyType.Focus;

			#endregion


			/*
			 * http://www.camelotherald.com/more/1036.shtml
			 * "- ALL melee weapon skills - This bonus will increase your
			 * skill in many weapon types. This bonus does not increase shield,
			 * parry, archery skills, or dual wield skills (hand to hand is the
			 * exception, as this skill is also the main weapon skill associated
			 * with hand to hand weapons, and not just the off-hand skill). If
			 * your item has "All melee weapon skills: +3" and your character
			 * can train in hammer, axe and sword, your item should give you
			 * a +3 increase to all three."
			 */

			#region Melee Skills

			// skills
			m_propertyTypes[(int)eProperty.Skill_Two_Handed] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Critical_Strike] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Crushing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Flexible_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Polearms] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Slashing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Staff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Thrusting] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Sword] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Hammer] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Axe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Blades] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Blunt] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Piercing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Large_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Celtic_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Scythe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Thrown_Weapons] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_HandToHand] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_FistWraps] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_MaulerStaff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;

			m_propertyTypes[(int)eProperty.Skill_Dual_Wield] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Left_Axe] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Celtic_Dual] = ePropertyType.Skill | ePropertyType.SkillDualWield;

			#endregion

			#region Magical Skills

			m_propertyTypes[(int)eProperty.Skill_Power_Strikes] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Magnetism] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Aura_Manipulation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Body] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Chants] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Death_Servant] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_DeathSight] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Earth] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Enhancement] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Fire] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Cold] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Instruments] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Matter] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mind] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pain_working] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Rejuvenation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Smiting] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SoulRending] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Spirit] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Wind] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mending] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Augmentation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Darkness] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Suppression] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Runecarving] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Stormcalling] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_BeastCraft] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Light] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Void] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mana] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Battlesongs] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Enchantments] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mentalism] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Regrowth] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nurture] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nature] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Music] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Valor] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Subterranean] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_BoneArmy] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Verdant] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Creeping] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Arboreal] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pacification] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Savagery] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nightshade] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pathfinding] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Summoning] = ePropertyType.Skill | ePropertyType.SkillMagical;

			// no idea about these
			m_propertyTypes[(int)eProperty.Skill_Dementia] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_ShadowMastery] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_VampiiricEmbrace] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_EtherealShriek] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_PhantasmalWail] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SpectralForce] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SpectralGuard] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_OdinsWill] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Cursing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Hexing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Witchcraft] = ePropertyType.Skill | ePropertyType.SkillMagical;

			#endregion

			#region Other

			m_propertyTypes[(int)eProperty.Skill_Long_bows] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_Composite] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_RecurvedBow] = ePropertyType.Skill | ePropertyType.SkillArchery;

			m_propertyTypes[(int)eProperty.Skill_Parry] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Shields] = ePropertyType.Skill;

			m_propertyTypes[(int)eProperty.Skill_Stealth] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Cross_Bows] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_ShortBow] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Envenom] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Archery] = ePropertyType.Skill | ePropertyType.SkillArchery;

			#endregion
		}

		/// <summary>
		/// Initializes the race resist table
		/// </summary>
		public static void InitializeRaceResists()
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// http://camelot.allakhazam.com/Start_Stats.html
				IList<Race> races;
	
				try
				{
					races = GameServer.Database.SelectAllObjects<Race>();
				}
				catch
				{
					m_raceResists.Clear();
					return;
				}
				
				if (races != null)
				{
				
					m_raceResists.Clear();
		
					foreach (Race race in races)
					{
						m_raceResists.Add(race.ID, new int[10]);
						m_raceResists[race.ID][0] = race.ResistBody;
						m_raceResists[race.ID][1] = race.ResistCold;
						m_raceResists[race.ID][2] = race.ResistCrush;
						m_raceResists[race.ID][3] = race.ResistEnergy;
						m_raceResists[race.ID][4] = race.ResistHeat;
						m_raceResists[race.ID][5] = race.ResistMatter;
						m_raceResists[race.ID][6] = race.ResistSlash;
						m_raceResists[race.ID][7] = race.ResistSpirit;
						m_raceResists[race.ID][8] = race.ResistThrust;
						m_raceResists[race.ID][9] = race.ResistNatural;
					}
					
					races = null;
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		private static void RegisterPropertyNames()
		{
			#region register...
			m_propertyNames.Add(eProperty.Strength, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.Strength"));
			m_propertyNames.Add(eProperty.Dexterity, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Dexterity"));
			m_propertyNames.Add(eProperty.Constitution, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Constitution"));
			m_propertyNames.Add(eProperty.Quickness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Quickness"));
			m_propertyNames.Add(eProperty.Intelligence, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Intelligence"));
			m_propertyNames.Add(eProperty.Piety, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                "SkillBase.RegisterPropertyNames.Piety"));
			m_propertyNames.Add(eProperty.Empathy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Empathy"));
			m_propertyNames.Add(eProperty.Charisma, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.Charisma"));

			m_propertyNames.Add(eProperty.MaxMana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Power"));
			m_propertyNames.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Hits"));

			// resists (does not say "resist" on live server)
			m_propertyNames.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Body"));
			m_propertyNames.Add(eProperty.Resist_Natural, "Essence");
			m_propertyNames.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Cold"));
			m_propertyNames.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Crush"));
			m_propertyNames.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Energy"));
			m_propertyNames.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Heat"));
			m_propertyNames.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Matter"));
			m_propertyNames.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Slash"));
			m_propertyNames.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Spirit"));
			m_propertyNames.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Thrust"));

			// Eden - Mythirian bonus
			m_propertyNames.Add(eProperty.BodyResCapBonus, "Body cap");
			m_propertyNames.Add(eProperty.ColdResCapBonus, "Cold cap");
			m_propertyNames.Add(eProperty.CrushResCapBonus, "Crush cap");
			m_propertyNames.Add(eProperty.EnergyResCapBonus, "Energy cap");
			m_propertyNames.Add(eProperty.HeatResCapBonus, "Heat cap");
			m_propertyNames.Add(eProperty.MatterResCapBonus, "Matter cap");
			m_propertyNames.Add(eProperty.SlashResCapBonus, "Slash cap");
			m_propertyNames.Add(eProperty.SpiritResCapBonus, "Spirit cap");
			m_propertyNames.Add(eProperty.ThrustResCapBonus, "Thrust cap");
            m_propertyNames.Add(eProperty.MythicalSafeFall, "Mythical Safe Fall");
            m_propertyNames.Add(eProperty.MythicalDiscumbering, "Mythical Discumbering");
            m_propertyNames.Add(eProperty.MythicalCoin, "Mythical Coin");
            m_propertyNames.Add(eProperty.SpellLevel, "Spell Focus");
			//Eden - special actifacts bonus
			m_propertyNames.Add(eProperty.Conversion, "Conversion");
			m_propertyNames.Add(eProperty.ExtraHP, "Extra Health Points");
			m_propertyNames.Add(eProperty.StyleAbsorb, "Style Absorb");
			m_propertyNames.Add(eProperty.ArcaneSyphon, "Arcane Syphon");
			m_propertyNames.Add(eProperty.RealmPoints, "Realm Points");
			//[Freya] Nidel
			m_propertyNames.Add(eProperty.BountyPoints, "Bounty Points");
			m_propertyNames.Add(eProperty.XpPoints, "Experience Points");

			// skills
			m_propertyNames.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.TwoHanded"));
			m_propertyNames.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.BodyMagic"));
			m_propertyNames.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Chants"));
			m_propertyNames.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.CriticalStrike"));
			m_propertyNames.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Crossbows"));
			m_propertyNames.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Crushing"));
			m_propertyNames.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.DeathServant"));
			m_propertyNames.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Deathsight"));
			m_propertyNames.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.DualWield"));
			m_propertyNames.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EarthMagic"));
			m_propertyNames.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Enhancement"));
			m_propertyNames.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Envenom"));
			m_propertyNames.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.FireMagic"));
			m_propertyNames.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
			m_propertyNames.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ColdMagic"));
			m_propertyNames.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Instruments"));
			m_propertyNames.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Longbows"));
			m_propertyNames.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.MatterMagic"));
			m_propertyNames.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MindMagic"));
			m_propertyNames.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Painworking"));
			m_propertyNames.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Parry"));
			m_propertyNames.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Polearms"));
			m_propertyNames.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Rejuvenation"));
			m_propertyNames.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Shields"));
			m_propertyNames.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Slashing"));
			m_propertyNames.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Smiting"));
			m_propertyNames.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Soulrending"));
			m_propertyNames.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.SpiritMagic"));
			m_propertyNames.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Staff"));
			m_propertyNames.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Stealth"));
			m_propertyNames.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Thrusting"));
			m_propertyNames.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.WindMagic"));
			m_propertyNames.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Sword"));
			m_propertyNames.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Hammer"));
			m_propertyNames.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Axe"));
			m_propertyNames.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.LeftAxe"));
			m_propertyNames.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Spear"));
			m_propertyNames.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Mending"));
			m_propertyNames.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Augmentation"));
			m_propertyNames.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Darkness"));
			m_propertyNames.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Suppression"));
			m_propertyNames.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Runecarving"));
			m_propertyNames.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Stormcalling"));
			m_propertyNames.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.BeastCraft"));
			m_propertyNames.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.LightMagic"));
			m_propertyNames.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.VoidMagic"));
			m_propertyNames.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ManaMagic"));
			m_propertyNames.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Composite"));
			m_propertyNames.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Battlesongs"));
			m_propertyNames.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Enchantment"));

			m_propertyNames.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Blades"));
			m_propertyNames.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Blunt"));
			m_propertyNames.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Piercing"));
			m_propertyNames.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.LargeWeapon"));
			m_propertyNames.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Mentalism"));
			m_propertyNames.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Regrowth"));
			m_propertyNames.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Nurture"));
			m_propertyNames.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Nature"));
			m_propertyNames.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Music"));
			m_propertyNames.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.CelticDual"));
			m_propertyNames.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CelticSpear"));
			m_propertyNames.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.RecurvedBow"));
			m_propertyNames.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Valor"));
			m_propertyNames.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CaveMagic"));
			m_propertyNames.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.BoneArmy"));
			m_propertyNames.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Verdant"));
			m_propertyNames.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Creeping"));
			m_propertyNames.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Arboreal"));
			m_propertyNames.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Scythe"));
			m_propertyNames.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.ThrownWeapons"));
			m_propertyNames.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.HandToHand"));
			m_propertyNames.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ShortBow"));
			m_propertyNames.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Pacification"));
			m_propertyNames.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Savagery"));
			m_propertyNames.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.NightshadeMagic"));
			m_propertyNames.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Pathfinding"));
			m_propertyNames.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Summoning"));
			m_propertyNames.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Archery"));

			// Mauler
			m_propertyNames.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.FistWraps"));
			m_propertyNames.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.MaulerStaff"));
			m_propertyNames.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.PowerStrikes"));
			m_propertyNames.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Magnetism"));
			m_propertyNames.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                  "SkillBase.RegisterPropertyNames.AuraManipulation"));


			//Catacombs skills
			m_propertyNames.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Dementia"));
			m_propertyNames.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.ShadowMastery"));
			m_propertyNames.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
			m_propertyNames.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EtherealShriek"));
			m_propertyNames.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.PhantasmalWail"));
			m_propertyNames.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralForce"));
			m_propertyNames.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralGuard"));
			m_propertyNames.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.OdinsWill"));
			m_propertyNames.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Cursing"));
			m_propertyNames.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Hexing"));
			m_propertyNames.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Witchcraft"));


			// Classic Focii
			m_propertyNames.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.DarknessFocus"));
			m_propertyNames.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.SuppressionFocus"));
			m_propertyNames.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
			m_propertyNames.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.FireMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.WindMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.LightMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MindMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
			m_propertyNames.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.MentalismFocus"));
			m_propertyNames.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.SummoningFocus"));
			// SI Focii
			// Mid
			m_propertyNames.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
			// Alb
			m_propertyNames.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.PainworkingFocus"));
			m_propertyNames.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.DeathsightFocus"));
			m_propertyNames.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.DeathservantFocus"));
			// Hib
			m_propertyNames.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.VerdantFocus"));
			m_propertyNames.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
			m_propertyNames.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ArborealFocus"));
			// Catacombs Focii
			m_propertyNames.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
			m_propertyNames.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
			m_propertyNames.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
			m_propertyNames.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.CursingFocus"));
			m_propertyNames.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.HexingFocus"));
			m_propertyNames.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

			m_propertyNames.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.MaximumSpeed"));
			m_propertyNames.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Concentration"));

			m_propertyNames.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
			m_propertyNames.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

			m_propertyNames.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.HealthRegeneration"));
			m_propertyNames.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.PowerRegeneration"));
			m_propertyNames.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                    "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
			m_propertyNames.Add(eProperty.SpellRange, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.SpellRange"));
			m_propertyNames.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ArcheryRange"));
			m_propertyNames.Add(eProperty.Acuity, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                 "SkillBase.RegisterPropertyNames.Acuity"));

			m_propertyNames.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.AllMagicSkills"));
			m_propertyNames.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
			m_propertyNames.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ALLSpellLines"));
			m_propertyNames.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
			m_propertyNames.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.AllArcherySkills"));

			m_propertyNames.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EffectiveLevel"));


			//Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
			//Date : 20-Jan-2005
			//Missing bonusses begin
			m_propertyNames.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EvadeChance"));
			m_propertyNames.Add(eProperty.BlockChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.BlockChance"));
			m_propertyNames.Add(eProperty.ParryChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.ParryChance"));
			m_propertyNames.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.FumbleChance"));
			m_propertyNames.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.MeleeDamage"));
			m_propertyNames.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.RangedDamage"));
			m_propertyNames.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
			m_propertyNames.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.StunDuration"));
			m_propertyNames.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
			m_propertyNames.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
			m_propertyNames.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.DefensiveBonus"));
			m_propertyNames.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.PieceAblative"));
			m_propertyNames.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.NegativeReduction"));
			m_propertyNames.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
			m_propertyNames.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.SpellPowerCost"));
			m_propertyNames.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.StyleCostReduction"));
			m_propertyNames.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ToHitBonus"));
			m_propertyNames.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ArcherySpeed"));
			m_propertyNames.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.ArrowRecovery"));
			m_propertyNames.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.StatBuffSpells"));
			m_propertyNames.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.CastingSpeed"));
			m_propertyNames.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ExperienceLoss"));
			m_propertyNames.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
			m_propertyNames.Add(eProperty.Fatigue, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Fatigue"));
			m_propertyNames.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
			m_propertyNames.Add(eProperty.PowerPool, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.PowerPool"));
			//Magiekraftvorrat
			m_propertyNames.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ResistPierce"));
			m_propertyNames.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
			m_propertyNames.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.SpellDuration"));
			m_propertyNames.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.StyleDamage"));
			m_propertyNames.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MeleeSpeed"));
			//Missing bonusses end

			m_propertyNames.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
			m_propertyNames.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
			m_propertyNames.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
			m_propertyNames.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
			m_propertyNames.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
			m_propertyNames.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.PietyBonusCap"));
			m_propertyNames.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
			m_propertyNames.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
			m_propertyNames.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
			m_propertyNames.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
			m_propertyNames.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.PowerBonusCap"));
			m_propertyNames.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.WeaponSkill"));
			m_propertyNames.Add(eProperty.AllSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.AllSkills"));
			m_propertyNames.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
			m_propertyNames.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
			m_propertyNames.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
			m_propertyNames.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "SkillBase.RegisterPropertyNames.CriticalHealHit"));
			#endregion
		}

		#endregion
		
		#region Armor resists

		// lookup table for armor resists
		private const int REALM_BITCOUNT = 2;
		private const int DAMAGETYPE_BITCOUNT = 4;
		private const int ARMORTYPE_BITCOUNT = 3;
		private static readonly int[] m_armorResists = new int[1 << (REALM_BITCOUNT + DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT)];

		/// <summary>
		/// Gets the natural armor resist to the give damage type
		/// </summary>
		/// <param name="armor"></param>
		/// <param name="damageType"></param>
		/// <returns>resist value</returns>
		public static int GetArmorResist(InventoryItem armor, eDamageType damageType)
		{
			if (armor == null) return 0;
			int realm = armor.Template.Realm - (int)eRealm._First;
			int armorType = armor.Template.Object_Type - (int)eObjectType._FirstArmor;
			int damage = damageType - eDamageType._FirstResist;
			if (realm < 0 || realm > eRealm._LastPlayerRealm - eRealm._First) return 0;
			if (armorType < 0 || armorType > eObjectType._LastArmor - eObjectType._FirstArmor) return 0;
			if (damage < 0 || damage > eDamageType._LastResist - eDamageType._FirstResist) return 0;

			const int realmBits = DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT;

			return m_armorResists[(realm << realmBits) | (armorType << DAMAGETYPE_BITCOUNT) | damage];
		}

		private static void InitArmorResists()
		{
			const int resistant		= 10;
			const int vulnerable	= -5;

			// melee resists (slash, crush, thrust)

			// alb armor - neutral to slash
			// plate and leather resistant to thrust
			// chain and studded vulnerable to thrust
			WriteMeleeResists(eRealm.Albion, eObjectType.Leather, 0, vulnerable, resistant);
			WriteMeleeResists(eRealm.Albion, eObjectType.Plate,   0, vulnerable, resistant);
			WriteMeleeResists(eRealm.Albion, eObjectType.Studded, 0, resistant,  vulnerable);
			WriteMeleeResists(eRealm.Albion, eObjectType.Chain,   0, resistant,  vulnerable);


			// hib armor - neutral to thrust
			// reinforced and leather vulnerable to crush
			// scale resistant to crush
			WriteMeleeResists( eRealm.Hibernia, eObjectType.Leather,    resistant,  vulnerable, 0 );
			WriteMeleeResists( eRealm.Hibernia, eObjectType.Reinforced, resistant,  vulnerable, 0 );
			WriteMeleeResists(eRealm.Hibernia,  eObjectType.Scale,      vulnerable, resistant, 0);


			// mid armor - neutral to crush
			// studded and leather resistant to thrust
			// chain vulnerabel to thrust
			WriteMeleeResists(eRealm.Midgard, eObjectType.Studded, vulnerable, 0, resistant);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Leather, vulnerable, 0, resistant);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Chain,   resistant,  0, vulnerable);


			// magical damage (Heat, Cold, Matter, Energy)
			// Leather
			WriteMagicResists(eRealm.Albion,   eObjectType.Leather, vulnerable, resistant, vulnerable, 0);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Leather, vulnerable, resistant, vulnerable, 0);
			WriteMagicResists(eRealm.Midgard,  eObjectType.Leather, vulnerable, resistant, vulnerable, 0);

			// Reinforced/Studded
			WriteMagicResists(eRealm.Albion,   eObjectType.Studded,    resistant, vulnerable, vulnerable, vulnerable);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Reinforced, resistant, vulnerable, vulnerable, vulnerable);
			WriteMagicResists(eRealm.Midgard,  eObjectType.Studded,    resistant, vulnerable, vulnerable, vulnerable);

			// Chain
			WriteMagicResists(eRealm.Albion,  eObjectType.Chain, resistant, 0, 0, vulnerable);
			WriteMagicResists(eRealm.Midgard, eObjectType.Chain, resistant, 0, 0, vulnerable);

			// Scale/Plate
			WriteMagicResists(eRealm.Albion,   eObjectType.Plate, resistant, vulnerable, resistant, vulnerable);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Scale, resistant, vulnerable, resistant, vulnerable);
		}

		private static void WriteMeleeResists(eRealm realm, eObjectType armorType, int slash, int crush, int thrust)
		{
			if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
				throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
			if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
				throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");

			int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
			off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
			m_armorResists[off + (eDamageType.Slash - eDamageType._FirstResist)] = slash;
			m_armorResists[off + (eDamageType.Crush - eDamageType._FirstResist)] = crush;
			m_armorResists[off + (eDamageType.Thrust - eDamageType._FirstResist)] = thrust;
		}

		private static void WriteMagicResists(eRealm realm, eObjectType armorType, int heat, int cold, int matter, int energy)
		{
			if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
				throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
			if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
				throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");

			int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
			off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
			m_armorResists[off + (eDamageType.Heat - eDamageType._FirstResist)] = heat;
			m_armorResists[off + (eDamageType.Cold - eDamageType._FirstResist)] = cold;
			m_armorResists[off + (eDamageType.Matter - eDamageType._FirstResist)] = matter;
			m_armorResists[off + (eDamageType.Energy - eDamageType._FirstResist)] = energy;
		}

		#endregion

		/// <summary>
		/// Check if property belongs to all of specified types
		/// </summary>
		/// <param name="prop">The property to check</param>
		/// <param name="type">The types to check</param>
		/// <returns>true if property belongs to all types</returns>
		public static bool CheckPropertyType(eProperty prop, ePropertyType type)
		{
			int property = (int)prop;
			if (property < 0 || property >= m_propertyTypes.Length)
				return false;

			return (m_propertyTypes[property] & type) == type;
		}

		/// <summary>
		/// Gets a new AbilityActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static IAbilityActionHandler GetAbilityActionHandler(string keyName)
		{
			m_syncLockUpdates.EnterReadLock();
			Type handlerType;
			bool exists;
			try
			{
				exists = m_abilityActionHandler.TryGetValue(keyName, out handlerType);
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (exists)
			{
				return GetNewAbilityActionHandler(handlerType);
			}
			
			return null;
		}

		/// <summary>
		/// Gets a new SpecActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static ISpecActionHandler GetSpecActionHandler(string keyName)
		{
			m_syncLockUpdates.EnterReadLock();
			Type handlerType;
			bool exists;
			try
			{
				exists = m_specActionHandler.TryGetValue(keyName, out handlerType);
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (exists)
			{
				try
				{
					ISpecActionHandler newHndl = GetNewSpecActionHandler(handlerType);
					return newHndl;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Error while instanciating ISpecActionHandler {0} From Handler {2}: {1}", keyName, e, handlerType);
				}
			}
			
			return null;
		}

		/// <summary>
		/// Register or Overwrite a Spell Line
		/// </summary>
		/// <param name="line"></param>
		public static void RegisterSpellLine(SpellLine line)
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				if (m_spellLineIndex.ContainsKey(line.KeyName))
					m_spellLineIndex[line.KeyName] = line;
				else
					m_spellLineIndex.Add(line.KeyName, line);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Add a new style to a specialization.  If the specialization does not exist it will be created.
		/// After adding all styles call SortStyles to sort the list by level
		/// </summary>
		/// <param name="style"></param>
		public static void AddScriptedStyle(Specialization spec, DBStyle style)
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				if (!m_specsStyles.ContainsKey(spec.KeyName))
					m_specsStyles.Add(spec.KeyName, new Dictionary<int, List<Tuple<Style, byte>>>());
				
				if (!m_specsStyles[spec.KeyName].ContainsKey(style.ClassId))
					m_specsStyles[spec.KeyName].Add(style.ClassId, new List<Tuple<Style, byte>>());
			
				Style st = new Style(style);
				
				m_specsStyles[spec.KeyName][style.ClassId].Add(new Tuple<Style, byte>(st, (byte)style.SpecLevelRequirement));
	
				KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(st.ID, style.ClassId);
				if (!m_styleIndex.ContainsKey(styleKey))
					m_styleIndex.Add(styleKey, st);
	
				if (!m_specsByName.ContainsKey(spec.KeyName))
					RegisterSpec(spec);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Register or Overwrite a spec in Cache
		/// </summary>
		/// <param name="spec"></param>
		public static void RegisterSpec(Specialization spec)
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				Tuple<Type, string, ushort, int> entry = new Tuple<Type, string, ushort, int>(spec.GetType(), spec.Name, spec.Icon, spec.ID);
				
				if (m_specsByName.ContainsKey(spec.KeyName))
					m_specsByName[spec.KeyName] = entry;
				else
					m_specsByName.Add(spec.KeyName, entry);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Remove a Spell Line from Cache
		/// </summary>
		/// <param name="LineKeyName"></param>
		public static void UnRegisterSpellLine(string LineKeyName)
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				if (m_spellLineIndex.ContainsKey(LineKeyName))
					m_spellLineIndex.Remove(LineKeyName);
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// returns level 1 instantiated realm abilities, only for readonly use!
		/// </summary>
		/// <param name="classID"></param>
		/// <returns></returns>
		public static List<RealmAbility> GetClassRealmAbilities(int classID)
		{
			List<DBAbility> ras = new List<DBAbility>();
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_classRealmAbilities.ContainsKey(classID))
				{
					foreach (string str in m_classRealmAbilities[classID])
					{
						try
						{
							ras.Add(m_abilityIndex[str]);
						}
						catch
						{
						}
					}
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
						
			return ras.Select(e => GetNewAbilityInstance(e)).Where(ab => ab is RealmAbility).Cast<RealmAbility>().OrderByDescending(el => el.MaxLevel).ThenBy(el => el.KeyName).ToList();
		}

		/// <summary>
		/// Return this character class RR5 Ability Level 1 or null
		/// </summary>
		/// <param name="charclass"></param>
		/// <returns></returns>
		public static Ability GetClassRR5Ability(int charclass)
		{
			return GetClassRealmAbilities(charclass).Where(ab => ab is RR5RealmAbility).FirstOrDefault();
		}

		/// <summary>
		/// Get Ability by internal ID, used for Tooltip Details.
		/// </summary>
		/// <param name="internalID"></param>
		/// <returns></returns>
		public static Ability GetAbilityByInternalID(int internalID)
		{
			m_syncLockUpdates.EnterReadLock();
			string ability = null;
			try
			{
				ability = m_abilityIndex.Where(it => it.Value.InternalID == internalID).FirstOrDefault().Value.KeyName;
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (!Util.IsEmpty(ability, true))
				return GetAbility(ability, 1);

			return GetAbility(string.Format("INTERNALID:{0}", internalID), 1);
		}
		
		/// <summary>
		/// Get Ability by Keyname
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Ability GetAbility(string keyname)
		{
			return GetAbility(keyname, 1);
		}
		
		/// <summary>
		/// Get Ability by dbid.
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Ability GetAbility(int databaseID)
		{
			m_syncLockUpdates.EnterReadLock();
			string ability = null;
			try
			{
				ability = m_abilityIndex.Where(it => it.Value.AbilityID == databaseID).FirstOrDefault().Value.KeyName;
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (!Util.IsEmpty(ability, true))
				return GetAbility(ability, 1);
				
			return GetAbility(string.Format("DBID:{0}", databaseID), 1);
		}

		/// <summary>
		/// Get Ability by Keyname and Level
		/// </summary>
		/// <param name="keyname"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public static Ability GetAbility(string keyname, int level)
		{
			m_syncLockUpdates.EnterReadLock();
			DBAbility dbab = null;
			try
			{
				if (m_abilityIndex.ContainsKey(keyname))
				{
					dbab = m_abilityIndex[keyname];
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (dbab != null)
			{
				Ability dba = GetNewAbilityInstance(dbab);
				dba.Level = level;
				return dba;
			}

			if (log.IsWarnEnabled)
				log.Warn("Ability '" + keyname + "' unknown");

			return new Ability(keyname, "?" + keyname, "", 0, 0, level, 0);
		}

		/// <summary>
		/// return all spells for a specific spell-line
		/// if no spells are associated or spell-line is unknown the list will be empty
		/// </summary>
		/// <param name="spellLineID">KeyName of spell-line</param>
		/// <returns>list of spells, never null</returns>
		public static List<Spell> GetSpellList(string spellLineID)
		{
			List<Spell> spellList = new List<Spell>();
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_lineSpells.ContainsKey(spellLineID))
				{
					foreach (var element in m_lineSpells[spellLineID])
						spellList.Add((Spell)element.Clone());
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			return spellList;
		}

		/// <summary>
		/// Update or add a spell to the global spell list.  Useful for adding procs and charges to items without restarting server.
		/// This will not update a spell in a spell line.
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static bool UpdateSpell(int spellID)
		{
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				DBSpell dbSpell = GameServer.Database.SelectObject<DBSpell>("SpellID = " + spellID);
	
				if (dbSpell != null)
				{
					Spell spell = new Spell(dbSpell, 1);
	
					if (m_spellIndex.ContainsKey(spellID))
					{
						m_spellIndex[spellID] = spell;
					}
					else
					{
						m_spellIndex.Add(spellID, spell);
					}
					
					// Update tooltip index
					if (spell.InternalID != 0)
					{
						if (m_spellToolTipIndex.ContainsKey((ushort)spell.InternalID))
							m_spellToolTipIndex[(ushort)spell.InternalID] = spell.ID;
						else
							m_spellToolTipIndex.Add((ushort)spell.InternalID, spell.ID);
					}
	
					return true;
				}
	
				return false;
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}
		
		/// <summary>
		/// Get Spell Lines attached to a Spec (with class hint).
		/// </summary>
		/// <param name="specName">Spec Key Name</param>
		/// <returns></returns>
		public static IList<Tuple<SpellLine, int>> GetSpecsSpellLines(string specName)
		{
			IList<Tuple<SpellLine, int>> list = new List<Tuple<SpellLine, int>>();
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_specsSpellLines.ContainsKey(specName))
				{
					foreach(Tuple<SpellLine, int> entry in m_specsSpellLines[specName])
					{
						list.Add(new Tuple<SpellLine, int>((SpellLine)entry.Item1.Clone(), entry.Item2));
					}
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			return list;
		}
		
		/// <summary>
		/// Return the spell line, creating a temporary one if not found
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static SpellLine GetSpellLine(string keyname)
		{
			return GetSpellLine(keyname, true);
		}


		/// <summary>
		/// Return a spell line
		/// </summary>
		/// <param name="keyname">The key name of the line</param>
		/// <param name="create">Should we create a temp spell line if not found?</param>
		/// <returns></returns>
		public static SpellLine GetSpellLine(string keyname, bool create)
		{
			SpellLine result = null;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_spellLineIndex.ContainsKey(keyname))
					result = (SpellLine)m_spellLineIndex[keyname].Clone();
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			
			if (result != null)
				return result;

			// Mob Spells is specifically scripted...
			if (keyname == GlobalSpellsLines.Mob_Spells)
                return new SpellLine(GlobalSpellsLines.Mob_Spells, GlobalSpellsLines.Mob_Spells, "", true);

			if (create)
			{
				if (log.IsWarnEnabled)
				{
					log.WarnFormat("Spell-Line {0} unknown, creating temporary line.", keyname);
				}

				return new SpellLine(keyname, string.Format("{0}?", keyname), "", true);
			}

			return null;
		}
		
		/// <summary>
		/// Add a scripted spell to a spellline
		/// will try to add to global spell list if not exists (preventing obvious harcoded errors...)
		/// </summary>
		/// <param name="spellLineID"></param>
		/// <param name="spell"></param>
		public static void AddScriptedSpell(string spellLineID, Spell spell)
		{
			// lock Skillbase for writes
			m_syncLockUpdates.EnterWriteLock();
			Spell spcp = null;
			try
			{
				if (spell.ID > 0 && !m_spellIndex.ContainsKey(spell.ID))
				{
					spcp = (Spell)spell.Clone();
					// Level 1 for storing...
					spcp.Level = 1;
					
					m_spellIndex.Add(spell.ID, spcp);
					
					// Add Tooltip Index
					if (spcp.InternalID != 0 && !m_spellToolTipIndex.ContainsKey((ushort)spcp.InternalID))
						m_spellToolTipIndex.Add((ushort)spcp.InternalID, spcp.ID);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
			
			// let the base handler do this...
			if (spcp != null)
			{
				AddSpellToSpellLine(spellLineID, spell.ID, spell.Level);
				return;
			}

			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// Cannot store it in spell index !! ID could be wrongly set we can't rely on it !
				if (!m_lineSpells.ContainsKey(spellLineID))
					m_lineSpells.Add(spellLineID, new List<Spell>());
				
				// search for duplicates
				bool added = false;
				for (int r = 0; r < m_lineSpells[spellLineID].Count; r++)
				{
					try
					{
						if (m_lineSpells[spellLineID][r] != null && 
						    (spell.ID > 0 && m_lineSpells[spellLineID][r].ID == spell.ID && m_lineSpells[spellLineID][r].Name.ToLower().Equals(spell.Name.ToLower()) && m_lineSpells[spellLineID][r].SpellType.ToLower().Equals(spell.SpellType.ToLower()))
						    || (m_lineSpells[spellLineID][r].Name.ToLower().Equals(spell.Name.ToLower()) && m_lineSpells[spellLineID][r].SpellType.ToLower().Equals(spell.SpellType.ToLower())))
						{
							m_lineSpells[spellLineID][r] = spell;
							added = true;
						}
					}
					catch
					{
					}
				}
				
				// try regular add (this could go wrong if duplicate detection is bad...)
				if (!added)
					m_lineSpells[spellLineID].Add(spell);
				
				m_lineSpells[spellLineID] = m_lineSpells[spellLineID].OrderBy(e => e.Level).ThenBy(e => e.ID).ToList();
				    
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Add an existing spell to a spell line, adding a new line if needed.
		/// Primarily used for Champion spells but can be used to make any custom spell list
		/// From spells already loaded from the DB
		/// </summary>
		/// <param name="spellLineID"></param>
		/// <param name="spellID"></param>
		public static void AddSpellToSpellLine(string spellLineID, int spellID, int level = 1)
		{
			// Lock SkillBase for writes
			m_syncLockUpdates.EnterWriteLock();
			try
			{
				// Add Spell Line if needed (doesn't create the spellline index...)
				if(!m_lineSpells.ContainsKey(spellLineID))
					m_lineSpells.Add(spellLineID, new List<Spell>());
							
				try
				{
					Spell spl = (Spell)m_spellIndex[spellID].Clone();
					spl.Level = level;
					
					// search if it exists
					bool added = false;
					for (int r = 0; r < m_lineSpells[spellLineID].Count ; r++)
					{
						if (m_lineSpells[spellLineID][r] != null && m_lineSpells[spellLineID][r].ID == spl.ID)
						{
							// Replace
							m_lineSpells[spellLineID][r] = spl;
							added = true;
						}
					}
					
					if (!added)
						m_lineSpells[spellLineID].Add(spl);
					
					m_lineSpells[spellLineID] = m_lineSpells[spellLineID].OrderBy(e => e.Level).ThenBy(e => e.ID).ToList();
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Spell Line {1} Error {0} when adding Spell ID: {2}", e, spellLineID, spellID);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitWriteLock();
			}
		}

		/// <summary>
		/// Get Specialization By Internal ID, used for Tooltip
		/// </summary>
		/// <param name="internalID"></param>
		/// <returns></returns>
		public static Specialization GetSpecializationByInternalID(int internalID)
		{
			m_syncLockUpdates.EnterReadLock();
			string spec = null;
			try
			{
				spec = m_specsByName.Where(e => e.Value.Item4 == internalID).Select(e => e.Key).FirstOrDefault();
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (!Util.IsEmpty(spec, true))
				return GetSpecialization(spec, false);
			
			return GetSpecialization(string.Format("INTERNALID:{0}", internalID), true);
		}
		
		/// <summary>
		/// Get a loaded specialization, warn if not found and create a dummy entry
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Specialization GetSpecialization(string keyname)
		{
			return GetSpecialization(keyname, true);
		}

		/// <summary>
		/// Get a specialization by Keyname
		/// </summary>
		/// <param name="keyname"></param>
		/// <param name="create">if not found generate a warning and create a dummy entry</param>
		/// <returns></returns>
		public static Specialization GetSpecialization(string keyname, bool create)
		{
			Specialization spec = null;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_specsByName.ContainsKey(keyname))
					spec = GetNewSpecializationInstance(keyname, m_specsByName[keyname]);
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (spec.KeyName == keyname)
				return spec;

			if (create)
			{
				if (log.IsWarnEnabled)
				{
					log.WarnFormat("Specialization {0} unknown", keyname);
				}

				// Untrainable Spec by default to prevent garbage in player display...
				return new UntrainableSpecialization(keyname, "?" + keyname, 0, 0);
			}

			return null;
		}

		public static List<Specialization> GetSpecializationByType(Type type)
		{
			List<Specialization> result = null;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				result = m_specsByName.Where(ts => type.IsAssignableFrom(ts.Value.Item1))
					.Select(ts => GetNewSpecializationInstance(ts.Key, ts.Value)).ToList();
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (result == null)
				result = new List<Specialization>();
			
			return result;
		}
		
		/// <summary>
		/// Get a Class Specialization Career's to use data oriented Specialization Abilities and Skills.
		/// </summary>
		/// <param name="classID">Character Class ID</param>
		/// <returns>Dictionary of Specialization with their Level Requirement (including ClassId 0 for game wide specs)</returns>
		public static IDictionary<Specialization, int> GetSpecializationCareer(int classID)
		{
			Dictionary<Specialization, int> dictRes = new Dictionary<Specialization, int>();
			m_syncLockUpdates.EnterReadLock();
			IDictionary<string, int> entries = new Dictionary<string, int>();
			try
			{
				if (m_specsByClass.ContainsKey(classID))
				{
					entries = new Dictionary<string, int>(m_specsByClass[classID]);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			foreach (KeyValuePair<string, int> constraint in entries)
			{
				try
				{
					Specialization spec = GetSpecialization(constraint.Key, false);
					spec.LevelRequired = constraint.Value;
					dictRes.Add(spec, constraint.Value);
				}
				catch
				{
				}
			}

			m_syncLockUpdates.EnterReadLock();
			entries = new Dictionary<string, int>();
			try
			{
				if (m_specsByClass.ContainsKey(0))
				{
					entries = new Dictionary<string, int>(m_specsByClass[0]);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			// all Character Career's (mainly for sprint...)
			foreach (KeyValuePair<string, int> constraint in entries)
			{
				try
				{
					Specialization spec = GetSpecialization(constraint.Key, false);
					spec.LevelRequired = constraint.Value;
					dictRes.Add(spec, constraint.Value);
				}
				catch
				{
				}
			}
			
			return dictRes;
		}
		
		/// <summary>
		/// return all styles for a specific specialization
		/// if no style are associated or spec is unknown the list will be empty
		/// </summary>
		/// <param name="specID">KeyName of spec</param>
		/// <param name="classId">ClassID for which style list is requested</param>
		/// <returns>list of styles, never null</returns>
		public static List<Style> GetStyleList(string specID, int classId)
		{
			m_syncLockUpdates.EnterReadLock();
			List<Tuple<Style, byte>> entries = new List<Tuple<Style, byte>>();
			try
			{
				if(m_specsStyles.ContainsKey(specID) && m_specsStyles[specID].ContainsKey(classId))
				{
					entries = new List<Tuple<Style, byte>>(m_specsStyles[specID][classId]);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			List<Style> styleRes = new List<Style>();
				
			foreach(Tuple<Style, byte> constraint in entries)
				styleRes.Add((Style)constraint.Item1.Clone());
				
			return styleRes;
		}

		/// <summary>
		/// returns spec dependent abilities
		/// </summary>
		/// <param name="specID">KeyName of spec</param>
		/// <returns>list of abilities or empty list</returns>
		public static List<Ability> GetSpecAbilityList(string specID, int classID)
		{
			m_syncLockUpdates.EnterReadLock();
			List<Tuple<string, byte, int, int>> entries = new List<Tuple<string, byte, int, int>>();
			try
			{
				if (m_specsAbilities.ContainsKey(specID))
				{
					entries = new List<Tuple<string, byte, int, int>>(m_specsAbilities[specID]);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			List<Ability> abRes = new List<Ability>();
			foreach (Tuple<string, byte, int, int> constraint in entries)
			{
				if (constraint.Item4 != 0 && constraint.Item4 != classID)
					continue;
				
				Ability ab = GetNewAbilityInstance(constraint.Item1, constraint.Item3);
				ab.Spec = specID;
				ab.SpecLevelRequirement = constraint.Item2;
				abRes.Add(ab);
			}
				
			return abRes;
		}

		/// <summary>
		/// Find style by Internal ID, needed for Tooltip Use.
		/// </summary>
		/// <param name="internalID"></param>
		/// <returns></returns>
		public static Style GetStyleByInternalID(int internalID)
		{
			Style style = null;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				style = m_styleIndex.Where(e => e.Value.InternalID == internalID).Select(e => e.Value).FirstOrDefault();
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (style != null)
				return (Style)style.Clone();
			
			return style;
		}

		/// <summary>
		/// Find style with specific id and return a copy of it
		/// </summary>
		/// <param name="styleID">id of style</param>
		/// <param name="classId">ClassID for which style list is requested</param>
		/// <returns>style or null if not found</returns>
		public static Style GetStyleByID(int styleID, int classId)
		{
			KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(styleID, classId);
			Style style;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				m_styleIndex.TryGetValue(styleKey, out style);
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (style != null)
				return (Style)style.Clone();
			
			return style;
		}

		/// <summary>
		/// Get List of Spell, ClassId, Chance Constraints for this Style Procs...
		/// </summary>
		/// <param name="style"></param>
		/// <returns></returns>
		public static IList<Tuple<Spell, int, int>> GetStyleProcsByID(Style style)
		{
			List<Tuple<Spell, int, int>> procres = new List<Tuple<Spell, int, int>>();
			m_syncLockUpdates.EnterReadLock();
			Dictionary<int, Tuple<Spell, int>> entries = new Dictionary<int, Tuple<Spell, int>>();
			try
			{
				if (m_stylesProcs.ContainsKey(style.ID))
				{
					entries = new Dictionary<int, Tuple<Spell, int>>(m_stylesProcs[style.ID]);
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			foreach(KeyValuePair<int, Tuple<Spell, int>> item in entries)
				procres.Add(new Tuple<Spell, int, int>(item.Value.Item1, item.Key, item.Value.Item2));
			
			return procres;
		}
		
		/// <summary>
		/// Returns spell with id, level of spell is always 1
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static Spell GetSpellByID(int spellID)
		{
			Spell spell;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				m_spellIndex.TryGetValue(spellID, out spell);
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (spell != null)
				return (Spell)spell.Clone();
			
			return null;
		}

		/// <summary>
		/// Returns spell with id, level of spell is always 1
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static Spell GetSpellByTooltipID(ushort ttid)
		{
			Spell spell;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				int spellid;
				if (m_spellToolTipIndex.TryGetValue(ttid, out spellid))
				{
					m_spellIndex.TryGetValue(spellid, out spell);
				}
				else
				{
					spell = null;
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (spell != null)
				return (Spell)spell.Clone();
			
			return null;
		}

		/// <summary>
		/// Will attempt to find either in the spell line given or in the list of all spells
		/// </summary>
		/// <param name="spellID"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		public static Spell FindSpell(int spellID, SpellLine line)
		{
			Spell spell = null;

			if (line != null)
			{
				List<Spell> spells = GetSpellList(line.KeyName);
				foreach (Spell lineSpell in spells)
				{
					if (lineSpell.ID == spellID)
					{
						spell = lineSpell;
						break;
					}
				}
			}

			if (spell == null)
			{
				spell = GetSpellByID(spellID);
			}

			return spell;
		}


		/// <summary>
		/// Get display name of property
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static string GetPropertyName(eProperty prop)
		{
			string name;
			if (!m_propertyNames.TryGetValue(prop, out name))
			{
				name = "Property" + ((int)prop);
			}
			return name;
		}

		/// <summary>
		/// determine race-dependent base resist
		/// </summary>
		/// <param name="race">Value must be greater than 0</param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetRaceResist(int race, eResist type)
		{
			if( race == 0 )
				return 0;

			int resistValue = 0;
			
			if (m_raceResists.ContainsKey(race))
			{
				int resistIndex;

				if (type == eResist.Natural)
					resistIndex = 9;
				else
					resistIndex = (int)type - (int)eProperty.Resist_First;

				if (resistIndex < m_raceResists[race].Length)
				{
					resistValue = m_raceResists[race][resistIndex];
				}
				else
				{
					log.WarnFormat("No resists defined for type:  {0}", type.ToString());
				}
			}
			else
			{
				log.WarnFormat("No resists defined for race:  {0}", race);
			}

			return resistValue;
		}

		/// <summary>
		/// Convert object type to spec needed to use that object
		/// </summary>
		/// <param name="objectType">type of the object</param>
		/// <returns>spec names needed to use that object type</returns>
		public static string ObjectTypeToSpec(eObjectType objectType)
		{
			string res = null;
			if (!m_objectTypeToSpec.TryGetValue(objectType, out res))
				if (log.IsWarnEnabled)
					log.Warn("Not found spec for object type " + objectType);
			return res;
		}

		/// <summary>
		/// Convert spec to skill property
		/// </summary>
		/// <param name="specKey"></param>
		/// <returns></returns>
		public static eProperty SpecToSkill(string specKey)
		{
			eProperty res;
			if (!m_specToSkill.TryGetValue(specKey, out res))
			{
				//if (log.IsWarnEnabled)
				//log.Warn("No skill property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return res;
		}

		/// <summary>
		/// Convert spec to focus
		/// </summary>
		/// <param name="specKey"></param>
		/// <returns></returns>
		public static eProperty SpecToFocus(string specKey)
		{
			eProperty res;
			if (!m_specToFocus.TryGetValue(specKey, out res))
			{
				//if (log.IsWarnEnabled)
				//log.Warn("No skill property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return res;
		}
		
		private static ISpecActionHandler GetNewSpecActionHandler(Type type)
		{
			ISpecActionHandler handl = null;
			
			try
			{
				handl = (ISpecActionHandler)type.Assembly.CreateInstance(type.FullName);
				return handl;
			}
			catch
			{
			}
			
			return handl;
		}
		
		private static IAbilityActionHandler GetNewAbilityActionHandler(Type type)
		{
			IAbilityActionHandler handl = null;
			
			try
			{
				handl = (IAbilityActionHandler)type.Assembly.CreateInstance(type.FullName);
				return handl;
			}
			catch
			{
			}
			
			return handl;
		}
		
		private static Ability GetNewAbilityInstance(string keyname, int level)
		{
			Ability ab = null;
			DBAbility dba = null;
			m_syncLockUpdates.EnterReadLock();
			try
			{
				if (m_abilityIndex.ContainsKey(keyname))
				{
					dba = m_abilityIndex[keyname];
				}
			}
			finally
			{
				m_syncLockUpdates.ExitReadLock();
			}
			
			if (dba != null)
			{
				ab = GetNewAbilityInstance(dba);
				ab.Level = level;
			}
			
			return ab;
		}
		
		private static Ability GetNewAbilityInstance(DBAbility dba)
		{
			// try instanciating ability
			Ability ab = null;
			
			if (Util.IsEmpty(dba.Implementation, true) == false)
			{
				// Try instanciating Ability
				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					try
					{
						ab = (Ability)asm.CreateInstance(
							typeName: dba.Implementation, // string including namespace of the type
							ignoreCase: true,
							bindingAttr: BindingFlags.Default,
							binder: null,
							args: new object[] { dba, 0 },
							culture: null,
							activationAttributes: null);
						
						// instanciation worked
						if (ab != null) 
						{								
							break;
						}
					}
					catch
					{
					}
				}
				
				if (ab == null)
				{
					// Something Went Wrong when instanciating
					ab = new Ability(dba, 0);
					
					if (log.IsWarnEnabled)
						log.WarnFormat("Could not Instanciate Ability {0} from {1} reverting to default Ability...", dba.KeyName, dba.Implementation);
				}
			}
			else
			{
				ab = new Ability(dba, 0);
			}
			
			return ab;
		}
		
		private static Specialization GetNewSpecializationInstance(string keyname, Tuple<Type, string, ushort, int> entry)
		{
			Specialization gameSpec = null;
			
			try
			{
				gameSpec = (Specialization)entry.Item1.Assembly.CreateInstance(
						typeName: entry.Item1.FullName, // string including namespace of the type
						ignoreCase: true,
						bindingAttr: BindingFlags.Default,
						binder: null,
						args: new object[] { keyname, entry.Item2, entry.Item3, entry.Item4 },
						culture: null,
						activationAttributes: null);
					
					// instanciation worked
					if (gameSpec != null) 
					{	
						return gameSpec;
					}

			}
			catch
			{
			}
			
			return GetNewSpecializationInstance(keyname, entry.Item1.FullName, entry.Item2, entry.Item3, entry.Item4);
		}
		
		private static Specialization GetNewSpecializationInstance(string keyname, string type, string name, ushort icon, int id)
		{
			Specialization gameSpec = null;
			// Try instanciating Specialization
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					gameSpec = (Specialization)asm.CreateInstance(
						typeName: type, // string including namespace of the type
						ignoreCase: true,
						bindingAttr: BindingFlags.Default,
						binder: null,
						args: new object[] { keyname, name, icon, id },
						culture: null,
						activationAttributes: null);
					
					// instanciation worked
					if (gameSpec != null) 
					{						
						break;
					}
				}
				catch
				{
				}
			}
			
			if (gameSpec == null)
			{
				// Something Went Wrong when instanciating
				gameSpec = new Specialization(keyname, name, icon, id);
				
				if (log.IsErrorEnabled)
					log.ErrorFormat("Could not Instanciate Specialization {0} from {1} reverting to default Specialization...", keyname, type);
			}
			
			return gameSpec;

		}
	}
}
