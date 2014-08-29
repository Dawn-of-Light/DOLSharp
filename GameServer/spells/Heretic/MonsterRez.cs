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
using System.Collections.Concurrent;

using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Reanimate a fallen realm mate into a monster that procs DoT
	/// </summary>
	[SpellHandlerAttribute("ReanimateCorpse")]
	public class MonsterResurrectHandler : ResurrectSpellHandler
	{
		// Constructor
		public MonsterResurrectHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
        {
        }

		public override byte IllnessReduction {
			get { return 100; }
		}
		
		/// <summary>
		/// Prevent from launching SubSpell on finish cast.
		/// </summary>
		/// <param name="target"></param>
		public override void FinishCastSubSpell(GameLiving target)
		{
		}
		
		/// <summary>
		/// Start Subspell Monster Summon on Revive
		/// </summary>
		/// <param name="living"></param>
		protected override void ResurrectLiving(GameLiving living)
		{
			base.ResurrectLiving(living);
			base.FinishCastSubSpell(living);
		}
		
		/// <summary>
		/// No Resurrect Immunity for Monster Rez
		/// </summary>
		/// <param name="target"></param>
		protected override void StartResurrectImmunity(GameLiving target)
		{
		}
	}

	/// <summary>
	/// Transform a realm mate into an horrible beast.
	/// </summary>
	[SpellHandlerAttribute("SummonMonster")]
	public class SummonMonsterHandler : AllAbsorbtionBuffHandler
	{
		private ConcurrentDictionary<GameLiving, ushort> m_ownerModel = new ConcurrentDictionary<GameLiving, ushort>();
		
		public override void OnEffectStart(GameSpellEffect effect)
		{

			GameLiving target = effect.Owner;
			if(!m_ownerModel.ContainsKey(target))
			{
				if (m_ownerModel.TryAdd(target, target.Model))
				{
					target.Model = (ushort)Spell.LifeDrainReturn;
				}
			}
			
			// Prevent the target from attacking
			target.DisarmedTime = Caster.CurrentRegion.Time + effect.RemainingTime;
			target.SilencedTime = Caster.CurrentRegion.Time + effect.RemainingTime;
			
			GameEventMgr.AddHandler(target, GamePlayerEvent.Linkdeath, new DOLEventHandler(EventRaised));
			GameEventMgr.AddHandler(target, GamePlayerEvent.Quit, new DOLEventHandler(EventRaised));
			GameEventMgr.AddHandler(target, GamePlayerEvent.RegionChanged, new DOLEventHandler(EventRaised));

			base.OnEffectStart(effect);
		}

		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);
			
			// Trigger subspell set on amnesia chance to prevent the chaining subspell security and allow to change caster
			if (Spell.AmnesiaChance > 0)
			{
				Spell spell = SkillBase.GetSpellByID(Spell.AmnesiaChance);
				//we need subspell ID to be 0, we don't want spells linking off the subspell
				if (effect.Owner != null && spell != null && !spell.HasSubSpell)
				{
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(effect.Owner, spell, SpellLine);
					spellhandler.StartSpell(effect.Owner);
				}
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameLiving target = effect.Owner;

			if (m_ownerModel.ContainsKey(target))
			{
				ushort model;
				if (m_ownerModel.TryGetValue(target, out model))
				{
					target.Model = model;
				}
			}

			target.Health = 1;
			// Allow Attacking Again
			target.DisarmedTime = 0;
			target.SilencedTime = 0;

			GameEventMgr.RemoveHandler(target, GamePlayerEvent.Linkdeath, new DOLEventHandler(EventRaised));
			GameEventMgr.RemoveHandler(target, GamePlayerEvent.Quit, new DOLEventHandler(EventRaised));
			GameEventMgr.RemoveHandler(target, GamePlayerEvent.RegionChanged, new DOLEventHandler(EventRaised));

			if(effect.Owner.IsAlive && effect.Owner.ObjectState == GameObject.eObjectState.Active)
			{
				RezDmgImmunityEffect immunityEffect = new RezDmgImmunityEffect();
				immunityEffect.Start(effect.Owner);
			}
			
			return base.OnEffectExpires(effect, noMessages);
		}

		/// <summary>
		/// Cancel Effect if player change region or quit.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		public void EventRaised(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving target = sender as GameLiving;
			if (target == null) 
				return;

			GameSpellEffect effect = SpellHelper.FindEffectOnTarget(target, this);
			if (effect != null)
				effect.Cancel(false);
		}

		// Constructor
		public SummonMonsterHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}

	}
}
