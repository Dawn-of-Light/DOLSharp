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

		private bool m_hasHandlers = false;

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

		private void AddQuitHandlers(GamePlayer player)
		{
			GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuits));
			GameEventMgr.AddHandler(player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerQuits));
		}

		private void RemoveQuitHandlers(GamePlayer player)
		{
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuits));
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerQuits));
		}

		private void PlayerQuits(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				GamePlayer player = sender as GamePlayer;

				if (player != null)
				{
					player.Inventory.RemoveItem(this);
				}
				else
				{
					throw new Exception(sender.ToString());
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed to remove guild banner {0} from non guild player!  Sender: {1}", ex.Message);
			};
		}



		/// <summary>
		/// Player receives this item (added to players inventory)
		/// </summary>
		/// <param name="player"></param>
		public override void OnReceive(GamePlayer player)
		{
			if (player.Guild != null && Template.Id_nb == "GuildBanner_" + player.Guild.GuildID && player.GuildBanner == null)
			{
				// first off we make sure we've recorded that our guild has recovered a banner, if needed

				if (player.Guild != null && player.Guild.GuildBanner == false)
				{
					player.Guild.GuildBanner = true;
				}

				// now check to see if this is a valid summon, or if something went wrong

				if (player.Group == null && player.Client.Account.PrivLevel == (int)ePrivLevel.Player)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.BannerNoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.Inventory.RemoveItem(this);
					return;
				}

				foreach (GamePlayer guildPlayer in player.Guild.ListOnlineMembers())
				{
					if (guildPlayer.GuildBanner != null)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.BannerGuildSummoned"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
						player.Inventory.RemoveItem(this);
						return;
					}
				}

				if (player.Group != null)
				{
					foreach (GamePlayer groupPlayer in player.Group.GetPlayersInTheGroup())
					{
						if (groupPlayer.GuildBanner != null)
						{
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.BannerGroupSummoned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							player.Inventory.RemoveItem(this);
							return;
						}
					}
				}

				if (player.CurrentRegion.IsRvR)
				{
					GuildBanner banner = new GuildBanner(player);
					banner.Start();
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.BannerSummoned"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					player.Guild.UpdateGuildWindow();
				}
				else
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.BannerNotRvR"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					player.Inventory.RemoveItem(this);
				}
			}
			else
			{
				// use the model to determine if we are a group member or an enemy

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

					GameServer.Database.AddObject(template);
					player.Inventory.RemoveItem(this);
					GameInventoryItem trophy = new GameInventoryItem(template);
					player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, trophy);
				}
				else
				{
					// If a player duplicates a banner they will pick it up here and have duplicates
					// but the duplicates do nothing and will be removed on logout
					AddQuitHandlers(player);
					m_hasHandlers = true;
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

			if (m_hasHandlers)
			{
				RemoveQuitHandlers(player);
			}
		}



		/// <summary>
		/// Drop this item on the ground
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override WorldInventoryItem Drop(GamePlayer player)
		{
			WorldInventoryItem worldItem = WorldInventoryItem.CreateFromTemplate(this);

			Point2D itemloc = player.GetPointFromHeading(player.Heading, 30);
			worldItem.X = itemloc.X;
			worldItem.Y = itemloc.Y;
			worldItem.Z = player.Z;
			worldItem.Heading = player.Heading;
			worldItem.CurrentRegionID = player.CurrentRegionID;

			worldItem.AddOwner(player);

			if (player.Guild != null && Template.Id_nb == "GuildBanner_" + player.Guild.GuildID)
			{
				player.Guild.GuildBanner = false;
				player.Guild.SendMessageToGuildMembers(player.Name + " has dropped the guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			}

			worldItem.AddToWorld();

			return worldItem;
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
