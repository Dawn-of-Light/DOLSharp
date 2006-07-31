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
using DOL.Database;
using DOL.GS.Scripts;
using log4net;
using DOL.GS.Styles;
using System.Collections;

namespace DOL.GS
{
	/// <summary>
	/// A npc template
	/// </summary>
	public class NpcTemplate : INpcTemplate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected int 			m_templateId;
		protected string		m_name;
		protected string		m_guildName;
		protected ushort		m_model;
		protected byte			m_size;
		protected short			m_maxSpeed;
		protected byte 			m_parryChance;
		protected byte 			m_evadeChance;
		protected byte 			m_blockChance;
		protected byte 			m_leftHandSwingChance;
		protected uint			m_flags;
		protected IGameInventory	m_inventory;
		protected eDamageType	m_meleeDamageType;
		protected readonly IList m_styles;
		protected readonly IList m_spells;

		/// <summary>
		/// Constructs a new NpcTemplate
		/// </summary>
		/// <param name="data">The source npc template data</param>
		public NpcTemplate(DBNpcTemplate data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			m_templateId = data.TemplateId;
			m_name = data.Name;
			m_guildName = data.GuildName;
			m_model = data.Model;
			m_size = data.Size;
			m_maxSpeed = data.MaxSpeed;
			m_parryChance = data.ParryChance;
			m_evadeChance = data.EvadeChance;
			m_blockChance = data.BlockChance;
			m_leftHandSwingChance = data.LeftHandSwingChance;

			string[] splitedSpells=data.Spells.Split('|');
			IList spells = SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells);
			m_spells = new ArrayList();
			if (spells != null)
			{
				foreach (string id in splitedSpells)
				{
					foreach (Spell spell in spells) 
					{
						if (Convert.ToString(spell.ID) == id)
						{
							m_spells.Add(spell);
							break;
						}
					}
				}
			}

			//TODO : style system for mob
			if (data.Styles!=null && data.Styles!="")
			{
				string[] splitedStyles=data.Styles.Split('|');
				m_styles = new ArrayList();
				Style style=null;
				foreach(string st in splitedStyles)
				{
					style=SkillBase.GetStyleByID(Convert.ToInt32(st),0);
					if (style!=null)
					{
						m_styles.Add(style);
					}
				}
			}
			
			if (data.Ghost)
			{
				m_flags |= (int)GameNPC.eFlags.TRANSPARENT;
			}

			m_meleeDamageType = (eDamageType)data.MeleeDamageType;
			if (data.MeleeDamageType == 0)
				m_meleeDamageType = eDamageType.Slash;

			GameNpcInventoryTemplate temp = new GameNpcInventoryTemplate();
			if (data.EquipmentTemplateID != "" && !temp.LoadFromDatabase(data.EquipmentTemplateID))
			{
				temp = null;
				if (log.IsErrorEnabled)
					log.ErrorFormat("Failed to load mob template equipment '{0}'", data.EquipmentTemplateID);
			}
			else
			{
				temp = temp.CloseTemplate();
			}
			m_inventory = temp;
		}

		/// <summary>
		/// Gets the npc template ID
		/// </summary>
		public int TemplateId { get { return m_templateId; } }
		/// <summary>
		/// Gets the template npc name
		/// </summary>
		public string Name { get { return m_name; } }
		/// <summary>
		/// Gets the template npc guild name
		/// </summary>
		public string GuildName { get { return m_guildName; } }
		/// <summary>
		/// Gets the template npc model
		/// </summary>
		public ushort Model { get { return m_model; } }
		/// <summary>
		/// Gets the template npc size
		/// </summary>
		public byte Size { get { return m_size; } }

		/// <summary>
		/// Gets the template npc max speed
		/// </summary>
		public short MaxSpeed { get { return m_maxSpeed; } }

		/// <summary>
		/// Gets the template npc flags
		/// </summary>
		public uint Flags { get { return m_flags; } }
		/// <summary>
		/// Gets the template npc inventory
		/// </summary>
		public IGameInventory Inventory { get { return m_inventory; } }
		/// <summary>
		/// Gets the template npc melee damage type
		/// </summary>
		public eDamageType MeleeDamageType { get { return m_meleeDamageType; } }
		/// <summary>
		/// Gets the template npc parry chance
		/// </summary>
		public byte ParryChance { get { return m_parryChance; } }
		/// <summary>
		/// Gets the template npc evade chance
		/// </summary>
		public byte EvadeChance { get { return m_evadeChance; } }
		/// <summary>
		/// Gets the template npc block chance
		/// </summary>
		public byte BlockChance { get { return m_blockChance; } }

		/// <summary>
		/// Gets the template npc left hand swing chance
		/// </summary>
		public byte LeftHandSwingChance { get { return m_leftHandSwingChance; } }

		/// <summary>
		/// Gets the template npc spells name array 
		/// </summary>
		public IList Spells{ get { return m_spells; } }

		/// <summary>
		/// Gets the template npc styles name array 
		/// </summary>
		public IList Styles{ get { return m_styles; } }

	}
}
