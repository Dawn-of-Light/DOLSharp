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
using System.Collections.Generic;

using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Pve Resurrection Illness
	/// </summary>
	[SpellHandler(GlobalSpells.PvERessurectionIllnessSpellType)]
	public class PveResurrectionIllnessHandler : AbstractIllnessSpellHandler
	{
		public PveResurrectionIllnessHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}
	
	}

	/// <summary>
	/// Pve Resurrection Illness
	/// </summary>
	[SpellHandler(GlobalSpells.RvRRessurectionIllnessSpellType)]
	public class RvrResurrectionIllnessHandler : AbstractIllnessSpellHandler
	{
		public RvrResurrectionIllnessHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}		
	}
	
	/// <summary>
	/// Contains all common code for illness spell handlers (and negative spell effects without animation) 
	/// </summary>
	public class AbstractIllnessSpellHandler : SingleStatDebuff
	{
		public override eProperty Property1 { get { return eProperty.LivingEffectiveness; } }

		/// <summary>
		/// Illness Reduction in Percent (only used before startSpell)
		/// </summary>
		private byte m_IllnessEffectiveness = 100;
		
		/// <summary>
		/// Illness Reduction in Percent (only used before startSpell)
		/// </summary>
		public byte IllnessEffectiveness
		{
			get { return m_IllnessEffectiveness; }
			set { m_IllnessEffectiveness = value; }
		}
		
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if(IllnessEffectiveness <= 0)
				return;
			
			base.ApplyEffectOnTarget(target, effectiveness);
		}
		
		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		public override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return (int)Math.Max(1, (Spell.Duration * IllnessEffectiveness * 0.01));
		}
		
		/// <summary>
		/// Spell Effect effectiveness is fixed.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, 1);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Rusurrection Illness>
 
				The player's effectiveness is greatly reduced due to being recently resurrected.
 
				- Effectiveness penality: 50%
				- 4:56 remaining time
 
				<End Info>
				*/
				var list = new List<string>();

				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("- Effectiveness penality: "+Spell.Value+"%");
				return list;
			}
		}

		/// <summary>
		/// No Effect Animation for Illness.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="clientEffect"></param>
		/// <param name="boltDuration"></param>
		/// <param name="noSound"></param>
		/// <param name="success"></param>
		public override void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
		{
		}
		
		public AbstractIllnessSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}
	
	}
}
