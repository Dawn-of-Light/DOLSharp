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
using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.GS.Spells;
using DOL.GS.Styles;

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
			UpdateNPCEquipmentAppearance();
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
					UpdateNPCEquipmentAppearance();
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
			get { return (Brain as IControlledBrain).GetPlayerOwner().Effectiveness; }
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
					default: return (Brain as IControlledBrain).GetPlayerOwner().GetModifiedSpecLevel(keyName);
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
			get { return (Brain as IControlledBrain).GetPlayerOwner().GetModified(eProperty.CriticalSpellHitChance); }
			set { }
		}

		#endregion

		#region Stats

		/// <summary>
		/// Pet strength is determined by using template stength as a percentage multiplier
		/// So A template value of 50 would mean pet strength is 50% of normal, 150 = 150% of normal, etc
		/// </summary>
		public override short Strength
		{
			get
			{
				short str = (short)(20 + Level * 6);
				if (base.Strength > 0)
				{
					str = (short)(str * base.Strength * .01);
				}

				return Math.Max((short)1, str);
			}
		}

		/// <summary>
		/// Base constitution.
		/// </summary>
		public override short Constitution
		{
			get
			{
				if (base.Constitution == 0)
					return 30;
				else
					return base.Constitution;
			}
		}

		/// <summary>
		/// Base dexterity.
		/// </summary>
		public override short Dexterity
		{
			get
			{
				if (base.Dexterity == 0)
					return 30;
				else
					return base.Dexterity;
			}
		}

		/// <summary>
		/// Base quickness.
		/// </summary>
		public override short Quickness
		{
			get
			{
				if (base.Quickness == 0)
					return 30;
				else
					return base.Quickness;
			}
		}

		public override int MaxHealth
		{
			get
			{
				int hp = base.MaxHealth;
				double hpPercent = 1.0;

				// apply boosted hp reduction
				if (Constitution > 0 && Constitution < 30)
				{
					hpPercent = (Constitution * 3.4) * .01;
				}

				return (int)(hp * hpPercent);
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
					if (item != null)
						weaponSpeed += item.SPD_ABS;
					else
				{
					weaponSpeed += 34;
				}
				weaponSpeed = (weapons.Length > 0) ? weaponSpeed / weapons.Length : 34.0;
			}
			else
			{
				weaponSpeed = 34.0;
			}

			double speed = 100 * weaponSpeed * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);
			return (int)(speed * GetModified(eProperty.MeleeSpeed) * 0.01);
		}

		/// <summary>
		/// Whether or not pet can use left hand weapon.
		/// </summary>
		public override bool CanUseLefthandedWeapon
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Calculates how many times left hand can swing.
		/// </summary>
		/// <returns></returns>
		public override int CalculateLeftHandSwingCount()
		{
			return 0;
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

		/// <summary>
		/// Get weapon skill for the pet (for formula see Spydor's Web,
		/// http://daoc.nisrv.com/modules.php?name=Weapon_Skill_Calc).
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override double GetWeaponSkill(InventoryItem weapon)
		{
			if (weapon == null)
				return base.GetWeaponSkill(weapon);

			double factor = 1.9;
			double baseWS = 380;
			return ((GetWeaponStat(weapon) - 50) * factor + baseWS) * (1 + WeaponSpecLevel(weapon) / 100);
		}

		/// <summary>
		/// Weapon specialisation is up to level, if a weapon is equipped.
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override int WeaponSpecLevel(InventoryItem weapon)
		{
			return (weapon != null) ? Level : base.WeaponSpecLevel(weapon);
		}

		#endregion

		public override void Die(GameObject killer)
		{
			GameEventMgr.Notify(GameLivingEvent.PetReleased, this);
			base.Die(killer);
			CurrentRegion = null;
		}
		
		/// <summary>
		/// Spawn texts are in database
		/// </summary>
		protected override void BuildAmbientTexts()
		{
			base.BuildAmbientTexts();
			
			// also add the pet specific ambient texts if none found
			if (ambientTexts.Count == 0)
				ambientTexts = GameServer.Database.SelectObjects<MobXAmbientBehaviour>("`Source` ='pet';");
		}

		public override bool IsObjectGreyCon(GameObject obj)
		{
			GameObject tempobj = obj;
			if (Brain is IControlledBrain)
			{
				GamePlayer player = (Brain as IControlledBrain).GetPlayerOwner();
				if (player != null)
					tempobj = player;
			}
			return base.IsObjectGreyCon(tempobj);
		}
	}
}
