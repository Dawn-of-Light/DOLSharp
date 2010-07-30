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

using System;
using DOL.GS;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Perfector
    //the link isnt corrently working so correct me if you see any timers wrong.


    //ML1 Cure NS - already handled in another area

    //ML2 GRP Cure Disease - already handled in another area

    //shared timer 1
    #region Perfecter-3
    [SpellHandlerAttribute("FOH")]
    public class FOHSpellHandler : FontSpellHandler
    {
        // constructor
        public FOHSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = true;

            //Construct a new font.
            font = new GameFont();
            font.Model = 2585;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the font spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7245;
            dbs.ClientEffect = 7245;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Realm";
            dbs.Radius = 0;
            dbs.Type = "HealOverTime";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			dbs.Message1 = spell.Message1;
			dbs.Message2 = spell.Message2;
			dbs.Message3 = spell.Message3;
			dbs.Message4 = spell.Message4;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //ML4 Greatness - passive increases 20% concentration

    //shared timer 1
    #region Perfecter-5
    [SpellHandlerAttribute("FOP")]
    public class FOPSpellHandler : FontSpellHandler
    {
        // constructor
        public FOPSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = false;

            //Construct a new font.
            font = new GameFont();
            font.Model = 2583;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the font spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7212;
            dbs.ClientEffect = 7212;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Realm";
            dbs.Radius = 0;
            dbs.Type = "PowerOverTime";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			dbs.Message1 = spell.Message1;
			dbs.Message2 = spell.Message2;
			dbs.Message3 = spell.Message3;
			dbs.Message4 = spell.Message4;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //shared timer 1
    #region Perfecter-6
    [SpellHandlerAttribute("FOR")]
    public class FORSpellHandler : FontSpellHandler
    {
        // constructor
        public FORSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = true;
            ApplyOnCombat = true;

            //Construct a new font.
            font = new GameFont();
            font.Model = 2581;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the font spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7214;
            dbs.ClientEffect = 7214;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Realm";
            dbs.Radius = 0;
            dbs.Type = "MesmerizeDurationBuff";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //shared timer 2
    //ML7 Leaping Health - already handled in another area

    //no shared timer
    #region Perfecter-8
    [SpellHandlerAttribute("SickHeal")]
    public class SickHealSpellHandler : RemoveSpellEffectHandler
    {
        // constructor
        public SickHealSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            m_spellTypesToRemove = new List<string>();
            m_spellTypesToRemove.Add("PveResurrectionIllness");
            m_spellTypesToRemove.Add("RvrResurrectionIllness");
        }
    }
    #endregion

    //shared timer 1
    #region Perfecter-9
    [SpellHandlerAttribute("FOD")]
    public class FODSpellHandler : FontSpellHandler
    {
        // constructor
        public FODSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnCombat = true;

            //Construct a new font.
            font = new GameFont();
            font.Model = 2582;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the font spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7310;
            dbs.ClientEffect = 7310;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "PowerRend";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion

    //shared timer 2
    //ML10 Rampant Healing - already handled in another area

    #region PoT
    [SpellHandlerAttribute("PowerOverTime")]
    public class PoTSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute heal over time spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // TODO: correct formula
            double eff = 1.25;
            if (Caster is GamePlayer)
            {
                double lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
                if (lineSpec < 1)
                    lineSpec = 1;
                eff = 0.75;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level * 0.5;
                    if (eff > 1.25)
                        eff = 1.25;
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
            //"{0} seems calm and healthy."
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            base.OnEffectPulse(effect);
            OnDirectEffect(effect.Owner, effect.Effectiveness);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target.InCombat) return;
            if (target.ObjectState != GameObject.eObjectState.Active) return;
            if (target.IsAlive == false) return;
            if (target is GamePlayer)
            {
                GamePlayer player = target as GamePlayer;
                if (player.CharacterClass.ID == (int)eCharacterClass.Vampiir 
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerHib
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerMid
                    || player.CharacterClass.ID == (int)eCharacterClass.MaulerAlb)
                    return;
            }

            base.OnDirectEffect(target, effectiveness);
            double heal = Spell.Value * effectiveness;
            if (heal < 0) target.Mana += (int)(-heal * target.MaxMana / 100);
            else target.Mana += (int)heal;
            //"You feel calm and healthy."
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
                //"Your meditative state fades."
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                //"{0}'s meditative state fades."
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }


        // constructor
        public PoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region CCResist
    [SpellHandler("CCResist")]
    public class CCResistSpellHandler : MasterlevelHandling
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
        	base.OnEffectStart(effect);
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDuration] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.StunDuration] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDuration] += (int)m_spell.Value;
             
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();       
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDuration] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.StunDuration] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDuration] -= (int)m_spell.Value;
            
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();  
            }
            return base.OnEffectExpires(effect,noMessages);
        }

        // constructor
        public CCResistSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}
