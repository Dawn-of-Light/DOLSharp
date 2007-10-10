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
    /// Alvarus spell handler
    /// Water breathing is a subspell
    /// </summary>
    [SpellHandlerAttribute("AlvarusMorph")]
    public class AlvarusMorph : Morph
    {
    	GameSpellEffect m_effect = null;
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
        	if(target is GamePlayer)
        		if(!((GamePlayer)target).IsSwimming)
        		{
        			MessageToCaster("You must be under water to use this ability.", eChatType.CT_SpellResisted);
        			return;
        		}
        	
        	base.ApplyEffectOnTarget(target, effectiveness);
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {   
        	if(effect.Owner is GamePlayer)
        		GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.SwimmingStatus, new DOLEventHandler(SwimmingStatusChange));
        	m_effect = effect;
        	base.OnEffectStart(effect);
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
        	if(effect.Owner is GamePlayer)
        		GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.SwimmingStatus, new DOLEventHandler(SwimmingStatusChange));            
            return base.OnEffectExpires(effect, noMessages);
        }        
        private void SwimmingStatusChange(DOLEvent e, object sender, EventArgs args)
        {
        	GamePlayer player = sender as GamePlayer;
        	if(player!=null)
        		if(!player.IsSwimming) OnEffectExpires(m_effect, true);
        }        
        public AlvarusMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

    }
}
