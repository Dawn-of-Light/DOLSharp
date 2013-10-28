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
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// ShadowShroud Ability NS RR5 RA
	/// </summary>
	/// <author>Stexx</author>
	public class ShadowShroudAbility : RR5RealmAbility
	{
		public const int DURATION = 30 * 1000;
		public const double ABSPERCENT = 10; // 10% damage absorb
		public const int MISSHITBONUS = 10; // 10% misshit bonus
		public const int EFFECT = 1565;

		public ShadowShroudAbility(DBAbility dba, int level) : base(dba, level) { }

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
				ShadowShroudEffect ShadowShroud = (ShadowShroudEffect)player.EffectList.GetOfType<ShadowShroudEffect>();
				if (ShadowShroud != null)
					ShadowShroud.Cancel(false);

				new ShadowShroudEffect().Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 300;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Reduce all incoming damage by 10% and increase the Nightshade’s chance to be missed by 10% for 30 seconds");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: Instant");
			list.Add("Re-use : 5 minutes");

		}

	}
}
