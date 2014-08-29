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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.AI.Brain;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Archery Class, based on Bolt, handle all arrow type and subspells.
	/// </summary>
	[SpellHandler("Archery")]
	public class ArcheryHandler : BoltSpellHandler
	{
		/// <summary>
		/// Define Shot type (from LifeDrainReturn)
		/// </summary>
		public enum eShotType
		{
			_First = 0,
			Other = 0,
			Critical = 1,
			Power = 2,
			PointBlank = 3,
			Rapid = 4,
			Volley = 5,
			Magical = 6,
			_Last = 6,
		}

		/// <summary>
		/// Archery Shot doesn't break stealth on cast.
		/// </summary>
		public override bool UnstealthCasterOnStart
		{
			get { return false; }
		}
		
		/// <summary>
		/// Only Critical shot are unblockable
		/// </summary>
		protected override bool IsBlockable {
			get { return ShotType != eShotType.Critical; }
		}
		
		/// <summary>
		/// Archery doesn't use Partial Damage like Bolts.
		/// </summary>
		protected override bool BlockPartialDamage {
			get { return false; }
		}
		
		public virtual eShotType ShotType {
			get {
				if (Spell.LifeDrainReturn >= (int)eShotType._First && Spell.LifeDrainReturn <= (int)eShotType._Last)
					return (eShotType)Spell.LifeDrainReturn;
				
				return eShotType.Other;
			}
		}
		
		/// <summary>
		/// Send message about shot and not spell
		/// </summary>
		public override void SendSpellMessages()
		{
			MessageToCaster("You prepare a " + Spell.Name, eChatType.CT_YouHit);
		}

		/// <summary>
		/// Send Interrupt Message about Shot not Spell
		/// </summary>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public override string GetInterruptMessage(GameLiving attacker)
		{
			return attacker.GetName(0, true) + " attacks you and your shot is interrupted!";
		}
		
		/// <summary>
		/// Calculates the effective casting time (Power Shot has no Dex modifier)
		/// </summary>
		/// <returns>effective casting time in milliseconds</returns>
		public override int CalculateCastingTime()
		{
			if (ShotType == eShotType.Power)
			{
				return (int)Math.Max(Caster.MinimumCastingSpeed, Spell.CastTime * (1.0 - Math.Min(Caster.CastingSpeedReductionCap, Caster.GetModified(eProperty.CastingSpeed) * 0.01)));
			}

			return base.CalculateCastingTime();
		}

		/// <summary>
		/// Determines what damage type to use. For archery the player can choose if not a Magical Shot.
		/// </summary>
		/// <returns></returns>
		public override eDamageType DetermineSpellDamageType()
		{
			if (ShotType != eShotType.Magical)
			{
				GameSpellEffect ef = SpellHelper.FindEffectOnTarget(Caster, typeof(ArrowDamageTypes));
				if (ef != null)
				{
					return ef.SpellHandler.Spell.DamageType;
				}
				else
				{
					return eDamageType.Slash;
				}
			}
			
			return base.DetermineSpellDamageType();
		}

		/// <summary>
		/// Archery checks for Disarm and Bow Wielding...
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
		{
			if (!base.CheckBeginCast(selectedTarget, quiet))
			{
				return false;
			}
			
			// Check For Disarm
			if (Caster.IsDisarmed)
			{
				MessageToCaster("You're disarmed and can't prepare a shot!", eChatType.CT_System);
				return false;
			}
			
			// Check For Stealth if Critical
			if(ShotType == eShotType.Critical && !Caster.IsStealthed)
			{
				MessageToCaster("You must be stealthed and wielding a bow to use this ability!", eChatType.CT_SpellResisted);
				return false;
			}

			// Check For Bow
			if (Caster is GamePlayer && Caster.AttackWeapon != null && !GlobalConstants.IsBowWeapon((eObjectType)Caster.AttackWeapon.Object_Type))
			{
				MessageToCaster("You must be wielding a bow to use this ability!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Archery doesn't use Power but Endurance
		/// </summary>
		/// <param name="target"></param>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int PowerCost(GameLiving target, bool consume)
		{
			return 0;
		}

		/// <summary>
		/// Adjust damage based on chance to hit.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="hitChance"></param>
		/// <returns></returns>
		public override int AdjustDamageForHitChance(int damage, int hitChance)
		{
			int adjustedDamage = damage;

			if (hitChance < 55)
			{
				adjustedDamage += (int)(adjustedDamage * (hitChance - 55) * ServerProperties.Properties.ARCHERY_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER * 0.01);
			}
			else if (hitChance > 100)
			{
				// (0.25% damage bonus for point over hitChance, cap 25% bonuses, then capped by spell capdamage...)
				adjustedDamage += (int)(adjustedDamage * Math.Min(100, hitChance - 100) * ServerProperties.Properties.ARCHERY_HITCHANCE_DAMAGE_RAISE_MULTIPLIER * 0.01);
			}
			
			if (log.IsDebugEnabled)
				log.DebugFormat("Archery AdjDmgToHitChc - Projectile Adjusted Damage for Hit Chance : {0}", adjustedDamage);
			
			return Math.Max(1, adjustedDamage);
		}

		/// <summary>
		/// Archery need Endurance Check instead of Power Check
		/// </summary>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		protected override bool CheckPower(GameLiving target, bool quiet)
		{
			if (Spell.UsePower && (Caster.Endurance <= 0 || Caster.Endurance < CalculateEnduranceCost(false)))
			{
				if (!quiet)
					MessageToCaster("You don't have enough endurance to use this!", eChatType.CT_SpellResisted);
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Archery Endurance Cost is %age based, use ArcaneSyphon Bonus.
		/// </summary>
		/// <param name="consume"></param>
		/// <returns></returns>
		public override int CalculateEnduranceCost(bool consume)
		{
			if(consume)
			{
				int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
				if (syphon > 0)
				{
					if(Util.Chance(syphon))
					{
						return 0;
					}
				}
			}
			return (int)(Caster.MaxEndurance * (Spell.Power * -0.01));
		}
		
		/// <summary>
		/// Volley Targets is capped by Spell.Value
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			IList<GameLiving> targets = base.SelectTargets(castTarget);
			
			if(ShotType == eShotType.Volley && Spell.Value > 0)
			{
				// Remove target until count is ok
				while(targets.Count > Spell.Value)
				{
					targets.RemoveAt(Util.Random(targets.Count-1));
				}
			}
			
			return targets;
		}

		/// <summary>
		/// Unstealth Archer on Arrow shoot and consume Endurance
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			// Unstealth on Arrow shot
			if (Caster is GamePlayer && Caster.IsStealthed)
			{
				(Caster as GamePlayer).Stealth(false);
			}

			if (ShotType == eShotType.Volley)
			{
				// always put archer into combat when using volley
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;

			}
			
			// Consume Endurance.
			Caster.ChangeEndurance(Caster, GameLiving.eEnduranceChangeType.Spell, -1 * CalculateEnduranceCost(true));

			base.FinishSpellCast(target);
		}
	
		/// <summary>
		/// Archery doesn't Resist but Miss
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void OnBoltArrival(GameLiving target, double effectiveness)
		{
			int hitChance = CalculateToHitChance(target);
			
			if(!Util.Chance(hitChance))
			{
				OnSpellMissed(target);
				return;
			}
			
			// Subspell should be triggered when Arrow is a sure hit !
			OnBoltDirectEffect(target, effectiveness);
		}
		
		/// <summary>
		/// Change Damage Type to Ranged
		/// Apply DPS Modifier and Volley Modifier
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
			
			// Archery is Ranged type
			ad.AttackType = AttackData.eAttackType.Ranged;

			// Modifier
			if (ad.AttackResult == eAttackResult.HitUnstyled)
			{
				// Damage Reduction from DPS
				int spellRequiredDPS = 12 + 3 * Spell.Level;

				if (Caster.AttackWeapon != null && Caster.AttackWeapon.DPS_AF < spellRequiredDPS)
				{
					double percentReduction = (double)Caster.AttackWeapon.DPS_AF / (double)spellRequiredDPS;
					ad.Modifier = (int)(ad.Modifier * percentReduction);
					ad.Damage = (int)(ad.Damage * percentReduction);
					ad.CriticalDamage = (int)(ad.CriticalDamage * percentReduction);
				}

				// Volley damage reduction based on live testing - tolakram
				if (ShotType == eShotType.Volley)
				{
					ad.Modifier = (int)(ad.Modifier * 0.815);
					ad.Damage = (int)(ad.Damage * 0.815);
					ad.CriticalDamage = (int)(ad.CriticalDamage * 0.815);
				}
			}

			return ad;
		}
		
		public override void OnHitSubSpell(GameLiving target)
		{
			base.OnHitSubSpell(target);
			// Launch Proc !
			if (ShotType != eShotType.Volley)
				Caster.CheckWeaponMagicalEffect(GetLastAttackData(target), Caster.AttackWeapon);
		}
		
		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				//list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				//list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add("Instrument require: " + GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement));
				if (Spell.Damage != 0)
					list.Add("Damage: " + Spell.Damage.ToString("0.###;0.###'%'"));
				else if (Spell.Value != 0)
					list.Add("Value: " + Spell.Value.ToString("0.###;0.###'%'"));
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));
				if (Spell.Power != 0)
					list.Add("Endurance cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);
				if (Spell.DamageType != eDamageType.Natural)
					list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));
				return list;
			}
		}
		
		public ArcheryHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) 
		{
		}
	}
}
