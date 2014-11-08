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

using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Base Class for SpellHandler Behavior
	/// Handles most of objects properties for readability
	/// </summary>
	public class SpellHandlerBase
	{
		#region spell handler properties
		/// <summary>
		/// SpellHandler Cast Timer Object
		/// </summary>
		protected DelayedCastTimer m_castTimer;	

		private Spell m_spell;

		/// <summary>
		/// The spell that we want to handle
		/// </summary>
		public virtual Spell Spell
		{
			get { return m_spell; }
			protected set { m_spell = value; }
		}
				
		private SpellLine m_spellLine;
		
		/// <summary>
		/// The spell line the spell belongs to
		/// </summary>
		public virtual SpellLine SpellLine
		{
			get { return m_spellLine; }
			protected set { m_spellLine = value; }
		}

		private GameLiving m_caster;
		
		/// <summary>
		/// The caster of the spell
		/// </summary>
		public virtual GameLiving Caster
		{
			get { return m_caster; }
			protected set { m_caster = value; }
		}
		
		private GameLiving m_spellTarget = null;
		
		/// <summary>
		/// The target for this spell
		/// </summary>
		public virtual GameLiving SpellTarget {
			get { return m_spellTarget; }
			protected set { m_spellTarget = value; }
		}

		protected InventoryItem m_spellItem = null;
		
		/// <summary>
		/// Item Casting Spell (if Any)
		/// </summary>
		public virtual InventoryItem SpellItem {
			get { return m_spellItem; }
			protected set { m_spellItem = value; }
		}

		private SpellCastingAbilityHandler m_ability = null;

		/// <summary>
		/// Ability that casts a spell (if Any)
		/// </summary>
		public virtual SpellCastingAbilityHandler Ability
		{
			get { return m_ability; }
			set { m_ability = value; }
		}


		/// <summary>
		/// Is the spell being cast?
		/// </summary>
		public bool IsCasting
		{
			get { return m_castTimer != null && m_castTimer.IsAlive; }
		}

		/// <summary>
		/// Does the spell have a positive effect?
		/// </summary>
		public virtual bool HasPositiveEffect
		{
			get
			{
				if (m_spell.Target.ToLower() != "enemy" && m_spell.Target.ToLower() != "cone" && m_spell.Target.ToLower() != "area")
					return true;

				return false;
			}
		}

		/// <summary>
		/// Is this Spell purgeable
		/// </summary>
		public virtual bool IsUnPurgeAble
		{
			get { return false; }
		}



		#endregion

		#region cast sequence properties
		private bool m_interrupted = false;
		
		/// <summary>
		/// Has the spell been interrupted
		/// </summary>
		public virtual bool Interrupted {
			get { return m_interrupted; }
			protected set { m_interrupted = value; }
		}

		private int m_stage = 0;

		/// <summary>
		/// Delayedcast Stage
		/// </summary>
		public virtual int Stage
		{
			get { return m_stage; }
			set { m_stage = value; }
		}

		private long m_started = 0;
		
		/// <summary>
		/// Use to store Time when the delayedcast started
		/// </summary>
		public virtual long Started {
			get { return m_started; }
			protected set { m_started = value; }
		}

		private bool m_startReuseTimer = true;

		/// <summary>
		/// Shall we start the reuse timer
		/// </summary>
		public virtual bool StartReuseTimer
		{
			get { return m_startReuseTimer; }
			protected set { m_startReuseTimer = value; }
		}

		/// <summary>
		/// Can this spell be queued with other spells?
		/// </summary>
		public virtual bool CanQueue
		{
			get { return true; }
		}

		/// <summary>
		/// Does this spell break stealth on start of cast?
		/// </summary>
		public virtual bool UnstealthCasterOnStart
		{
			get { return true; }
		}
		
		/// <summary>
		/// Does this spell break stealth on Finish of cast?
		/// </summary>
		public virtual bool UnstealthCasterOnFinish
		{
			get { return true; }
		}
		#endregion
		
		#region animation propertiers
		
		public virtual GameLiving AnimationSource
		{
			get { return Caster; }
		}
		
		#endregion
		
		#region tooltip handling
		/// <summary>
		/// Return the given Delve Writer with added keyvalue pairs.
		/// </summary>
		/// <param name="dw"></param>
		/// <param name="id"></param>
		public virtual void TooltipDelve(ref MiniDelveWriter dw, int id)
		{
			if (dw == null)
				return;
			
            dw.AddKeyValuePair("Function", "light"); // Function of type "light" allows custom description to show with no hardcoded text.  Temporary Fix - tolakram
            //.Value("Function", spellHandler.FunctionName ?? "0")
            dw.AddKeyValuePair("Index", unchecked((short)id));
			dw.AddKeyValuePair("Name", Spell.Name);
			
			if (Spell.CastTime > 2000)
				dw.AddKeyValuePair("cast_timer", Spell.CastTime - 2000); //minus 2 seconds (why mythic?)
			else if (!Spell.IsInstantCast)
				dw.AddKeyValuePair("cast_timer", 0); //minus 2 seconds (why mythic?)
			
			if (Spell.IsInstantCast)
				dw.AddKeyValuePair("instant","1");
			//.Value("damage", spellHandler.GetDelveValueDamage, spellHandler.GetDelveValueDamage != 0)
			if ((int)Spell.DamageType > 0)
				dw.AddKeyValuePair("damage_type", (int) Spell.DamageType + 1); // Damagetype not the same as dol
			//.Value("type1", spellHandler.GetDelveValueType1, spellHandler.GetDelveValueType1 > 0)
			if (Spell.Level > 0)
				dw.AddKeyValuePair("level", Spell.Level);
			if (Spell.CostPower)
				dw.AddKeyValuePair("power_cost", Spell.Power);
			//.Value("round_cost",spellHandler.GetDelveValueRoundCost,spellHandler.GetDelveValueRoundCost!=0)
			//.Value("power_level", spellHandler.GetDelveValuePowerLevel,spellHandler.GetDelveValuePowerLevel!=0)
			if (Spell.Range > 0)
				dw.AddKeyValuePair("range", Spell.Range);
			if (Spell.Duration > 0)
				dw.AddKeyValuePair("duration", Spell.Duration/1000); //seconds
			if (GetDurationType() > 0)
				dw.AddKeyValuePair("dur_type", GetDurationType());
			//.Value("parm",spellHandler.GetDelveValueParm,spellHandler.GetDelveValueParm>0)
			if (Spell.HasRecastDelay)
				dw.AddKeyValuePair("timer_value", Spell.RecastDelay/1000);
			//.Value("bonus", spellHandler.GetDelveValueBonus, spellHandler.GetDelveValueBonus > 0)
			//.Value("no_combat"," ",Util.Chance(50))//TODO
			//.Value("link",14000)
			//.Value("ability",4) // ??
			//.Value("use_timer",4)
			if (GetSpellTargetType() > 0)
				dw.AddKeyValuePair("target", GetSpellTargetType());
			//.Value("frequency", spellHandler.GetDelveValueFrequency, spellHandler.GetDelveValueFrequency != 0)
			if (!string.IsNullOrEmpty(Spell.Description))
				dw.AddKeyValuePair("description_string", Spell.Description);
			if (Spell.IsAoE)
				dw.AddKeyValuePair("radius", Spell.Radius);
			if (Spell.IsConcentration)
				dw.AddKeyValuePair("concentration_points", Spell.Concentration);
			//.Value("num_targets", spellHandler.GetDelveValueNumTargets, spellHandler.GetDelveValueNumTargets>0)
			//.Value("no_interrupt", spell.Interruptable ? (char)0 : (char)1) //Buggy?
			//log.Info(dw.ToString());
		}
		
		/// <summary>
		/// Returns delve code for target
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual int GetSpellTargetType()
		{
			switch (Spell.Target)
			{
				case "Realm":
					return 7;
				case "Self":
					return 0;
				case "Enemy":
					return 1;
				case "Pet":
					return 6;
				case "Group":
					return 3;
				case "Area":
					return 0; // TODO
				default:
					return 0;
			}
		}

		protected virtual int GetDurationType()
		{
			//2-seconds,4-conc,5-focus
			if (Spell.Duration>0)
			{
				return 2;
			}
			if (Spell.IsConcentration)
			{
				return 4;
			}


			return 0;
		}
		#endregion		
	}
}
