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
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Quests;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a static Item in the gameworld
	/// </summary>
	public class StormWorldDataQuestItem : GameStaticItem
	{
		public override string Name
		{
			get
			{
				string[] name = base.Name.Split(';');

				if (name != null && name.Length > 1)
				{
					return name[0];
				}

				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override void SaveIntoDatabase()
		{
			WorldObject obj = null;
			if (InternalID != null)
			{
				obj = (WorldObject)GameServer.Database.FindObjectByKey<WorldObject>(InternalID);
			}
			if (obj == null)
			{
				if (LoadedFromScript == false)
				{
					obj = new WorldObject();
				}
				else
				{
					return;
				}
			}
			obj.Name = base.Name;
			obj.Model = Model;
			obj.Emblem = Emblem;
			obj.Realm = (byte)Realm;
			obj.Heading = Heading;
			obj.Region = CurrentRegionID;
			obj.X = X;
			obj.Y = Y;
			obj.Z = Z;
			obj.ClassType = this.GetType().ToString();
			obj.RespawnInterval = RespawnInterval;

			if (InternalID == null)
			{
				GameServer.Database.AddObject(obj);
				InternalID = obj.ObjectId;
			}
			else
			{
				GameServer.Database.SaveObject(obj);
			}
		}


		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(4);
			list.Insert(0, "You target " + GetName(0, false) + ".");
			ChatUtil.SendDebugMessage(player, base.Name);
			return list;
		}

		/// <summary>
		/// Let's remove this item from the world and give the player an item in their inventory
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (base.Interact(player))
			{
				string[] name = base.Name.Split(';');

				if (name.Length < 5)
				{
					ChatUtil.SendDebugMessage(player, "Object name not set correctly.  Format: 'Name;id_nb;DataQuestID;Step1;Step2'.  id_nb can be blank, steps can be the same or 0 if not used.");
					ChatUtil.SendDebugMessage(player, "If RespawnInterval is 0 then object will only vanish, temporarily, for the player.");
					return false;
				}

				string idnb = name[1]; // item can be blank
				int dataQuestID = 0;
				int step1 = -1;
				int step2 = -1;

				if (int.TryParse(name[2], out dataQuestID) == false)
				{
					ChatUtil.SendDebugMessage(player, "No dataquest ID specified or ID invalid!");
					return false;
				}

				if (int.TryParse(name[3], out step1) == false)
				{
					ChatUtil.SendDebugMessage(player, "Invalid step1 specified!");
					return false;
				}

				if (int.TryParse(name[4], out step2) == false)
				{
					ChatUtil.SendDebugMessage(player, "Invalid step2 specified!");
					return false;
				}

				CharacterXDataQuest dq = DataQuest.GetCharacterQuest(player, dataQuestID, false);

				if (dq == null || dq.Step == 0)
				{
					// player is not doing quest
					return false;
				}

				if (step1 != 0 && dq.Step < step1)
				{
					// player not yet to the correct step
					return false;
				}

				if (step2 != 0 && dq.Step > step2)
				{
					// player is past the max step allowed
					return false;
				}

				if (string.IsNullOrEmpty(idnb) == false)
				{
					lock (player.Inventory)
					{
						if (player.Inventory.IsSlotsFree(1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
						{
							ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(idnb);

							if (item == null)
							{
								ChatUtil.SendDebugMessage(player, string.Format("Can't find itemtemplate '{0}'!", idnb));
								return false;
							}

							InventoryItem inv = GameInventoryItem.Create<ItemTemplate>(item);
							player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inv);
							player.Out.SendMessage("You pick up " + GetName(0, false), GS.PacketHandler.eChatType.CT_Important, GS.PacketHandler.eChatLoc.CL_SystemWindow);
						}
						else
						{
							player.Out.SendMessage("Your inventory is full!", DOL.GS.PacketHandler.eChatType.CT_Important, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
							return false;
						}
					}
				}

				if (RespawnInterval > 0)
				{
					RemoveFromWorld(RespawnInterval);
				}
			}

			return false;
		}
	}
}
