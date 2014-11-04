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
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Based on HealSpellHandler.cs
	/// Spell calculates a percentage of the caster's health.
	/// Heals target for the full amount, Caster loses half that amount in health.
	/// </summary>
	[SpellHandlerAttribute("LifeTransfer")]
	public class LifeTransferSpellHandler : SpellHandler
	{
		// constructor
		public LifeTransferSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		/// <summary>
		/// Execute lifetransfer spell
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			IList targets = SelectTargets(target);
			if (targets.Count <= 0) return false;

			bool healed = false;
			int transferHeal;
			double spellValue = m_spell.Value;

			transferHeal = (int)(Caster.MaxHealth / 100 * Math.Abs(spellValue));

			//Needed to prevent divide by zero error
			if (transferHeal <= 0)
				transferHeal = 0; 
			else
			{
				//Remaining health is used if caster does not have enough health, leaving caster at 1 hitpoint
				if ( (transferHeal >> 1) >= Caster.Health )
					transferHeal = ( (Caster.Health - 1) << 1);
			}



			foreach(GameLiving healTarget in targets)
			{
				if (target.IsDiseased)
				{
					MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
					healed |= HealTarget(healTarget, ( transferHeal >>= 1 ));	
				}

				else healed |= HealTarget(healTarget, transferHeal);
			}

			if (!healed && Spell.Target == "Realm")
			{
				m_caster.Mana -= PowerCost(target) >> 1;	// only 1/2 power if no heal
			}
			else
			{
				m_caster.Mana -= PowerCost(target);
				m_caster.Health -= transferHeal >> 1;
			}

			// send animation for non pulsing spells only
			if (Spell.Pulse == 0)
			{
				if (healed)
				{
					// send animation on all targets if healed
					foreach(GameLiving healTarget in targets)
						SendEffectAnimation(healTarget, 0, false, 1);
				}
				else
				{
					// show resisted effect if not healed
					SendEffectAnimation(Caster, 0, false, 0);
				}
			}

			return true;
		}

		/// <summary>
		/// Heals hit points of one target and sends needed messages, no spell effects
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public virtual bool HealTarget(GameLiving target, int amount)
		{
			if (target==null || target.ObjectState!=GameLiving.eObjectState.Active) return false;

			// we can't heal people we can attack
			if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
				return false;

			if (!target.IsAlive) 
			{
				MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster == target)
			{
				MessageToCaster("You cannot transfer life to yourself.", eChatType.CT_SpellResisted);
				return false;
			}
			
			if (amount <= 0) //Player does not have enough health to transfer
			{
				MessageToCaster("You do not have enough health to transfer.", eChatType.CT_SpellResisted);
				return false;  
			}


			int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, amount);

            #region PVP DAMAGE

            long healedrp = 0;

            if (m_caster is NecromancerPet &&
                ((m_caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null
                || m_caster is GamePlayer)
            {

                if (target is NecromancerPet && ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                {
                    if (target.DamageRvRMemory > 0)
                    {
                        healedrp = (long)Math.Max(heal, 0);
                        target.DamageRvRMemory -= healedrp;
                    }
                }
            }

            if (healedrp > 0 && m_caster != target && m_spellLine.KeyName != GlobalSpellsLines.Item_Spells &&
                m_caster.CurrentRegionID != 242 && m_spell.Pulse == 0) // On Exclu zone COOP
            {
                GamePlayer joueur_a_considerer = (m_caster is NecromancerPet ? ((m_caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() : m_caster as GamePlayer);

                int POURCENTAGE_SOIN_RP = ServerProperties.Properties.HEAL_PVP_DAMAGE_VALUE_RP; // ...% de bonus RP pour les soins effectués
                long Bonus_RP_Soin = Convert.ToInt64((double)healedrp * POURCENTAGE_SOIN_RP / 100);

                if (Bonus_RP_Soin >= 1)
                {
                    PlayerStatistics stats = joueur_a_considerer.Statistics as PlayerStatistics;

                    if (stats != null)
                    {
                        stats.RPEarnedFromHitPointsHealed += (uint)Bonus_RP_Soin;
                        stats.HitPointsHealed += (uint)healedrp;
                    }

                    joueur_a_considerer.GainRealmPoints(Bonus_RP_Soin, false);
                    joueur_a_considerer.Out.SendMessage("Vous gagnez " + Bonus_RP_Soin.ToString() + " points de royaume pour avoir soigné un membre de votre royaume.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }

            #endregion PVP DAMAGE

			if (heal == 0) 
			{
				if (Spell.Pulse == 0)
				{
					MessageToCaster(target.GetName(0, true)+" is fully healed.", eChatType.CT_SpellResisted);
				}

				return false;
			}

			
			MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " hit points!", eChatType.CT_Spell);
			MessageToLiving(target, "You are healed by " + m_caster.GetName(0, false) + " for " + heal + " hit points.", eChatType.CT_Spell);
			if(heal < amount)
					MessageToCaster(target.GetName(0, true)+" is fully healed.", eChatType.CT_Spell);

			return true;
		}
	}
}
