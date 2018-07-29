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
		/// Returns the chance for a critical hit with a spell.
		/// </summary>
		public override int SpellCriticalChance
		{
			get { return (Brain as IControlledBrain).GetLivingOwner().GetModified(eProperty.CriticalSpellHitChance); }
			set { }
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
			StripOwnerBuffs(Owner);
		
			GameEventMgr.Notify(GameLivingEvent.PetReleased, this);
			base.Die(killer);
			CurrentRegion = null;
		}
		
		/// <summary>
		/// Strips any buffs this pet cast on owner
		/// </summary>
		/// <param name="owner">
		/// The target to strip buffs off of.
		/// </param>
		public virtual void StripOwnerBuffs(GameLiving owner)
		{
			if (owner != null & owner.EffectList != null)
			{
			   	foreach (IGameEffect effect in owner.EffectList)
				{
					if (effect != null && effect is GameSpellEffect)
					{
						GameSpellEffect spelleffect = effect as GameSpellEffect;
						if (spelleffect.SpellHandler != null && spelleffect.SpellHandler.Caster != null
							&& spelleffect.SpellHandler.Caster == this)
								spelleffect.Cancel(false);
					}
				}
			}
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
