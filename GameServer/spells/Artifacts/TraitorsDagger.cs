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
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("TraitorsDaggerProc")]
	public class TraitorsDaggerProc : DefensiveProcSpellHandler
	{
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Shade(true);
			}
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Shade(false);
			}
			return base.OnEffectExpires(effect, noMessages);
		}
		public TraitorsDaggerProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}

	[SpellHandlerAttribute("TraitorsDaggerSummon")]
	public class TraitorsDaggerSummon : SummonSpellHandler
	{
		private DBSpell dbs;
		private Spell s;
		private SpellLine sl;
		private ISpellHandler trap;

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

			(pet.Brain as IAggressiveBrain).AddToAggroList(target, 1);
			(pet.Brain as ProcPetBrain).Think();
		}

		protected override GamePet GetGamePet(INpcTemplate template)
		{
			return new TraitorDaggerPet(template);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new ProcPetBrain(owner);
		}
		
		protected override void SetBrainToOwner(IControlledBrain brain)
		{
		}

		protected override void AddHandlers()
		{
			GameEventMgr.AddHandler(pet, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
		}

		protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			if (args == null || args.AttackData == null)
				return;

			// Spirit procs lifetap when hitting ennemy
			if (trap != null)
				trap.StartSpell(args.AttackData.Target);
		}

		public TraitorsDaggerSummon(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			dbs = new DBSpell();
			dbs.Name = "Increased Essence Consumption";
			dbs.Icon = 661;
			dbs.ClientEffect = 661;
			dbs.DamageType = 10;
			dbs.Target = "Enemy";
			dbs.Radius = 0;
			dbs.Type = "Lifedrain";
			dbs.Damage = 50;
			dbs.LifeDrainReturn = 100;
			dbs.Value = -100;
			dbs.Duration = 0;
			dbs.Frequency = 0;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = 350;
			s = new Spell(dbs, 1);
			sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
			trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
		}
	}
}

namespace DOL.GS
{
	public class TraitorDaggerPet : GamePet
	{
		public override int MaxHealth
		{
			get { return Level * 25; }
		}
		public override void OnAttackedByEnemy(AttackData ad) { }
		public TraitorDaggerPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}