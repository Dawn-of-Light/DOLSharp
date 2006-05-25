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
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Abstract CC spell handler
	/// </summary>
	public abstract class AbstractCCSpellHandler : ImmunityEffectSpellHandler
	{
		public override void OnEffectStart(GameSpellEffect effect)
		{
			SendEffectAnimation(effect.Owner, 0, false, 1);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			MessageToCaster(Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner, m_caster);

			GamePlayer player = effect.Owner as GamePlayer;
			if(player != null) 
			{
				player.Client.Out.SendUpdateMaxSpeed();
				if(player.PlayerGroup != null)
					player.PlayerGroup.UpdateMember(player, false, false);
			}
			else
			{
				effect.Owner.StopAttack();
			}
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
			if (effect.Owner == null) return 0;

			base.OnEffectExpires(effect, noMessages);
			
			GamePlayer player = effect.Owner as GamePlayer;

			if(player != null) 
			{
				player.Client.Out.SendUpdateMaxSpeed();
				if( player.PlayerGroup != null) 
					player.PlayerGroup.UpdateMember(player, false, false);
			}
			else
			{
				GameNPC npc = effect.Owner as GameNPC;
				if (npc != null)
				{
					IAggressiveBrain aggroBrain = npc.Brain as IAggressiveBrain;
					if (aggroBrain != null)
						aggroBrain.AddToAggroList(Caster, 1);
				}
			}
			return 60000;
		}

		// constructor
		public AbstractCCSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	/// <summary>
	/// Mezz 
	/// </summary>
	[SpellHandlerAttribute("Mesmerize")]
	public class MesmerizeSpellHandler : AbstractCCSpellHandler
	{
		public override void OnEffectStart(GameSpellEffect effect)
		{			
			effect.Owner.Mez = true;
			effect.Owner.StopCurrentSpellcast();
			GameEventMgr.AddHandler(effect.Owner, GameLivingBaseEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			base.OnEffectStart(effect);
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
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingBaseEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			effect.Owner.Mez = false;
			return base.OnEffectExpires(effect,noMessages);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration *= target.GetModified(eProperty.MesmerizeDuration) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
			GameLiving living = sender as GameLiving;
			if (attackArgs == null) return;
			if (living == null) return;

			switch (attackArgs.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
					if (effect != null)
						effect.Cancel(false);//call OnEffectExpires
					break;
			}
		}

		// constructor
		public MesmerizeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	/// <summary>
	/// Stun 
	/// </summary>
	[SpellHandlerAttribute("Stun")]
	public class StunSpellHandler : AbstractCCSpellHandler
	{
		public override void OnEffectStart(GameSpellEffect effect)
		{			
			effect.Owner.Stun=true;
			effect.Owner.StopAttack();
			effect.Owner.StopCurrentSpellcast();
			base.OnEffectStart(effect);
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
			effect.Owner.Stun=false;
			return base.OnEffectExpires(effect,noMessages);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration *= target.GetModified(eProperty.StunDuration) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable cannot stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (compare.Spell.SpellType == "StyleStun") return true;
			return base.IsOverwritable(compare);
		}

		// constructor
		public StunSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
