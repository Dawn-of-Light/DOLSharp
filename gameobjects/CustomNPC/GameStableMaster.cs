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
using DOL.Language;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Stable master that sells and takes horse route tickes
	/// </summary>
	[NPCGuildScript("Stable Master", eRealm.None)]
	public class GameStableMaster : GameMerchant
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new stable master
		/// </summary>
		public GameStableMaster()
		{
		}

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
					String destination = item.Name.Substring(10);
					//					String destination = item.Name.Substring(item.Name.IndexOf(" to "));
					//PathPoint path = MovementMgr.Instance.LoadPath(this.Name+"=>"+destination);
					PathPoint path = MovementMgr.LoadPath(item.Id_nb);

					if ((path != null) && ((Math.Abs(path.X - this.X)) < 500) && ((Math.Abs(path.Y - this.Y)) < 500))
					{
						player.Inventory.RemoveCountFromStack(item, 1);

						GameHorse horse = new GameHorse();
						eRace user_race = (eRace)player.Race;
						foreach (GameNPC npc in GetNPCsInRadius(400))
						{ // Allow for SI mounts -Echostorm
							if (npc.Name == LanguageMgr.GetTranslation(player.Client, "GameStableMaster.HorseName")
								|| npc.Name == "Dragon Fly" || npc.Name == "Ampheretere" || npc.Name == "Gryphon")
							{
								horse.Model = npc.Model;
								horse.Size = npc.Size;
								horse.Name = npc.Name;
								horse.Level = npc.Level;
								//horse.Realm = npc.Realm;
								break;
							}
						}
						switch (user_race)
						{
							case eRace.Briton: horse.Size = 50; //Briton
								break;
							case eRace.Avalonian: horse.Size = 55; //Avalonian
								break;
							case eRace.Highlander: horse.Size = 55; //Highlander
								break;
							case eRace.Saracen: horse.Size = 50; //Saracen
								break;
							case eRace.Norseman: horse.Size = 55; //Norseman
								break;
							case eRace.Troll: horse.Size = 67; //Troll
								break;
							case eRace.Dwarf: horse.Size = 42; //Dwarf
								break;
							case 8: horse.Size = 38; //Kobold
								break;
							case 9: horse.Size = 50; //Celt
								break;
							case 10: horse.Size = 50; //Firbolg
								break;
							case 11: horse.Size = 55; //Elf
								break;
							case 12: horse.Size = 31; //Lurikeen
								break;
							case 13: horse.Size = 45; //Inconnu
								break;
							case 14: horse.Size = 52; //Valkyn
								break;
							case 15: horse.Size = 55; //Sylvan
								break;
							case 16: horse.Size = 65; //HalfOgre
								break;
							case 17: horse.Size = 48; //Frostalf
								break;
							case 18: horse.Size = 48; //Shar
								break;
							case 19: horse.Size = 65; //AlbionMinotaur
								break;
							case 20: horse.Size = 65; //MidgardMinotaur
								break;
							case 21: horse.Size = 65; //HiberniaMinotaur
								break;

						}

						horse.Realm = source.Realm;
						horse.X = path.X;
						horse.Y = path.Y;
						horse.Z = path.Z;
						horse.CurrentRegion = CurrentRegion;
						horse.Heading = Point2D.GetHeadingToLocation(path, path.Next);
						horse.AddToWorld();
						horse.CurrentWayPoint = path;
						GameEventMgr.AddHandler(horse, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
						new MountHorseAction(player, horse).Start(400);
						new HorseRideAction(horse).Start(4000);
						return true;
					}
					else
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameStableMaster.UnknownWay", destination), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}

			return base.ReceiveItem(source, item);
		}
		private void SendReply(GamePlayer target, string msg)
		{
			target.Out.SendMessage(
				msg,
				eChatType.CT_System, eChatLoc.CL_PopupWindow);
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