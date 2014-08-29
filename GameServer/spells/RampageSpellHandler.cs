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
using DOL.GS.PacketHandler;
using DOL.Events;

using log4net;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Rampage")]
    public class RampageBuffHandler : SpellHandler
    {
    	
    	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    	
		public override void OnEffectStart(DOL.GS.Effects.GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			
			if(effect.Owner != null)
				GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
		}
		
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if(effect.Owner != null)
				GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
					
			return base.OnEffectExpires(effect, noMessages);
		}
    	
		protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs args)
		{
			MessageToCaster("Rampage debug : receive attack", eChatType.CT_Spell);				
			if(sender == null || !(args is AttackedByEnemyEventArgs))
				return;
			
			AttackedByEnemyEventArgs attackArgs = (AttackedByEnemyEventArgs)args;
			
			if(attackArgs.AttackData != null)
				log.InfoFormat("Received Attack : {0}", attackArgs.AttackData);
			
			MessageToCaster("Rampage debug : test if it's a not resisted spell", eChatType.CT_Spell);				
			if(attackArgs.AttackData == null || attackArgs.AttackData.AttackType != AttackData.eAttackType.Spell || attackArgs.AttackData.IsSpellResisted)
				return;
			
			MessageToCaster("Rampage debug : test if it's a debuff", eChatType.CT_Spell);				
			if(attackArgs.AttackData.SpellHandler != null && attackArgs.AttackData.SpellHandler is SingleStatDebuff)
			{
				attackArgs.AttackData.IsSpellResisted = true;
				MessageToCaster("The debuff was deflected by your Rampage Ability !", eChatType.CT_Spell);
			}
		}
		
        public RampageBuffHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
}
