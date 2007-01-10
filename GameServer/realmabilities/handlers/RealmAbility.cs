using System;
using System.Collections;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Base for all Realm Abilities
	/// </summary>
	public class RealmAbility : Ability
	{
		public RealmAbility(DBAbility ability, int level) : base(ability, level) { }

		public virtual int CostForUpgrade(int currentLevel)
		{
			return 1000;
		}

		/// <summary>
		/// true if player can immediately use that ability
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CheckRequirement(GamePlayer player)
		{
			return true;
		}

		/// <summary>
		/// max level this RA can reach
		/// </summary>
		public virtual int MaxLevel
		{
			get { return 0; }
		}

		public override string Name
		{
			get
			{
				return (Level <= 1) ? base.Name : m_name + " " + getRomanLevel();
			}
		}


		public override eSkillPage SkillType
		{
			get
			{
				return eSkillPage.RealmAbilities;
			}
		}
	}



	public class TimedRealmAbility : RealmAbility
	{

		public TimedRealmAbility(DBAbility ability, int level) : base(ability, level) { }

		public override int CostForUpgrade(int level)
		{
			return (level + 1) * 5;
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return true;
		}

		public virtual int GetReUseDelay(int level)
		{
			return 0;
		}

		protected string FormatTimespan(int seconds)
		{
			if (seconds >= 60)
			{
				return String.Format("{0:00}:{1:00} min", seconds / 60, seconds % 60);
			}
			else
			{
				return seconds + " sec";
			}
		}

		public virtual void AddReUseDelayInfo(IList list)
		{
			for (int i = 1; i <= MaxLevel; i++)
			{
				int reUseTime = GetReUseDelay(i);
				list.Add("Level " + i + ": Can use every: " + ((reUseTime == 0) ? "always" : FormatTimespan(reUseTime)));
			}
		}

		public virtual void AddEffectsInfo(IList list)
		{
		}

		public override int MaxLevel
		{
			get
			{
				return 3;
			}
		}

		public void DisableSkill(GameLiving living)
		{
			if (living is GamePlayer)
			{
				((GamePlayer)living).DisableSkill(this, GetReUseDelay(Level) * 1000);
			}
		}

		public override IList DelveInfo
		{
			get
			{
				IList list = base.DelveInfo;
				int size = list.Count;
				AddEffectsInfo(list);
				if (list.Count > size)
				{ // something was added
					list.Insert(size, "");	// add empty line
				}
				size = list.Count;
				AddReUseDelayInfo(list);
				if (list.Count > size)
				{ // something was added
					list.Insert(size, "");	// add empty line
				}
				return list;
			}
		}


		/// <summary>
		/// Send spell effect animation on caster and send messages
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spellEffect"></param>
		/// <param name="success"></param>
		public virtual void SendCasterSpellEffectAndCastMessage(GameLiving caster, ushort spellEffect, bool success)
		{
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(caster, caster, spellEffect, 0, false, success ? (byte)1 : (byte)0);

				if (Point3D.GetDistance(caster, player) <= WorldMgr.INFO_DISTANCE)
				{
					if (player == caster)
					{
						player.Out.SendMessage("You cast a " + m_name + " Spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage(caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}


		/// <summary>
		/// Sends cast message to environment
		/// </summary>
		protected virtual void SendCastMessage(GameLiving caster)
		{
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (Point3D.GetDistance(caster, player) <= WorldMgr.INFO_DISTANCE)
				{
					if (player == caster)
					{
						player.Out.SendMessage("You cast a " + m_name + " Spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Checks for any of the given conditions and returns true if there was any
		/// prints messages
		/// </summary>
		/// <param name="living"></param>
		/// <param name="bitmask"></param>
		/// <returns></returns>
		public bool CheckPreconditions(GameLiving living, long bitmask)
		{
			GamePlayer player = living as GamePlayer;
			if ((bitmask & DEAD) != 0 && !living.IsAlive)
			{
				if (player != null)
				{
					player.Out.SendMessage("You cannot use this ability while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & MEZZED) != 0 && living.IsMezzed)
			{
				if (player != null)
				{
					player.Out.SendMessage("You cannot use this ability while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & STUNNED) != 0 && living.IsStunned)
			{
				if (player != null)
				{
					player.Out.SendMessage("You cannot use this ability while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & SITTING) != 0 && living.IsSitting)
			{
				if (player != null)
				{
					player.Out.SendMessage("You cannot use this ability while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & INCOMBAT) != 0 && living.InCombat)
			{
				if (player != null)
				{
					player.Out.SendMessage("You have been in combat recently and cannot use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & NOTINCOMBAT) != 0 && !living.InCombat)
			{
				if (player != null)
				{
					player.Out.SendMessage("You must be in combat recently to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if ((bitmask & STEALTHED) != 0 && living.IsStealthed)
			{
				if (player != null)
				{
					player.Out.SendMessage("You cannot use this ability while stealthed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			if (player != null && (bitmask & NOTINGROUP) != 0 && player.PlayerGroup == null)
			{
				player.Out.SendMessage("You must be in a group use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			return false;
		}

		public const long DEAD = 0x00000001;
		public const long SITTING = 0x00000010;
		public const long MEZZED = 0x00000100;
		public const long STUNNED = 0x00001000;
		public const long INCOMBAT = 0x00010000;
		public const long NOTINCOMBAT = 0x00100000;
		public const long NOTINGROUP = 0x01000000;
		public const long STEALTHED = 0x10000000;
	}


	public class L5RealmAbility : RealmAbility
	{

		public L5RealmAbility(DBAbility ability, int level) : base(ability, level) { }

		public override int CostForUpgrade(int level)
		{
			switch (level)
			{
				case 0: return 1;
				case 1: return 3;
				case 2: return 6;
				case 3: return 10;
				case 4: return 14;
				default: return 1000;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return Level <= 5;
		}

		public override int MaxLevel
		{
			get
			{
				return 5;
			}
		}
	}
}