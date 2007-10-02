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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Amnesia")]
	public class AmnesiaSpellHandler : SpellHandler
	{
		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if (target == null || !target.IsAlive)
				return;

			//have to do it here because OnAttackedByEnemy is not called to not get aggro
			if (target.Realm == 0 || Caster.Realm == 0)
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
			else target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
			SendEffectAnimation(target, 0, false, 1);

			if (target is GamePlayer)
			{
				((GamePlayer)target).NextCombatStyle = null;
				((GamePlayer)target).NextCombatBackupStyle = null;
			}
			target.StopCurrentSpellcast(); //stop even if MoC or QC
			MessageToLiving (target, "Your mind goes blank and you forget what you were doing!", eChatType.CT_Spell);

            GameSpellEffect effect;
            effect = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (effect != null)
            {
                effect.Cancel(false);
                return;
            }

			if (target is GameNPC)
			{
				GameNPC npc = (GameNPC)target;
				IAggressiveBrain aggroBrain = npc.Brain as IAggressiveBrain;
				if (aggroBrain != null)
				{
					if (Util.Chance(Spell.AmnesiaChance))
						aggroBrain.ClearAggroList();
				}
			}
		}

		/// <summary>
		/// When spell was resisted
		/// </summary>
		/// <param name="target">the target that resisted the spell</param>
		protected override void OnSpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
			if (Spell.CastTime == 0)
			{
				// start interrupt even for resisted instant amnesia
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			}
		}

		// constructor
		public AmnesiaSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
