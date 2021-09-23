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
using System.Collections.Generic;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("EnduranceDrain")]
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
			target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
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


		public override IList<string> DelveInfo 
		{
			get 
			{
				var list = new List<string>();
				list.Add("Name: " + Spell.Name);
				list.Add("Description: " + Spell.Description);
				list.Add("Target: " + Spell.Target);
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Drain Endurance By: " + Spell.Damage + "%");

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
								
				return list;
			}
		}

        public override string ShortDescription => $"{Spell.Damage}% endurance is stolen from the target and given to the caster.";
    }
}
