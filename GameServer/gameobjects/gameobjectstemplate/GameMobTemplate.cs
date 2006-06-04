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
using System.Collections;
using System.Text;
using System.Threading;
using DOL.GS;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class is the template class for all GameMob
	/// </summary>
	public class GameMobTemplate : GameNPCTemplate
	{
		#region FactionID / RespawnInterval (GameMob properties)

		/// <summary>
		/// The faction if of this template
		/// </summary>
		private int m_factionID;

		/// <summary>
		/// Gets or sets the faction id
		/// </summary>
		public int FactionID
		{
			get { return m_factionID; }
			set	{ m_factionID = value; }
		}

		/// <summary>
		/// The time to wait before each mob respawn
		/// </summary>
		protected int m_respawnInterval;

		/// <summary>
		/// The Respawn Interval of this mob in milliseconds
		/// </summary>
		public virtual int RespawnInterval
		{
			get { return m_respawnInterval; }
			set { m_respawnInterval = value; }
		}
		#endregion
		
		/// <summary>
		/// Create a GameMob based on this template
		/// </summary>
		public override GameObject CreateInstance()
		{
			GameMob obj = new GameMob();
			obj.Name = m_Name;
			obj.Realm = m_Realm;
			obj.Model = m_Model;
            obj.Level = (byte)Util.Random(m_minLevel, m_maxLevel);
			obj.GuildName = m_guildName;
			obj.MaxSpeedBase = m_maxSpeedBase;
            obj.Size = (byte)Util.Random(m_minSize, m_maxSize);
			obj.Flags = m_flags;
			obj.MeleeDamageType = m_meleeDamageType;
			obj.EvadeChance = m_evadeChance;
			obj.BlockChance = m_blockChance;
			obj.ParryChance = m_parryChance;
			obj.LeftHandSwingChance = m_leftHandSwingChance;
			obj.Inventory = NPCInventoryMgr.GetNPCInventory(m_inventoryID);
			if (obj.Inventory != null)
			{
				if (obj.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
					obj.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
				else if (obj.Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
					obj.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			}
			obj.OwnBrain = m_ownBrainTemplate.CreateInstance();
			obj.OwnBrain.Body = obj;
			obj.Faction = FactionMgr.GetFaction(m_factionID);
			obj.RespawnInterval = m_respawnInterval;
			return obj;
		}
	}
}
