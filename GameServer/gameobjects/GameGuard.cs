using DOL.AI.Brain;
using DOL.Language;
using System.Collections;

namespace DOL.GS
{
	public class GameGuard : GameNPC
	{
		public GameGuard()
			: base()
		{
			m_ownBrain = new GuardBrain();
			m_ownBrain.Body = this;
		}

		public override void DropLoot(GameObject killer)
		{
			//Guards dont drop loot when they die
		}

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(4);
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameGuard.GetExamineMessages.Examine", GetName(0, true), GetPronoun(0, true), GetAggroLevelString(player, false)));
			return list;
		}

		public override void StartMeleeAttack(GameObject attackTarget)
		{
			base.StartMeleeAttack(attackTarget);

			switch (Realm)
			{
				case eRealm.Albion: Say("Have at thee, fiend!"); break;
				case eRealm.Midgard: Say("Death to the intruders!"); break;
				case eRealm.Hibernia: Say("The wicked shall be scourned!"); break;
			}
		}
	}
}