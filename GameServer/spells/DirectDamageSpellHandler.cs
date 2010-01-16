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
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.Keeps;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("DirectDamage")]
	public class DirectDamageSpellHandler : SpellHandler
	{
		private bool m_castFailed = false;

		/// <summary>
		/// Execute direct damage spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			if (!m_castFailed)
			{
				m_caster.Mana -= PowerCost(target);
			}

			base.FinishSpellCast(target);
		}

		private const string LOSEFFECTIVENESS = "LOS Effectivness";

		/// <summary>
		/// execute direct effect
		/// </summary>
		/// <param name="target">target that gets the damage</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;

			bool spellOK = true;

			if (Spell.Target.ToLower() == "cone" ||	(Spell.Target == "Enemy" && Spell.Radius > 0 && Spell.Range == 0))
				spellOK = false;

			if (!spellOK || CheckLOS(Caster))
			{
				GamePlayer player = null;
				if (target is GamePlayer)
				{
					player = target as GamePlayer;
				}
				else
				{
					if (Caster is GamePlayer)
					{
						player = Caster as GamePlayer;
					}
					else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
					{
						IControlledBrain brain = (Caster as GameNPC).Brain as IControlledBrain;
						player = brain.GetPlayerOwner();
					}
				}
				if (player != null)
				{
					player.TempProperties.setProperty(LOSEFFECTIVENESS, effectiveness);
					player.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(DealDamageCheckLOS));
				}
				else
				{
					DealDamage(target, effectiveness);
				}
			}
			else
			{
				DealDamage(target, effectiveness);
			}
		}

		private bool CheckLOS(GameLiving living)
		{
			foreach (AbstractArea area in living.CurrentAreas)
			{
				if (area.CheckLOS)
					return true;
			}
			return false;
		}

		private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if (player == null || Caster.ObjectState != GameObject.eObjectState.Active)
				return;

			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
					{
						double effectiveness = (double)player.TempProperties.getProperty<object>(LOSEFFECTIVENESS, null);
						DealDamage(target, effectiveness);

						// Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
						GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
					}
				}
				catch (Exception e)
				{
					m_castFailed = true;

					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
			else
			{
				if (Spell.Target.ToLower() == "enemy" && Spell.Radius == 0 && Spell.Range != 0)
				{
					m_castFailed = true;
					MessageToCaster("You can't see your target!", eChatType.CT_SpellResisted);
				}
			}
		}

		private void DealDamage(GameLiving target, double effectiveness)
		{
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			DamageTarget(ad, true);
			SendDamageMessages(ad);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
		}


		/*
		 * We need to send resist spell los check packets because spell resist is calculated first, and
		 * so you could be inside keep and resist the spell and be interupted when not in view
		 */
		protected override void OnSpellResisted(GameLiving target)
		{
			if (target is GamePlayer /*&& Caster.TempProperties.getProperty("player_in_keep_property", false)*/)
			{
				GamePlayer player = target as GamePlayer;
				player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(ResistSpellCheckLOS));
			}
			else SpellResisted(target);
		}

		private void ResistSpellCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				try
				{
					GameLiving target = Caster.CurrentRegion.GetObject(targetOID) as GameLiving;
					if (target != null)
						SpellResisted(target);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
				}
			}
		}

		private void SpellResisted(GameLiving target)
		{
			base.OnSpellResisted(target);
		}

		// constructor
		public DirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
