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
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Reduce range needed to cast the sepll
	/// </summary>
	[SpellHandler("Nearsight")]
	public class NearsightSpellHandler : ImmunityEffectSpellHandler
	{
		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			// percent category
			effect.Owner.BuffBonusCategory3[(int)eProperty.ArcheryRange] += (int)Spell.Value;
			effect.Owner.BuffBonusCategory3[(int)eProperty.SpellRange] += (int)Spell.Value;
			SendEffectAnimation(effect.Owner, 0, false, 1);
			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
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
			// percent category
			effect.Owner.BuffBonusCategory3[(int)eProperty.ArcheryRange] -= (int)Spell.Value;
			effect.Owner.BuffBonusCategory3[(int)eProperty.SpellRange] -= (int)Spell.Value;
			if (!noMessages) {
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			return 60000;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				// value should be in percents
				/*
				 * <Begin Info: Encrust Eyes>
				 * Function: nearsight
				 * Target's effective range of all their ranged attacks (archery and magic) reduced.
				 *  
				 * Value: 25%
				 * Target: Targetted
				 * Range: 2300
				 * Duration: 2:0 min
				 * Power cost: 5
				 * Casting time:      2.0 sec
				 * Damage: Matter
				 *  
				 * <End Info>
				 */

				ArrayList list = new ArrayList();

				list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if(Spell.Damage != 0) list.Add("Damage: " + Spell.Damage.ToString("0.###;0.###'%'"));
				if(Spell.Value != 0) list.Add(string.Format("Value: {0}%", (int)Spell.Value));
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
		public NearsightSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}
}
