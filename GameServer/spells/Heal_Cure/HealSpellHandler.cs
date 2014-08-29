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
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    using Effects;

    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("Heal")]
    public class HealSpellHandler : SpellHandler
    {
    	/// <summary>
    	/// Did Heal succeed in Healing Target(s) ?
    	/// </summary>
    	private bool m_healSucceeded = false;
    	
    	/// <summary>
    	/// Did Heal succeed in Healing Target(s) ?
    	/// </summary>
		public virtual bool HealSucceeded {
			get { return m_healSucceeded; }
		}
    	
        // constructor
        public HealSpellHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// If it's a necro in shade form or other living with similar shade effect
			if(Spell.Target.ToLower() != "group" && target.ControlledBrain is CasterNotifiedPetBrain && target.ControlledBrain.Body != null && target.EffectList.GetOfType<NecromancerShadeEffect>() != null)
			{
				base.ApplyEffectOnTarget(target.ControlledBrain.Body, effectiveness);
			}
			else
			{
				base.ApplyEffectOnTarget(target, effectiveness);
			}
		}
        
        /// <summary>
		/// Execute heal spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_healSucceeded = false;
			
			base.FinishSpellCast(target);
			
			if(HealSucceeded)
	    	{
	    		Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
	    	}
	    	else
	    	{
	    		Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true) >> 1);
	    		m_startReuseTimer = false;
	    	}
		}

        /// <summary>
        /// Direct Heal Effect.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			
			if (target == null || target.ObjectState != GameLiving.eObjectState.Active) 
            	return;

			// we can't heal people we can attack
			if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
				return;

			// no healing of keep components
			if (target is Keeps.GameKeepComponent || target is Keeps.GameKeepDoor)
				return;
			
			// Pulsing Chant/HoT should not trigger messages
			if (!target.IsAlive && !Spell.IsPulsing && !Spell.IsPulsingEffect)
            {
                //"You cannot heal the dead!" sshot550.tga
                MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
                return;
            }
			
			// Pulsing Chant/HoT should not trigger messages
            if (!Spell.IsPulsing && !Spell.IsPulsingEffect && target != Caster && target is GamePlayer && Caster is GamePlayer && (target as GamePlayer).NoHelp)
            {
                //player not grouped or player grouped in different group
                if (target.Group == null || Caster.Group != target.Group)
                {
                    MessageToCaster("That player does not want assistance", eChatType.CT_SpellResisted);
                    return;
                }
            }	
			
			int minHeal;
            int maxHeal;
            
			CalculateHealVariance(out minHeal, out maxHeal, target, effectiveness, Spell.Value);
			
			int healValue = Util.Random(minHeal, maxHeal);
			
			// Pulsing HoT should not trigger messages neither disease effect
			if (!Spell.IsPulsingEffect && target.IsDiseased)
			{
				if(!Spell.IsPulsing)
					MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
				
				healValue >>= 1;
			}
			
			int healed = HealTarget(target, healValue);
			
			// send animation for non pulsing spells only
            if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
            {
                // show resisted effect if not healed
				if(target.IsAlive)
					SendEffectAnimation(target, 0, false, healed > 0 ? (byte)1 : (byte)0);
            }
			
            if(healed > 0)
            	m_healSucceeded = true;
		}
        
        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>amount of healed hit points</returns>
        public virtual int HealTarget(GameLiving target, int amount)
        {
            // Healing Effectiveness Bonus
            amount = (int)(amount * (1 + Caster.GetModified(eProperty.HealingEffectiveness) * 0.01));
            
            // Add Critical heal Value
            int criticalvalue = 0;
            if (Util.Chance(Caster.GetModified(eProperty.CriticalHealHitChance)))
            	criticalvalue = Util.Random((int)(amount * 0.25), (int)(amount * 0.5) + 1);
            
            // Healing Critical Value
            amount += criticalvalue;

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, amount);

            // Rp Earning for Heals. (Pulsing and Spread don't earn RP)
            if(!Spell.IsPulsing && !Spell.IsPulsingEffect && Spell.Target.ToLower() != "self" && target != Caster)
            {
            	int healRPBonus = (int)(Math.Min(heal, target.DamageRvRMemory) * ServerProperties.Properties.HEAL_PVP_DAMAGE_VALUE_RP * 0.01);
				
            	GamePlayer player = null;
            	
            	if(Caster is GameNPC && ((GameNPC)Caster).Brain is IControlledBrain)
            	{
            		player = ((IControlledBrain)((GameNPC)Caster).Brain).GetPlayerOwner();
            	}
            	else if(Caster is GamePlayer)
            	{
            		player = (GamePlayer)Caster;
            	}
            	
            	if (player != null && healRPBonus >= 1)
				{
					PlayerStatistics stats = player.Statistics as PlayerStatistics;
				
					if (stats != null)
					{
					    stats.RPEarnedFromHitPointsHealed += (uint)healRPBonus;
					    stats.HitPointsHealed += (uint)heal;
					}
				
					player.GainRealmPoints(healRPBonus, false, false);
					player.Out.SendMessage("You earn " + healRPBonus.ToString() + " realm point" + (healRPBonus > 1 ? "s" : "") + " for healing a member of your realm.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
            }
            
            
            // Messages for heal
            if (heal == 0)
            {
            	// Don't send them for pulsing spells.
                if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
                {
                    if (target == Caster) 
                        MessageToCaster("You are fully healed.", eChatType.CT_SpellResisted);
                    else 
                        MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_SpellResisted);
                }
                
                return 0;
            }
            else
            {
				if (Caster == target)
				{
					if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
						MessageToCaster("You heal yourself for " + heal + " hit points.", eChatType.CT_Spell);
					
					if (heal < amount)
					{
						if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
							MessageToCaster("You are fully healed.", eChatType.CT_Spell);
					}
				}
				else
				{
					if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
					{
						MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " hit points!", eChatType.CT_Spell);
						MessageToLiving(target, "You are healed by " + m_caster.GetName(0, false) + " for " + heal + " hit points.", eChatType.CT_Spell);
					}
					
					if (heal < amount)
					{
						if (!Spell.IsPulsing && !Spell.IsPulsingEffect)
							MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_Spell);
					}

				}
				
				if(heal < amount)
				{
					// reset RvR Damage
					target.DamageRvRMemory = 0;					
				}
				else
				{
					// Reduce RvR Memory
					target.DamageRvRMemory -= Math.Min(heal, target.DamageRvRMemory);
				}
				
				// Display critical heals for non pulsing
				if (heal > 0 && criticalvalue > 0 && !Spell.IsPulsing && !Spell.IsPulsingEffect)
					MessageToCaster("Your heal criticals for an extra " + criticalvalue + " amount of hit points!", eChatType.CT_Spell);
            }
 
            return heal;
        }


        /// <summary>
        /// Calculates heal variance based on spec
        /// </summary>
        /// <param name="min">store min variance here</param>
        /// <param name="max">store max variance here</param>
        public virtual void CalculateHealVariance(out int min, out int max, GameLiving target, double effectiveness, double spellValue)
        {
			double minEffectiveness = 0.75;
			double maxEffectiveness = 1.25;
			
			if(spellValue < 0)
			{
				min = max = Math.Min(1, (int)(target.MaxHealth * spellValue * -0.01));
			}
			else
			{
				if (SpellLine.KeyName == GlobalSpellsLines.Potions_Effects)
				{
					// Potions have 1.0 min effectiveness
					minEffectiveness = 1.0;
				}				
				else if ((SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect && UseMinVariance) || SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
				{
					// Style Healing Effect with UseMinVariance are capped to max heal effect.
					minEffectiveness = 1.25;
				}
				else if (SpellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
				{
					// Reserved spell have fixed effect
					minEffectiveness = maxEffectiveness = 1.0;
				}
				else if(Caster is GamePlayer)
				{
					// Spec Line heal spell fixed to 1.25
					if (!SpellLine.IsBaseLine)
					{
						minEffectiveness = maxEffectiveness = 1.25;
					}
					else
					{
						minEffectiveness = Math.Min(1.0, (Math.Max(1.0, Caster.GetModifiedSpecLevel(SpellLine.Spec)) - 1.0) / Math.Max(1.0, Spell.Level)) + 0.25;
					}
				}
				
				min = Math.Max(1, (int)(minEffectiveness * spellValue * effectiveness));
				max = Math.Max(1, (int)(maxEffectiveness * spellValue * effectiveness));
			}
        }
    }
}
