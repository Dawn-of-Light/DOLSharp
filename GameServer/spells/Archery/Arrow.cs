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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.SkillHandler;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell Handler for firing arrows
	/// </summary>
	public class ArrowSpellHandler : SpellHandler
	{
		/// <summary>
		/// Fire arrow
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
            m_caster.Endurance -= CalculateEnduranceCost();
			//if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent) && Spell.SpellType != "SiegeArrow")
			//{
			//    MessageToCaster(String.Format("Your spell has no effect on the {0}!", target.Name), eChatType.CT_SpellResisted);
			//    return;
			//}
			base.FinishSpellCast(target);
		}

		#region LOS Checks for Keeps
		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void StartSpell(GameLiving target)
		{
			int targetCount = 0;

			foreach (GameLiving targ in SelectTargets(target))
			{
				DealDamage(targ);
				targetCount++;

				if (Spell.Target.ToLower() == "area" && targetCount >= Spell.Value)
				{
					// Volley is limited to Volley # + 2 targets.  This number is stored in Spell.Value
					break;
				}
			}
		}

		private bool CheckLOS(GameLiving living)
		{
			foreach (AbstractArea area in living.CurrentAreas)
			{
				if (area.CheckLOS)
					return true;
			}
			return false;
		}

		private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				GameLiving target = (GameLiving)(Caster.CurrentRegion.GetObject(targetOID));
				if (target != null)
					DealDamage(target);
			}
		}

		private void DealDamage(GameLiving target)
		{
            int ticksToTarget = m_caster.GetDistanceTo( target ) * 100 / 85; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			}
			ArrowOnTargetAction arrow = new ArrowOnTargetAction(Caster, target, this);
			arrow.Start(1 + ticksToTarget);
		}
		#endregion

		/// <summary>
		/// Delayed action when arrow reach the target
		/// </summary>
		protected class ArrowOnTargetAction : RegionAction
		{
			/// <summary>
			/// The arrow target
			/// </summary>
			protected readonly GameLiving m_arrowTarget;

			/// <summary>
			/// The spell handler
			/// </summary>
			protected readonly ArrowSpellHandler m_handler;

			/// <summary>
			/// Constructs a new ArrowOnTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="arrowTarget">The arrow target</param>
			/// <param name="spellHandler"></param>
			public ArrowOnTargetAction(GameLiving actionSource, GameLiving arrowTarget, ArrowSpellHandler spellHandler) : base(actionSource)
			{
				if (arrowTarget == null)
					throw new ArgumentNullException("arrowTarget");
				if (spellHandler == null)
					throw new ArgumentNullException("spellHandler");
				m_arrowTarget = arrowTarget;
				m_handler = spellHandler;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			public virtual void OnTickBase()
			{
				OnTick();
			}


			protected override void OnTick()
			{
				GameLiving target = m_arrowTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (target == null || !target.IsAlive || target.ObjectState != GameObject.eObjectState.Active || target.CurrentRegionID != caster.CurrentRegionID) return;

				int missrate = 100 - m_handler.CalculateToHitChance(target);
				// add defence bonus from last executed style if any
				AttackData targetAD = (AttackData)target.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
				if (targetAD != null
					&& targetAD.AttackResult == GameLiving.eAttackResult.HitStyle
					&& targetAD.Style != null)
				{
					missrate += targetAD.Style.BonusToDefense;
				}

				AttackData ad = m_handler.CalculateDamageToTarget(target, 0.5); // half of the damage is magical

				// check for bladeturn miss
				if (ad.AttackResult == GameLiving.eAttackResult.Missed)
				{
					// Not sure if pet or npc should aggro on BT block or not
					//target.OnAttackedByEnemy(ad);
					//if (target is GameNPC)
					//{
					//    IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
					//    if (aggroBrain != null)
					//        aggroBrain.AddToAggroList(caster, 1);
					//}
					return;
				}

				if (Util.Chance(missrate)) 
				{
					ad.AttackResult = GameLiving.eAttackResult.Missed;
					m_handler.MessageToCaster("You miss!", eChatType.CT_YouHit);
					m_handler.MessageToLiving(target, caster.GetName(0, false) + " missed!", eChatType.CT_Missed);
					target.OnAttackedByEnemy(ad);
					target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, caster);
					if(target is GameNPC)
					{
						IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
						if (aggroBrain != null)
							aggroBrain.AddToAggroList(caster, 1);
					}
					return;
				}

				// Block
				bool blocked = false;
				if (target is GamePlayer && !target.IsStunned && !target.IsSitting) 
				{ // mobs left out yet
					GamePlayer player = (GamePlayer)target;
					InventoryItem lefthand = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					if (lefthand!=null && (player.AttackWeapon==null || player.AttackWeapon.Item_Type==Slot.RIGHTHAND || player.AttackWeapon.Item_Type==Slot.LEFTHAND)) 
					{
						if (target.IsObjectInFront(caster, 180) && lefthand.Object_Type == (int)eObjectType.Shield) 
						{
							// TODO: shield size, which field to use?
							// TODO: 30% chance to block arrows/bolts
							double shield = 0.5 * player.GetModifiedSpecLevel(Specs.Shields);
							double blockchance = ((player.Dexterity*2)-100)/40.0 + shield + (0*3) + 5;
                            blockchance += 30;
							blockchance -= target.GetConLevel(caster) * 5;
							if (blockchance >= 100) blockchance = 99;
							if (blockchance <= 0) blockchance = 1;

							if (target.IsEngaging)
							{
								EngageEffect engage = (EngageEffect)target.EffectList.GetOfType(typeof(EngageEffect));
								if (engage != null && target.AttackState && engage.EngageTarget == caster)
								{
									// Engage raised block change to 85% if attacker is engageTarget and player is in attackstate							
									// You cannot engage a mob that was attacked within the last X seconds...
									if (engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK)
									{
										if (engage.Owner is GamePlayer)
											(engage.Owner as GamePlayer).Out.SendMessage(engage.EngageTarget.GetName(0, true) + " has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}  // Check if player has enough endurance left to engage
									else if (engage.Owner.Endurance < EngageAbilityHandler.ENGAGE_DURATION_LOST)
									{
										engage.Cancel(false); // if player ran out of endurance cancel engage effect
									}
									else
									{
										engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
										if (engage.Owner is GamePlayer)
											(engage.Owner as GamePlayer).Out.SendMessage("You concentrate on blocking the blow!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

										if (blockchance < 85)
											blockchance = 85;
									}
								}
							}

							if (blockchance >= Util.Random(1, 100)) 
							{
								m_handler.MessageToLiving(player, "You partially block " + caster.GetName(0, false) + "'s spell!", eChatType.CT_Missed);
								m_handler.MessageToCaster(player.GetName(0, true) + " blocks!", eChatType.CT_YouHit);
								blocked = true;
							}
						}
					}
				}

				// simplified melee damage calculation
				if (blocked == false)
				{
					// TODO: armor resists to damage type

					double damage = m_handler.Spell.Damage / 2; // another half is physical damage
					if(target is GamePlayer)
						ad.ArmorHitLocation = ((GamePlayer)target).CalculateArmorHitLocation(ad);

					InventoryItem armor = null;
					if (target.Inventory != null)
						armor = target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

					double ws = (caster.Level * 8 * (1.0 + (caster.GetModified(eProperty.Dexterity) - 50)/200.0));

					damage *= ((ws + 90.68) / (target.GetArmorAF(ad.ArmorHitLocation) + 20*4.67));
					damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
					ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) / -100.0);
					damage += ad.Modifier;
					if (damage < 0) damage = 0;
					ad.Damage += (int)damage;
				}

				// apply total damage cap	
				ad.UncappedDamage = ad.Damage;
				ad.Damage = (int)Math.Min(ad.Damage, m_handler.Spell.Damage * 3);

				if (caster is GamePlayer)
				{
					ad.Damage = (int)(ad.Damage * ((GamePlayer)caster).Effectiveness);
				}

				// fix critical damage
				if (blocked == false && ad.CriticalDamage > 0)
				{
					if (m_handler.Spell.Target.ToLower() == "area")
					{
						ad.CriticalDamage = 0;
					}
					else
					{
						int critMax = (target is GamePlayer) ? ad.Damage / 2 : ad.Damage;
						ad.CriticalDamage = Util.Random(critMax / 10, critMax);
					}
				}

				m_handler.SendDamageMessages(ad);
				m_handler.DamageTarget(ad, false, (blocked ? 0x02 : 0x14));
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, caster);
				
				//Eden
				if(m_handler.Spell.SubSpellID!=0)
				{
					Spell subspell = SkillBase.GetSpellByID(m_handler.Spell.SubSpellID);
					if (subspell != null)
					{
                       subspell.Level = m_handler.Spell.Level;
						ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_handler.Caster, subspell, SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect));
						if(spellhandler!=null) spellhandler.StartSpell(target);
					}
				}
              	if (m_handler.Caster.AttackWeapon != null && GlobalConstants.IsBowWeapon((eObjectType)m_handler.Caster.AttackWeapon.Object_Type))
                {
                    if (ad.AttackResult == GameLiving.eAttackResult.HitUnstyled || ad.AttackResult == GameLiving.eAttackResult.HitStyle)
                        CheckWeaponMagicalEffect(ad, m_handler.Caster.AttackWeapon);
				}
			}


			protected virtual void CheckWeaponMagicalEffect(AttackData ad, InventoryItem weapon)
            {
                if (weapon == null) return;
                double procChance = weapon.SPD_ABS * 0.0025;
                if (weapon.ProcSpellID != 0 && Util.ChanceDouble(procChance))
                    StartWeaponMagicalEffect(ad, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects),
                        weapon.ProcSpellID);
                if (weapon.ProcSpellID1 != 0 && Util.ChanceDouble(procChance))
                    StartWeaponMagicalEffect(ad, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects),
                        weapon.ProcSpellID1);
                if (weapon.PoisonSpellID != 0)
                {
                    if (ad.Target.EffectList.GetOfType(typeof(RemedyEffect)) != null)
                    {
                        if (ad.Attacker is GamePlayer)
                            (ad.Attacker as GamePlayer).Out.SendMessage("Your target is protected against your poison by a magical effect.",
                                eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    StartWeaponMagicalEffect(ad, SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons),
                        weapon.PoisonSpellID);
                }
            }

            protected virtual void StartWeaponMagicalEffect(AttackData ad, SpellLine spellLine, int spellID)
            {
                if (spellLine == null) return;

				List<Spell> spells = SkillBase.GetSpellList(spellLine.KeyName);
                foreach (Spell spell in spells)
                {
                    if (spell.ID == spellID)
                    {
                        ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(ad.Attacker, spell, spellLine);
                        if (spellHandler != null)
                            spellHandler.StartSpell(ad.Target);
                    }
                }
            }
		}

		// constructor
		public ArrowSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
