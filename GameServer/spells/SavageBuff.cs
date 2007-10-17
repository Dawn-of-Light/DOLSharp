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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	
    // Main class for savage buffs
	public abstract class AbstractSavageBuff : PropertyChangingSpell
	{
		public override int BonusCategory1 { get { return 1; } }
	
		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			BlissfulIgnoranceEffect BlissfulIgnorance = (BlissfulIgnoranceEffect)m_caster.EffectList.GetOfType(typeof(BlissfulIgnoranceEffect));
			if (BlissfulIgnorance == null)
			{
				int cost = 0;
				if(m_spell.Power<0)
					cost = (int)(m_caster.MaxHealth * Math.Abs(m_spell.Power) * 0.01);
				else
					cost = m_spell.Power;
				
				effect.Owner.ChangeHealth(effect.Owner, GameLiving.eHealthChangeType.Spell, -cost);
			}
			SendUpdates(effect.Owner);
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			SendUpdates(effect.Owner);
			return 0;
		}

		// constructor
		public AbstractSavageBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}		
	}
	
	public abstract class AbstractSavageStatBuff : AbstractSavageBuff
	{
		/// <summary>
        /// Sends needed updates on start/stop
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
			}
		}
		// constructor
		public AbstractSavageStatBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}				
	}
	public abstract class AbstractSavageResistBuff : AbstractSavageBuff
	{
		/// <summary>
        /// Sends needed updates on start/stop
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;
			if (player != null)
			{
				player.Out.SendCharResistsUpdate();
				player.UpdatePlayerStatus();
			}
		}
		// constructor
		public AbstractSavageResistBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}				
	}
	
	[SpellHandlerAttribute("SavageParryBuff")]
	public class SavageParryBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ParryChance; } }

		// constructor
		public SavageParryBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageEvadeBuff")]
	public class SavageEvadeBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.EvadeChance; } }

		// constructor
		public SavageEvadeBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageCombatSpeedBuff")]
	public class SavageCombatSpeedBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeSpeed; } }

		// constructor
		public SavageCombatSpeedBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageDPSBuff")]
	public class SavageDPSBuff : AbstractSavageStatBuff
	{
		public override eProperty Property1 { get { return eProperty.MeleeDamage; } }

		// constructor
		public SavageDPSBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}	
	[SpellHandlerAttribute("SavageSlashResistanceBuff")]
	public class SavageSlashResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Slash; } }

		// constructor
		public SavageSlashResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageThrustResistanceBuff")]
	public class SavageThrustResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Thrust; } }

		// constructor
		public SavageThrustResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
	[SpellHandlerAttribute("SavageCrushResistanceBuff")]
	public class SavageCrushResistanceBuff : AbstractSavageResistBuff
	{
		public override eProperty Property1 { get { return eProperty.Resist_Crush; } }

		// constructor
		public SavageCrushResistanceBuff(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}


