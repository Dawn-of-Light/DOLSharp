using System;
using DOL.GS.PacketHandler;
using System.Text;

namespace DOL.GS.SkillHandler
{
	class SavageAbilities
	{
		public const string SavageTaunt = "SavageTaunt";
		public const string SavageDamage = "SavageDPSBuff";
		public const string SavageEvade = "SavageEvade";
		public const string SavageHaste = "SavageHaste";
		public const string SavageHeal = "SavageHeal";
		public const string SavageParry = "SavageParry";


		public static void ApplyHPPenalty(GamePlayer player, int level)
		{
			double penalty = 0;
			switch (level)
			{
                case -1: penalty = 0.01; break;
				case -2: penalty = 0.02; break;
                case -3: penalty = 0.03; break;
				case -4: penalty = 0.04; break;
				case -5: penalty = 0.05; break;
			}

            double max = player.MaxHealth;
            int take = (int)(max * penalty);
            if (player.Health - take < 1)
                return;
            player.TakeDamage(player, eDamageType.Natural, take, 0);
		}

		public static bool CanUseSavageAbility(GamePlayer player, int level)
		{
            double penalty = 0;
            switch (level)
            {
                case -1: penalty = 0.01; break;
                case -2: penalty = 0.02; break;
                case -3: penalty = 0.03; break;
                case -4: penalty = 0.04; break;
                case -5: penalty = 0.05; break;
            }

			return player.HealthPercent > 10+(penalty*100);
		}

		public static bool SavagePreFireChecks(GamePlayer player)
		{
			if (player == null)
			{
				return false;
			}

			if (!player.IsAlive)
			{
				player.Out.SendMessage("You cannot use this while Dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage("You cannot use this while Mezzed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage("You cannot use this while Stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage("You must be standing to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
	}
}
