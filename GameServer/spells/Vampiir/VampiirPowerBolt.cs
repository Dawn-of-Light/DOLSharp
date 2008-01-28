using System;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirPowerBolt")]
	public class VampiirPowerBolt : SpellHandler //DirectDamageSpellHandler
	{
		public VampiirPowerBolt(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

        // cannot be casted in combat
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.InCombat == true)
            {
                MessageToCaster("You cannot cast this spell in combat!", eChatType.CT_SpellResisted);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }
				
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{

			if (target == null) return;
			if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;

            if (target.MaxMana > 0)
            {
                int power = (int) Spell.Value; //((target.Level * 5 * (int)(Spell.Value)) / 100)

                //if ( target is GameNPC)
                m_caster.Mana += power;
               
                if (target is GamePlayer)
                {
                    target.Mana -= power;
                    ((GamePlayer)target).Out.SendMessage(m_caster.Name + " takes " + power + " power!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                }
                
                if (target.Mana < 0) target.Mana = 0;

                if (m_caster is GamePlayer)
                {
                    ((GamePlayer)m_caster).Out.SendMessage("You receive " + power + " power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
            }
            else
                ((GamePlayer)m_caster).Out.SendMessage("You did not receive any power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			DamageTarget(ad, true);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

		public override int CalculateNeededPower(GameLiving target)
		{
			return 0;
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
				list.Add("Drain Endurance By: " + Spell.Value);

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
								
				return list;
			}
		}
		
		

	}
}























