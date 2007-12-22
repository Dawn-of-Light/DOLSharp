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

using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Gift of Perizor RA
    /// </summary>
    public class GiftOfPerizorAbility : RR5RealmAbility
    {
        public const int DURATION = 60 * 1000;
        private const int SpellRadius = 1500;

        public GiftOfPerizorAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				ArrayList targets = new ArrayList();
				if (player.Group == null)
					targets.Add(player);
				else
				{
					foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
					{
						if (WorldMgr.CheckDistance(p, player, SpellRadius) && p.IsAlive)
							targets.Add(p);
					}
				}
				foreach (GamePlayer target in targets)
				{
					//send spelleffect
					if (!target.IsAlive) continue;
					GiftOfPerizorEffect GiftOfPerizor = (GiftOfPerizorEffect)target.EffectList.GetOfType(typeof(GiftOfPerizorEffect));
					if (GiftOfPerizor != null) GiftOfPerizor.Cancel(false);
					target.TempProperties.setProperty("GiftOfPerizorOwner", player);
					new GiftOfPerizorEffect().Start(target);
				}
			}
			DisableSkill(living);
		}

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(System.Collections.IList list)
        {
            list.Add("Buff group with 25% damage reduction for 60 seconds, return damage reduced as power. 10min RUT.");
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 1 min");
            list.Add("Casting time: Instant");
        }

    }
}


