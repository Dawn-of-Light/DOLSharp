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
using System.Collections.Generic;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents an inventory item
	/// </summary>
	public class GuildBannerItem : GameInventoryItem
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Guild m_ownerGuild = null;
		private GamePlayer m_summonPlayer = null;

		public GuildBannerItem()
			: base()
		{
		}

		public GuildBannerItem(ItemTemplate template)
			: base(template)
		{
		}

		public GuildBannerItem(InventoryItem item)
			: base(item)
		{
			OwnerID = item.OwnerID;
			ObjectId = item.ObjectId;
		}

		/// <summary>
		/// What guild owns this banner
		/// </summary>
		public Guild OwnerGuild
		{
			get { return m_ownerGuild; }
			set { m_ownerGuild = value; }
		}

		public GamePlayer SummonPlayer
		{
			get { return m_summonPlayer; }
			set { m_summonPlayer = value; }
		}

		/// <summary>
		/// Player receives this item (added to players inventory)
		/// </summary>
		/// <param name="player"></param>
		public override void OnReceive(GamePlayer player)
		{
			if (player != SummonPlayer)
			{
				// for guild banners we don't actually add it to inventory but instead register
				// if it is rescued by a friendly player or taken by the enemy

				player.Inventory.RemoveItem(this);

				int trophyModel = 0;
				eRealm realm = eRealm.None;

				switch (Model)
				{
					case 3223:
						trophyModel = 3359;
						realm = eRealm.Albion;
						break;
					case 3224:
						trophyModel = 3361;
						realm = eRealm.Midgard;
						break;
					case 3225:
						trophyModel = 3360;
						realm = eRealm.Hibernia;
						break;
				}

				// if picked up by an enemy then turn this into a trophy
				if (realm != player.Realm)
				{
					ItemUnique template = new ItemUnique(Template);
					template.ClassType = "";
					template.Model = trophyModel;
					template.IsDropable = true;
					template.IsIndestructible = false;

					GameServer.Database.AddObject(template);
					GameInventoryItem trophy = new GameInventoryItem(template);
					player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, trophy);
					OwnerGuild.SendMessageToGuildMembers(player.Name + " of " + GlobalConstants.RealmToName(player.Realm) + " has captured your guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				}
				else
				{
					// A friendly player has picked up the banner.
					if (OwnerGuild != null)
					{
						OwnerGuild.GuildBanner = true;
						OwnerGuild.SendMessageToGuildMembers(player.Name + " has recovered your guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Player has dropped, traded, or otherwise lost this item
		/// </summary>
		/// <param name="player"></param>
		public override void OnLose(GamePlayer player)
		{
			if (player.Guild != null && Template.Id_nb == "GuildBanner_" + player.Guild.GuildID && player.GuildBanner != null)
			{
				player.GuildBanner.Stop();
			}
		}



		/// <summary>
		/// Drop this item on the ground
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override WorldInventoryItem Drop(GamePlayer player)
		{
			return null;
		}


		/// <summary>
		/// Is this a valid item for this player?
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckValid(GamePlayer player)
		{
			return false;
		}
	}
}
