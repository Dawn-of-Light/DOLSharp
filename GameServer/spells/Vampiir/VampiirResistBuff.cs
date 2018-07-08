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
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
    [SpellHandler("VampiirMeleeResistance")]
    public class VampiirMeleeResistance : SpellHandler
    {
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GameLiving living = effect.Owner;
            GamePlayer player = effect.Owner as GamePlayer;
            int value = (int)Spell.Value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += value;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameLiving living = effect.Owner;
            GamePlayer player = effect.Owner as GamePlayer;
            int value = (int)Spell.Value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] -= value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] -= value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] -= value;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }

            MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
            return 0;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            int specLevel = 0;
            if (Caster is GamePlayer player)
            {
                specLevel = player.GetModifiedSpecLevel(SpellLine.Spec);
            }

            effectiveness = 0.75 + (specLevel - 1) * 0.5 / Spell.Level;
            effectiveness = Math.Max(0.75, effectiveness);
            effectiveness = Math.Min(1.25, effectiveness);
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(16)
                {
                    $"Name: {Spell.Name}",
                    $"Description: {Spell.Description}",
                    $"Target: {Spell.Target}",
                    $"Casting time: {Spell.CastTime * 0.001:0.0## sec;-0.0## sec;'instant'}"
                };

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                list.Add("All Melee Resist Increased: 0" /*+ Spell.Value*/);
                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                return list;
            }
        }

        public VampiirMeleeResistance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("VampiirMagicResistance")]
    public class VampiirMagicResistance : SpellHandler
    {
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GamePlayer player = effect.Owner as GamePlayer;
            GameLiving living = effect.Owner;

            BedazzlingAuraEffect boad = player.EffectList.GetOfType<BedazzlingAuraEffect>();
            boad?.Cancel(false);

            int value = (int)Spell.Value;
            living.AbilityBonus[(int)eProperty.Resist_Body] += value;
            living.AbilityBonus[(int)eProperty.Resist_Cold] += value;
            living.AbilityBonus[(int)eProperty.Resist_Energy] += value;
            living.AbilityBonus[(int)eProperty.Resist_Heat] += value;
            living.AbilityBonus[(int)eProperty.Resist_Matter] += value;
            living.AbilityBonus[(int)eProperty.Resist_Spirit] += value;
            player.Out.SendCharStatsUpdate();
            player.UpdatePlayerStatus();
            player.Out.SendCharResistsUpdate();

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameLiving living = effect.Owner;
            int value = (int)Spell.Value;
            living.AbilityBonus[(int)eProperty.Resist_Body] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Cold] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Energy] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Heat] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Matter] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Spirit] -= value;
            if (living is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }

            MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
            return 0;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            int specLevel = 0;
            if (Caster is GamePlayer player)
            {
                specLevel = player.GetModifiedSpecLevel(SpellLine.Spec);
            }

            effectiveness = 0.75 + (specLevel - 1) * 0.5 / Spell.Level;
            effectiveness = Math.Max(0.75, effectiveness);
            effectiveness = Math.Min(1.25, effectiveness);
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(16)
                {
                    $"Name: {Spell.Name}",
                    $"Description: {Spell.Description}",
                    $"Target: {Spell.Target}",
                    $"Casting time: {Spell.CastTime * 0.001:0.0## sec;-0.0## sec;'instant'}"
                };

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{Spell.Duration % 60000 / 1000:00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                list.Add($"All Magic Resist Increased: {Spell.Value}");
                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                return list;
            }
        }

        public VampiirMagicResistance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}