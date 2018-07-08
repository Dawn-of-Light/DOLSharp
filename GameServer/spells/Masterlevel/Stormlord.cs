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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;
using DOL.Events;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
    // http://www.camelotherald.com/masterlevels/ma.php?ml=Stormlord
    // shared timer 1
    [SpellHandler("DazzlingArray")]
    public class DazzlingArraySpellHandler : StormSpellHandler
    {
        // constructor
        public DazzlingArraySpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7210,
                ClientEffect = 7210,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Realm",
                Radius = 0,
                Type = "StormMissHit",
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

            // should be 4
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    [SpellHandler("StormMissHit")]
    public class StormMissHit : MasterlevelBuffHandling
    {
        public override eProperty Property1 => eProperty.MissHit;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);

            if (target is GamePlayer player)
            {
                player.Out.SendMessage("You're harder to hit!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public StormMissHit(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("VacuumVortex")]
    public class VacuumVortexSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute direct damage spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override IList<GameLiving> SelectTargets(GameObject CasterTarget)
        {

            var list = new List<GameLiving>(8);
            foreach (GameNPC storms in Caster.GetNPCsInRadius(350))
            {
                if ((storms is GameStorm) && GameServer.ServerRules.IsSameRealm(storms, Caster, true))
                {
                    list.Add(storms);
                }
            }

            return list;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            // base.OnDirectEffect(target, effectiveness);
            var targets = SelectTargets(Caster);

            if (targets == null)
            {
                return;
            }

            foreach (GameStorm targetStorm in targets)
            {
                if (targetStorm.Movable)
                {
                    GameNPC targetNPC = targetStorm;
                    int range = Util.Random(0, 750);
                    double angle = Util.RandomDouble() * 2 * Math.PI;
                    targetNPC.WalkTo(targetNPC.X + (int)(range * Math.Cos(angle)), targetNPC.Y + (int)(range * Math.Sin(angle)), targetNPC.Z, targetNPC.MaxSpeed);
                }
            }
        }

        public VacuumVortexSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 2
    [SpellHandler("EnervatingGas")]
    public class EnervatingGasSpellHandler : StormSpellHandler
    {
        // constructor
        public EnervatingGasSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7273,
                ClientEffect = 7273,
                Damage = Math.Abs(spell.Damage),
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "StormEnduDrain",
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

            // should be 2
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    [SpellHandler("StormEnduDrain")]
    public class StormEndudrain : SpellHandler
    {

        public StormEndudrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);

            neweffect.Start(target);

            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            // spell damage should 25;
            int end = (int)Spell.Damage;
            target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, -end);

            if (target is GamePlayer)
            {
                ((GamePlayer)target).Out.SendMessage($" You lose {end} endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            } 

            (Caster as GamePlayer)?.Out.SendMessage($"{target.Name} loses {end} endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
    }

    // shared timer 1
    [SpellHandler("InebriatingFumes")]
    public class InebriatingFumesSpellHandler : StormSpellHandler
    {
        // constructor
        public InebriatingFumesSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7258,
                ClientEffect = 7258,
                Damage = spell.Damage,
                DamageType = (int) spell.Damage,
                Target = "Enemy",
                Radius = 0,
                Type = "StormDexQuickDebuff",
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

            // should be 2
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    /// <summary>
    /// Dex/Qui stat specline debuff
    /// </summary>
    [SpellHandler("StormDexQuickDebuff")]
    public class StormDexQuickDebuff : DualStatDebuff
    {
        public override eProperty Property1 => eProperty.Dexterity;

        public override eProperty Property2 => eProperty.Quickness;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);

            if (target is GamePlayer player)
            {
                player.Out.SendMessage("Your dexterity and quickness decreased!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public StormDexQuickDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 2
    [SpellHandler("MentalSiphon")]
    public class MentalSiphonSpellHandler : StormSpellHandler
    {
        // constructor
        public MentalSiphonSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7303,
                ClientEffect = 7303,
                Damage = Math.Abs(spell.Damage),
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "PowerDrainStorm",
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

            // should be 2
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    [SpellHandler("PowerDrainStorm")]
    public class PowerDrainStormSpellHandler : SpellHandler
    {
        public PowerDrainStormSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);
            int mana = (int)Spell.Damage;
            target.ChangeMana(target, GameLiving.eManaChangeType.Spell, -mana);

            if (target is GamePlayer)
            {
                ((GamePlayer)target).Out.SendMessage($"{Caster.Name} steals you {mana} points of power!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }

            StealMana(target, mana);

            // target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
        }

        public virtual void StealMana(GameLiving target, int mana)
        {
            if (!Caster.IsAlive)
            {
                return;
            }

            Caster.ChangeMana(target, GameLiving.eManaChangeType.Spell, mana);
            SendCasterMessage(target, mana);
        }

        public virtual void SendCasterMessage(GameLiving target, int mana)
        {
            MessageToCaster($"You steal {target.Name} for {mana} power!", eChatType.CT_YouHit);
            if (mana > 0)
            {
                MessageToCaster($"You steal {mana} power points{(mana == 1 ? "." : "s.")}", eChatType.CT_Spell);
            }

            // else
            // {
            //   MessageToCaster("You cannot absorb any more power.", eChatType.CT_SpellResisted);
            // }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }
    }

    // no shared timer
    [SpellHandler("FocusingWinds")]
    public class FocusingWindsSpellHandler : SpellHandler
    {
        private GameSpellEffect m_effect;

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            m_effect = effect;
            if (effect.Owner is GameStorm targetStorm)
            {
                targetStorm.Movable = false;
                MessageToCaster("Now the vortex of this storm is locked!", eChatType.CT_YouWereHit);
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(LivingMoves));
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GameStorm targetStorm)
            {
                targetStorm.Movable = true;
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(LivingMoves));
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        public void LivingMoves(DOLEvent e, object sender, EventArgs args)
        {
            if (!(sender is GameLiving))
            {
                return;
            }

            if (e == GameLivingEvent.Moving)
            {
                MessageToCaster("You are moving. Your concentration fades", eChatType.CT_SpellExpires);
                OnEffectExpires(m_effect, true);
            }
        }

        public FocusingWindsSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 1
    [SpellHandler("ChokingVapors")]
    public class ChokingVaporsSpellHandler : StormSpellHandler
    {
        // constructor
        public ChokingVaporsSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7223,
                ClientEffect = 7223,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "StormStrConstDebuff",
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

            // should be 2
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    /// <summary>
    /// Str/Con stat specline debuff
    /// </summary>
    [SpellHandler("StormStrConstDebuff")]
    public class StormStrConstDebuff : DualStatDebuff
    {
        public override eProperty Property1 => eProperty.Strength;

        public override eProperty Property2 => eProperty.Constitution;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);

            if (target is GamePlayer player)
            {
                player.Out.SendMessage("Your strenght and constitution decreased!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public StormStrConstDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // shared timer 1
    [SpellHandler("SenseDullingCloud")]
    public class SenseDullingCloudSpellHandler : StormSpellHandler
    {
        // constructor
        public SenseDullingCloudSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7305,
                ClientEffect = 7305,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "StormAcuityDebuff",
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

            // should be 2
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    /// <summary>
    /// Acuity stat baseline debuff
    /// </summary>
    [SpellHandler("StormAcuityDebuff")]
    public class StormAcuityDebuff : SingleStatDebuff
    {
        public override eProperty Property1
        {

            get
            {
                eProperty temp = eProperty.Acuity;
                if (m_spellTarget.Realm == eRealm.Albion)
                {
                    temp = eProperty.Intelligence;
                }

                if (m_spellTarget.Realm == eRealm.Midgard)
                {
                    temp = eProperty.Piety;
                }

                if (m_spellTarget.Realm == eRealm.Hibernia)
                {
                    temp = eProperty.Intelligence;
                }

                return temp;
            }
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);

            if (target is GamePlayer player)
            {
                player.Out.SendMessage("Your acuity decreased!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public StormAcuityDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("EnergyTempest")]
    public class EnergyTempestSpellHandler : StormSpellHandler
    {
        // constructor
        public EnergyTempestSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // Construct a new storm.
            storm = new GameStorm
            {
                Realm = caster.Realm,
                X = caster.X,
                Y = caster.Y,
                Z = caster.Z,
                CurrentRegionID = caster.CurrentRegionID,
                Heading = caster.Heading,
                Owner = (GamePlayer) caster,
                Movable = true
            };

            // Construct the storm spell
            dbs = new DBSpell
            {
                Name = spell.Name,
                Icon = 7216,
                ClientEffect = 7216,
                Damage = spell.Damage,
                DamageType = (int) spell.DamageType,
                Target = "Enemy",
                Radius = 0,
                Type = "StormEnergyTempest",
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
            tempest = ScriptMgr.CreateSpellHandler(Caster, s, sl);
        }
    }

    [SpellHandler("StormEnergyTempest")]
    public class StormEnergyTempest : SpellHandler
    {
        /// <summary>
        /// Calculates the base 100% spell damage which is then modified by damage variance factors
        /// </summary>
        /// <returns></returns>
        public override double CalculateDamageBase(GameLiving target)
        {
            GamePlayer player = Caster as GamePlayer;

            // % damage procs
            if (Spell.Damage < 0)
            {
                double spellDamage = 0;

                if (player != null)
                {
                    // This equation is used to simulate live values - Tolakram
                    spellDamage = target.MaxHealth * -Spell.Damage * .01 / 2.5;
                }

                if (spellDamage < 0)
                {
                    spellDamage = 0;
                }

                return spellDamage;
            }

            return base.CalculateDamageBase(target);
        }

        public override double DamageCap(double effectiveness)
        {
            if (Spell.Damage < 0)
            {
                return m_spellTarget.MaxHealth * -Spell.Damage * .01 * 3.0 * effectiveness;
            }

            return base.DamageCap(effectiveness);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            neweffect.Start(target);

            // calc damage
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            DamageTarget(ad, true);
            SendDamageMessages(ad);
            target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public StormEnergyTempest(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // ML 10 Arcing Power - already handled in another area
}
