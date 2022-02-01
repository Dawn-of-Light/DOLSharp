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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS.spells
{
	/// <summary>
	/// Power Rend is a style effect unique to the Valkyrie's sword specialization line.
	/// </summary>
	[SpellHandlerAttribute("PowerRend")]
	public class PowerRendSpellHandler : SpellHandler
	{
		private Random m_rng = new Random();

		public PowerRendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

				
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active)
				return;

			SendEffectAnimation(target, m_spell.ClientEffect, boltDuration: 0, noSound: false, success: 1);
						
			var mesmerizeEffect = target.FindEffectOnTarget("Mesmerize");
			if (mesmerizeEffect != null)
				mesmerizeEffect.Cancel(false);

			var speedDecreaseEffect = target.FindEffectOnTarget("SpeedDecrease");
			if (speedDecreaseEffect != null)
				speedDecreaseEffect.Cancel(false);

						
			bool targetIsGameplayer = target is GamePlayer;
			var necroPet = target as NecromancerPet;
			
			if (targetIsGameplayer || necroPet != null)
			{
				int powerRendValue;

				if (targetIsGameplayer)
				{
					powerRendValue = (int)(target.MaxMana * Spell.Value * GetVariance());
					if (powerRendValue > target.Mana)
						powerRendValue = target.Mana;
					target.Mana -= powerRendValue;
					target.MessageToSelf(string.Format(m_spell.Message2, powerRendValue), eChatType.CT_Spell);
				}
				else
				{
					powerRendValue = (int)(necroPet.Owner.MaxMana * Spell.Value * GetVariance());
					if (powerRendValue > necroPet.Owner.Mana)
						powerRendValue = necroPet.Owner.Mana;
					necroPet.Owner.Mana -= powerRendValue;
					necroPet.Owner.MessageToSelf(string.Format(m_spell.Message2, powerRendValue), eChatType.CT_Spell);
				}

				MessageToCaster(string.Format(m_spell.Message1, powerRendValue), eChatType.CT_Spell);
			}
		}
		
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

		public override int CalculateSpellResistChance(GameLiving target) => 100 - CalculateToHitChance(target);
				
		protected override void OnSpellResisted(GameLiving target) => base.OnSpellResisted(target);

		private double GetVariance()
		{
			int intRandom = m_rng.Next(0, 37);
			double factor = 1 + (double)intRandom / 100;
			return factor;
		}

        public override string ShortDescription 
			=> $"Target loses {Spell.Value}% power.";
    }
}
