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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Fury")]
    public class FuryHandler : SpellHandler
    {
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, 1);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            int value = (int)m_spell.Value;

            SendEffectAnimation(effect.Owner, 0, false, 1);
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Body] += value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Cold] += value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Energy] += value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Heat] += value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Matter] += value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Spirit] += value;

            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            int value = (int)m_spell.Value;

            effect.Owner.AbilityBonus[(int)eProperty.Resist_Body] -= value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Cold] -= value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Energy] -= value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Heat] -= value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Matter] -= value;
            effect.Owner.AbilityBonus[(int)eProperty.Resist_Spirit] -= value;

            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }

            return 0;
        }

        public FuryHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}