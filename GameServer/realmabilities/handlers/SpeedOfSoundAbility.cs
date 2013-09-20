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
				|| player.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ArmsLengthEffect), typeof(ChargeEffect)) > 0)
			{
				player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: m_duration = 10000; break;
					case 2: m_duration = 20000; break;
					case 3: m_duration = 30000; break;
					case 4: m_duration = 45000; break;
					case 5: m_duration = 60000; break;
					default: return;
				}
			}
			else
			{
				switch (Level)
				{
					case 1: m_duration = 10000; break;
					case 2: m_duration = 30000; break;
					case 3: m_duration = 60000; break;
					default: return;
				}
			}

			DisableSkill(living);

			ArrayList targets = new ArrayList();

			if (player.Group == null)
				targets.Add(player);
			else
			{
				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (player.IsWithinRadius( p, m_range ) && p.IsAlive)
						targets.Add(p);
				}
			}

			bool success;
			foreach (GamePlayer target in targets)
			{
				//send spelleffect
				success = target.EffectList.CountOfType<SpeedOfSoundEffect>() == 0;
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
