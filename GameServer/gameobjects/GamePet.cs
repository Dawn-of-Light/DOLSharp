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
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.AI;

namespace DOL.GS
{
	public class GamePet : GameNPC
	{
		new private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public GamePet(INpcTemplate template) : base(template)
		{
			if (Inventory != null)
			{
				if (Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
					SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
				else if (Inventory.GetItem(eInventorySlot.RightHandWeapon) != null)
					SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
				else if (Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
					SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			}
			AddStatsToWeapon();
			BroadcastLivingEquipmentUpdate();
		}

        public GamePet(ABrain brain) : base(brain)
        {

        }

		public GameLiving Owner
		{
			get
			{
				if (Brain is IControlledBrain)
				{
					return (Brain as IControlledBrain).Owner;
				}

				return null;
			}
		}


		#region Inventory

		/// <summary>
		/// Load equipment for the pet.
		/// </summary>
		/// <param name="templateID">Equipment Template ID.</param>
		/// <returns>True on success, else false.</returns>
		protected virtual void AddStatsToWeapon()
		{
			if (Inventory != null)
			{
				InventoryItem item;
				if ((item = Inventory.GetItem(eInventorySlot.TwoHandWeapon)) != null)
				{
					item.DPS_AF = (int)(Level * 3.3);
					item.SPD_ABS = 50;
				}
				if ((item = Inventory.GetItem(eInventorySlot.RightHandWeapon)) != null)
				{
					item.DPS_AF = (int)(Level * 3.3);
					item.SPD_ABS = 37;
				}
				if ((item = Inventory.GetItem(eInventorySlot.LeftHandWeapon)) != null)
				{
					item.DPS_AF = (int)(Level * 3.3);
					item.SPD_ABS = 50;
				}
				if ((item = Inventory.GetItem(eInventorySlot.DistanceWeapon)) != null)
				{
					item.DPS_AF = (int)(Level * 3.3);
					item.SPD_ABS = 50;
					SwitchWeapon(eActiveWeaponSlot.Distance);
					BroadcastLivingEquipmentUpdate();
				}
			}
		}

		#endregion

		#region Shared Melee & Spells

		/// <summary>
		/// Multiplier for melee and magic.
		/// </summary>
		public override double Effectiveness
		{
			get 
            {
                GameLiving gl = (Brain as IControlledBrain).GetLivingOwner();
                if (gl != null)
                    return gl.Effectiveness;

                return 1.0;
            }
		}

		/// <summary>
		/// Specialisation level including item bonuses and RR.
		/// </summary>
		/// <param name="keyName">The specialisation line.</param>
		/// <returns>The specialisation level.</returns>
		public override int GetModifiedSpecLevel(string keyName)
		{
			switch (keyName)
			{
				case Specs.Slash:
				case Specs.Crush:
				case Specs.Two_Handed:
				case Specs.Shields:
				case Specs.Critical_Strike:
				case Specs.Large_Weapons:
					return Level;
                default: return (Brain as IControlledBrain).GetLivingOwner().GetModifiedSpecLevel(keyName);
			}
		}

		#endregion

		#region Spells

		/// <summary>
		/// Called when spell has finished casting.
		/// </summary>
		/// <param name="handler"></param>
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			base.OnAfterSpellCastSequence(handler);
			Brain.Notify(GameNPCEvent.CastFinished, this, new CastingEventArgs(handler));
		}

		/// <summary>
		/// Scale the passed spell according to PET_SCALE_SPELL_MAX_LEVEL
		/// </summary>
		/// <param name="spell">The spell to scale</param>
		/// <returns>The scaled spell</returns>
		public Spell ScalePetSpell(Spell spell)
		{
			if (ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL <= 0)
				return spell;

			Spell scaledSpell = spell;
			double CasterLevel = Level;

			// Cap the level we scale BD minions' spell effects to the player's modified spec for the spec line the pet is from
			if (this is BDSubPet subpet && subpet.Owner is CommanderPet commander && commander.Owner is GamePlayer player)
				CasterLevel = Math.Min(subpet.Level, player.GetModifiedSpecLevel(subpet.PetSpecLine));

			switch (spell.SpellType.ToString().ToLower())
			{
				// Scale Damage
				case "damageovertime":
				case "damageshield":
				case "damageadd":
				case "directdamage":
				case "directdamagewithdebuff":
				case "lifedrain":
				case "damagespeeddecrease":
				case "stylebleeding": // Style Effect
					scaledSpell.Damage *= CasterLevel / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL;
					break;
				// Scale Value
				case "enduranceregenbuff":
				case "enduranceheal":
				case "endurancedrain":
				case "powerregenbuff":
				case "powerheal":
				case "powerdrain":
				case "powerhealthenduranceregenbuff":
				case "combatspeedbuff":
				case "hastebuff":
				case "celeritybuff":
				case "combatspeeddebuff":
				case "hastedebuff":
				case "heal":
				case "combatheal":
				case "healthregenbuff":
				case "healovertime":
				case "constitutionbuff":
				case "dexteritybuff":
				case "strengthbuff":
				case "constitutiondebuff":
				case "dexteritydebuff":
				case "strengthdebuff":
				case "armorfactordebuff":
				case "armorfactorbuff":
				case "armorabsorptionbuff":
				case "armorabsorptiondebuff":
				case "dexterityquicknessbuff":
				case "strengthconstitutionbuff":
				case "dexterityquicknessdebuff":
				case "strengthconstitutiondebuff":
				case "taunt":
				case "unbreakablespeeddecrease":
				case "speeddecrease":
				case "stylecombatspeeddebuff": // Style Effect
				case "stylespeeddecrease": // Style Effect
				//case "styletaunt":  Taunt styles already scale with damage, leave their values alone.
					scaledSpell.Value *= CasterLevel / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL;
					break;
				// Scale Duration
				case "disease":
				case "stun":
				case "unrresistablenonimunitystun":
				case "mesmerize":
				case "stylestun": // Style Effect
					scaledSpell.Duration = (int)Math.Ceiling(spell.Duration * CasterLevel / ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL);
					break;
				default: break; // Don't mess with types we don't know
			} // switch (m_spell.SpellType.ToString().ToLower())

			return scaledSpell;
		}

		#endregion

		#region Stats
		/// <summary>
		/// Set stats according to PET_AUTOSET values, then scale them according to the values in the DB
		/// </summary>
		public override void AutoSetStats()
		{
			// Assign values from Autoset
			Strength = (short)Math.Max(1, Properties.PET_AUTOSET_STR_BASE);
			Constitution = (short)Math.Max(1, Properties.PET_AUTOSET_CON_BASE);
			Quickness = (short)Math.Max(1, Properties.PET_AUTOSET_QUI_BASE);
			Dexterity = (short)Math.Max(1, Properties.PET_AUTOSET_DEX_BASE);
			Intelligence = (short)Math.Max(1,Properties.PET_AUTOSET_INT_BASE);
			Empathy = (short)30;
			Piety = (short)30;
			Charisma = (short)30;
			
			// Now add stats for levelling
			Strength += (short)Math.Round(10.0 * (Level - 1) * Properties.PET_AUTOSET_STR_MULTIPLIER);
			Constitution += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_CON_MULTIPLIER);
			Quickness += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_QUI_MULTIPLIER);
			Dexterity += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_DEX_MULTIPLIER);
			Intelligence += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_INT_MULTIPLIER);
			Empathy += (short)(Level - 1);
			Piety += (short)(Level - 1);
			Charisma += (short)(Level - 1);

			// Now scale them according to NPCTemplate values
			if (NPCTemplate != null)
			{
				if (NPCTemplate.Strength > 0)
					Strength = (short)Math.Round(Strength * (NPCTemplate.Strength / 100.0));
				if (NPCTemplate.Constitution > 0)
					Constitution = (short)Math.Round(Constitution * (NPCTemplate.Constitution / 100.0));
				if (NPCTemplate.Quickness > 0)
					Quickness = (short)Math.Round(Quickness * (NPCTemplate.Quickness / 100.0));
				if (NPCTemplate.Dexterity > 0)
					Dexterity = (short)Math.Round(Dexterity * (NPCTemplate.Dexterity / 100.0));
				if (NPCTemplate.Intelligence > 0)
					Intelligence = (short)Math.Round(Intelligence * (NPCTemplate.Intelligence / 100.0));
				// Except for CHA, EMP, AND PIE as those don't have autoset values.
				if (NPCTemplate.Empathy > 0)
					Empathy = (short)NPCTemplate.Empathy;
				if (NPCTemplate.Piety > 0)
					Piety = (short)NPCTemplate.Piety;
				if (NPCTemplate.Charisma > 0)
					Charisma = (short)NPCTemplate.Charisma;
			}
		}
		#endregion

		#region Melee

		/// <summary>
		/// The type of damage the currently active weapon does.
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override eDamageType AttackDamageType(InventoryItem weapon)
		{
			if (weapon != null)
			{
				switch ((eWeaponDamageType)weapon.Type_Damage)
				{
						case eWeaponDamageType.Crush: return eDamageType.Crush;
						case eWeaponDamageType.Slash: return eDamageType.Slash;
				}
			}

			return eDamageType.Crush;
		}

		/// <summary>
		/// Get melee speed in milliseconds.
		/// </summary>
		/// <param name="weapons"></param>
		/// <returns></returns>
		public override int AttackSpeed(params InventoryItem[] weapons)
		{
			double weaponSpeed = 0.0;

			if (weapons != null)
			{
				foreach (InventoryItem item in weapons)
				{
					if (item != null)
					{
						weaponSpeed += item.SPD_ABS;
					}
					else
					{
						weaponSpeed += 34;
					}
				}

				weaponSpeed = (weapons.Length > 0) ? weaponSpeed / weapons.Length : 34.0;
			}
			else
			{
				weaponSpeed = 34.0;
			}

			double speed = 100 * weaponSpeed * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);
			return (int)Math.Max(500.0, (speed * (double)GetModified(eProperty.MeleeSpeed) * 0.01)); // no bonus is 100%, opposite how players work
		}

		/// <summary>
		/// Calculate how fast this pet can cast a given spell
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public override int CalculateCastingTime(SpellLine line, Spell spell)
		{
			int ticks = spell.CastTime;

			double percent = DexterityCastTimeReduction;
			percent -= GetModified(eProperty.CastingSpeed) * .01;

			ticks = (int)(ticks * Math.Max(CastingSpeedReductionCap, percent));
			if (ticks < MinimumCastingSpeed)
				ticks = MinimumCastingSpeed;

			return ticks;
		}

		/// <summary>
		/// Pick a random style for now.
		/// </summary>
		/// <returns></returns>
		protected override Style GetStyleToUse()
		{
			if (Styles != null && Styles.Count > 0 && Util.Chance(Properties.GAMENPC_CHANCES_TO_STYLE + Styles.Count))
			{
				Style style = (Style)Styles[Util.Random(Styles.Count - 1)];
				if (StyleProcessor.CanUseStyle(this, style, AttackWeapon))
					return style;
			}

			return base.GetStyleToUse();
		}
		#endregion

		public override void Die(GameObject killer)
		{
			StripBuffs();
		
			GameEventMgr.Notify(GameLivingEvent.PetReleased, this);
			base.Die(killer);
			CurrentRegion = null;
		}

		/// <summary>
		/// Targets the pet has buffed, to allow correct buff removal when the pet dies
		/// </summary>
		private List<GameLiving> m_buffedTargets = null;

		/// <summary>
		/// Add a target to the pet's list of buffed targets
		/// </summary>
		/// <param name="living">Target to add to the list</param>
		public void AddBuffedTarget(GameLiving living)
		{
			if (living == this)
				return;

			if (m_buffedTargets == null)
				m_buffedTargets = new List<GameLiving>(1);

			if (!m_buffedTargets.Contains(living))
				m_buffedTargets.Add(living);
		}

		/// <summary>
		/// Strips any buffs this pet cast
		/// </summary>
		public virtual void StripBuffs()
		{
			if (m_buffedTargets != null)
				foreach (GameLiving living in m_buffedTargets)
					if (living != this && living.EffectList != null)
						foreach (IGameEffect effect in living.EffectList)
							if (effect is GameSpellEffect spellEffect && spellEffect.SpellHandler != null 
								&& spellEffect.SpellHandler.Caster != null && spellEffect.SpellHandler.Caster == this)
								effect.Cancel(false);
		}
		
		/// <summary>
		/// Spawn texts are in database
		/// </summary>
		protected override void BuildAmbientTexts()
		{
			base.BuildAmbientTexts();
			
			// also add the pet specific ambient texts if none found
			if (ambientTexts.Count == 0)
				ambientTexts = GameServer.Instance.NpcManager.AmbientBehaviour["pet"];
		}

		public override bool IsObjectGreyCon(GameObject obj)
		{
			GameObject tempobj = obj;
			if (Brain is IControlledBrain)
			{
                GameLiving player = (Brain as IControlledBrain).GetLivingOwner();
				if (player != null)
					tempobj = player;
			}
			return base.IsObjectGreyCon(tempobj);
		}
	}
}
