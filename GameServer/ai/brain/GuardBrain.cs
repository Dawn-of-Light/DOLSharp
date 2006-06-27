using DOL.GS;

namespace DOL.AI.Brain
{
	public class GuardBrain : StandardMobBrain
	{
		public GuardBrain()
			: base()
		{
			ThinkInterval = 3000;
		}

		protected override void CheckPlayerAggro()
		{
			//Check if we are already attacking, return if yes
			if (Body.AttackState)
				return;
			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort) AggroRange))
			{
				if (m_aggroTable.ContainsKey(player))
					continue; // add only new players
				if (!player.Alive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
					continue;
				if (player.Steed != null)
					continue; //do not attack players on steed
				if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
					continue;

				AddToAggroList(player, player.EffectiveLevel<<1);
			}
		}

		protected override void CheckNPCAggro()
		{
			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
				{
					switch (Body.Realm)
					{
						case 1: Body.Say("Have at thee, fiend!"); break;
						case 2: Body.Say("Death to the intruders!"); break;
						case 3: Body.Say("The wicked shall be scourned!"); break;
					}
					AddToAggroList(npc, npc.Level<<1);
					return;
				}
			}
		}
	}
}
