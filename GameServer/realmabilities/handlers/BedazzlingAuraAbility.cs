using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	public class BedazzlingAuraAbility : TimedRealmAbility
	{
		public BedazzlingAuraAbility(DBAbility dba, int level) : base(dba, level) { }

		public const string BofBaSb = "RA_DAMAGE_DECREASE";

		int m_range = 1500;
		int m_value = 0;

		public override void Execute(GameLiving living)
		{
			GamePlayer player = living as GamePlayer;
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			if (player.TempProperties.getProperty(BofBaSb, false))
			{
				player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (Level)
			{
				case 1: m_value = 10; break;
				case 2: m_value = 20; break;
				case 3: m_value = 40; break;
				default: return;
			}

			DisableSkill(living);

			ArrayList targets = new ArrayList();
			if (player.Group == null)
				targets.Add(player);
			else
			{
				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (player.IsWithinRadius(p, m_range ) && p.IsAlive)
						targets.Add(p);
				}
			}
			bool success;
			foreach (GamePlayer target in targets)
			{
				//send spelleffect
				if (!target.IsAlive) continue;
				if (target.CharacterClass.Name=="Vampiir" && SpellHandler.FindEffectOnTarget(target, "VampiirMagicResistance") != null ) continue;
				success = !target.TempProperties.getProperty(BofBaSb, false);
				foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					visPlayer.Out.SendSpellEffectAnimation(player, target, 7030, 0, false, CastSuccess(success));
				if (success)
					if (target != null)
					{
						new BedazzlingAuraEffect().Start(target, m_value);
					}
			}

		}
		private byte CastSuccess(bool suc)
		{
			if (suc)
				return 1;
			else
				return 0;
		}
		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
