using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class ChargeAbility : TimedRealmAbility
	{
		public const int DURATION = 15;

		public ChargeAbility(DBAbility dba, int level) : base(dba, level) { }

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;

			/* 0_o
			if (player.IsSpeedWarped)
			{
				player.Out.SendMessage("You cannot use this ability while speed warped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}*/

			if (player != null)
			{
				ChargeEffect charge = (ChargeEffect)player.EffectList.GetOfType(typeof(ChargeEffect));
				if (charge != null)
					charge.Cancel(false);
				player.Out.SendUpdateMaxSpeed();

				new ChargeEffect().Start(player);
			}
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
	}
}
