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
using DOL.Events;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Dream Sphere self morph spell handler
    /// The DoT proc is a subspell, affects only caster
    /// </summary>
    [SpellHandlerAttribute("DreamMorph")]
    public class DreamMorph : Morph
    {
    	private GameSpellEffect m_effect = null;
        public override void OnEffectStart(GameSpellEffect effect)
        {
         	m_effect = effect;    
        	base.OnEffectStart(effect);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.TakeDamage, new DOLEventHandler(LivingTakeDamage));       	       
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.TakeDamage, new DOLEventHandler(LivingTakeDamage));            	                         
            return base.OnEffectExpires(effect, noMessages);
        }
        // Event : player takes damage, effect cancels
        public void LivingTakeDamage(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;
            if (player == null) return;
            if (e == GamePlayerEvent.TakeDamage)
            {
            	OnEffectExpires(m_effect, true);
            	return;
            }
        }     
        public DreamMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    /// <summary>
    /// Dream Sphere group morph spell handler
    /// The DoT proc is a subspell, affects only caster
    /// </summary>
    [SpellHandlerAttribute("DreamGroupMorph")]
    public class DreamGroupMorph : DreamMorph
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {  
        	// Same Effect for everyone except caster that get a ToHit bonus
            Caster.BaseBuffBonusCategory[(int)eProperty.ToHitBonus] += (int)m_spell.Value;
            base.OnEffectStart(effect);
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.BaseBuffBonusCategory[(int)eProperty.ToHitBonus] -= (int)m_spell.Value;
            return base.OnEffectExpires(effect, noMessages);
        }
        public DreamGroupMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
