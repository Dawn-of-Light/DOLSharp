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

/*
 * https://github.com/Xanxicar/DOLSharp 2020-01-13
 * 
 * Moved class PowerRendSpellHandler out of Convoker.cs where it is being abused to poorly simulate a power trap spell.
 * Added PowerRendSpellHandler.cs as a separate code file declaring it a Valkyrie only melee style effect.
 * Changed logic to reflect Pendragon test results.
 * Changed messages to Caster and targets.
 * Improved precision of the amount of power drained.
 * Added random variance.
 * Added spell casting interruption.
 * Added spell resist chance.
 * Added breaking of mesmerization, root and snare.
 * 
 * 
 * Testing showed that none of the Valkyrie's spec lines had any impact on the amount of power subtracted.
 * Neither did any stats or the choice of weapon.
 * A Naked Valkyrie attacking with a 1-handed level 1 training sword subtracts the same amount of power
 * as when fully equipped attacking with a 2-handed level 51 sword.
 * 
 * The percental amount of power subtracted refers to the entire characters maximum power pool including realm abilities and all applicable item bonuses.
 * The minimum amount subtracted corresponds to the percentage of power that the delve info states.
 * That is 3% of MaxMana for Aurora and 6% of MaxMana for Odin's Bite.
 * The amount drained is however subject to a random upwards variance that changes behavior slightly depending on the target affected.
 * 
 * Neither is the absolute variance consistant nor is the relative deviation.
 * We observed that the higher the targets power pool is the smaller the relative deviation gets.
 * Measured deviation from power pool percentage of 5 different classes ranges from +37% to +47%.
 * The exact business logic behind this remains elusive.
 * Therefore we decided to go with an arbitrary constant maximum upwards deviation on the low end of the deviation range.
 */

using System;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS.spells
{
	/// <summary>
	/// Power Rend is a style effect unique to the Valkyrie's sword specialization line.
	/// It subtracts from targets within the area of effect a percentage of their power pool (plus a bit of random variance on top).
	/// The power is NOT returned or added to the Valkyrie.
	/// 
	/// Even if the following is atypical behavior for style effects:
	/// It DOES break CC.
	/// It DOES interrupt spell casting.
	/// It CAN be resisted.
	/// It is NOT subject to spell effectiveness.
	/// It is NOT subject to effectiveness falloff either.
	/// </summary>
	[SpellHandlerAttribute("PowerRend")]
	public class PowerRendSpellHandler : SpellHandler
	{
		private Random m_rng = new Random();
		public PowerRendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		/// <summary>
		/// Method applies PowerRend to targets and is called once for every target within the AoE.
		/// Only players and necro pets are concerned with having their power drained.
		/// All mobs, NPCs and pets are considered to always have full power anyways,
		/// hence we apply only interrupts, CC breaking and animations to them.
		/// </summary>
		/// <param name="target">A single GameLiving from a collection of targets within the area of effect.</param>
		/// <param name="effectiveness">Is not used in this context. Inherited from the parent class SpellHandler.</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active)
				return;

			SendEffectAnimation(target, m_spell.ClientEffect, boltDuration: 0, noSound: false, success: 1);

			// Break Mezz
			var mesmerizeEffect = target.FindEffectOnTarget("Mesmerize");
			if (mesmerizeEffect != null)
				mesmerizeEffect.Cancel(false);

			// Break Root & Snare
			var speedDecreaseEffect = target.FindEffectOnTarget("SpeedDecrease");
			if (speedDecreaseEffect != null)
				speedDecreaseEffect.Cancel(false);

			// Apply power rend only to players
			#region Players only
			int powerRendValue;
			bool targetIsGameplayer = target is GamePlayer;
			var necroPet = target as NecromancerPet;
			
			if (targetIsGameplayer || necroPet != null)
			{
				if (targetIsGameplayer)
				{
					// player
					powerRendValue = (int)((target.MaxMana * Spell.Value) * GetRandomMultiplier());
					if (powerRendValue > target.Mana)
						powerRendValue = target.Mana;
					target.Mana -= powerRendValue;
					target.MessageToSelf(string.Format(m_spell.Message2, powerRendValue), eChatType.CT_Spell);
				}
				else
				{
					// necro pet
					powerRendValue = (int)((necroPet.Owner.MaxMana * Spell.Value) * GetRandomMultiplier());
					if (powerRendValue > necroPet.Owner.Mana)
						powerRendValue = necroPet.Owner.Mana;
					necroPet.Owner.Mana -= powerRendValue;
					necroPet.Owner.MessageToSelf(string.Format(m_spell.Message2, powerRendValue), eChatType.CT_Spell);
				}
				// Caster is the Valkyrie
				MessageToCaster(string.Format(m_spell.Message1, powerRendValue), eChatType.CT_Spell);
			}
			#endregion
		}

		/// <summary>
		/// Extends base class behavior to implement interrupt timers.
		/// </summary>
		/// <param name="target">The GameLiving affected by the Power Rend style effect.</param>
		/// <param name="effectiveness">Does not apply here.</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target == null || target.CurrentRegion == null)
				return;

			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}

			base.ApplyEffectOnTarget(target, effectiveness);

			target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);

			if (target is GameNPC)
			{
				IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
		}

		/// <summary>
		/// Overriding is necessary as the parent class makes style effects always hit otherwise.
		/// </summary>
		/// <param name="target">A single GameLiving from a collection of targets within the area of effect.</param>
		/// <returns>Integer representing the chance of a spell resist to occur. 100 corresponds to a 100% chance.</returns>
		public override int CalculateSpellResistChance(GameLiving target) => 100 - CalculateToHitChance(target);

		/// <summary>
		/// Overriding is necessary for spell resisting to occur.
		/// </summary>
		/// <param name="target">A single GameLiving from a collection of targets within the area of effect.</param>
		protected override void OnSpellResisted(GameLiving target) => base.OnSpellResisted(target);

		/// <summary>
		/// Represents the arbitrary random variation of the PowerRend style effect.
		/// </summary>
		/// <returns>A random factor by which the power rend value is to be multiplied.</returns>
		private double GetRandomMultiplier()
		{
			int intRandom = m_rng.Next(0, 31);
			double factor = 1 + (double)intRandom / 100;
			return factor;
		}
	}
}
