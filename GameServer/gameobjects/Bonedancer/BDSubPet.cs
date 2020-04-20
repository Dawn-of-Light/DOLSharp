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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Database;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Styles;

namespace DOL.GS
{
	public class BDSubPet : BDPet
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the different subpet ids
		/// </summary>
		public enum SubPetType : byte
		{
			Melee = 0,
			Healer = 1,
			Caster = 2,
			Debuffer = 3,
			Buffer = 4,
			Archer = 5
		}

		public bool MinionsAssisting
		{ 
			get { return Owner is CommanderPet commander && commander.MinionsAssisting; } 
		}

		protected string m_PetSpecLine = null;
		/// <summary>
		/// Returns the spell line specialization this pet was summoned from
		/// </summary>
		public string PetSpecLine
		{
			get
			{
				// This is really inefficient, so only do it once, and only if we actually need it
				if (m_PetSpecLine == null && Brain is IControlledBrain brain && brain.GetPlayerOwner() is GamePlayer player)
				{
					// Get the spell that summoned this pet
					DBSpell dbSummoningSpell = GameServer.Database.SelectObject<DBSpell>("LifeDrainReturn=@TemplateId", new QueryParameter("@TemplateID", NPCTemplate.TemplateId));
					if (dbSummoningSpell != null)
					{
						// Figure out which spell line the summoning spell is from
						DBLineXSpell dbLineSpell = GameServer.Database.SelectObject<DBLineXSpell>("SpellID=@SpellID", new QueryParameter("@SpellID", dbSummoningSpell.SpellID));
						if (dbLineSpell != null)
						{
							// Now figure out what the spec name is
							SpellLine line = player.GetSpellLine(dbLineSpell.LineName);
							if (line != null)
								m_PetSpecLine = line.Spec;
						}
					}
				}

				return m_PetSpecLine;
			}
		}

		/// <summary>
		/// Create a minion.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public BDSubPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override short MaxSpeed
		{
			get
			{
				return (Brain as IControlledBrain).Owner.MaxSpeed;
			}
		}

		/// <summary>
		/// Changes the commander's weapon to the specified weapon template
		/// </summary>
		public void MinionGetWeapon(CommanderPet.eWeaponType weaponType)
		{
			ItemTemplate itemTemp = CommanderPet.GetWeaponTemplate(weaponType);

			if (itemTemp == null)
				return;

			InventoryItem weapon;

			weapon = GameInventoryItem.Create(itemTemp);
			if (weapon != null)
			{
				if (Inventory == null)
					Inventory = new GameNPCInventory(new GameNpcInventoryTemplate());
				else
					Inventory.RemoveItem(Inventory.GetItem((eInventorySlot)weapon.Item_Type));

				Inventory.AddItem((eInventorySlot)weapon.Item_Type, weapon);
				SwitchWeapon((eActiveWeaponSlot)weapon.Hand);
			}
		}

		/// <summary>
		/// Sort spells into specific lists, scaling pet spell as appropriate
		/// </summary>
		public override void SortSpells()
		{
			if (Spells.Count < 1 || Level < 1)
				return;

			if (DOL.GS.ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL < 1)
				base.SortSpells();
			else
			{
				int scaleLevel = Level;

				// Some minions have multiple spells, so only grab their owner's spec once per pet.
				if (DOL.GS.ServerProperties.Properties.PET_CAP_BD_MINION_SPELL_SCALING_BY_SPEC
					&& Brain is IControlledBrain brain && brain.GetPlayerOwner() is GamePlayer BD)
				{
					int spec = BD.GetModifiedSpecLevel(PetSpecLine);

					if (spec > 0 && spec < scaleLevel)
						scaleLevel = spec;
				}

				SortSpells(scaleLevel);
			}
		}

		/// <summary>
		/// Scale the passed spell according to PET_SCALE_SPELL_MAX_LEVEL, capping by BD spec if appropriate
		/// </summary>
		/// <param name="spell">The spell to scale</param>
		/// <param name="casterLevel">The level to scale the pet spell to, 0 to use pet level</param>
		public override void ScalePetSpell(Spell spell, int casterLevel = 0)
		{
			if (ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL < 1 || spell == null || Level < 1)
				return;

			if (casterLevel < 1)
			{
				casterLevel = Level;

				// Style procs and subspells can't be scaled in advance, so we need to check BD spec here as well				
				if (DOL.GS.ServerProperties.Properties.PET_CAP_BD_MINION_SPELL_SCALING_BY_SPEC
					&& Brain is IControlledBrain brain && brain.GetPlayerOwner() is GamePlayer BD)
				{
					int spec = BD.GetModifiedSpecLevel(PetSpecLine);

					if (spec > 0 && spec < casterLevel)
						casterLevel = spec;
				}
			}

			base.ScalePetSpell(spell, casterLevel);
		}
	}
}
