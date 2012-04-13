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
using log4net;

namespace DOL.GS.Spells
{
	#region DirectDamage
	[SpellHandlerAttribute("WarlockDirectDamage")]
	public class WarlockDirectDamageSpellHandler : DirectDamageSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockDirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion DirectDamage

	#region DamageOverTime
	[SpellHandlerAttribute("WarlockDamageOverTime")]
	public class WarlockDamageOverTimeSpellHandler : DoTSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.CalculateCastingTime();
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockDamageOverTimeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion DamageOverTime

	#region Heal
	[SpellHandlerAttribute("WarlockHeal")]
	public class WarlockHealSpellHandler : HealSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockHealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion Heal

	#region Heal
	[SpellHandlerAttribute("WarlockHealBolt")]
	public class WarlockHealBoltSpellHandler : HealSpellHandler
	{
		public override bool StartSpell(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			foreach (GameLiving targ in SelectTargets(target))
				DealDamage(targ);
			return true;
		}

		private void DealDamage(GameLiving target)
		{
			int ticksToTarget = m_caster.GetDistanceTo(target) * 90 / 125; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
			bolt.Start(1 + ticksToTarget);
		}

		protected class BoltOnTargetAction : RegionAction
		{
			protected readonly GameLiving m_boltTarget;
			protected readonly WarlockHealBoltSpellHandler m_handler;

			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, WarlockHealBoltSpellHandler spellHandler)
				: base(actionSource)
			{
				if (boltTarget == null) throw new ArgumentNullException("boltTarget");
				if (spellHandler == null) throw new ArgumentNullException("spellHandler");
				m_boltTarget = boltTarget;
				m_handler = spellHandler;
			}

			protected override void OnTick()
			{
				GameLiving healTarget = m_boltTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (healTarget == null || m_handler == null) return;
				if (healTarget.CurrentRegionID != caster.CurrentRegionID) return;
				if (healTarget.ObjectState != GameObject.eObjectState.Active) return;
				if (!healTarget.IsAlive) return;

				bool healed = false;
				int minHeal;
				int maxHeal;
				m_handler.CalculateHealVariance(out minHeal, out maxHeal);

				int heal = Util.Random(minHeal, maxHeal);
				if (m_handler.SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
					heal = maxHeal;
				if (healTarget.IsDiseased)
				{
					m_handler.MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
					heal >>= 1;
				}
				healed |= m_handler.HealTarget(healTarget, heal);

				if (m_handler.Spell.Pulse == 0 && healTarget.IsAlive)
					m_handler.SendEffectAnimation(healTarget, 0, false, healed ? (byte)1 : (byte)0);

				if (healed && caster is GamePlayer)
					caster.TempProperties.setProperty("last-heal-action", caster.CurrentRegion.Time);
			}
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockHealBoltSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion Heal

	#region SpreadHeal
	[SpellHandlerAttribute("WarlockSpreadHeal")]
	public class WarlockSpreadHealSpellHandler : SpreadhealSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockSpreadHealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion SpreadHeal

	#region Root
	[SpellHandlerAttribute("WarlockRoot")]
	public class WarlockRootSpellHandler : SpeedDecreaseSpellHandler
	{
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			DealDamage(target, effectiveness);
		}

		public void ApplyEffect(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		private void DealDamage(GameLiving target, double effectiveness)
		{
			int ticksToTarget = m_caster.GetDistanceTo(target) * 90 / 125; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, effectiveness, this);
			bolt.Start(1 + ticksToTarget);
		}

		protected class BoltOnTargetAction : RegionAction
		{
			protected readonly GameLiving m_boltTarget;
			protected readonly WarlockRootSpellHandler m_handler;
			protected readonly double m_effectiveness;

			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, double effectiveness, WarlockRootSpellHandler spellHandler)
				: base(actionSource)
			{
				if (boltTarget == null) throw new ArgumentNullException("boltTarget");
				if (spellHandler == null) throw new ArgumentNullException("spellHandler");
				m_boltTarget = boltTarget;
				m_handler = spellHandler;
				m_effectiveness = effectiveness;
			}

			protected override void OnTick()
			{
				GameLiving target = m_boltTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (target == null || m_handler == null) return;
				if (target.CurrentRegionID != caster.CurrentRegionID) return;
				if (target.ObjectState != GameObject.eObjectState.Active) return;
				if (!target.IsAlive) return;

				m_handler.ApplyEffect(target, m_effectiveness);
			}
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockRootSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion Root

	#region Lifedrain
	[SpellHandlerAttribute("WarlockLifedrain")]
	public class WarlockLifedrainSpellHandler : LifedrainSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.CalculateCastingTime();
		}

		public WarlockLifedrainSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion Lifedrain

	#region LifedrainPri
	[SpellHandlerAttribute("WarlockLifedrainPri")]
	public class WarlockLifedrainPriSpellHandler : LifedrainSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockLifedrainPriSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion LifedrainPri

	#region Bolt
	[SpellHandlerAttribute("WarlockBolt")]
	public class WarlockBoltSpellHandler : BoltSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckPrimary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			WarlockMgr.SetPrimary(this, Caster);
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return Spell.CastTime;
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockBoltSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion Bolt

