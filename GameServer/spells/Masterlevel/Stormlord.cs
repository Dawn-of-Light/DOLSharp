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
using DOL.GS.Scripts;
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Spells
{
    #region Stormlord-1
    [SpellHandlerAttribute("DazzlingArray")]
    public class DazzlingArraySpellHandler : StormSpellHandler
    {
        // constructor
        public DazzlingArraySpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7210;
            dbs.ClientEffect = 7210;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Realm";
            dbs.Radius = 0;
            dbs.Type = "MissHit";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    #region Stormlord-2
    [SpellHandlerAttribute("VacuumVortex")]
    public class VacuumVortexSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute direct damage spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target == null || !target.IsAlive)
                return;
            if (target is GameStorm)
            {
                GameStorm targetStorm = target as GameStorm;
                if (targetStorm.Movable)
                {
                    GameNPC targetNPC = target as GameNPC;
                    int range = Util.Random(0, 750);
                    double angle = Util.RandomDouble() * 2 * Math.PI;
                    targetNPC.WalkTo(targetNPC.X + (int)(range * Math.Cos(angle)), targetNPC.Y + (int)(range * Math.Sin(angle)), targetNPC.Z, targetNPC.MaxSpeed);
                }
            }
        }

        public VacuumVortexSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }	
    #endregion

    #region Stormlord-3
    [SpellHandlerAttribute("EnervatingGas")]
    public class EnervatingGasSpellHandler : StormSpellHandler
    {
        // constructor
        public EnervatingGasSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7273;
            dbs.ClientEffect = 7273;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "MLEndudrain";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion

    #region Stormlord-4
    [SpellHandlerAttribute("InebriatingFumes")]
    public class InebriatingFumesSpellHandler : StormSpellHandler
    {
        // constructor
        public InebriatingFumesSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7258;
            dbs.ClientEffect = 7258;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.Damage;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "DexterityQuicknessDebuff";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion

    #region Stormlord-5
    [SpellHandlerAttribute("MentalSiphon")]
    public class MentalSiphonSpellHandler : StormSpellHandler
    {
        // constructor
        public MentalSiphonSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7303;
            dbs.ClientEffect = 7303;
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
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion

    #region Stormlord-6
    [SpellHandlerAttribute("FocusingWinds")]
    public class FocusingWindsSpellHandler : SpellHandler
    {
        private GameSpellEffect m_effect;
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            m_effect = effect;
            if (effect.Owner is GameStorm)
            {
                GameStorm targetStorm = effect.Owner as GameStorm;
                targetStorm.Movable = false;
                GameEventMgr.AddHandler(m_caster, GameLivingEvent.Moving, new DOLEventHandler(LivingMoves));
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GameStorm)
            {
                GameStorm targetStorm = effect.Owner as GameStorm;
                targetStorm.Movable = true;
                GameEventMgr.RemoveHandler(m_caster, GameLivingEvent.Moving, new DOLEventHandler(LivingMoves));
            }
            return base.OnEffectExpires(effect, noMessages);
        }

        public void LivingMoves(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;
            if (player == null) return;
            if (e == GameLivingEvent.Moving)
            {
                MessageToCaster("You are moving. Your concentration fades", eChatType.CT_SpellExpires);
                OnEffectExpires(m_effect, true);
                return;
            }
        }
        public FocusingWindsSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }	
    #endregion

    #region Stormlord-7
    [SpellHandlerAttribute("ChokingVapors")]
    public class ChokingVaporsSpellHandler : StormSpellHandler
    {
        // constructor
        public ChokingVaporsSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7223;
            dbs.ClientEffect = 7223;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "StrengthConstitutionDebuff";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    #region Stormlord-8
    [SpellHandlerAttribute("SenseDullingCloud")]
    public class SenseDullingCloudSpellHandler : StormSpellHandler
    {
        // constructor
        public SenseDullingCloudSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7305;
            dbs.ClientEffect = 7305;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "AcuityDebuff";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion

    #region Stormlord-9
    [SpellHandlerAttribute("EnergyTempest")]
    public class EnergyTempestSpellHandler : StormSpellHandler
    {
        // constructor
        public EnergyTempestSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new storm.
            storm = new GameStorm();
            storm.Realm = caster.Realm;
            storm.X = caster.X;
            storm.Y = caster.Y;
            storm.Z = caster.Z;
            storm.CurrentRegionID = caster.CurrentRegionID;
            storm.Heading = caster.Heading;
            storm.Owner = (GamePlayer)caster;

            // Construct the storm spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7216;
            dbs.ClientEffect = 7216;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "DirectDamage";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }	
    #endregion
}
