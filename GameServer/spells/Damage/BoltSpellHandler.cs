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

using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.AI.Brain;
using DOL.Language;
using DOL.Database;

using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Description of DamageBoltSpellHandler.
	/// </summary>
	[SpellHandlerAttribute("Bolt")]
	public class BoltSpellHandler : DirectDamageSpellHandler, IBoltSpellHandler
	{
		protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Bolt Spell Helper for Timed action.
		/// </summary>
		private BoltSpellHelper m_boltSpellHelper;
		
		/// <summary>
		/// Does Blocking Spell still cast partial damage ?
		/// </summary>
		protected virtual bool BlockPartialDamage
		{
			get { return true; }
		}
		
		/// <summary>
		/// Is the spell Blockable by shields.
		/// </summary>
		protected virtual bool IsBlockable
		{
			get { return true; }
		}
		
		/// <summary>
		/// Bolt spell Constructor
		/// </summary>
		public BoltSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			m_boltSpellHelper = new BoltSpellHelper(this);
		}
		
		/// <summary>
		/// Don't trigger subspell on Cast but rather on bolt arrival.
		/// </summary>
		/// <param name="target"></param>
		public override void FinishCastSubSpell(GameLiving target)
		{
		}

		/// <summary>
		/// Don't trigger subspell on Pulse (this shouldn't be used with bolt...)
		/// </summary>		
		public override void SpellPulseSubSpell(GameLiving target)
		{			
		}
		
		/// <summary>
		/// Resist Chance is calculated on Cast, Projectile need this to be calculated on arrival and are using miss chance...
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Bolt/Archery use Caster Level instead of Spell Level, and isn't modified by Bonuses to spells...
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateToHitChance(GameLiving target)
		{
			GameLiving realCaster = Caster;
			
			if (Caster is GameNPC && (Caster as GameNPC).Brain is ControlledNpcBrain)
			{
				realCaster = ((ControlledNpcBrain)((GameNPC)Caster).Brain).GetLivingOwner();
			}
			
			int bonustohit = realCaster.GetModified(eProperty.ToHitBonus);

			// miss rate is 10 on same level opponent
			int hitchance = 90 + ((realCaster.EffectiveLevel - target.EffectiveLevel) / 2) + bonustohit;

			if (target is GameNPC && !(((GameNPC)target).Brain is IControlledBrain && ((IControlledBrain)((GameNPC)target).Brain).GetPlayerOwner() != null))
			{
				hitchance -= (int)(Caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
				hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
			}

			// Apply MissHit Bonus/Malus
			hitchance -= (int)(target.ChanceToBeMissed * 100);
			
			if (log.IsDebugEnabled)
				log.DebugFormat("Bolt CalcToHitChc - Projectile hit chance {0}", hitchance);

			return hitchance;
		}
		
		/// <summary>
		/// Need to Delay Start Direct Effect to After bolt landed on target !
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			m_boltSpellHelper.StartBoltTimer(target, effectiveness);
		}
		
		/// <summary>
		/// Called When Bolt Arrive on Target.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void OnBoltArrival(GameLiving target, double effectiveness)
		{
			// Projectile Spells don't Resist, They Miss !
			int hitchance = CalculateToHitChance(target);
			
			if (!Util.Chance(hitchance))
			{
			    OnSpellMissed(target);
			    return;
			}

			// Miss rate from attackers
			int missRate = 0;
			
			if (target.InCombat)
			{
				foreach (GameLiving attckr in target.Attackers)
				{
					GameLiving attacker = attckr;
					
					if (attacker != Caster)
					{
						int distance = target.GetDistanceTo(attacker);
						if(distance <= 50)
						{
							// each attacker within 50 units adds a 60% chance to miss
							missRate += 60;
						}
						else if (distance <= 125)
						{
							// each attacker within 125 units adds a 30% chance to miss
							missRate += 30;
						}
						else if (distance <= 200)
						{
							// each attacker within 200 units adds a 20% chance to miss
							missRate += 20;
						}
					}
				}
			}
			
			if (Util.Chance(missRate)) 
			{
				MessageToCaster("You miss due to attackers in your way!", eChatType.CT_YouHit);
				OnSpellMissed(target);
				return;
			}
			
			OnBoltDirectEffect(target, effectiveness);
		}

		/// <summary>
		/// Bolt Direct Effect.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void OnBoltDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
		}

		/// <summary>
		/// Bolt Subspell Cast.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void OnHitSubSpell(GameLiving target)
		{
			base.FinishCastSubSpell(target);
		}
		
		/// <summary>
		/// Send Message about blocked Bolt.
		/// </summary>
		/// <param name="ad"></param>
		public override void SendDamageMessages(AttackData ad)
		{
			if(BlockPartialDamage)
			{
				// Block and guard Message
				if (ad.AttackResult == eAttackResult.Blocked)
				{
					MessageToCaster("Your projectile was partially blocked!", eChatType.CT_SpellResisted);
					MessageToLiving(ad.Target, "You blocked a projectile from " + Caster.GetName(0, false) + " and received partial damage!", eChatType.CT_Missed);
				}
				else if (ad.AttackResult == eAttackResult.Bodyguarded)
				{
					// Guarded
					GuardEffect guard = ad.Target.EffectList.GetOfType<GuardEffect>();
					if(guard != null && guard.GuardSource != null)
					{
						MessageToCaster("Your projectile was partially blocked by " + guard.GuardSource.GetName(0, false) + "!", eChatType.CT_SpellResisted);
						MessageToLiving(guard.GuardSource, "You blocked a projectile from " + Caster.GetName(0, false) + " targeted to " + ad.Target.GetName(0, false) + "!", eChatType.CT_Missed);
						MessageToLiving(ad.Target, guard.GuardSource.GetName(0, true) + " blocked a projectile from " + Caster.GetName(0, false) + " you receive partial damage!", eChatType.CT_Missed);
					}
					else
					{
						MessageToCaster("Your projectile was partially blocked by another living!", eChatType.CT_SpellResisted);
						MessageToLiving(ad.Target, "The projectile from " + Caster.GetName(0, false) + " was blocked and you received partial damage!", eChatType.CT_Missed);
					}
				}
			}
			else
			{
				if (ad.AttackResult == eAttackResult.Blocked)
				{
					MessageToLiving(ad.Target, "You block " + Caster.GetName(0, false) + "'s attack!", eChatType.CT_Missed);
					MessageToCaster(Caster.GetName(0, false) + " blocks your attack !", eChatType.CT_Missed);
					return;
				}
			}
		}
		
		/// <summary>
		/// Calculate Bolt Damage (Half Magic/Half Physic/Block)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = new AttackData();
			ad.Attacker = Caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.CausesCombat = true;
			ad.SpellHandler = this;
			ad.AttackResult = eAttackResult.HitUnstyled;
			ad.ArmorHitLocation = eArmorSlot.NOTSET;
			ad.Modifier = 0;
			ad.DamageType = DetermineSpellDamageType();
			
			// Block
			if(IsBlockable)
			{
				double blockChance = target.TryBlock(ad);
				
				if (log.IsDebugEnabled)
					log.DebugFormat("Bolt CalcDmgToTgt - Block Chance {0}", blockChance);
				
				if(Util.ChanceDouble(blockChance))
				{
					ad.AttackResult = eAttackResult.Blocked;
				}
				
				// Guard Only if it wasn't already Blocked.
				if(!(ad.AttackResult == eAttackResult.Blocked))
				{
					// Guard
					Tuple<double, GuardEffect> guardChance = target.TryBeingGuard(ad);
					
					if (log.IsDebugEnabled)
						log.DebugFormat("Bolt CalcDmgToTgt - Guard Chance {0}", guardChance.Item1);
					
					if(guardChance.Item1 > 0 && Util.ChanceDouble(guardChance.Item1))
					{
						// Use BodyGuarded to notice that someone else blocked the blow (but shouldn't take the hit !)
						if(BlockPartialDamage)
						{
							ad.AttackResult = eAttackResult.Bodyguarded;
						}
						else
						{
							// if no damage we can change the AD target safely.
							ad.AttackResult = eAttackResult.Blocked;
							ad.Target = guardChance.Item2.GuardSource;
						}
						
					}
				}
			}
			
			// Get Magical Damage without resists
			int totalDamage = CalculateBaseDamageToTarget(target, effectiveness);
			// Damage Cap
			int damageCap = (int)DamageCap(target, effectiveness);
			
			// Halve it to make it physical damage/magical damage
			double physicalDamage = 0;
			int magicalDamage = totalDamage >> 1;
			int modifier;
			
			// Not Blocked
			if(ad.AttackResult == eAttackResult.HitUnstyled)
			{
				physicalDamage = magicalDamage;
				
				// Get Armor hit Location
				if(target is GamePlayer)
					ad.ArmorHitLocation = ((GamePlayer)target).CalculateArmorHitLocation(ad);
				
				int manaStat;
				if (Caster is GamePlayer)
				{
					manaStat = Caster.GetModified((eProperty)((GamePlayer)Caster).CharacterClass.ManaStat);
				}
				else
				{
					manaStat = Caster.GetModified(eProperty.Intelligence);
				}
				
				// Apply Resists to Physical damage
				double armorAF = ((Caster.Level * 8 * (1.0 + (manaStat - 50) / 200.0)) + 90.68) / (target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67);
				double armorAbs = target.GetArmorAbsorb(ad.ArmorHitLocation);
				double armorResist = 0;
				if (target.Inventory != null)
					armorResist = SkillBase.GetArmorResist(target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation), ad.DamageType)*-0.01;
				
				physicalDamage *= armorAF;
				physicalDamage *= 1.0 - armorAbs;
				
				modifier = (int)(physicalDamage*armorResist);
				
				ad.Modifier += modifier;
			}	
			
			int finalDamage = magicalDamage + (int)physicalDamage;
			
			// Clamp
			finalDamage = Math.Min(damageCap, finalDamage);
			
			// Apply Resists to damage
			modifier = CalculateResistDamageToTarget(target, effectiveness, finalDamage);
			ad.Modifier += modifier;		
			
			// Cap resist modifier
			if ((finalDamage + ad.Modifier) > damageCap)
			{
				ad.Modifier -= finalDamage + ad.Modifier - damageCap; 
			}
			
			finalDamage += ad.Modifier;
			
			if (log.IsDebugEnabled)
				log.DebugFormat("Bolt CalcDmgToTgt - Bolt Physical {0}, Magical {1}, magical modifier {2} (Cap {3}) finale : {4}", physicalDamage, magicalDamage, ad.Modifier, damageCap, finalDamage);
			
			// Clamp
			if(BlockPartialDamage || ad.AttackResult == eAttackResult.HitUnstyled)
			{
				ad.Damage = (int)Math.Max(1, finalDamage);
			}
			else
			{
				// if no partial block damage set it to 0.
				ad.Damage = 0;
			}
			
			// Apply Critical Damage
			int criticalDamage = 0;
			
			// No critical for blocked
			if(ad.AttackResult == eAttackResult.HitUnstyled)
			{
				criticalDamage = CalculateCriticalDamageToTarget(target, effectiveness, ad.Damage);
			}
			
			ad.CriticalDamage = criticalDamage;
			
			return ad;
		}
		
		/// <summary>
		/// Projectile Bolt/Archery need to take care of Block and Bladeturn
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		/// <param name="attackResult"></param>
		public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			if (ad == null || ad.Target == null)
				return;

			SendDamageMessages(ad);
			
			// Block or Guarded, need block animation on the guard (this is just sideway of the rest of animations)
			if (ad.AttackResult == eAttackResult.Bodyguarded || ad.AttackResult == eAttackResult.Blocked)
			{
				GameLiving shieldTarget = ad.Target;
				
				if (ad.AttackResult == eAttackResult.Bodyguarded)
				{
					// Get Guard
					GuardEffect guard = ad.Target.EffectList.GetOfType<GuardEffect>();
					if(guard != null && guard.GuardSource != null)
					{
						shieldTarget = guard.GuardSource;
					}
				}
				
				// send block animation to shieldTarget
				foreach (GamePlayer player in shieldTarget.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					ushort attackWeapon = 0;
					ushort defenderShield = 0;
					
					if (ad.Attacker != null && ad.Attacker.AttackWeapon != null && GlobalConstants.IsBowWeapon((eObjectType)ad.Attacker.AttackWeapon.Object_Type))
					{
						attackWeapon = (ushort)ad.Attacker.AttackWeapon.Model;
					}
					else
					{
						// default bow, fix bolt trouble... TODO : find a better model
						attackWeapon = 132;
					}
					
					if (shieldTarget != null && shieldTarget.Inventory != null)
					{
						InventoryItem shield = shieldTarget.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
						if (shield != null)
							defenderShield = (ushort)shield.Model;
					}
					
					GamePlayer playCombat = player;
					if(playCombat != null)
						playCombat.Out.SendCombatAnimation(ad.Attacker, shieldTarget, attackWeapon, defenderShield, 0, 0, GlobalConstants.GetAttackResultByte(eAttackResult.Blocked), shieldTarget.HealthPercent);
				}
				
				// Force to Blocked for attack data.
				ad.AttackResult = eAttackResult.Blocked;
			}

			// If we're gonna Damage the Target with a Spell, the attack result is HitUnstyled !			
			if (ad.AttackResult == eAttackResult.Blocked && BlockPartialDamage)
				ad.AttackResult = eAttackResult.HitUnstyled;
			
			// Attacked living may modify the attack data.  Primarily used for keep doors and components.
			ad.Target.ModifyAttack(ad);
			
			// Attacked living may take action based on notification, and modify the Attack Data Object !
			ad.Target.OnAttackedByEnemy(ad);
			
			// Check if the Attack is a success to take action (could be modified by OnAttackedByEnemy)
			if (ad.IsSpellResisted || ad.AttackResult == eAttackResult.Missed)
			{
				// Projectiles Miss
				OnSpellMissed(ad.Target);
				return;
			}
			
			// save Attack data.
			AppendAttackData(ad);
			
			// If spell is successful call subspell and damage.
			if (ad.AttackResult == eAttackResult.HitUnstyled)
			{
				// Send Damage Message after event notification
				base.SendDamageMessages(ad);
				
				OnHitSubSpell(ad.Target);
			
				// Effectively DealDamage
				ad.Attacker.DealDamage(ad);
			
				foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer playCombat = player;
					if(playCombat != null)
						playCombat.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);
				}
			}
		}
		
		/// <summary>
		/// Miss for Bolt Type spell.
		/// Send Message/Interrupt/AttackData
		/// </summary>
		/// <param name="target"></param>
		public virtual void OnSpellMissed(GameLiving target)
		{
			// Deliver message to the caster.
			if (Caster is GamePlayer)
				MessageToCaster(LanguageMgr.GetTranslation(((GamePlayer)Caster).Client.Account.Language, "GamePlayer.Attack.Miss"), eChatType.CT_Missed);

			// Report resisted spell attack data to any type of living object, no need
			// to decide here what to do. For example, NPCs will use their brain.
			AttackData ad = new AttackData();
			ad.Attacker = Caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.SpellHandler = this;
			ad.AttackResult = eAttackResult.Missed;
			ad.IsSpellResisted = true;
			target.OnAttackedByEnemy(ad);

			target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);


			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
		}
	}
}
