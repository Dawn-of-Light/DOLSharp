using System;
using DOL.Database2;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Anger of the Gods RA
	/// </summary>
	public class EpiphanyAbility : RR5RealmAbility
	{
		public EpiphanyAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			bool deactivate = false;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (player.Group != null)
				{
					SendCasterSpellEffectAndCastMessage(living, 7066, true);
					foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
					{
						if (!CheckPreconditions(member, DEAD) && member.CurrentRegionID == living.CurrentRegionID
							&& member.CurrentRegionID == living.CurrentRegionID
							&& WorldMgr.GetDistance(living, member) <= 2000)
						{
							if (restoreMana(member, player))
								deactivate = true;
						}
					}
				}
				else
				{
					if (!CheckPreconditions(player, DEAD))
					{
						if (restoreMana(player, player))
							deactivate = true;
					}
				}
			}
			if (deactivate)
				DisableSkill(living);
		}

		private bool restoreMana(GameLiving target, GamePlayer owner)
		{
			int mana = (int)(target.MaxMana * 0.25);
			int modheal = target.MaxMana - target.Mana;
			if (modheal < 1)
				return false;
			if (modheal > mana)
				modheal = mana;
			if (target is GamePlayer && target != owner)
				((GamePlayer)target).Out.SendMessage(owner.Name + " restores you " + modheal + " points of mana", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			if (target != owner)
				owner.Out.SendMessage("You restore" + target.Name + " " + modheal + " points of mana", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			if (target == owner)
				owner.Out.SendMessage("You restore yourself " + modheal + " points of mana", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			target.Mana += modheal;
			return true;

		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}


		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("25% Group power refresh. Skald must be out of combat to use. (group members may be in combat)");
			list.Add("");
			list.Add("Target: Group");
			list.Add("Casting time: instant");
		}

	}
}