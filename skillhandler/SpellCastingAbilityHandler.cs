using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.SkillHandler
{
	public class SpellCastingAbilityHandler : IAbilityActionHandler 
	{
		protected Ability m_ability = null;
		public Ability Ability
		{
			get { return m_ability; }
		}
		public virtual int SpellID
		{
			get { return 0; }
		}

		public virtual long Preconditions
		{
			get { return 0; }
		}

		protected Spell m_spell = null;
		public virtual Spell Spell
		{
			get
			{
				if (m_spell == null)
					m_spell = SkillBase.GetSpellByID(SpellID);
				return m_spell;
			}
		}

		protected SpellLine m_spellLine = null;
		public virtual SpellLine SpellLine
		{
			get
			{
				if (m_spellLine == null)
					m_spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
				return m_spellLine;
			}
		}

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
				return;

			m_ability = ab;

			if (CheckPreconditions(player, Preconditions)) return;
 
			if (SpellLine != null && Spell != null)
			{
				player.CastSpell(this);
			}
		}

		/// <summary>
		/// Checks for any of the given conditions and returns true if there was any
		/// prints messages
		/// </summary>
		/// <param name="living"></param>
		/// <param name="bitmask"></param>
		/// <returns></returns>
		public virtual bool CheckPreconditions(GameLiving living, long bitmask)
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
			if (player != null && (bitmask & NOTINGROUP) != 0 && player.Group == null)
			{
				player.Out.SendMessage("You must be in a group use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			if (player != null && (bitmask & TARGET) != 0 && player.TargetObject == null)
			{
				player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			return false;
		}

		/*
		 * Stored in hex, different values in binary
		 * e.g.
		 * 16|8|4|2|1
		 * ----------
		 * 1
		 * 0|0|0|0|1 stored as 0x00000001
		 * 2
		 * 0|0|0|1|0 stored as 0x00000002
		 * 4
		 * 0|0|1|0|0 stored as 0x00000004
		 * 8
		 * 0|1|0|0|0 stored as 0x00000008
		 * 16
		 * 1|0|0|0|0 stored as 0x00000010
		 */
		public const long DEAD = 0x00000001;
		public const long SITTING = 0x00000002;
		public const long MEZZED = 0x00000004;
		public const long STUNNED = 0x00000008;
		public const long INCOMBAT = 0x00000010;
		public const long NOTINCOMBAT = 0x00000020;
		public const long NOTINGROUP = 0x00000040;
		public const long STEALTHED = 0x000000080;
		public const long TARGET = 0x000000100;
	}
}
