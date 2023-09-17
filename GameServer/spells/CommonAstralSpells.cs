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
using DOL.GS.Geometry;
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
                m_caster.BaseBuffBonusCategory[(int)property] += (int)m_spell.Value;
                m_spellTarget.DebuffCategory[(int)property] -= (int)m_spell.Value;
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            foreach (eProperty property in Stats)
            {
                m_spellTarget.DebuffCategory[(int)property] += (int)m_spell.Value;
                m_caster.BaseBuffBonusCategory[(int)property] -= (int)m_spell.Value;
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

    /// <summary>
    /// Summons a Elemental that only follows the caster.
    /// </summary>
    [SpellHandler("SummonHealingElemental")]
    public class SummonHealingElemental : MasterlevelHandling
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        GameNPC summoned = null;
        GameSpellEffect beffect = null;
        public SummonHealingElemental(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)  {}

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
            {
                return;
            }

            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
                MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
                return;
            }

            var summonPosition = target.Position.TurnedAround() + Vector.Create(target.Orientation, length: 64);
            beffect = CreateSpellEffect(target, effectiveness);
            {
                BrittleBrain controlledBrain = new BrittleBrain(player);
                controlledBrain.IsMainPet = false;
                summoned = new GameNPC(template);
                summoned.SetOwnBrain(controlledBrain);
                summoned.Position = summonPosition;
                summoned.Realm = target.Realm;
                summoned.CurrentSpeed = 0;
                summoned.Level = Caster.Level;
                summoned.Size = 50;
                summoned.AddToWorld();
                controlledBrain.AggressionState = eAggressionState.Passive;
                beffect.Start(Caster);
            }
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
            if (summoned != null)
            {
                summoned.Health = 0; // to send proper remove packet
                summoned.Delete();
            }
            return base.OnEffectExpires(effect, noMessages);
        }
    }

    /// <summary>
    /// Summons a Elemental that follows the target and attacks the target.
    /// </summary>
    [SpellHandler("SummonElemental")]
    public class SummonElemental : SummonSpellHandler
    {
        private ISpellHandler _trap;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            //Set pet infos & Brain
            base.ApplyEffectOnTarget(target, effectiveness);
            ProcPetBrain petBrain = (ProcPetBrain)m_pet.Brain;
            m_pet.Level = Caster.Level;
            m_pet.Strength = 0;
            petBrain.AddToAggroList(target, 1);
            petBrain.Think();
        }

        protected override GamePet GetGamePet(INpcTemplate template) { return new SummonElementalPet(template); }
        protected override IControlledBrain GetPetBrain(GameLiving owner) { return new ProcPetBrain(owner); }
        protected override void SetBrainToOwner(IControlledBrain brain) { }
        protected override void AddHandlers() { GameEventMgr.AddHandler(m_pet, GameLivingEvent.AttackFinished, EventHandler); }

        protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            if (args == null || args.AttackData == null)
                return;

            if (_trap == null)
            {
                _trap = MakeTrap();
            }
            if (Util.Chance(99))
            {
                _trap.CastSpell(args.AttackData.Target);
            }
        }
        // Creates the trap(spell)
        private ISpellHandler MakeTrap()
        {
            DBSpell dbs = new DBSpell();
            dbs.Name = "irritatin wisp";
            dbs.Icon = 4107;
            dbs.ClientEffect = 5435;
            dbs.DamageType = 15;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "DirectDamage";
            dbs.Damage = 80;
            dbs.Value = 0;
            dbs.Duration = 0;
            dbs.Frequency = 0;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 1500;
            Spell s = new Spell(dbs, 50);
            SpellLine sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            return ScriptMgr.CreateSpellHandler(m_pet, s, sl);
        }

        public SummonElemental(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    } 
}

namespace DOL.GS
{
    public class SummonHealingElementalPet : GamePet
    {
        public override int MaxHealth
        {
            get { return Level * 10; }
        }
        public override void OnAttackedByEnemy(AttackData ad) { }
        public SummonHealingElementalPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
    }

    public class SummonElementalPet : GamePet
    {
        public override int MaxHealth
        {
            get { return Level * 10; }
        }
        public override void OnAttackedByEnemy(AttackData ad) { }
        public SummonElementalPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
    }
}
