using System.Collections;

using DOL.Database;
using DOL.GS;

namespace DOL.GS.SkillHandler
{
	public class StatChangingAbility : PropertyChangingAbility
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public StatChangingAbility(DBAbility dba, int level, eProperty[] property)
			: base(dba, level, property)
		{

		}

		public StatChangingAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{

		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		public override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;	// need new prop system to not worry about updates
			if (player != null)
			{
				player.Out.SendCharStatsUpdate();
				player.Out.SendCharResistsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
			}

			if (target.IsAlive)
			{
				if (target.Health < target.MaxHealth) target.StartHealthRegeneration();
				else if (target.Health > target.MaxHealth) target.Health = target.MaxHealth;

				if (target.Mana < target.MaxMana) target.StartPowerRegeneration();
				else if (target.Mana > target.MaxMana) target.Mana = target.MaxMana;

				if (target.Endurance < target.MaxEndurance) target.StartEnduranceRegeneration();
				else if (target.Endurance > target.MaxEndurance) target.Endurance = target.MaxEndurance;
			}
		}
	}
}
