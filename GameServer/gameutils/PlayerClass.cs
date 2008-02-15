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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// Denotes a class as a DOL Character class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PlayerClassAttribute : Attribute
	{
		protected string m_name;
		protected string m_femaleName;
		protected string m_basename;
		protected int m_id;

		public PlayerClassAttribute(int id, string name, string basename, string femalename)
		{
			m_basename = basename;
			m_name = name;
			m_id = id;
			m_femaleName = femalename;
		}

		public PlayerClassAttribute(int id, string name, string basename)
		{
			m_basename = basename;
			m_name = name;
			m_id = id;
		}

		public int ID
		{
			get
			{
				return m_id;
			}
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public string BaseName
		{
			get
			{
				return m_basename;
			}
		}

		public string FemaleName
		{
			get
			{
				return m_femaleName;
			}
		}
	}


	/// <summary>
	/// Interface thats required for finding the Character Classes among the other scripts
	/// TODO: perhaps redundand, because DOLClassAttribute/CharacterClassSpec can be used
	/// </summary>
	public interface IClassSpec
	{
		int ID
		{
			get;
		}

		string Name
		{
			get;
		}

		string BaseName
		{
			get;
		}

		string Profession
		{
			get;
		}

		int BaseHP
		{
			get;
		}

		int SpecPointsMultiplier
		{
			get;
		}

		eStat PrimaryStat
		{
			get;
		}

		eStat SecondaryStat
		{
			get;
		}
		eStat TertiaryStat
		{
			get;
		}
		eStat ManaStat
		{
			get;
		}
		int WeaponSkillBase
		{
			get;
		}
		int WeaponSkillRangedBase
		{
			get;
		}
		eClassType ClassType
		{
			get;
		}
		string GetTitle(int level);
		void OnLevelUp(GamePlayer player);
		void OnRealmLevelUp(GamePlayer player);
		void OnSkillTrained(GamePlayer player, Specialization skill);
		bool CanUseLefthandedWeapon(GamePlayer player);
		IList AutoTrainableSkills();
		string FemaleName
		{
			get;
		}
		void SwitchToFemaleName();
		bool HasAdvancedFromBaseClass();
	}

	/// <summary>
	/// The type of character class
	/// </summary>
	public enum eClassType : int
	{
		/// <summary>
		/// The class has access to all spells
		/// </summary>
		ListCaster,
		/// <summary>
		/// The class has access to best one or two spells
		/// </summary>
		Hybrid,
		/// <summary>
		/// The class has no spells
		/// </summary>
		PureTank,
	}


	/// <summary>
	/// The Base class for all Character Classes in DOL
	/// </summary>
	public abstract class CharacterClassSpec : IClassSpec
	{
		/// <summary>
		/// id of class in Client
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Name of class
		/// </summary>
		protected string m_name;

		/// <summary>
		/// Female name of class
		/// </summary>
		protected string m_femaleName;

		/// <summary>
		/// Base of this class
		/// </summary>
		protected string m_basename;

		/// <summary>
		/// Profession of character, e.g. Defenders of Albion
		/// </summary>
		protected string m_profession;

		/// <summary>
		/// multiplier for specialization points per level in 10th
		/// </summary>
		protected int m_specializationMultiplier = 10;

		/// <summary>
		/// BaseHP for hp calculation
		/// </summary>
		protected int m_baseHP = 600;

		/// <summary>
		/// Stat gained every level.
		///	see eStat consts
		/// </summary>
		protected eStat m_primaryStat = eStat.UNDEFINED;

		/// <summary>
		/// Stat gained every second level.
		/// see eStat consts
		/// </summary>
		protected eStat m_secondaryStat = eStat.UNDEFINED;

		/// <summary>
		/// Stat gained every third level.
		/// see eStat consts
		/// </summary>
		protected eStat m_tertiaryStat = eStat.UNDEFINED;

		/// <summary>
		/// Stat that affects the power/mana pool.
		/// Do not set if they do not have a power pool/spells
		/// </summary>
		protected eStat m_manaStat = eStat.UNDEFINED;

		/// <summary>
		/// Weapon Skill Base value to influence weapon skill calc
		/// </summary>
		protected int m_wsbase = 400;

		/// <summary>
		/// Weapon Skill Base value to influence ranged weapon skill calc
		/// </summary>
		protected int m_wsbaseRanged = 440;

		public CharacterClassSpec()
		{
			m_id = 0;
			m_name = "Unknown Class";
			m_basename = "Unknown Base Class";
			m_profession = "";

			// initialize members from attributes
			Attribute[] attrs = Attribute.GetCustomAttributes(this.GetType(), typeof(PlayerClassAttribute));
			foreach (Attribute attr in attrs)
			{
				if (attr is PlayerClassAttribute)
				{
					m_id = ((PlayerClassAttribute)attr).ID;
					m_name = ((PlayerClassAttribute)attr).Name;
					m_basename = ((PlayerClassAttribute)attr).BaseName;
					if (Util.IsEmpty(((PlayerClassAttribute)attr).FemaleName) == false)
						m_femaleName = ((PlayerClassAttribute)attr).FemaleName;
					break;
				}
			}
		}

		public void SwitchToFemaleName()
		{
			m_name = m_femaleName;
		}

		public string FemaleName
		{
			get { return m_femaleName; }
		}

		public int BaseHP
		{
			get { return m_baseHP; }
		}

		public int ID
		{
			get { return m_id; }
		}

		public string Name
		{
			get { return m_name; }
		}

		public string BaseName
		{
			get { return m_basename; }
		}

		public string Profession
		{
			get { return m_profession; }
		}

		public int SpecPointsMultiplier
		{
			get { return m_specializationMultiplier; }
		}

		public eStat PrimaryStat
		{
			get { return m_primaryStat; }
		}

		public eStat SecondaryStat
		{
			get { return m_secondaryStat; }
		}

		public eStat TertiaryStat
		{
			get { return m_tertiaryStat; }
		}

		public eStat ManaStat
		{
			get { return m_manaStat; }
		}

		public int WeaponSkillBase
		{
			get { return m_wsbase; }
		}

		public int WeaponSkillRangedBase
		{
			get { return m_wsbaseRanged; }
		}

		public virtual string GetTitle(int level)
		{
			return "None";
		}

		public virtual eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		/// <summary>
		/// Return the base list of Realm abilities that the class
		/// can train in.  Added by Echostorm for RAs
		/// </summary>
		/// <returns></returns>
		public virtual IList AutoTrainableSkills()
		{
			return new ArrayList(0);
		}


		/// <summary>
		/// Add all skills and other things that are required for current level
		/// </summary>
		/// <param name="player">player to modify</param>
        public virtual void OnLevelUp(GamePlayer player)
        {
            // Graveen - livelike autotrain 1.87 
            // if spent points and autotrain raise, these points are refund
            if (player.Level % 4 == 0)
            {
                    foreach (Specialization spec in player.GetSpecList())
                        foreach (string autotrainKey in AutoTrainableSkills())
                        {
                            if (autotrainKey != spec.KeyName) continue;
                            if (spec.Level < player.Level / 4)
                                player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(player.Client, "PlayerClass.OnLevelUp.Autotrain", spec.Name, (int)(player.Level / 4)));
                        }
            }
        }

		/// <summary>
		/// Add various skills as the player levels his realm rank up
		/// </summary>
		/// <param name="player">player to modify</param>
		public virtual void OnRealmLevelUp(GamePlayer player)
		{
			//we dont want to add things when players arent using their advanced class
			if (player.CharacterClass.BaseName == player.CharacterClass.Name)
				return;

			//add rr5 realm abilities
			if (player.RealmLevel >= 40)
			{
				Ability ab = SkillBase.getClassRealmAbility(player.CharacterClass.ID);
				if (ab != null)
					player.AddAbility(ab, true);
			}
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="skill">The skill that is trained</param>
		public virtual void OnSkillTrained(GamePlayer player, Specialization skill)
		{
		}

		/// <summary>
		/// Checks whether player has ability to use lefthanded weapons
		/// </summary>
		public virtual bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return false;
		}

		public virtual bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}

	/// <summary>
	/// Usable default Character Class, if not other can be found or used
	/// just for getting things valid in problematic situations
	/// </summary>
	public class DefaultCharacterClass : CharacterClassSpec
	{
		public DefaultCharacterClass()
			: base()
		{
			m_id = 0;
			m_name = "Unknown";
			m_basename = "Unknown Class";
			m_profession = "None";
		}
	}
}
