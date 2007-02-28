using DOL.AI.Brain;
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
			//You examine the Guardian Sergeant.  He is neutral towards you and is a guard.
			IList list = new ArrayList(4);
			list.Add("You examine the " + GetName(0, true) + ". " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a guard.");
			return list;
		}
	}
}