using System;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("EnduranceDrain")]
	public class EnduranceDrainSpellHandler : SpellHandler
	{
		public EnduranceDrainSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;

			int end = (int)(Spell.Damage);
 			target.ChangeEndurance(target,GameLiving.eEnduranceChangeType.Spell, (-end));

			if (target is GamePlayer)
			{
				((GamePlayer)target).Out.SendMessage(m_caster.Name + " steal you for " + end + " endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
			}

			StealEndurance(target,end);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}

		
		public virtual void StealEndurance(GameLiving target,int end)
		{
			if(!m_caster.IsAlive) return;
			m_caster.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, end);
			SendCasterMessage(target,end);
			
		}

		
		public virtual void SendCasterMessage(GameLiving target,int end)
		{
			MessageToCaster(string.Format("You steal {0} for {1} endurance!", target.Name, end), eChatType.CT_YouHit);
			if(end > 0) 
			{
				MessageToCaster("You steal " + end + " endurance point" + (end==1?".":"s."), eChatType.CT_Spell);
			}
			else 
			{
				MessageToCaster("You cannot absorb any more endurance.", eChatType.CT_SpellResisted);
			}
		}


		public override IList DelveInfo 
		{
			get 
			{
				ArrayList list = new ArrayList();
				//Name
				list.Add("Name: " + Spell.Name);
				//Description
				list.Add("Description: " + Spell.Description);
				//Target
				list.Add("Target: " + Spell.Target);
				//Cast
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				//Duration
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				//Recast
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				//Range
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				//Radius
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				//Cost
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				//Effect
				list.Add("Drain Endurance By: " + Spell.Damage + "%");

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
								
				return list;
			}
		}
	}
}























