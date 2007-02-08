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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damages the target and lowers their resistance to the spell's type.
	/// </summary>
	[SpellHandler("DirectDamageWithDebuff")]
	public class DirectDamageDebuffSpellHandler : AbstractResistDebuff
	{
		public override eProperty Property1 { get { return Caster.GetResistTypeForDamage(Spell.DamageType); } }
		public override string DebuffTypeName { get { return GlobalConstants.DamageTypeToName(Spell.DamageType); } }

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
			{
				MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
				return;
			}
			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			SendDamageMessages(ad);
			DamageTarget(ad, true);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);

			if (target.IsAlive)
				base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				/*
				<Begin Info: Lesser Raven Bolt>
				Function: dmg w/resist decrease
 
				Damages the target, and lowers the target's resistance to that spell type.
 
				Damage: 32
				Resist decrease (Cold): 10%
				Target: Targetted
				Range: 1500
				Duration: 1:0 min
				Power cost: 5
				Casting time:      3.0 sec
				Damage: Cold
 
				<End Info>
				*/

				ArrayList list = new ArrayList();

				list.Add("Function: dmg w/resist decrease");
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if(Spell.Damage != 0) list.Add("Damage: " + Spell.Damage.ToString("0.###;0.###'%'"));
				if(Spell.Value != 0) list.Add(String.Format("Resist decrease ({0}): {1}%", DebuffTypeName, Spell.Value));
				list.Add("Target: " + Spell.Target);
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Duration >= ushort.MaxValue*1000) list.Add("Duration: Permanent.");
				else if(Spell.Duration > 60000) list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if(Spell.Duration != 0) list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if(Spell.Frequency != 0) list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if(Spell.RecastDelay > 60000) list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if(Spell.RecastDelay > 0) list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if(Spell.Concentration != 0) list.Add("Concentration cost: " + Spell.Concentration);
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.DamageType != eDamageType.Natural) list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}

		// constructor
		public DirectDamageDebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
