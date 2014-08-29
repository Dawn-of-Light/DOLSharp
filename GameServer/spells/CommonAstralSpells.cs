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
using System.Collections.Generic;
using System.Reflection;

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Dex/Qui/Str/Con stat specline debuff and transfers them to the caster.
    /// </summary>
    [SpellHandlerAttribute("DexStrConQuiTap")]
    public class DexStrConQuiTap : SpellHandler
    {
        private IList<eProperty> m_stats;
        public IList<eProperty> Stats
        {
            get { return m_stats; }
            set { m_stats = value; }
        }

        public DexStrConQuiTap(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            Stats = new List<eProperty>();
            Stats.Add(eProperty.Dexterity);
            Stats.Add(eProperty.Strength);
            Stats.Add(eProperty.Constitution);
            Stats.Add(eProperty.Quickness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            foreach (eProperty property in Stats)
            {
                m_caster.BaseBuffBonusCategory[property] += (int)m_spell.Value;
                effect.Owner.DebuffCategory[property] -= (int)m_spell.Value;
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            foreach (eProperty property in Stats)
            {
                effect.Owner.DebuffCategory[property] += (int)m_spell.Value;
                m_caster.BaseBuffBonusCategory[property] -= (int)m_spell.Value;
            }
            return base.OnEffectExpires(effect, noMessages);
        }
    }

    /// <summary>
    /// A proc to lower target's ArmorFactor and ArmorAbsorption.
    /// </summary>
    [SpellHandlerAttribute("ArmorReducingEffectiveness")]
    public class ArmorReducingEffectiveness : DualStatDebuff
    {
        public override eProperty Property1 { get { return eProperty.ArmorFactor; } }
        public override eProperty Property2 { get { return eProperty.ArmorAbsorption; } }
        public ArmorReducingEffectiveness(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

}