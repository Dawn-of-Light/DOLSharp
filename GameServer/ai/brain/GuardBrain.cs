/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
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
