using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class SpeedOfSoundAbility : TimedRealmAbility
	{
		public SpeedOfSoundAbility(DBAbility dba, int level) : base(dba, level) { }

		int m_range = 2000;
		int m_duration = 1;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
			/* if (player.IsSpeedWarped)
			 {
				 player.Out.SendMessage("You cannot use this ability while speed warped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				 return;
			 }*/

			if (player.TempProperties.getProperty("Charging", false)
				|| player.EffectList.CountOfType(typeof(SpeedOfSoundEffect)) > 0
				|| player.EffectList.CountOfType(typeof(ArmsLengthEffect)) > 0
				|| player.EffectList.CountOfType(typeof(ChargeEffect)) > 0)
			{
				player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (Level)
			{
				case 1: m_duration = 1000; break;
				case 2: m_duration = 3000; break;
				case 3: m_duration = 6000; break;
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
				success = target.EffectList.CountOfType(typeof(SpeedOfSoundEffect)) == 0;
				foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					visPlayer.Out.SendSpellEffectAnimation(player, target, 7021, 0, false, CastSuccess(success));
				if (success)
				{
					GameSpellEffect speed = Spells.SpellHandler.FindEffectOnTarget(target, "SpeedEnhancement");
					if (speed != null)
						speed.Cancel(false);
					new SpeedOfSoundEffect(m_duration).Start(target);
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
