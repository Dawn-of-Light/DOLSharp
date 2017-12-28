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

		protected string m_PetSpecLine = null;
		/// <summary>
		/// Returns the spell line specialization this pet was summoned from
		/// </summary>
		public string PetSpecLine
		{
			get
			{
				// This is really inefficient, so only do it once, and only if we actually need it
				if (m_PetSpecLine == null && Owner is CommanderPet commander && commander.Owner is GamePlayer player)
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
		/// Create a commander.
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
	}
}
