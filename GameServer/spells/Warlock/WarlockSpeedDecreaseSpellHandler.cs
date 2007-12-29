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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for speed decreasing spells
	/// </summary>
	[SpellHandler("WarlockSpeedDecrease")]
	public class WarlockSpeedDecreaseSpellHandler : SpeedDecreaseSpellHandler 
	{

		private ushort m_playerModel;
		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			if(effect.Owner is GamePlayer)
			{
				m_playerModel = effect.Owner.Model;
				if(effect.Owner.Realm == eRealm.Albion)
					effect.Owner.Model = 581;
				else if(effect.Owner.Realm == eRealm.Midgard)
					effect.Owner.Model = 574;
				else if(effect.Owner.Realm == eRealm.Hibernia)
					effect.Owner.Model = 594;

				SendEffectAnimation(effect.Owner, 12126, 0, false, 1); 
				//GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(OnAttacked));	
				//GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnAttacked));	
				//GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.Quit, new DOLEventHandler(OnAttacked));	
			}
			//GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			//GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			if(effect.Owner is GamePlayer)
			{
				effect.Owner.Model = m_playerModel;
				//GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(OnAttacked));	
				//GameEventMgr.RemoveHandler(effect.Owner, GamePlayerEvent.Linkdeath, new DOLEventHandler(OnAttacked));	
				//GameEventMgr.RemoveHandler(effect.Owner, GamePlayerEvent.Quit, new DOLEventHandler(OnAttacked));	
			}
			return base.OnEffectExpires(effect, noMessages);
		}

		// constructor
		public WarlockSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}
	}
}
