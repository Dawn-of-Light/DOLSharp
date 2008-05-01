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
using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class MarkOfPreyAbility : RR5RealmAbility
	{
		public MarkOfPreyAbility(DBAbility dba, int level) : base(dba, level) { }

        int RANGE = 2000;
        public const int DURATION = 30 * 1000;
        public const int VALUE = 10;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player == null) return;

			ArrayList targets = new ArrayList();
			if (player.Group == null)
				targets.Add(player);
			else
				foreach (GamePlayer grpMate in player.Group.GetPlayersInTheGroup())
					if (WorldMgr.CheckDistance(grpMate, player, RANGE) && grpMate.IsAlive)
						targets.Add(grpMate);

			foreach (GamePlayer target in targets)
			{
                MarkofPreyEffect MarkOfPrey = (MarkofPreyEffect)target.EffectList.GetOfType(typeof(MarkofPreyEffect));
                if (MarkOfPrey != null)
                    MarkOfPrey.Cancel(false);

                new MarkofPreyEffect().Start(player,target);
			}

			DisableSkill(living);
        }
		
		public override int GetReUseDelay(int level)
		{
			return 600;
		}

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("Grants all members of the Vampiir's group a 30 second damage add that stacks with all other forms of damage add. All damage done via the damage add will be returned to the Vampiir as power.");
            list.Add("");
            list.Add("Target: Group");
            list.Add("Duration: 30s");
            list.Add("Casting time: Instant");
        }
    }
}
