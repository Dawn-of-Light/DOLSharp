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
using System.Collections.Generic;
using System.Collections.Generic;
using DOL.Events;
namespace DOL.GS.Effects
{
	/// <summary>
	/// The helper class for the guard ability
	/// </summary>
	public class MarkofPreyEffect : TimedEffect
	{
		private GamePlayer EffectOwner;
		private GamePlayer EffectCaster;
		private Group m_playerGroup;

		public MarkofPreyEffect()
			: base(RealmAbilities.MarkOfPreyAbility.DURATION)
		{ }

		/// <summary>
		/// Start guarding the player
		/// </summary>
		/// <param name="Caster"></param>
		/// <param name="CasterTarget"></param>
		public void Start(GamePlayer Caster, GamePlayer CasterTarget)
		{
			if (Caster == null || CasterTarget == null)
				return;

			m_playerGroup = Caster.Group;
			if (m_playerGroup != CasterTarget.Group)
				return;

			EffectCaster = Caster;
			EffectOwner = CasterTarget;
			foreach (GamePlayer p in EffectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				p.Out.SendSpellEffectAnimation(EffectCaster, EffectOwner, 7090, 0, false, 1);
			}
			GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			if (m_playerGroup != null)
				GameEventMgr.AddHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
			GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.AttackFinished, new DOLEventHandler(AttackFinished));
			EffectOwner.Out.SendMessage("Your weapon begins channeling the strength of the vampiir!", DOL.GS.PacketHandler.eChatType.CT_Spell, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
			base.Start(CasterTarget);
		}

		public override void Stop()
		{
			if (EffectOwner != null)
			{
				if (m_playerGroup != null)
					GameEventMgr.RemoveHandler(m_playerGroup, GroupEvent.MemberDisbanded, new DOLEventHandler(GroupDisbandCallback));
				GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.AttackFinished, new DOLEventHandler(AttackFinished));
				GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				m_playerGroup = null;
			}
			EffectOwner.Out.SendMessage("Your weapon returns to normal.", DOL.GS.PacketHandler.eChatType.CT_SpellExpires, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
			base.Stop();
		}

		/// <summary>
		/// Called when a player is inflicted in an combat action
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		private void AttackFinished(DOLEvent e, object sender, EventArgs args)
		{
			AttackFinishedEventArgs atkArgs = args as AttackFinishedEventArgs;
			if (atkArgs == null) return;
			if (atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
				&& atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle) return;
			if (atkArgs.AttackData.Target == null) return;
			GameLiving target = atkArgs.AttackData.Target;
			if (target == null) return;
			if (target.ObjectState != GameObject.eObjectState.Active) return;
			if (target.IsAlive == false) return;
			GameLiving attacker = sender as GameLiving;
			if (attacker == null) return;
			if (attacker.ObjectState != GameObject.eObjectState.Active) return;
			if (attacker.IsAlive == false) return;
			double dpsCap;
			dpsCap = (1.2 + 0.3 * attacker.Level) * 0.7;

			double dps = Math.Min(RealmAbilities.MarkOfPreyAbility.VALUE, dpsCap);
			double damage = dps * atkArgs.AttackData.WeaponSpeed * 0.1;
			double damageResisted = damage * target.GetResist(eDamageType.Heat) * -0.01;

			AttackData ad = new AttackData();
			ad.Attacker = attacker;
			ad.Target = target;
			ad.Damage = (int)(damage + damageResisted);
			ad.Modifier = (int)damageResisted;
			ad.DamageType = eDamageType.Heat;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
			target.OnAttackedByEnemy(ad);
			EffectCaster.ChangeMana(EffectOwner, GameLiving.eManaChangeType.Spell, (int)ad.Damage);
			if (attacker is GamePlayer)
				(attacker as GamePlayer).Out.SendMessage(string.Format("You hit {0} for {1} extra damage!", target.Name, ad.Damage), DOL.GS.PacketHandler.eChatType.CT_Spell, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
			attacker.DealDamage(ad);
		}

		/// <summary>
		/// Cancels effect if one of players disbands
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">The group</param>
		/// <param name="args"></param>
		protected void GroupDisbandCallback(DOLEvent e, object sender, EventArgs args)
		{
			MemberDisbandedEventArgs eArgs = args as MemberDisbandedEventArgs;
			if (eArgs == null) return;
			if (eArgs.Member == EffectOwner)
			{
				Cancel(false);
			}
		}
		/// <summary>
		/// Called when a player leaves the game
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		protected void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			Cancel(false);
		}

		public override string Name { get { return "Mark Of Prey"; } }
		public override ushort Icon { get { return 3089; } }

		// Delve Info
		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add("Grants a 30 second damage add that stacks with all other forms of damage add. All damage done via the damage add will be returned to the Vampiir as power.");
				return list;
			}
		}
	}
}
