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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Abstract CC spell handler
	/// </summary>
	public abstract class AbstractCCSpellHandler : ImmunityEffectSpellHandler
	{
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.HasAbility(Abilities.CCImmunity))
			{
				MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
				return;
			} 
			if (target.EffectList.GetOfType(typeof(ChargeEffect)) != null || target.TempProperties.getProperty("Charging", false))
			{
				MessageToCaster(target.Name + " is moving to fast for this spell to have any effect!", eChatType.CT_SpellResisted);
				return;
			}

			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

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

		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			double mocFactor = 1.0;
			Effects.MasteryofConcentrationEffect moc = (Effects.MasteryofConcentrationEffect)Caster.EffectList.GetOfType(typeof(Effects.MasteryofConcentrationEffect));
			if (moc != null)
			{
				RealmAbility ra = Caster.GetAbility(typeof(MasteryofConcentrationAbility)) as RealmAbility;
				if (ra != null)
					mocFactor = System.Math.Round((double)ra.Level * 25 / 100, 2);
				duration = (double)Math.Round(duration * mocFactor);
			}
			return (int)duration;
		}

        public override int CalculateSpellResistChance(GameLiving target)
        {
            int resistvalue = 0;
            int resist = 0;
            GameSpellEffect fury = SpellHandler.FindEffectOnTarget(target, "Fury");
            if (fury != null)
            {
            	resist += (int)fury.Spell.Value;
            }
            if (target.EffectList.GetOfType(typeof(AllureofDeathEffect)) != null)
            {
                resist += 75;
            }
            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
                return 0;
            if (HasPositiveEffect)
                return 0;
            //Calculate the Resistchance
            resistvalue = (100 - CalculateToHitChance(target) + resist);
            if (resistvalue > 100)
                resistvalue = 100;
            //always 1% resistchance!
            if (resistvalue < 1)
                resistvalue = 1;
            return resistvalue;
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
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
			effect.Owner.IsMezzed = true;
			effect.Owner.StopAttack();
			effect.Owner.StopCurrentSpellcast();
			effect.Owner.DisableTurning(true);
			GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
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
			GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
			effect.Owner.IsMezzed = false;
			effect.Owner.DisableTurning(false);
			return base.OnEffectExpires(effect,noMessages);
		}
		
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
			if (FindStaticEffectOnTarget(target, typeof(MezzRootImmunityEffect)) != null)
			{
				MessageToCaster("Your target is immune!", eChatType.CT_System);
				return;
			}
            GameSpellEffect mezblock = SpellHandler.FindEffectOnTarget(target, "CeremonialBracerMez");
            if (mezblock != null)
            {
                mezblock.Cancel(false);
                if (target is GamePlayer)
                    (target as GamePlayer).Out.SendMessage("Your item effect intercepts the mesmerization spell and fades!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
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

			bool remove = false;

			if (attackArgs.AttackData.SpellHandler == null)
			{
				switch (attackArgs.AttackData.AttackResult)
				{
					case GameLiving.eAttackResult.HitStyle:
					case GameLiving.eAttackResult.HitUnstyled:
					case GameLiving.eAttackResult.Blocked:
					case GameLiving.eAttackResult.Evaded:
					case GameLiving.eAttackResult.Fumbled:
					case GameLiving.eAttackResult.Missed:
					case GameLiving.eAttackResult.Parried:
						remove = true;
						break;
				}
			}
			else
			{
				//temporary fix for DirectDamageDebuff not breaking mez
				if (attackArgs.AttackData.SpellHandler is PropertyChangingSpell && attackArgs.AttackData.SpellHandler.HasPositiveEffect == false && attackArgs.AttackData.Damage > 0)
					remove = true;
				//debuffs/shears dont interrupt mez, neither does recasting mez
				else if (attackArgs.AttackData.SpellHandler is PropertyChangingSpell || attackArgs.AttackData.SpellHandler is MesmerizeSpellHandler
					|| attackArgs.AttackData.SpellHandler is NearsightSpellHandler || attackArgs.AttackData.SpellHandler.HasPositiveEffect) return;

				if (attackArgs.AttackData.AttackResult == GameLiving.eAttackResult.Missed || attackArgs.AttackData.AttackResult == GameLiving.eAttackResult.HitUnstyled)
					remove = true;
			}

			if (remove)
			{
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
				if (effect != null)
					effect.Cancel(false);//call OnEffectExpires
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
			effect.Owner.IsStunned=true;
			effect.Owner.StopAttack();
			effect.Owner.StopCurrentSpellcast();
			effect.Owner.DisableTurning(true);
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
			effect.Owner.IsStunned=false;
			effect.Owner.DisableTurning(false);
			return base.OnEffectExpires(effect,noMessages);
		}

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect stunblock = SpellHandler.FindEffectOnTarget(target, "CeremonialBracerStun");
            if (stunblock != null)
            {
                stunblock.Cancel(false);
                if (target is GamePlayer)
                    (target as GamePlayer).Out.SendMessage("Your item effect intercepts the stun spell and fades!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
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
