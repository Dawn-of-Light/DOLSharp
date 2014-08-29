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

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Spells
{
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

        protected override GamePet GetGamePet(INpcTemplate template) { return new UncontrolledTargetedPet(template); }
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
            : base(caster, spell, line) 
        {
        }
    }
}

