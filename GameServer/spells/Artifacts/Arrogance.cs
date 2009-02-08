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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells.Atlantis
{
    /// <summary>
    /// Arrogance spell handler
    /// </summary>
    [SpellHandlerAttribute("Arrogance")]
    public class Arrogance : SpellHandler
    {
    	GamePlayer playertarget = null;
    	
        /// <summary>
        /// The timer that will cancel the effect
        /// </summary>
        protected RegionTimer m_expireTimer;
        public override void OnEffectStart(GameSpellEffect effect)
        {
        	base.OnEffectStart(effect);
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Strength] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Piety] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Charisma] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorbtion] += (int)m_spell.Value;                       
            
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();       
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Strength] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Piety] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorbtion] -= (int)m_spell.Value;
             
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();  
                Start(player);
            }
            return base.OnEffectExpires(effect,noMessages);
        }

        protected virtual void Start(GamePlayer player)
        {
        	playertarget = player;
            StartTimers();
            player.DebuffCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Strength] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Piety] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Charisma] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.ArmorAbsorbtion] += (int)m_spell.Value;
            
            player.Out.SendCharStatsUpdate();
            player.UpdateEncumberance();
            player.UpdatePlayerStatus();
          	player.Out.SendUpdatePlayer(); 
        }

        protected virtual void Stop()
        {
            if (playertarget != null)
            {     
	            playertarget.DebuffCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Strength] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Piety] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.ArmorAbsorbtion] -= (int)m_spell.Value;;
	            
            	playertarget.Out.SendCharStatsUpdate();
            	playertarget.UpdateEncumberance();
            	playertarget.UpdatePlayerStatus();
          		playertarget.Out.SendUpdatePlayer(); 
            }
            StopTimers();
        }
        protected virtual void StartTimers()
        {
            StopTimers();
            m_expireTimer = new RegionTimer(playertarget, new RegionTimerCallback(ExpiredCallback), 10000);
        }
        protected virtual void StopTimers()
        {
            if (m_expireTimer != null)
            {
                m_expireTimer.Stop();
                m_expireTimer = null;
            }
        }
        protected virtual int ExpiredCallback(RegionTimer callingTimer)
        {
            Stop();
            return 0;
        }

        public Arrogance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