	#region PBAE
	[SpellHandlerAttribute("WarlockPBAE")]
	public class WarlockPBAESpellHandler : DirectDamageSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.CalculateCastingTime();
		}

		public WarlockPBAESpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion PBAE

	#region PowerRegenBuff
	[SpellHandlerAttribute("WarlockPowerRegenBuff")]
	public class WarlockPowerRegenBuffSpellHandler : PowerRegenSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.CalculateCastingTime();
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public WarlockPowerRegenBuffSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion PowerRegenBuff

	#region WarlockSpeedDecrease
	[SpellHandler("WarlockSpeedDecrease")]
	public class WarlockSpeedDecreaseSpellHandler : SpeedDecreaseSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget)) return false;
			return WarlockMgr.CheckSecondary(this, Caster);
		}

		public override bool CheckEndCast(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return true;
			return base.CheckEndCast(target);
		}

		public override int PowerCost(GameLiving target)
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.PowerCost(target);
		}

		public override int CalculateCastingTime()
		{
			if (WarlockMgr.CheckChamber(this, Caster)) return 0;
			return base.CalculateCastingTime();
		}

		private ushort m_playerModel;

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			if (effect.Owner is GamePlayer)
			{
				m_playerModel = effect.Owner.Model;
				if (effect.Owner.Realm == eRealm.Albion)
					effect.Owner.Model = 581;
				else if (effect.Owner.Realm == eRealm.Midgard)
					effect.Owner.Model = 574;
				else if (effect.Owner.Realm == eRealm.Hibernia)
					effect.Owner.Model = 594;

				SendEffectAnimation(effect.Owner, 12126, 0, false, 1);
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GamePlayer)
				effect.Owner.Model = m_playerModel;
			return base.OnEffectExpires(effect, noMessages);
		}

		public WarlockSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion WarlockSpeedDecrease


	#region WarlockMgr
	public class WarlockMgr
	{
		public static void SetPrimary(SpellHandler handler, GameLiving living)
		{
			if (living == null) return;
			GamePlayer player = living as GamePlayer;
			if (player == null) return;
			if (handler.Spell.InChamber) return;
			player.TempProperties.setProperty("Warlock-CastingPrimary", false);
			player.TempProperties.setProperty("Warlock-EndPrimary", handler.Caster.CurrentRegion.Time);
		}
		public static bool CheckPrimary(SpellHandler handler, GameLiving living)
		{
			if (living == null) return false;
			GamePlayer player = living as GamePlayer;
			if (player == null) return false;
			if (handler.Spell.InChamber) return true;
			bool powerless = handler.Caster.TempProperties.getProperty("Warlock-Powerless", false);
			bool range = handler.Caster.TempProperties.getProperty("Warlock-Range", false);
			bool uninterruptable = handler.Caster.TempProperties.getProperty("Warlock-Uninterruptable", false);

			if (powerless || range || uninterruptable)
			{
				(handler.Caster as GamePlayer).Out.SendMessage("You cannot cast this spell as a followup!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return false;
			}

			handler.Caster.TempProperties.setProperty("Warlock-CastingPrimary", true);
			handler.Caster.TempProperties.setProperty("Warlock-Primary", true);
			handler.Caster.TempProperties.setProperty("Warlock-PrimaryRange", handler.Spell.Range);
			return true;
		}
		public static bool CheckSecondary(SpellHandler handler, GameLiving living)
		{
			if (living == null) return false;
			GamePlayer player = living as GamePlayer;
			if (player == null) return false;
			bool powerless = player.TempProperties.getProperty("Warlock-Powerless", false);
			bool range = player.TempProperties.getProperty("Warlock-Range", false);
			bool uninterruptable = player.TempProperties.getProperty("Warlock-Uninterruptable", false);
			bool primary = player.TempProperties.getProperty("Warlock-Primary", false) || player.TempProperties.getProperty("Warlock-CastingPrimary", false);
			long endprimary = player.TempProperties.getProperty<long>("Warlock-EndPrimary", 0L);
			if (powerless || range || uninterruptable || (primary && endprimary + 1000 > player.CurrentRegion.Time))
			{
				CancelPrimary(handler, player);
				return true;
			}
			else if (!handler.Spell.InChamber && handler.Spell.SpellType != "Chamber")
			{
				player.Out.SendMessage("You cannot cast this spell directly!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
		public static bool CheckChamber(SpellHandler handler, GameLiving living)
		{
			if (living == null) return false;
			GamePlayer player = living as GamePlayer;
			if (player == null) return false;
			string castingChamber = player.TempProperties.getProperty<string>("Warlock-CastingChamber", "");
			return (castingChamber != "");
		}
		public static void CancelPrimary(SpellHandler handler, GameLiving living)
		{
			if (living == null) return;
			GamePlayer player = living as GamePlayer;
			if (player == null) return;
			if (handler.Spell.InChamber) return;
			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(player, "Range");
			if (effect == null)
				effect = SpellHandler.FindEffectOnTarget(player, "Powerless");
			if (effect == null)
				effect = SpellHandler.FindEffectOnTarget(player, "Uninterruptable");
			if (effect != null)
				effect.Cancel(false);
			player.TempProperties.setProperty("Warlock-Primary", false);
			player.TempProperties.setProperty("Warlock-CastingPrimary", false);
		}
	}
	#endregion WarlockMgr
}
