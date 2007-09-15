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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Confusion")]
	public class ConfusionSpellHandler : SpellHandler
	{
		public ConfusionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{}

		ArrayList targetList = new ArrayList();

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			if (effect.Owner == null) return;
			if (!effect.Owner.IsAlive || effect.Owner.ObjectState != GameLiving.eObjectState.Active) return;

			//SendEffectAnimation(effect.Owner, 0, false, 1); //send the effect

			if (effect.Owner is GamePlayer)
			{
				/*
				 *Q: What does the confusion spell do against players?
				 *A: According to the magic man, “Confusion against a player interrupts their current action, whether it's a bow shot or spellcast.
				 */
				if (Spell.Value < 0 || Util.Chance(Convert.ToInt32(Math.Abs(Spell.Value))))
				{
					//Spell value below 0 means it's 100% chance to confuse.
					GamePlayer player = effect.Owner as GamePlayer;

					player.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
				}

				effect.Cancel(false);
			}
			else if (effect.Owner is GameNPC)
			{
				//check if we should do anything at all.

				bool doConfuse = (Spell.Value < 0 || Util.Chance(Convert.ToInt32(Spell.Value)));

				if (!doConfuse)
					return;

				bool doAttackFriend = Spell.Value < 0 && Util.Chance(Convert.ToInt32(Math.Abs(Spell.Value)));
				
				GameNPC npc = effect.Owner as GameNPC;

				npc.IsConfused = true;

				if (log.IsDebugEnabled)
					log.Debug("CONFUSION: " + npc.Name + " was confused(true," + doAttackFriend.ToString() +")");

				if (npc.Brain is ControlledNpc)
				{
					//it's a pet.
					GamePlayer playerowner = ((ControlledNpc)npc.Brain).GetPlayerOwner();
					if (playerowner != null && playerowner.CharacterClass.ID == (int)eCharacterClass.Theurgist)
					{
						//Theurgist pets die.
						npc.Die(Caster);
						effect.Cancel(false);
						return;
					}
				}

				targetList.Clear();
				foreach (GamePlayer target in npc.GetPlayersInRadius(1000))
				{
					if (doAttackFriend)
						targetList.Add(target);
					else
					{
						//this should prevent mobs from attacking friends.
						if (GameServer.ServerRules.IsAllowedToAttack(npc, target, true))
							targetList.Add(target);
					}
				}

				foreach (GameNPC target in npc.GetNPCsInRadius(1000))
				{
					//don't agro yourself.
					if (target == npc)
						continue;

					if (doAttackFriend)
						targetList.Add(target);
					else
					{
						//this should prevent mobs from attacking friends.
						if (GameServer.ServerRules.IsAllowedToAttack(npc, target, true) && !GameServer.ServerRules.IsSameRealm(npc,target,true))
							targetList.Add(target);
					}
				}

				//targetlist should be full, start effect pulse.
				if (targetList.Count > 0)
				{
					npc.StopAttack();
					npc.StopCurrentSpellcast();

					GameLiving target = targetList[Util.Random(targetList.Count - 1)] as GameLiving;
					npc.StartAttack(target);
				}
			}
		}

		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);

			if (targetList.Count > 0)
			{
				GameNPC npc = effect.Owner as GameNPC;
				npc.StopAttack();
				npc.StopCurrentSpellcast();

				GameLiving target = targetList[Util.Random(targetList.Count - 1)] as GameLiving;

				npc.StartAttack(target);
			}
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			//every 5 seconds?
			return new GameSpellEffect(this, m_spell.Duration, 5000, 1);
		}

		public override bool HasPositiveEffect
		{
			get
			{
				return false;
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if(effect != null && effect.Owner != null && effect.Owner is GameNPC)
			{
				GameNPC npc = effect.Owner as GameNPC;
				npc.IsConfused = false;
			}
			return base.OnEffectExpires(effect, noMessages);
		}
	}
}
