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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// LootGeneratorOneTimeDrop
	/// This implementation make the loot drop only one time by player
	/// </summary>
	public class LootGeneratorOneTimeDrop : LootGeneratorBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///
		/// </summary>
		protected static HybridDictionary m_OTDXMob = null;

		/// <summary>
		/// Constrcut a new One Time Drop Loot Generator and load it's values from database.
		/// </summary>
		public LootGeneratorOneTimeDrop()
		{
			PreloadLootOTDs();
		}

		/// <summary>
		/// Loads the loottemplates
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadLootOTDs()
		{
			if (m_OTDXMob==null)
			{
				m_OTDXMob = new HybridDictionary(500);
				lock(m_OTDXMob)
				{
					DataObject[] m_lootOTDs=null;
					try
					{
						m_lootOTDs = GameServer.Database.SelectAllObjects(typeof(DBLootOTD));
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Loot One Time Drop: OTD could not be loaded:", e);
						return false;
					}

					if(m_lootOTDs != null)
					{
						foreach(DBLootOTD dbLootOTD in m_lootOTDs)
						{
							m_OTDXMob.Add(dbLootOTD.MobName,dbLootOTD);
						}
					}
				}
			}
			return true;
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);
			DBLootOTD lootOTD = (DBLootOTD) m_OTDXMob[mob.Name];
			foreach(GameObject gainer in mob.XPGainers.Keys)
			{
				GamePlayer player = null;
				if(gainer is GamePlayer)
					player = gainer as GamePlayer;
				else if (gainer is GameNPC)
				{
					IControlledBrain brain = ((GameNPC)gainer).Brain as IControlledBrain;
					if (brain != null)
						player = brain.Owner;
				}
				if ( player != null)
				{
					if ( (lootOTD.MinLevel < player.Level))
					{
						DataObject obj = GameServer.Database.SelectObject(typeof(DBOTDXCharacter), "CharacterName = '" + GameServer.Database.Escape(player.Name) + "' AND LootOTD_ID = '" + lootOTD.ObjectId + "'");
						if (obj != null) continue;

						string[] sclass = lootOTD.SerializedClassAllowed.Split(',');
						bool classFlag = false;
						foreach (string str in sclass)
						{
							if (str == player.CharacterClass.Name)
							{
								classFlag = true;
								break;
							}
						}
						if (!classFlag) continue;

						if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack,new InventoryItem(lootOTD.item)))
							player.Out.SendMessage("Your inventory is full!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

						DBOTDXCharacter OtdxChar = new DBOTDXCharacter();
						OtdxChar.CharacterName = player.Name;
						OtdxChar.LootOTD_ID = lootOTD.ObjectId;
						GameServer.Database.AddNewObject(OtdxChar);
					}
				}
			}
			return loot;//empty because all is done here because must appear in inventory
		}
	}
}
