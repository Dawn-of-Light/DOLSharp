
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
using DOL.Events;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Stable master that sells and takes horse route tickes
	/// </summary>
	public class GameBoatStableMaster : GameMerchant
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Called when the living is about to get an item from someone
		/// else
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			if (source is GamePlayer)
			{
				GamePlayer player = (GamePlayer)source;

				if (item.Name.ToLower().StartsWith("ticket to ") && item.Item_Type == 40)
				{
					foreach (GameNPC npc in GetNPCsInRadius(1500))
					{
						if (npc is GameHorseBoat)
						{
							player.Out.SendMessage("Please wait until the boat has departed before ordering a new one", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return false;
						}
					}

					String destination = item.Name.Substring(item.Name.IndexOf(" to ") + 4);
					PathPoint path = MovementMgr.LoadPath(item.TemplateID);
					//PathPoint path = MovementMgr.Instance.LoadPath(this.Name + "=>" + destination);
					if (path != null)
					{
						player.Inventory.RemoveCountFromStack(item, 1);

						GameHorseBoat boat = new GameHorseBoat();
						boat.Name = "Boat to " + destination;
						boat.Realm = source.Realm;
						boat.X = path.X;
						boat.Y = path.Y;
						boat.Z = path.Z;
						boat.CurrentRegion = CurrentRegion;
						boat.Heading = Point2D.GetHeadingToLocation(path, path.Next);
						boat.AddToWorld();
						boat.CurrentWayPoint = path;
						GameEventMgr.AddHandler(boat, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
						//new MountHorseAction(player, boat).Start(400);
						new HorseRideAction(boat).Start(30 * 1000);

						player.Out.SendMessage("I have summoned a boat to travel to " + destination + " you have 30 seconds to board before it departs.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						return true;
					}
					else
					{
						player.Out.SendMessage("I don't know the way to " + destination + " yet.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
				}
			}

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// Handles 'horse route end' events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="o"></param>
		/// <param name="args"></param>
		public void OnHorseAtPathEnd(DOLEvent e, object o, EventArgs args)
		{
			if (!(o is GameNPC)) return;
			GameNPC npc = (GameNPC)o;
			GameEventMgr.RemoveHandler(npc, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
			npc.StopMoving();
			npc.RemoveFromWorld();
		}

		/// <summary>
		/// Handles delayed player mount on horse
		/// </summary>
		protected class MountHorseAction : RegionAction
		{
			/// <summary>
			/// The target horse
			/// </summary>
			protected readonly GameNPC m_horse;

			/// <summary>
			/// Constructs a new MountHorseAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="horse">The target horse</param>
			public MountHorseAction(GamePlayer actionSource, GameNPC horse)
				: base(actionSource)
			{
				if (horse == null)
					throw new ArgumentNullException("horse");
				m_horse = horse;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				player.MountSteed(m_horse, true);
			}
		}

		/// <summary>
		/// Handles delayed horse ride actions
		/// </summary>
		protected class HorseRideAction : RegionAction
		{
			/// <summary>
			/// Constructs a new HorseStartAction
			/// </summary>
			/// <param name="actionSource"></param>
			public HorseRideAction(GameNPC actionSource)
				: base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC horse = (GameNPC)m_actionSource;
				horse.MoveOnPath(horse.MaxSpeed);
			}
		}
	}
}
