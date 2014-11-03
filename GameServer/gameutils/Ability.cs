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
using System.Reflection;

using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// callback handler for an ability that is activated by clicking on an associated icon
	/// </summary>
	public interface IAbilityActionHandler
	{
		void Execute(Ability ab, GamePlayer player);
	}

	/// <summary>
	/// the ability class
	/// nontrainable abilities have level 0
	/// trainable abilities have level > 0, level is displayed in roman numbers	
	/// </summary>
	public class Ability : NamedSkill
	{

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string m_spec;
		protected string m_serializedNames;
		protected string m_description;
		protected int m_speclevel;
		
		protected GameLiving m_activeLiving = null;


		public Ability(DBAbility dba)
			: this(dba, 0)
		{
		}

		public Ability(DBAbility dba, int level)
			: this(dba.KeyName, dba.Name, dba.Description, dba.AbilityID, (ushort)dba.IconID, level, dba.InternalID)
		{
		}

		public Ability(DBAbility dba, int level, string spec, int speclevel)
			: this(dba.KeyName, dba.Name, dba.Description, dba.AbilityID, (ushort)dba.IconID, level, spec, speclevel, dba.InternalID)
		{
		}

		public Ability(string keyname, string displayname, string description, int id, ushort icon, int level, int internalID)
			: this(keyname, displayname, description, id, icon, level, "", 0, internalID)
		{
		}

		public Ability(string keyname, string displayname, string description, int id, ushort icon, int level, string spec, int speclevel, int internalID)
			: base(keyname, displayname, id, icon, level, internalID)
		{
			m_spec = spec;
			m_speclevel = speclevel;
			m_serializedNames = displayname;
			m_description = description;
			UpdateCurrentName();
		}

		/// <summary>
		/// Updates the current ability name
		/// </summary>
		private void UpdateCurrentName()
		{
			try
			{
				string name = m_serializedNames;
				SortedDictionary<int, string> nameByLevel = new SortedDictionary<int, string>();
				foreach (string levelNamePair in name.Trim().SplitCSV())
				{
					if (levelNamePair.Trim().Length <= 0)
						continue;
					
					string[] levelAndName = levelNamePair.Trim().Split('|');
					
					if (levelAndName.Length < 2)
						continue;
					
					nameByLevel.Add(int.Parse(levelAndName[0]), levelAndName[1]);
				}

				foreach (KeyValuePair<int, string> entry in nameByLevel)
				{
					if (entry.Key > Level)
						break;
					
					name = entry.Value;
				}

				string roman = getRomanLevel();
				m_name = name.Replace("%n", roman);
			}
			catch (Exception e)
			{
				log.Error("Parsing ability display name: keyname='" + KeyName + "' m_serializedNames='" + m_serializedNames + "'", e);
			}
		}

		/// <summary>
		/// this is called when the ability should do its modifications (passive effects)
		/// </summary>
		/// <param name="living"></param>
		/// <param name="sendUpdates"></param>
		public virtual void Activate(GameLiving living, bool sendUpdates)
		{
			m_activeLiving = living;
		}

		/// <summary>
		/// this is called when the ability should remove its modifications
		/// </summary>
		/// <param name="living"></param>
		/// <param name="sendUpdates"></param>
		public virtual void Deactivate(GameLiving living, bool sendUpdates)
		{ 
		}

		/// <summary>
		/// Called when an ability level is changed while the ability is activated on a living
		/// </summary>
		public virtual void OnLevelChange(int oldLevel, int newLevel = 0)
		{
		}

		// /// <summary>
		// /// called when all modifications should be taken back
		// /// </summary>
		// /// <param name="living"></param>
		//public virtual void Deactivate(GameLiving living) {			
		//}

		/// <summary>
		/// active abilities (clicked by icon) are called back here
		/// </summary>
		/// <param name="living"></param>
		public virtual void Execute(GameLiving living)
		{
		}

		/// <summary>
		/// The Specialization thats need to be trained to get that ability
		/// </summary>
		public string Spec
		{
			get { return m_spec; }
			set { m_spec = value; }
		}

		/// <summary>
		/// The Specialization's level required to get that ability
		/// </summary>
		public int SpecLevelRequirement
		{
			get { return m_speclevel; }
			set { m_speclevel = value; }
		}

		/// <summary>
		/// Set the level of an ability
		/// </summary>
		public override int Level
		{
			get { return base.Level; }
			set
			{
				int oldLevel = m_level;
				base.Level = value;
				UpdateCurrentName();
				if (m_activeLiving != null)
					OnLevelChange(oldLevel);
			}
		}
		
		/// <summary>
		/// get the level represented as roman numbers
		/// </summary>
		/// <returns></returns>
		protected string getRomanLevel()
		{
			switch (Level)
			{
				case 1: return "I";
				case 2: return "II";
				case 3: return "III";
				case 4: return "IV";
				case 5: return "V";
				case 6: return "VI";
				case 7: return "VII";
				case 8: return "VIII";
				case 9: return "IX";
				case 10: return "X";
				case 11: return "XI";
				case 12: return "XII";
			}
			return "";
		}

		public virtual IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>(4);
				foreach (string part in m_description.Split(new char[] { '|' }))
				{
					list.Add(part);
				}

				return list;
			}
		}

		public override eSkillPage SkillType
		{
			get
			{
				return eSkillPage.Abilities;
			}
		}
	}
}
