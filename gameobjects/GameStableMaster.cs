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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Movement;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Stable master that sells and takes horse route tickes
	/// </summary>
	public class GameStableMaster : GameMerchant
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Called when the living is about to get an item from someone else
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			GamePlayer sourcePlayer = source as GamePlayer;
			TravelTicket ticket = item as TravelTicket;	
			if(sourcePlayer != null && ticket != null)
			{
				TripPath path = PathMgr.GetPath(ticket.TripPathID) as TripPath;
				if(path != null && path.Region == Region && path.StartingPoint != null && path.StartingPoint.NextPoint != null)
				{
					if(Position.CheckSquareDistance(path.StartingPoint.Position, 512 * 512))
					{
						sourcePlayer.Inventory.RemoveItem(item);

						GameSteed steed = new GameSteed();
						steed.Region = Region;
						steed.Position = path.StartingPoint.Position;
						steed.Heading = steed.Position.GetHeadingTo(path.StartingPoint.NextPoint.Position);
						steed.Name = path.SteedName;
						steed.Model = path.SteedModel;
						steed.Realm = source.Realm;
						steed.Level = 55;
						steed.Size = (byte)Util.Random(35, 75);
						steed.AddToWorld();

						sourcePlayer.MountSteed(steed);
						
						GameEventMgr.AddHandler(steed, GameSteedEvent.PathMoveEnds, new DOLEventHandler(OnSteedAtRouteEnd));					
						
						steed.MoveOnPath(path.StartingPoint.NextPoint);

						return true;
					}
				}
				
				sourcePlayer.Out.SendMessage("My horse doesn't know the route to this destination.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// Handles 'horse route end' events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="o"></param>
		/// <param name="args"></param>
		public void OnSteedAtRouteEnd(DOLEvent e, object o, EventArgs args)
		{
			GameSteed steed = o as GameSteed;
			if (steed == null) return;

			GameEventMgr.RemoveHandler(steed, GameSteedEvent.PathMoveEnds, new DOLEventHandler(OnSteedAtRouteEnd));
		
			if(steed.Rider != null)
			{
				steed.Rider.DismountSteed();
			}
			steed.RemoveFromWorld();
		}
	}
}
