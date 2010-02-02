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

namespace DOL.GS.Spells
{
    /// <summary>
    /// Maddening Scalars Defensive proc spell handler
    /// </summary>
    [SpellHandlerAttribute("MaddeningScalars")]
    public class MaddeningScalars : OffensiveProcSpellHandler
    {   	
   		public override void OnEffectStart(GameSpellEffect effect)
		{
            base.OnEffectStart(effect);
            if(effect.Owner is GamePlayer)
            {
	            GamePlayer player = effect.Owner as GamePlayer;
	            if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model = (ushort)Spell.LifeDrainReturn; // 102 official model
	   			player.Out.SendUpdatePlayer();
   			}
   		}
   		
  		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
  			if(effect.Owner is GamePlayer)
            {
	            GamePlayer player = effect.Owner as GamePlayer; 				
  				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model = player.CreationModel;
    			player.Out.SendUpdatePlayer();
    		}	
    		return base.OnEffectExpires(effect,noMessages);
  		}
        
        public MaddeningScalars(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
