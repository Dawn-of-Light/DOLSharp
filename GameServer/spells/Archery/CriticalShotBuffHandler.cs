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

using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Archery Critical Shot Buff Reducing Crit shot and Power Shot Damage.
	/// </summary>
	[SpellHandlerAttribute("CriticalShotBuff")]
	public class CriticalShotBuffHandler : AttackModifierBuffSpellHandler
	{
		public CriticalShotBuffHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}
		
		/// <summary>
		/// Duration not modified by resist or bonuses
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}
		
		/// <summary>
		/// Check if attack is Archery attack Critical or Power shot
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnAttackedByEnemy(DOLEvent e, object sender, EventArgs args)
		{
			if(args is AttackedByEnemyEventArgs)
			{
				if (((AttackedByEnemyEventArgs)args).AttackData != null)
				{
					AttackData ad = ((AttackedByEnemyEventArgs)args).AttackData;
					
					if (ad.AttackResult == eAttackResult.HitUnstyled && !ad.IsSpellResisted && ad.SpellHandler is ArcheryHandler)
					{
						// Power Shot or Critical
						if (((ArcheryHandler)ad.SpellHandler).ShotType == ArcheryHandler.eShotType.Critical || ((ArcheryHandler)ad.SpellHandler).ShotType == ArcheryHandler.eShotType.Power)
						{
							GameSpellEffect effect = SpellHelper.FindEffectOnTarget(ad.Target, this);
							if (effect != null)
							{
								// if effect is over 5 sec, full effect halving damge
								if (effect.RemainingTime >= 5000)
								{
									ad.Damage >>= 1;
									ad.CriticalDamage >>= 1;
								}
								else
								{
									// if effect is under 5 sec slowly lowering resist from 50% to 100%
									double ratio = 1.0 - (0.5 * effect.RemainingTime / 5000);
									
									ad.Damage = (int)(ad.Damage*ratio);
									ad.CriticalDamage = (int)(ad.Damage*ratio);
								}
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Don't send client animation
		/// </summary>
		/// <param name="target"></param>
		/// <param name="clientEffect"></param>
		/// <param name="boltDuration"></param>
		/// <param name="noSound"></param>
		/// <param name="success"></param>
		public override void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
		{
		}
	}
}
