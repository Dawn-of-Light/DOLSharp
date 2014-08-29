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
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS
{
	/// <summary>
	/// The necromancer pets.
	/// </summary>
	/// <author>Aredhel</author>
	public class NecromancerPet : GamePet
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Summoner Constitution Bonus
		/// </summary>
		protected int m_summonConBonus;
		
		/// <summary>
		/// Summoner Hit Points Bonus
		/// </summary>
		protected int m_summonHitsBonus;

		/// <summary>
		/// Create necromancer pet from template. Con and hit bonuses from
		/// items the caster was wearing when the summon started, will be
		/// transferred to the pet.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner">Player who summoned this pet.</param>
		/// <param name="summonConBonus">Item constitution bonuses of the player.</param>
		/// <param name="summonHitsBonus">Hits bonuses of the player.</param>
		public NecromancerPet(INpcTemplate npcTemplate, int summonConBonus, int summonHitsBonus)
			: base(npcTemplate)
		{
			// Transfer bonuses.
			
			m_summonConBonus = summonConBonus;
			m_summonHitsBonus = summonHitsBonus;
		}

        /// <summary>
        /// gets the DamageRvR Memory of this NecromancerPet
        /// </summary>
        public override long DamageRvRMemory
        {
            get
            {
            	if(Brain is IControlledBrain)
            	{
            		GameLiving owner = ((IControlledBrain)Brain).GetLivingOwner();
            		if(owner != null)
            			return owner.DamageRvRMemory;
            	}
            	
                return base.DamageRvRMemory;
            }
            set
            {
            	if(Brain is IControlledBrain)
            	{
            		GameLiving owner = ((IControlledBrain)Brain).GetLivingOwner();
            		if(owner != null)
            		{
            			owner.DamageRvRMemory = value;
            			return;
            		}
            		
            	}

                base.DamageRvRMemory = value;
            }
        }

		/// <summary>
		/// Proc IDs for various pet weapons.
		/// </summary>
		protected enum Procs
		{
			Cold = 32050,
			Disease = 32014,
			Heat = 32053,
			Poison = 32013,
			Stun = 2165,
			AbsDebuff = 62300,
		};

		#region Stats

		/// <summary>
		/// Get modified bonuses for the pet; some bonuses come from the shade,
		/// some come from the pet.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int GetModified(eProperty property)
		{
			if (Brain == null || (Brain as IControlledBrain) == null)
				return base.GetModified(property);

            GameLiving owner = (Brain as IControlledBrain).GetLivingOwner();

			switch (property)
			{
				case eProperty.Strength:
				case eProperty.Dexterity:
				case eProperty.Quickness:
				case eProperty.Resist_Crush:
				case eProperty.Resist_Body:
				case eProperty.Resist_Cold:
				case eProperty.Resist_Energy:
				case eProperty.Resist_Heat:
				case eProperty.Resist_Matter:
				case eProperty.Resist_Slash:
				case eProperty.Resist_Spirit:
				case eProperty.Resist_Thrust:
					{
						// Get item bonuses from the shade, but buff bonuses from the pet.

						int itemBonus = owner.GetModifiedFromItems(property);
						int buffBonus = GetModifiedFromBuffs(property);
						int debuff = DebuffCategory[property];

						// Base stats from the pet; add this to item bonus
						// afterwards, as it is treated the same way for
						// debuffing purposes.

						int baseBonus = 0;
						switch (property)
						{
							case eProperty.Strength:
								baseBonus = Strength;
								break;
							case eProperty.Dexterity:
								baseBonus = Dexterity;
								break;
							case eProperty.Quickness:
								baseBonus = Quickness;
								break;
						}

						itemBonus += baseBonus;

						// Apply debuffs. 100% Effectiveness for player buffs, but only 50%
						// effectiveness for item bonuses.

						buffBonus -= Math.Abs(debuff);

						if (buffBonus < 0)
						{
							itemBonus += buffBonus / 2;
							buffBonus = 0;
							if (itemBonus < 0)
								itemBonus = 0;
						}

						return itemBonus + buffBonus;
					}
				
				case eProperty.Constitution:
					{
						int baseBonus = Constitution;
						int buffBonus = GetModifiedFromBuffs(eProperty.Constitution);
						int debuff = DebuffCategory[property];

						// Apply debuffs. 100% Effectiveness for player buffs, but only 50%
						// effectiveness for base bonuses.

						buffBonus -= Math.Abs(debuff);

						if (buffBonus < 0)
						{
							baseBonus += buffBonus / 2;
							buffBonus = 0;
							if (baseBonus < 0)
								baseBonus = 0;
						}

						return baseBonus + buffBonus;
					}
				
				case eProperty.MaxHealth:
					{
						int conBonus = (int)(3.1 * Constitution);
						int hitsBonus = (int)(32.5 * Level + m_summonHitsBonus);
						int debuff = DebuffCategory[property];

						// Apply debuffs. As only base constitution affects pet
						// health, effectiveness is a flat 50%.

						conBonus -= Math.Abs(debuff) / 2;

						if (conBonus < 0)
							conBonus = 0;

						return conBonus + hitsBonus;
					}
			}

			return base.GetModified(property);
		}

		/// <summary>
		/// Current health (absolute value).
		/// </summary>
		public override int Health
		{
			get
			{
				return base.Health;
			}
			set
			{
				value = Math.Min(value, MaxHealth);
				value = Math.Max(value, 0);

				if (Health == value)
				{
					base.Health = value; //needed to start regeneration
					return;
				}

				int oldPercent = HealthPercent;
				base.Health = value;
				if (oldPercent != HealthPercent)
				{
					// Update pet health in group window.

					GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
					if (owner.Group != null)
						owner.Group.UpdateMember(owner, false, false);
				}
			}
		}

		/// <summary>
		/// Base strength.
		/// </summary>
		public override short Strength
		{
			get
			{
				return (short)(60 + Level);
			}
		}

		/// <summary>
		/// Base constitution.
		/// </summary>
		public override short Constitution
		{
			get
			{
				return (short)(60 + Level / 2 + m_summonConBonus);
			}
		}

		/// <summary>
		/// Base dexterity.
		/// </summary>
		public override short Dexterity
		{
			get
			{
				return 60;
			}
		}

		/// <summary>
		/// Base quickness.
		/// </summary>
		public override short Quickness
		{
			get
			{
				return (short)(60 + Level / 2);
			}
		}

		#endregion

		#region Melee

		/// <summary>
		/// Toggle taunt mode on/off.
		/// </summary>
		private void ToggleTauntMode()
		{
			TauntEffect tauntEffect = EffectList.GetOfType<TauntEffect>();
			GamePlayer owner = (Brain as IControlledBrain).Owner as GamePlayer;

			if (tauntEffect != null)
			{
				// It's on, so let's switch it off.
				tauntEffect.Stop();
				
				if (owner != null)
					owner.Out.SendMessage(String.Format("{0} seems to be less aggressive than before.",
					                                    GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				// It's off, so let's turn it on.
				new TauntEffect().Start(this);
				
				if (owner != null)
					owner.Out.SendMessage(String.Format("{0} enters an aggressive stance.",
					                                    GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Spells

		/// <summary>
		/// Pet-only insta spells.
		/// </summary>
		public String PetInstaSpellLine
		{
			get { return "Necro Pet Insta Spells"; }
		}

		public override bool CheckBeforeCast(ISpellHandler spellHandler)
		{
			int power = spellHandler.PowerCost((TargetObject != null && TargetObject is GameLiving) ? (GameLiving)TargetObject : Owner, false);

			if (Owner.Mana < power)
			{
				Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.NotEnoughPower));
				return false;
			}
				
			return base.CheckBeforeCast(spellHandler);
		}

		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			if (SpellTimer != null)
			{
				if (this == null || this.ObjectState != eObjectState.Active || !this.IsAlive || this.TargetObject == null || (this.TargetObject is GameLiving && this.TargetObject.ObjectState != eObjectState.Active || !(this.TargetObject as GameLiving).IsAlive))
					SpellTimer.Stop();
				else
					SpellTimer.Start(1);
			}
			if (m_runningSpellHandler != null)
			{
				//prevent from relaunch
				m_runningSpellHandler.CastingCompleteEvent -= new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler = null;
			}

			Brain.Notify(GameNPCEvent.CastFinished, this, new CastingEventArgs(handler));
		}

		public override bool CanCastInCombat(Spell spell)
		{
			// Necromancer pets can always start to cast while in combat
			return true;
		}

		public override void ModifyAttack(AttackData attackData)
		{
			base.ModifyAttack(attackData);

			if ((Owner as GamePlayer).Client.Account.PrivLevel > (int)ePrivLevel.Player)
			{
				attackData.Damage = 0;
				attackData.CriticalDamage = 0;
			}
		}


		/// <summary>
		/// Insta cast baseline buffs (STR+DEX) on the pet.
		/// </summary>
		protected void Empower()
		{
			if (AttackState) return;

			SpellLine buffLine = SkillBase.GetSpellLine(PetInstaSpellLine);
			if (buffLine == null)
				return;

			List<Spell> buffList = SkillBase.GetSpellList(PetInstaSpellLine);
			if (buffList.Count == 0)
				return;

			// Find the best baseline buffs for this level.

			int maxLevel = Level;
			Spell strBuff = null, dexBuff = null;
			foreach (Spell spell in buffList)
			{
				if (spell.Level <= maxLevel)
				{
					switch (spell.SpellType)
					{
						case "StrengthBuff":
							{
								if (strBuff == null)
									strBuff = spell;
								else
									strBuff = (strBuff.Level < spell.Level) ? spell : strBuff;
							}
							break;
						case "DexterityBuff":
							{
								if (dexBuff == null)
									dexBuff = spell;
								else
									dexBuff = (dexBuff.Level < spell.Level) ? spell : dexBuff;
							}
							break;
					}
				}
			}

			// Insta buff.

			if (strBuff != null)
				CastSpell(strBuff, buffLine);
			if (dexBuff != null)
				CastSpell(dexBuff, buffLine);
		}

		/// <summary>
		/// Taunt the current target.
		/// </summary>
		public void Taunt()
		{
			if (IsIncapacitated)
				return;

			SpellLine chantsLine = SkillBase.GetSpellLine("Chants");
			if (chantsLine == null)
				return;

			List<Spell> chantsList = SkillBase.GetSpellList("Chants");
			if (chantsList.Count == 0)
				return;

			// Find the best paladin taunt for this level.

			Spell tauntSpell = null;
			foreach (Spell spell in chantsList)
				if (spell.SpellType == "Taunt" && spell.Level <= Level)
					tauntSpell = spell;

			if (tauntSpell != null && GetSkillDisabledDuration(tauntSpell) == 0)
				CastSpell(tauntSpell, chantsLine);
		}

		#endregion

		/// <summary>
		/// Actions to be taken when the pet receives a whisper.
		/// </summary>
		/// <param name="source">Source of the whisper.</param>
		/// <param name="text">"Text that was whispered</param>
		/// <returns>True if whisper was handled, false otherwise.</returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if(!base.WhisperReceive(source, text))
				return false;
			   
			GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
			
			if (source == null || source != owner) 
				return false;

			switch (text.ToLower())
			{
				case "arawn":
					if(owner != null)
						SayTo(owner, ReceiveArawn());
					return true;
				case "taunt":
					ToggleTauntMode();
					return true;
			}
			
			return true;
		}

		/// <summary>
		/// What Menu to display when Receiving Arawn Message
		/// </summary>
		/// <returns></returns>
		protected virtual string ReceiveArawn()
		{
			return "As one of the many cadaverous servants of Arawn, I am able to [taunt] your enemies so that they will focus on me instead of you.";
		}
		
		protected virtual void ReplyCommandOK(GamePlayer owner)
		{
			SayTo(owner, eChatLoc.CL_SystemWindow, "As you command.");
		}
		
		/// <summary>
		/// Load equipment for the pet.
		/// </summary>
		/// <param name="templateID">Equipment Template ID.</param>
		/// <returns>True on success, else false.</returns>
		protected virtual bool LoadEquipmentTemplate(String templateID)
		{
			if (!Util.IsEmpty(templateID))
			{
				GameNpcInventoryTemplate inventoryTemplate = new GameNpcInventoryTemplate();
				
				if (inventoryTemplate.LoadFromDatabase(templateID))
				{
					Inventory = new GameNPCInventory(inventoryTemplate);
					InventoryItem item;
					if ((item = Inventory.GetItem(eInventorySlot.TwoHandWeapon)) != null)
					{
						item.DPS_AF = (int)(Level * 3.3);
						item.SPD_ABS = 50;
						
						SwitchWeapon(eActiveWeaponSlot.TwoHanded);
					}
					else
					{
						if ((item = Inventory.GetItem(eInventorySlot.RightHandWeapon)) != null)
						{
							item.DPS_AF = (int)(Level * 3.3);
							item.SPD_ABS = 37;
						}
						if ((item = Inventory.GetItem(eInventorySlot.LeftHandWeapon)) != null)
						{
							item.DPS_AF = (int)(Level * 3.3);
							item.SPD_ABS = 37;
						}
						
						SwitchWeapon(eActiveWeaponSlot.Standard);
					}
				}
				
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Pet stayed out of range for too long, despawn it.
		/// </summary>
		public void CutTether()
		{
			if (Owner == null)
				return;
			
			Brain.Stop();
			Owner.Notify(GameNPCEvent.PetLost);
			Die(null);
		}
	}
}
