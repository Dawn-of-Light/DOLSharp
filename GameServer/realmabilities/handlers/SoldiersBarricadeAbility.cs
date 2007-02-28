using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{

	public class SoldiersBarricadeAbility : TimedRealmAbility
	{
		public SoldiersBarricadeAbility(DBAbility dba, int level) : base(dba, level) { }

		public const string BofBaSb = "RA_DAMAGE_DECREASE";

		int m_range = 1500;
		int m_duration = 30;
		int m_value = 0;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
			if (player.TempProperties.getProperty(BofBaSb, false))
			{
				player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			switch (Level)
			{
				case 1: m_value = 5; break;
				case 2: m_value = 15; break;
				case 3: m_value = 25; break;
				default: return;
			}
			DisableSkill(living);
			ArrayList targets = new ArrayList();
			if (player.PlayerGroup == null)
				targets.Add(player);
			else
			{
				foreach (GamePlayer grpMate in player.PlayerGroup.GetPlayersInTheGroup())
				{
					if (WorldMgr.CheckDistance(grpMate, player, m_range) && grpMate.IsAlive)
						targets.Add(grpMate);
				}
			}
			bool success;
			foreach (GamePlayer target in targets)
			{
				//send spelleffect
				if (!target.IsAlive) continue;
				success = !target.TempProperties.getProperty(BofBaSb, false);
				foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					visPlayer.Out.SendSpellEffectAnimation(player, target, 7015, 0, false, System.Convert.ToByte(success));
				if (success)
					if (target != null)
					{
						new SoldiersBarricadeEffect().Start(target, m_duration, m_value);
					}
			}

		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
