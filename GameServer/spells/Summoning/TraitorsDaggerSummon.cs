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
using DOL.Events;
using DOL.Database;

namespace DOL.GS.Spells
{
    [SpellHandler("TraitorsDaggerSummon")]
    public class TraitorsDaggerSummon : SummonSpellHandler
    {
        private ISpellHandler _trap;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            //Set pet infos & Brain
            base.ApplyEffectOnTarget(target, effectiveness);
            ProcPetBrain petBrain = (ProcPetBrain) m_pet.Brain;
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
            if(args == null || args.AttackData == null)
                return;
            // Spirit procs lifetap when hitting ennemy
            if(_trap == null)
            {
                _trap = MakeTrap();
            }
            if(Util.Chance(50))
            {
                _trap.CastSpell(args.AttackData.Target);
            }
        }

        private ISpellHandler MakeTrap()
        {
            DBSpell dbs = new DBSpell();
            dbs.Name = "Increased Essence Consumption";
            dbs.Icon = 11020;
            dbs.ClientEffect = 11020;
            dbs.DamageType = 10;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "PetLifedrain";
            dbs.Damage = 70;
            dbs.LifeDrainReturn = 100;
            dbs.Value = -100;
            dbs.Duration = 0;
            dbs.Frequency = 0;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = 350;
            Spell s = new Spell(dbs, 50);
            SpellLine sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            return ScriptMgr.CreateSpellHandler(m_pet, s, sl);
        }

        public TraitorsDaggerSummon(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    }
}
