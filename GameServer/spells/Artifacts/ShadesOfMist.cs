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
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Shades of Mist spell handler: Shape change (shade) on self with
    /// a defensive proc (200 pt. melee health buffer).
    /// </summary>
    /// <author>Aredhel</author>
    [SpellHandler("ShadesOfMist")]
    public class ShadesOfMist : DefensiveProcSpellHandler
    {
        /// <summary>
        /// Effect starting.
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            GamePlayer player = effect.Owner as GamePlayer;
            foreach (GameSpellEffect Effect in player.EffectList.GetAllOfType(typeof(GameSpellEffect)))
            {
                if (Effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("DreamMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("DreamGroupMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("AtlantisTabletMorph") ||
                    Effect.SpellHandler.Spell.SpellType.Equals("AlvarusMorph"))
                {
                    player.Out.SendMessage("You already have an active morph!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                    return;
                }
            }
            if (player != null)
                player.Model = player.ShadeModel;        
        }

        /// <summary>
        /// Effect expiring (duration spells only).
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="noMessages"></param>
        /// <returns>Immunity duration in milliseconds.</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GamePlayer player = effect.Owner as GamePlayer;

            if (player != null)
                player.Model = player.CreationModel;      
      
            return base.OnEffectExpires(effect, noMessages);
        }

        /// <summary>
        /// Creates a new ShadesOfMist spell handler.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public ShadesOfMist(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    }
}
