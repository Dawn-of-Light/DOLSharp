using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database2;

namespace DOL.GS.RealmAbilities
{
	public class ChargeAbility : TimedRealmAbility
	{
		public const int DURATION = 15;

		public ChargeAbility(DBAbility dba, int level) : base(dba, level) { }

		public override void Execute(GameLiving living)
		{
			if (living == null) return;
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			/* 0_o
			if (player.IsSpeedWarped)
			{
				player.Out.SendMessage("You cannot use this ability while speed warped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}*/

			if (living.TempProperties.getProperty("Charging", false)
				|| living.EffectList.CountOfType(typeof(SpeedOfSoundEffect)) > 0
				|| living.EffectList.CountOfType(typeof(ArmsLengthEffect)) > 0
				|| living.EffectList.CountOfType(typeof(ChargeEffect)) > 0)
			{
				if (living is GamePlayer)
					((GamePlayer)living).Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			ChargeEffect charge = (ChargeEffect)living.EffectList.GetOfType(typeof(ChargeEffect));
			if (charge != null)
				charge.Cancel(false);
			if (living is GamePlayer)
				((GamePlayer)living).Out.SendUpdateMaxSpeed();

			new ChargeEffect().Start(living);
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			switch (level)
			{
				case 1: return 900;
				case 2: return 300;
				case 3: return 90;
			}
			return 600;
		}

        public override bool CheckRequirement(GamePlayer player)
        {
            return player.Level >= 45;
        }
	}
}
