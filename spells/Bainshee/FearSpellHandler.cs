using System;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;
using System.Collections;
using log4net;
using System.Reflection;
namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Fear")]
	public class FearSpellHandler : SpellHandler 
	{
		//VaNaTiC->
		/*
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		*/
		//VaNaTiC<-

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast (target);
			
			GameNPC t = target as GameNPC;
			if(t!=null)
				t.WalkToSpawn();
		}

		public override IList SelectTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList();
			GameLiving target;
			
			target=Caster;
			foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius)) 
			{
				if(npc is GameNPC)
					list.Add(npc);
			}

			return list;
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void StartSpell(GameLiving target)
		{
			if (target == null) return;

			IList targets = SelectTargets(target);

			foreach (GameLiving t in targets)
			{
				if(t is GameNPC && t.Level <= m_spell.Value)
				{
					((GameNPC)t).AddBrain(new FearBrain());
				}
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameNPC mob = (GameNPC)effect.Owner;
			mob.RemoveBrain(mob.Brain);

			if(mob.Brain==null)
				mob.AddBrain(new StandardMobBrain());

			return base.OnEffectExpires (effect, noMessages);
		}


		public FearSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
