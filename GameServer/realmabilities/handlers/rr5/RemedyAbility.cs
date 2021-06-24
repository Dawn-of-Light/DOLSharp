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
 * [StephenxPimentel]
 * Updated to be a skillhandler to match 1.108.
 */
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Effects;
using log4net;
using DOL.GS.Keeps;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Remedy Realm Ability
	/// </summary>
    public class RemedyAbility : RealmAbility
	{
        public RemedyAbility(DBAbility dba, int level) : base(dba, level)
        {

        }


		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
        public override void Execute(GameLiving living)
		{
            GamePlayer player = living as GamePlayer;

            if (player == null || !player.IsAlive || player.IsSitting || player.IsMezzed || player.IsStunned)
            {
                return;
            }

            player.Out.SendSpellEffectAnimation(player, player, 7060, 0, false, 1);

            // SendCasterSpellEffectAndCastMessage(player, 7060, true);
            RemedyEffect effect = new RemedyEffect();
            effect.Start(player);

            player.DisableSkill(this, 300 * 1000);
		}
	}
}