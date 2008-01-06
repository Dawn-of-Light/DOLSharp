using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Bonus for owning certain amount of keeps
	/// The int value is the amount of keeps needed
	/// </summary>
	public enum eKeepBonusType : int
	{
		Coin_Drop_3 = 8,
		Experience_3 = 9,
		Bounty_Points_3 = 10,
		Craft_Timers_3 = 11,
		Coin_Drop_5 = 12,
		Experience_5 = 13,
		Bounty_Points_5 = 14,
		Craft_Timers_5 = 15,
		Power_Pool = 16,
		Endurance_Pool = 17,
		Power_Regeneration = 18,
		Health_Regeneration = 19,
		Melee_Critical = 20,
		Spell_Critical = 30
	}

	public class KeepBonusMgr
	{
		private static int albCount = 0;
		private static int midCount = 0;
		private static int hibCount = 0;

		/// <summary>
		/// does a realm have the amount of keeps required for a certain bonus
		/// </summary>
		/// <param name="type">the type of bonus</param>
		/// <param name="realm">the realm</param>
		/// <returns>true if the realm has the required amount of keeps</returns>
		public static bool RealmHasBonus(eKeepBonusType type, eRealm realm)
		{
			if (!ServerProperties.Properties.USE_LIVE_KEEP_BONUSES)
				return false;

			if (realm == eRealm.None)
				return false;

			int count = 0;
			switch (realm)
			{
				case eRealm.Albion: count = albCount; break;
				case eRealm.Midgard: count = midCount; break;
				case eRealm.Hibernia: count = hibCount; break;
			}

			return count >= (int)type;
		}

		/// <summary>
		/// Update the counts of the keeps that we store locally,
		/// we do this for performance reasons
		/// </summary>
		public static void UpdateCounts()
		{
			albCount = KeepMgr.GetKeepCountByRealm(eRealm.Albion);
			midCount = KeepMgr.GetKeepCountByRealm(eRealm.Midgard);
			hibCount = KeepMgr.GetKeepCountByRealm(eRealm.Hibernia);
		}
	}
}