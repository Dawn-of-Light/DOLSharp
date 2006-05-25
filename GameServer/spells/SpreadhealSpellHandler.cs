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
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("SpreadHeal")]
	public class SpreadhealSpellHandler : HealSpellHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int SPREADHEAL_RANGE = 2000;

		// constructor
		public SpreadhealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}


		/// <summary>
		/// Heals targets group
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public override bool HealTarget (GameLiving target, int amount)
		{
			Hashtable injuredTargets = new Hashtable();
			GameLiving mostInjuredLiving = target;
			double mostInjuredPercent = mostInjuredLiving.Health / (float)mostInjuredLiving.MaxHealth;

			int minHealVariance;
			int maxHealVariance;
			int targetHealCap;

			CalculateHealVariance(out minHealVariance, out maxHealVariance);

			if (minHealVariance >= maxHealVariance)
			{
				targetHealCap = maxHealVariance;
			}
			else
			{
				targetHealCap = Util.Random(minHealVariance, maxHealVariance);
			}

			int groupHealCap = targetHealCap;

			if (target is GamePlayer)
			{
				PlayerGroup group = ((GamePlayer)target).PlayerGroup;
				if (group != null) 
				{
					lock (group)
					{
						groupHealCap *= group.PlayerCount;
						targetHealCap *= 2;

						foreach (GamePlayer player in group)
						{
							if (!player.Alive) continue;
							//heal only if target is in range
							if (target.Position.CheckSquareDistance(player.Position, SPREADHEAL_RANGE*SPREADHEAL_RANGE))
							{
								double playerHealthPercent = player.Health / (double)player.MaxHealth;
								if (playerHealthPercent < 1)
								{
									injuredTargets.Add(player, playerHealthPercent);
									if (playerHealthPercent < mostInjuredPercent)
									{
										mostInjuredLiving = player;
										mostInjuredPercent = playerHealthPercent;
									}

									//DOLConsole.WriteLine("SpreadHeal: adding injured target " + player.Name + "; playerHealthPercent=" + playerHealthPercent);
								}
							}
						}
					}
				}
				else
				{
					// heal caster
					if (mostInjuredPercent < 1)
						injuredTargets.Add(target, mostInjuredPercent);
				}
			}
			else
			{
				//for non-players
				if (mostInjuredPercent < 1)
					injuredTargets.Add(target, mostInjuredPercent);
			}


			if (mostInjuredPercent >= 1)
			{
				//all are healed, 1/2 power
				SendEffectAnimation(target, 0, false, 0);
				MessageToCaster("Your group is already fully healed!", eChatType.CT_SpellResisted);
				return false;
			}

			double bestHealPercent = targetHealCap / (double)mostInjuredLiving.MaxHealth;
			double totalHealed = 0;
			Hashtable healAmount = new Hashtable();


			IDictionaryEnumerator iter = injuredTargets.GetEnumerator();
			//calculate heal for all targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				double targetHealthPercent = (double) iter.Value;
				//targets hp percent after heal is same as mostInjuredLiving
				double targetHealPercent = bestHealPercent + mostInjuredPercent - targetHealthPercent;
				int targetHeal = (int) (healTarget.MaxHealth * targetHealPercent);
				//DOLConsole.WriteLine("SpreadHeal: targetHealPercent=" + targetHealPercent + "; uncapped targetHeal=" + targetHeal + "; bestHealPercent=" + bestHealPercent + "; mostInjuredPercent=" + mostInjuredPercent + "; targetHealthPercent=" + targetHealthPercent);

				if (targetHeal > 0)
				{
					totalHealed += targetHeal;
					healAmount.Add(healTarget, targetHeal);
				}
			}

			iter = healAmount.GetEnumerator();
			//reduce healed hp according to targetHealCap and heal targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				double uncappedHeal = (int) iter.Value;
				int reducedHeal = (int) Math.Min(targetHealCap, uncappedHeal * (groupHealCap / totalHealed));

				//heal target
				base.HealTarget(healTarget, reducedHeal);

				if (log.IsDebugEnabled)
					log.Debug("SpreadHeal: " + target.GetName(0, false) + " heal for " + reducedHeal + "; HP percent after heal=" + target.Health / (float) target.MaxHealth);
			}
			if (log.IsDebugEnabled)
				log.Debug("SpreadHeal: totalHealed=" + totalHealed + "; targetHealCap=" + targetHealCap + "; groupHealCap=" + groupHealCap);

			return true;
		}
	}
}
