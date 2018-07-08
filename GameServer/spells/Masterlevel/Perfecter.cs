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
 */

using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
    // http://www.camelotherald.com/masterlevels/ma.php?ml=Perfector
    // the link isnt corrently working so correct me if you see any timers wrong.

    // ML1 Cure NS - already handled in another area

    // ML2 GRP Cure Disease - already handled in another area

    // shared timer 1
    [SpellHandler("FOH")]
    public class FOHSpellHandler : FontSpellHandler
    {
        // constructor
        public FOHSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = true;

            // Construct a new font.
            font = new GameFont
            {
                Model = 2585,
                Name = spell.Name,
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster
            };

            // Construct the font spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7245,
                ClientEffect = 7245,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Realm",
                Radius = 0,
                Type = "HealOverTime",
                Value = spell.Value,
                Duration = spell.ResurrectHealth,
                Frequency = spell.ResurrectMana,
                Pulse = 0,
                PulsePower = 0,
                LifeDrainReturn = spell.LifeDrainReturn,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE,
                Message1 = spell.Message1,
                Message2 = spell.Message2,
                Message3 = spell.Message3,
                Message4 = spell.Message4
            };

            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    // ML4 Greatness - passive increases 20% concentration

    // shared timer 1
    [SpellHandler("FOP")]
    public class FOPSpellHandler : FontSpellHandler
    {
        // constructor
        public FOPSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = false;

            // Construct a new font.
            font = new GameFont
            {
                Model = 2583,
                Name = spell.Name,
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster
            };

            // Construct the font spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7212,
                ClientEffect = 7212,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Realm",
                Radius = 0,
                Type = "PowerOverTime",
                Value = spell.Value,
                Duration = spell.ResurrectHealth,
                Frequency = spell.ResurrectMana,
                Pulse = 0,
                PulsePower = 0,
                LifeDrainReturn = spell.LifeDrainReturn,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE,
                Message1 = spell.Message1,
                Message2 = spell.Message2,
                Message3 = spell.Message3,
                Message4 = spell.Message4
            };

            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    // shared timer 1
    [SpellHandler("FOR")]
    public class FORSpellHandler : FontSpellHandler
    {
        // constructor
        public FORSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = true;
            ApplyOnCombat = true;

            // Construct a new font.
            font = new GameFont
            {
                Model = 2581,
                Name = spell.Name,
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster
            };

            // Construct the font spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7214,
                ClientEffect = 7214,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Realm",
                Radius = 0,
                Type = "MesmerizeDurationBuff",
                Value = spell.Value,
                Duration = spell.ResurrectHealth,
                Frequency = spell.ResurrectMana,
                Pulse = 0,
                PulsePower = 0,
                LifeDrainReturn = spell.LifeDrainReturn,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };

            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    // shared timer 2
    // ML7 Leaping Health - already handled in another area

    // no shared timer
    [SpellHandler("SickHeal")]
    public class SickHealSpellHandler : RemoveSpellEffectHandler
    {
        // constructor
        public SickHealSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            SpellTypesToRemove = new List<string> {"PveResurrectionIllness", "RvrResurrectionIllness"};
        }
    }

    // shared timer 1
    [SpellHandler("FOD")]
    public class FODSpellHandler : FontSpellHandler
    {
        // constructor
        public FODSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnCombat = true;

            // Construct a new font.
            font = new GameFont
            {
                Model = 2582,
                Name = spell.Name,
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster
            };

            // Construct the font spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7310,
                ClientEffect = 7310,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "PowerRend",
                Value = spell.Value,
                Duration = spell.ResurrectHealth,
                Frequency = spell.ResurrectMana,
                Pulse = 0,
                PulsePower = 0,
                LifeDrainReturn = spell.LifeDrainReturn,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };

            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    // shared timer 2
    // ML10 Rampant Healing - already handled in another area
    [SpellHandler("PowerOverTime")]
    public class PoTSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute heal over time spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // TODO: correct formula
            double eff = 1.25;
            if (Caster is GamePlayer)
            {
                double lineSpec = Caster.GetModifiedSpecLevel(SpellLine.Spec);
                if (lineSpec < 1)
                {
                    lineSpec = 1;
                }

                eff = 0.75;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level * 0.5;
                    if (eff > 1.25)
                    {
                        eff = 1.25;
                    }
                }
            }

            base.ApplyEffectOnTarget(target, eff);
        }

        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellEffect(this, Spell.Duration, Spell.Frequency, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            SendEffectAnimation(effect.Owner, 0, false, 1);

            // "{0} seems calm and healthy."
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            base.OnEffectPulse(effect);
            OnDirectEffect(effect.Owner, effect.Effectiveness);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target.InCombat)
            {
                return;
            }

            if (target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (target.IsAlive == false)
            {
                return;
            }

            if (target is GamePlayer player)
            {
                if (player.CharacterClass.ID == (int)eCharacterClass.Vampiir
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerHib
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerMid
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerAlb)
                {
                    return;
                }
            }

            base.OnDirectEffect(target, effectiveness);
            double heal = Spell.Value * effectiveness;
            if (heal < 0)
            {
                target.Mana += (int)(-heal * target.MaxMana / 100);
            }
            else
            {
                target.Mana += (int)heal;
            }

            // "You feel calm and healthy."
            MessageToLiving(target, Spell.Message1, eChatType.CT_Spell);
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);
            if (!noMessages)
            {
                // "Your meditative state fades."
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);

                // "{0}'s meditative state fades."
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return 0;
        }

        // constructor
        public PoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("CCResist")]
    public class CCResistSpellHandler : MasterlevelHandling
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDurationReduction] += (int)Spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.StunDurationReduction] += (int)Spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDurationReduction] += (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDurationReduction] -= (int)Spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.StunDurationReduction] -= (int)Spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDurationReduction] -= (int)Spell.Value;

            if (effect.Owner is GamePlayer player)
            {
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }

            return base.OnEffectExpires(effect,noMessages);
        }

        // constructor
        public CCResistSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
