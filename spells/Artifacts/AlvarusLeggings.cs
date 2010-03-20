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
            GamePlayer targetPlayer = target as GamePlayer;

			if (targetPlayer == null)
				return;

            if (!targetPlayer.IsUnderwater)
            {
                MessageToCaster("You must be under water to use this ability.", eChatType.CT_SpellResisted);
                return;
            }
            foreach (GameSpellEffect Effect in targetPlayer.EffectList.GetAllOfType(typeof(GameSpellEffect)))
            {
                if (
                    Effect.SpellHandler.Spell.SpellType.Equals("ShadesOfMist") || 
                    Effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("DreamMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("DreamGroupMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("AtlantisTabletMorph"))
                {
                    targetPlayer.Out.SendMessage("You already have an active morph!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                    return;
                }
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            m_effect = effect;
            base.OnEffectStart(effect);
            GamePlayer player = effect.Owner as GamePlayer;
            if (player == null) return;
            GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.SwimmingStatus, new DOLEventHandler(SwimmingStatusChange));

        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player == null) return base.OnEffectExpires(effect, noMessages);
            GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.SwimmingStatus, new DOLEventHandler(SwimmingStatusChange));  
            return base.OnEffectExpires(effect, noMessages);
        }        
        private void SwimmingStatusChange(DOLEvent e, object sender, EventArgs args)
        {
            OnEffectExpires(m_effect, true);
        }        
        public AlvarusMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

    }
}
