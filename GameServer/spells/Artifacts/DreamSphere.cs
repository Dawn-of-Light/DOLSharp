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
    
    //the self dream-morph doesnt break on damage/attacked by enemy only grp-target 1 does
    [SpellHandlerAttribute("DreamMorph")]
    public class DreamMorph : OffensiveProcSpellHandler
	{   	
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if(effect.Owner is GamePlayer)
			{
				GamePlayer player=effect.Owner as GamePlayer;
                foreach (GameSpellEffect Effect in player.EffectList.GetAllOfType(typeof(GameSpellEffect)))
                {
                    if (Effect.SpellHandler.Spell.SpellType.Equals("ShadesOfMist") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("DreamGroupMorph") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("AtlantisTabletMorph") || 
                        Effect.SpellHandler.Spell.SpellType.Equals("AlvarusMorph"))
                    {
                        player.Out.SendMessage("You already have a activate morph!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                        return;
                    }
                }
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer && (ushort)Spell.LifeDrainReturn > 0) 
                    player.Model = (ushort)Spell.LifeDrainReturn;
				player.Out.SendUpdatePlayer();
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect,bool noMessages)
		{
			if(effect.Owner is GamePlayer)
			{
				GamePlayer player=effect.Owner as GamePlayer; 				
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model = player.CreationModel;
				player.Out.SendUpdatePlayer();
			}	
			return base.OnEffectExpires(effect,noMessages);
		}

        public DreamMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    /// <summary>
    /// Dream Sphere group morph spell handler
    /// The DoT proc is a subspell, affects only caster
    /// </summary> 

    //http://support.darkageofcamelot.com/kb/article.php?id=745
    //- The Panther Form level 10 ability of the Dreamsphere artifact has been changed.
    //When a character in panther form is attacked, they revert to normal form and lose all associated bonuses. 
    //This change is specific to the Dreamsphere only and does not affect other shapechange forms

    //http://www.daoc-toa.net/img/dreamPrey.jpg
    //http://www.daoc-toa.net/img/dreamCat.jpg
    [SpellHandlerAttribute("DreamGroupMorph")]
    public class DreamGroupMorph : DreamMorph
    {
    	private GameSpellEffect m_effect = null;
        public override void OnEffectStart(GameSpellEffect effect)
        {
         	m_effect = effect;    
        	base.OnEffectStart(effect);
            GamePlayer player = effect.Owner as GamePlayer;
            foreach (GameSpellEffect Effect in player.EffectList.GetAllOfType(typeof(GameSpellEffect)))
            {
                if (Effect.SpellHandler.Spell.SpellType.Equals("ShadesOfMist") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("DreamMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("AtlantisTabletMorph") || 
                    Effect.SpellHandler.Spell.SpellType.Equals("AlvarusMorph"))
                {
                    player.Out.SendMessage("You already have a activate morph!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                    return;
                }
            }
            if(player == null) return;
            //GameEventMgr.AddHandler(player, GamePlayerEvent.TakeDamage, new DOLEventHandler(LivingTakeDamage));
			GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(LivingTakeDamage));
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if(player == null) return base.OnEffectExpires(effect, noMessages);
            //GameEventMgr.RemoveHandler(player, GamePlayerEvent.TakeDamage, new DOLEventHandler(LivingTakeDamage));
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(LivingTakeDamage));
            return base.OnEffectExpires(effect, noMessages);
        }
        // Event : player takes damage, effect cancels
        private void LivingTakeDamage(DOLEvent e, object sender, EventArgs args)
        {
            OnEffectExpires(m_effect, true);
        }
        public DreamGroupMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
