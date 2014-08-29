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
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("SavageParryBuff")]
	public class SavageParryBuff : ParryChanceBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageParryBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageEvadeBuff")]
	public class SavageEvadeBuff : EvadeChanceBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageEvadeBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageCombatSpeedBuff")]
	public class SavageCombatSpeedBuff : CombatSpeedBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageCombatSpeedBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageDPSBuff")]
	public class SavageDPSBuff : DPSBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageDPSBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageSlashResistanceBuff")]
	public class SavageSlashResistanceBuff : SlashResistBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageSlashResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageThrustResistanceBuff")]
	public class SavageThrustResistanceBuff : ThrustResistBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageThrustResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
	
	[SpellHandlerAttribute("SavageCrushResistanceBuff")]
	public class SavageCrushResistanceBuff : CrushResistBuff
	{
		/// <summary>
		/// Health consume Helper
		/// </summary>
		private HealthConsumeHelper m_healthConsumeHelper;
		
		/// <summary>
		/// Specific Delve for Health Cost
		/// </summary>
		public override IList<string> DelveInfo { get { return m_healthConsumeHelper.DelveInfo; } }
		
		/// <summary>
		/// Don't Use Power
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}
		
		/// <summary>
		/// Consume Health.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns></returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			m_healthConsumeHelper.ConsumeHealth(effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}
		
		// constructor
		public SavageCrushResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
			m_healthConsumeHelper = new HealthConsumeHelper(this);
		}
	}
}


