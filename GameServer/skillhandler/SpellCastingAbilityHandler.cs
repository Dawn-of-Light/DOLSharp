using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.SkillHandler
{
	public class SpellCastingAbilityHandler : IAbilityActionHandler 
	{
		protected Ability m_ability = null;
		public virtual int SpellID
		{
			get { return 0; }
		}

		public virtual long Preconditions
		{
			get { return 0; }
		}

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
				return;

			m_ability = ab;

			if (CheckPreconditions(player, Preconditions)) return;

			SpellLine spline = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
 			Spell abSpell = SkillBase.GetSpellByID(SpellID);
 
			if (spline != null && abSpell != null)
			{
				player.CastSpell(abSpell, spline);
				player.DisableSkill(ab, (abSpell.RecastDelay==0 ? 3 : abSpell.RecastDelay));
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
}
