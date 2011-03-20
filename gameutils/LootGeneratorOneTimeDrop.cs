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
using System.Collections.Generic;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// LootGeneratorOneTimeDrop
	/// This implementation make the loot drop only one time by player
	/// </summary>
	public class LootGeneratorOneTimeDrop : LootGeneratorBase
	{
		/// <summary>
		///
		/// </summary>
		protected static Dictionary<string, List<LootOTD>> m_mobOTDList = new Dictionary<string,List<LootOTD>>();

		public LootGeneratorOneTimeDrop()
		{
			PreloadLootOTDs();
		}

		public static void ReloadLootOTD()
		{
			PreloadLootOTDs();
		}

		protected static bool PreloadLootOTDs()
		{
			lock (m_mobOTDList)
			{
				m_mobOTDList.Clear();
				IList<LootOTD> lootOTDs;

				try
				{
					lootOTDs = GameServer.Database.SelectAllObjects<LootOTD>();
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
					{
						log.Error("LootGeneratorOneTimeDrop: Drops could not be loaded:", e);
					}

					return false;
				}

				if (lootOTDs != null && lootOTDs.Count > 0)
				{
					int count = 0;

					foreach (LootOTD l in lootOTDs)
					{
						IList<Mob> mobs = GameServer.Database.SelectObjects<Mob>("Name = '" + GameServer.Database.Escape(l.MobName) + "'");

						if (mobs == null || mobs.Count == 0)
						{
							log.ErrorFormat("Can't find MobName {0} for OTD {1}", l.MobName, l.ItemTemplateID);
							continue;
						}

						ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(l.ItemTemplateID);

						if (item == null)
						{
							log.ErrorFormat("Can't find ItemTemplate {0} for OTD MobName {1}", l.ItemTemplateID, l.MobName);
							continue;
						}

						if (m_mobOTDList.ContainsKey(l.MobName.ToLower()))
						{
							List<LootOTD> drops = m_mobOTDList[l.MobName.ToLower()];

							if (drops.Contains(l) == false)
							{
								drops.Add(l);
								count++;
							}
							else
							{
								log.ErrorFormat("Same OTD ItemTemplate {0} specified multiple times for MobName {1}", l.ItemTemplateID, l.MobName);
							}
						}
						else
						{
							List<LootOTD> drops = new List<LootOTD>();
							drops.Add(l);
							m_mobOTDList.Add(l.MobName.ToLower(), drops);
							count++;
						}

					}

					log.InfoFormat("One Time Drop generator pre-loaded {0} drops.", count);
				}
			}

			return true;
		}


		/// <summary>
		/// Refresh the OTDs for this mob
		/// </summary>
		/// <param name="mob"></param>
		public override void Refresh(GameNPC mob)
		{
			if (mob == null)
				return;

			IList<LootOTD> otds = GameServer.Database.SelectObjects<LootOTD>("MobName = '" + GameServer.Database.Escape(mob.Name) + "'");

			lock (m_mobOTDList)
			{
				if (m_mobOTDList.ContainsKey(mob.Name.ToLower()))
				{
					m_mobOTDList.Remove(mob.Name.ToLower());
				}
			}

			if (otds != null)
			{
				lock (m_mobOTDList)
				{
					List<LootOTD> newList = new List<LootOTD>();

					foreach (LootOTD otd in otds)
					{
						newList.Add(otd);
					}

					m_mobOTDList.Add(mob.Name.ToLower(), newList);
				}
			}
		}


		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);
			List<LootOTD> lootOTDs = null;

			try
			{
				if (m_mobOTDList.ContainsKey(mob.Name.ToLower()))
				{
					lootOTDs = m_mobOTDList[mob.Name.ToLower()];
				}

				if (lootOTDs != null)
				{
					foreach (GameObject gainer in mob.XPGainers.Keys)
					{
						GamePlayer player = null;

						if (gainer is GamePlayer)
						{
							player = gainer as GamePlayer;
						}
						else if (gainer is GameNPC)
						{
							IControlledBrain brain = ((GameNPC)gainer).Brain as IControlledBrain;
							if (brain != null)
							{
								player = brain.GetPlayerOwner();
							}
						}

						if (player != null)
						{
							foreach (LootOTD drop in lootOTDs)
							{
								if (drop.MinLevel <= player.Level)
								{
									CharacterXOneTimeDrop hasDrop = GameServer.Database.SelectObject<CharacterXOneTimeDrop>("CharacterID = '" + GameServer.Database.Escape(player.QuestPlayerID) + "' AND ItemTemplateID = '" + GameServer.Database.Escape(drop.ItemTemplateID) + "'");

									if (hasDrop == null)
									{
										ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(drop.ItemTemplateID);

										if (item != null)
										{
											if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, GameInventoryItem.Create<ItemTemplate>(item)))
											{
												CharacterXOneTimeDrop charXDrop = new CharacterXOneTimeDrop();
												charXDrop.CharacterID = player.QuestPlayerID;
												charXDrop.ItemTemplateID = drop.ItemTemplateID;
												GameServer.Database.AddObject(charXDrop);

												player.Out.SendMessage(string.Format("You receive {0} from {1}!", item.GetName(1, false), mob.GetName(1, false)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
											}
											else
											{
												// do not drop, player will have to try again
												player.Out.SendMessage("Your inventory is full and a one time drop cannot be added!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
												log.DebugFormat("OTD Failed, Inventory full: {0} from mob {1} for player {2}.", drop.ItemTemplateID, drop.MobName, player.Name);
												break;
											}
										}
										else
										{
											log.ErrorFormat("Error trying to drop ItemTemplate {0} from {1}.  Item not found.", drop.ItemTemplateID, drop.MobName);
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("LootGeneratorOneTimeDrop exception for mob " + mob.Name + ":", ex);
			}

			return loot;
		}
	}
}
