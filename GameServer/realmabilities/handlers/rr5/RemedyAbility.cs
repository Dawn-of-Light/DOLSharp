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
using DOL.GS.Effects;
using log4net;
using DOL.GS.Keeps;
using DOL.GS.Spells;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Remedy Realm Ability
	/// </summary>
    [SkillHandlerAttribute(Abilities.Remedy)]
	public class RemedyAbility : IAbilityActionHandler
	{
		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
        public void Execute(Ability ab, GamePlayer player)
		{
            if (player.IsAlive || player.IsSitting || player.IsMezzed || player.IsStunned)
                return;

			if (player != null)
			{
                player.Out.SendSpellEffectAnimation(player, player, 7060, 0, false, 1);
                //SendCasterSpellEffectAndCastMessage(player, 7060, true);
				RemedyEffect effect = new RemedyEffect();
				effect.Start(player);

                player.Out.SendDisableSkill(ab, 300);
			}
		}

        /*
        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("Gives you immunity to weapon poisons for 1 minute. This spell wont purge already received poisons!");
            list.Add("This spell costs 10% of your HP. These will be regained by the end of the effect.");
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 60 sec");
            list.Add("Casting time: instant");
        }
         */ 
	}
}