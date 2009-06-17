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
using DOL.GS.Effects;
using DOL.Database;
using DOL.Events;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("TraitorsDaggerProc")]
	public class TraitorsDaggerProc : OffensiveProcSpellHandler
	{
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Shade(true);
                player.Out.SendUpdatePlayer();
			}
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Shade(false);
                player.Out.SendUpdatePlayer();
			}
			return base.OnEffectExpires(effect, noMessages);
		}
   
		public TraitorsDaggerProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

    [SpellHandler("DdtProcDd")]
    public class DdtProcDd:DirectDamageSpellHandler
    {
        public DdtProcDd(GameLiving caster,Spell spell,SpellLine line) : base(caster,spell,line) { }

        public override void OnDirectEffect(GameLiving target,double effectiveness)
        {
            base.OnDirectEffect(target,effectiveness);
            Caster.ChangeHealth(Caster,GameLiving.eHealthChangeType.Spell,-Spell.ResurrectHealth);
        }
    }

    [SpellHandler("TraitorsDaggerSummon")]
    public class TraitorsDaggerSummon : SummonSpellHandler
    {
        private ISpellHandler _trap;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            //Set pet infos & Brain
            base.ApplyEffectOnTarget(target, effectiveness);
            ProcPetBrain petBrain = (ProcPetBrain) pet.Brain;
            petBrain.AddToAggroList(target, 1);
            petBrain.Think();
        }

        protected override GamePet GetGamePet(INpcTemplate template) { return new TraitorDaggerPet(template); }
        protected override IControlledBrain GetPetBrain(GameLiving owner) { return new ProcPetBrain(owner); }
        protected override void SetBrainToOwner(IControlledBrain brain) { }
        protected override void AddHandlers() { GameEventMgr.AddHandler(pet, GameLivingEvent.AttackFinished, EventHandler); }

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
            return ScriptMgr.CreateSpellHandler(pet, s, sl);
        }

        public TraitorsDaggerSummon(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    }
}

namespace DOL.GS
{
	public class TraitorDaggerPet : GamePet
	{
		public override int MaxHealth
		{
			get { return Level * 15; }
		}
		public override void OnAttackedByEnemy(AttackData ad) { }
		public TraitorDaggerPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}