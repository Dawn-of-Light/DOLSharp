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
using DOL.GS;
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// The Base class for all Character Classes in DOL
	/// </summary>
	public abstract class CharacterClassBase : ICharacterClass
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

		/// <summary>
		/// The GamePlayer for this character
		/// </summary>
		public GamePlayer Player { get; set; }

		private static readonly string[] AutotrainableSkills = new string[0];

		public CharacterClassBase()
		{
			m_id = 0;
			m_name = "Unknown Class";
			m_basename = "Unknown Base Class";
			m_profession = "";

			// initialize members from attributes
			Attribute[] attrs = Attribute.GetCustomAttributes(this.GetType(), typeof(CharacterClassAttribute));
			foreach (Attribute attr in attrs)
			{
				if (attr is CharacterClassAttribute)
				{
					m_id = ((CharacterClassAttribute)attr).ID;
					m_name = ((CharacterClassAttribute)attr).Name;
					m_basename = ((CharacterClassAttribute)attr).BaseName;
					if (Util.IsEmpty(((CharacterClassAttribute)attr).FemaleName) == false)
						m_femaleName = ((CharacterClassAttribute)attr).FemaleName;
					break;
				}
			}
		}

		public virtual void Init(GamePlayer player)
		{
			Player = player;
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

		/// <summary>
		/// Maximum number of pulsing spells that can be active simultaneously
		/// </summary>
		public virtual ushort MaxPulsingSpells
		{
			get { return 1; }
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
		public virtual IList<string> GetAutotrainableSkills()
		{
			return AutotrainableSkills;
		}

		/// <summary>
		/// Add all skills and other things that are required for current level
		/// </summary>
		/// <param name="player">player to modify</param>
		public virtual void OnLevelUp(GamePlayer player)
		{
			// Grav: autotrain in player.OnLevelUp()
			// If this is a PvE server, issue one realm specialty point
			// for each level starting at level 20.
			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
			{
				// Somewhere in the code, regardless of server type, one RA point
				// is being assigned at lvl 20 already (I don't know where that's
				// coming from).  That's why the following is <= 20 and not just < 20.
				if (player.Level <= 20) return;
				player.RealmSpecialtyPoints++;
				player.SaveIntoDatabase();
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

		public virtual void SetControlledBrain(IControlledBrain controlledBrain)
		{
			if (controlledBrain == Player.ControlledBrain) return;
			if (controlledBrain == null)
			{
				Player.Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client, "GamePlayer.SetControlledNpc.ReleaseTarget2", Player.ControlledBrain.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client, "GamePlayer.SetControlledNpc.ReleaseTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (controlledBrain.Owner != Player)
					throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Player.Name + ", owner=" + controlledBrain.Owner.Name + ")", "controlledNpc");
				if (Player.ControlledBrain == null)
					Player.InitControlledBrainArray(1);
				Player.Out.SendPetWindow(controlledBrain.Body, ePetWindowAction.Open, controlledBrain.AggressionState, controlledBrain.WalkState);
				if (controlledBrain.Body != null)
				{
					Player.Out.SendNPCCreate(controlledBrain.Body); // after open pet window again send creation NPC packet
					if (controlledBrain.Body.Inventory != null)
						Player.Out.SendLivingEquipmentUpdate(controlledBrain.Body);
				}
			}

			Player.ControlledBrain = controlledBrain;

		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		public virtual void CommandNpcRelease()
		{
			IControlledBrain controlledBrain = Player.ControlledBrain;
			if (controlledBrain == null)
				return;

			GameNPC npc = controlledBrain.Body;
			if (npc == null)
				return;

			Player.Notify(GameLivingEvent.PetReleased, npc);
		}

		/// <summary>
		/// Invoked when pet is released.
		/// </summary>
		public virtual void OnPetReleased()
		{
		}

		/// <summary>
		/// Can this character start an attack?
		/// </summary>
		/// <param name="attackTarget"></param>
		/// <returns></returns>
		public virtual bool StartAttack(GameObject attackTarget)
		{
			return true;
		}


		/// <summary>
		/// Return the health percent of this character
		/// </summary>
		public virtual byte HealthPercentGroupWindow
		{
			get
			{
				return Player.HealthPercent;
			}
		}


		/// <summary>
		/// Create a shade effect for this player.
		/// </summary>
		/// <returns></returns>
		public virtual ShadeEffect CreateShadeEffect()
		{
			return new ShadeEffect();
		}

		/// <summary>
		/// Changes shade state of the player.
		/// </summary>
		/// <param name="state">The new state.</param>
		public virtual void Shade(bool makeShade)
		{
			if (Player.IsShade == makeShade)
			{
				if (makeShade && (Player.ObjectState == GameObject.eObjectState.Active))
					Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client, "GamePlayer.Shade.AlreadyShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (makeShade)
			{
				// Turn into a shade.
				Player.Model = Player.ShadeModel;
				Player.ShadeEffect = CreateShadeEffect();
				Player.ShadeEffect.Start(Player);
			}
			else
			{
				// Drop shade form.
				Player.ShadeEffect.Stop();
				Player.ShadeEffect = null;
				Player.Model = Player.CreationModel;
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client, "GamePlayer.Shade.NoLongerShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Called when player is removed from world.
		/// </summary>
		/// <returns></returns>
		public virtual bool RemoveFromWorld()
		{
			return true;
		}

		/// <summary>
		/// What to do when this character dies
		/// </summary>
		/// <param name="killer"></param>
		public virtual void Die(GameObject killer)
		{
		}

		public virtual void Notify(DOLEvent e, object sender, EventArgs args)
		{
		}

		public virtual bool CanChangeCastingSpeed(SpellLine line, Spell spell)
		{
			return true;
		}
	}

	/// <summary>
	/// Usable default Character Class, if not other can be found or used
	/// just for getting things valid in problematic situations
	/// </summary>
	public class DefaultCharacterClass : CharacterClassBase
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
