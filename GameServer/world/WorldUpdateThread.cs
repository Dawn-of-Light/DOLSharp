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
using System.Threading;
using System.Threading.Tasks;

using System.Linq;
using System.Collections.Generic;

using log4net;

using DOL.GS.Housing;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Geometry;

namespace DOL.GS
{
	/// <summary>
	/// Description of WorldUpdateThread.
	/// </summary>
	public static class WorldUpdateThread
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Minimum Player Update Loop Refresh Rate. (ms)
		/// </summary>
		private static readonly uint MIN_PLAYER_WORLD_UPDATE_RATE = 50;
		
		/// <summary>
		/// Minimum NPC Update Loop Refresh Rate. (ms)
		/// </summary>
		private static readonly uint MIN_NPC_UPDATE_RATE = 1000;
		
		/// <summary>
		/// Minimum Static Item Update Loop Refresh Rate. (ms)
		/// </summary>
		private static readonly uint MIN_ITEM_UPDATE_RATE = 10000;

		/// <summary>
		/// Minimum Housing Update Loop Refresh Rate. (ms)
		/// </summary>
		private static readonly uint MIN_HOUSING_UPDATE_RATE = 10000;
		
		/// <summary>
		/// Minimum Player Position Update Loop Refresh Rate. (ms)
		/// </summary>
		private static readonly uint MIN_PLAYER_UPDATE_RATE = 1000;
		
		/// <summary>
		/// Get the Player World Update Refresh Rate.
		/// </summary>
		/// <returns></returns>
		private static uint GetPlayerWorldUpdateInterval
		{
			get { return Math.Max(ServerProperties.Properties.WORLD_PLAYER_UPDATE_INTERVAL, MIN_PLAYER_WORLD_UPDATE_RATE); }
		}
		
		/// <summary>
		/// Get Player NPC Refresh Rate.
		/// </summary>
		/// <returns></returns>
		private static uint GetPlayerNPCUpdateInterval
		{
			get { return Math.Max(ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL, MIN_NPC_UPDATE_RATE); }
		}
		
		/// <summary>
		/// Get Player Static Item Refresh Rate.
		/// </summary>
		/// <returns></returns>
		private static uint GetPlayerItemUpdateInterval
		{
			get { return Math.Max(ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL, MIN_ITEM_UPDATE_RATE); }
		}
		
		/// <summary>
		/// Get Player Housing Item Refresh Rate.
		/// </summary>
		/// <returns></returns>
		private static uint GetPlayerHousingUpdateInterval
		{
			get { return Math.Max(ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL, MIN_HOUSING_UPDATE_RATE); }
		}
		
		/// <summary>
		/// Get Player to Other Player Update Rate
		/// </summary>
		/// <returns></returns>
		private static uint GetPlayertoPlayerUpdateInterval
		{
			get { return Math.Max(ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL, MIN_PLAYER_UPDATE_RATE); }
		}
		
		/// <summary>
		/// Update all World Around Player
		/// </summary>
		/// <param name="player">The player needing update</param>
		private static void UpdatePlayerWorld(GamePlayer player)
		{
			UpdatePlayerWorld(player, GameTimer.GetTickCount());
		}
		
		/// <summary>
		/// Update all World Around Player
		/// </summary>
		/// <param name="player">The player needing update</param>
		/// <param name="nowTicks">The actual time of the refresh.</param>
		private static void UpdatePlayerWorld(GamePlayer player, long nowTicks)
		{
			// Update Player Player's
			if (ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL > 0)
				UpdatePlayerOtherPlayers(player, nowTicks);
			
			// Update Player Mob's
			if (ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL > 0)
				UpdatePlayerNPCs(player, nowTicks);

			// Update Player Static Item
			if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
				UpdatePlayerItems(player, nowTicks);
			
			// Update Player Doors
			if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
				UpdatePlayerDoors(player, nowTicks);
			
			// Update Player Housing
			if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
				UpdatePlayerHousing(player, nowTicks);
		}

		private static void UpdatePlayerOtherPlayers(GamePlayer player, long nowTicks)
		{
			// Get All Player in Range
			var players = player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE).Cast<GamePlayer>().Where(p => p != null && p.IsVisibleTo(player) && (!p.IsStealthed || player.CanDetect(p))).ToArray();

			try
			{
				// Clean Cache
				foreach (var objEntry in player.Client.GameObjectUpdateArray)
				{
					var objKey = objEntry.Key;
					GameObject obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
					// We have a Player in cache that is not in vincinity
					// For updating "out of view" we allow a halved refresh time. 
					if (obj is GamePlayer && !players.Contains((GamePlayer)obj) && (nowTicks - objEntry.Value) >= GetPlayertoPlayerUpdateInterval)
					{
						long dummy;
						
						// Update him out of View and delete from cache
						if (obj.IsVisibleTo(player) && (((GamePlayer)obj).IsStealthed == false || player.CanDetect((GamePlayer)obj)))
							player.Client.Out.SendPlayerForgedPosition((GamePlayer)obj);
						
						player.Client.GameObjectUpdateArray.TryRemove(objKey, out dummy);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Cleaning OtherPlayers cache for Player : {0}, Exception : {1}", player.Name, e);
			}		
			
			try
			{
				// Now Send Remaining Players.
				foreach (GamePlayer lplayer in players)
				{
					GamePlayer otherply = lplayer;
					
					if (otherply != null)
					{						
						// Get last update time
						long lastUpdate;
						if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(otherply.CurrentRegionID, (ushort)otherply.ObjectID), out lastUpdate))
						{
							// This Player Needs Update
							if ((nowTicks - lastUpdate) >= GetPlayertoPlayerUpdateInterval)
							{
								player.Client.Out.SendPlayerForgedPosition(otherply);
							}
						}
						else
						{
							player.Client.Out.SendPlayerForgedPosition(otherply);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while updating OtherPlayers for Player : {0}, Exception : {1}", player.Name, e);
			}
		}
		
		/// <summary>
		/// Send Mobs Update to Player depending on last refresh time.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="nowTicks"></param>
		private static void UpdatePlayerNPCs(GamePlayer player, long nowTicks)
		{
			// Get All Mobs in Range
			var npcs = player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE).Cast<GameNPC>().Where(n => n != null && n.IsVisibleTo(player)).ToArray();

			try
			{
				// Clean Cache
				foreach (var objEntry in player.Client.GameObjectUpdateArray)
				{
					var objKey = objEntry.Key;
					GameObject obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
					
					// Brain is updating to its master, no need to handle it.
					if (obj is GameNPC && ((GameNPC)obj).Brain is IControlledBrain && ((IControlledBrain)((GameNPC)obj).Brain).GetPlayerOwner() == player)
						continue;
					
					// We have a NPC in cache that is not in vincinity
					if (obj is GameNPC && !npcs.Contains((GameNPC)obj) && (nowTicks - objEntry.Value) >= GetPlayerNPCUpdateInterval)
					{
						// Update him out of View
						if (obj.IsVisibleTo(player))
							player.Client.Out.SendObjectUpdate(obj);
						
						long dummy;
						// this will add the object to the cache again, remove it after sending...
						player.Client.GameObjectUpdateArray.TryRemove(objKey, out dummy);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Cleaning NPC cache for Player : {0}, Exception : {1}", player.Name, e);
			}
			
			try
			{
				// Now Send remaining npcs
				foreach (GameNPC lnpc in npcs)
				{
					GameNPC npc = lnpc;
										
					// Get last update time
					long lastUpdate;
					if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(npc.CurrentRegionID, (ushort)npc.ObjectID), out lastUpdate))
					{
						// This NPC Needs Update
						if ((nowTicks - lastUpdate) >= GetPlayerNPCUpdateInterval)
						{
							player.Client.Out.SendObjectUpdate(npc);
						}
					}
					else
					{
						// Not in cache, Object entering in range, sending update will add it to cache.
						player.Client.Out.SendObjectUpdate(npc);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while updating NPC for Player : {0}, Exception : {1}", player.Name, e);
			}
		}
		
		/// <summary>
		/// Send Game Static Item depending on last refresh time
		/// </summary>
		/// <param name="player"></param>
		/// <param name="nowTicks"></param>
		private static void UpdatePlayerItems(GamePlayer player, long nowTicks)
		{
			// Get All Static Item in Range
			var objs = player.GetItemsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE).Cast<GameStaticItem>().Where(i => i != null && i.IsVisibleTo(player)).ToArray();

			try
			{
				// Clean Cache
				foreach (var objEntry in player.Client.GameObjectUpdateArray)
				{
					var objKey = objEntry.Key;
					GameObject obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
					// We have a Static Item in cache that is not in vincinity
					if (obj is GameStaticItem && !objs.Contains((GameStaticItem)obj) && (nowTicks - objEntry.Value) >= GetPlayerItemUpdateInterval)
					{
						long dummy;
						player.Client.GameObjectUpdateArray.TryRemove(objKey, out dummy);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Cleaning Static Item cache for Player : {0}, Exception : {1}", player.Name, e);
			}
			
			try
			{
				// Now Send remaining objects
				foreach (GameStaticItem lobj in objs)
				{
					GameStaticItem staticObj = lobj;
					// Get last update time
					long lastUpdate;
					if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(staticObj.CurrentRegionID, (ushort)staticObj.ObjectID), out lastUpdate))
					{
						// This Static Object Needs Update
						if ((nowTicks - lastUpdate) >= GetPlayerItemUpdateInterval)
						{
							player.Client.Out.SendObjectCreate(staticObj);
						}
					}
					else
					{
						// Not in cache, Object entering in range, sending update will add it to cache
						player.Client.Out.SendObjectCreate(staticObj);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while updating Static Item for Player : {0}, Exception : {1}", player.Name, e);
			}
		}
		
		/// <summary>
		/// Send Game Doors Depening on last refresh time.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="nowTicks"></param>
		private static void UpdatePlayerDoors(GamePlayer player, long nowTicks)
		{
			// Get All Game Doors in Range
			var doors = player.GetDoorsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE).Cast<IDoor>().OfType<GameObject>().Where(o => o.IsVisibleTo(player)).ToArray();
			
			try
			{
				// Clean Cache
				foreach (var objEntry in player.Client.GameObjectUpdateArray)
				{
					var objKey = objEntry.Key;
					GameObject obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
					// We have a Door in cache that is not in vincinity
					if (obj is IDoor && !doors.Contains(obj) && (nowTicks - objEntry.Value) >= GetPlayerItemUpdateInterval)
					{
						long dummy;
						player.Client.GameObjectUpdateArray.TryRemove(objKey, out dummy);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Cleaning Doors cache for Player : {0}, Exception : {1}", player.Name, e);
			}
			
			try
			{
				// Now Send remaining doors
				foreach (IDoor ldoor in doors)
				{
					IDoor door = ldoor;
					
					// Get last update time
					long lastUpdate;
					if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(((GameObject)door).CurrentRegionID, (ushort)door.ObjectID), out lastUpdate))
					{
						// This Door Needs Update
						if ((nowTicks - lastUpdate) >= GetPlayerItemUpdateInterval)
						{
							player.SendDoorUpdate(door);
						}
					}
					else
					{
						// Not in cache, Door entering in range, sending update will add it to cache.
						player.SendDoorUpdate(door);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while updating Doors for Player : {0}, Exception : {1}", player.Name, e);
			}
		}
		
		/// <summary>
		/// Send Housing Items depending on last refresh.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="nowTicks"></param>
		public static void UpdatePlayerHousing(GamePlayer player, long nowTicks)
		{
			// If no house update needed exit.
			if (player.CurrentRegion == null || !player.CurrentRegion.HousingEnabled)
				return;
			
			// Get All House in Region
			IDictionary<int, House> housesDict = HouseMgr.GetHouses(player.CurrentRegionID);
			// Build Vincinity List
			var houses = housesDict.Values
                .Where(h => h != null && player.Coordinate.DistanceTo(h.Position) <= HousingConstants.HouseViewingDistance)
                .ToArray();
			
			try
			{
				// Clean Cache
				foreach (var houseEntry in player.Client.HouseUpdateArray)
				{
					var houseKey = houseEntry.Key;
					House house = HouseMgr.GetHouse(houseKey.Item1, houseKey.Item2);
					
					// We have a House in cache that is not in vincinity
					if (!houses.Contains(house) && (nowTicks - houseEntry.Value) >= (GetPlayerHousingUpdateInterval >> 2))
					{
						long dummy;
						player.Client.HouseUpdateArray.TryRemove(houseKey, out dummy);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Cleaning House cache for Player : {0}, Exception : {1}", player.Name, e);
			}
			
			try
			{
				// Now Send remaining houses
				foreach (House lhouse in houses)
				{
					House house = lhouse;
					
					// Get last update time
					long lastUpdate;
					if (player.Client.HouseUpdateArray.TryGetValue(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber), out lastUpdate))
					{
						// This House Needs Update
						if ((nowTicks - lastUpdate) >= GetPlayerHousingUpdateInterval)
						{
							player.Client.Out.SendHouseOccupied(house, house.IsOccupied);							
						}
					}
					else
					{
						// Not in cache, House entering in range !
						player.Client.Out.SendHouse(house);
						player.Client.Out.SendGarden(house);
						player.Client.Out.SendHouseOccupied(house, house.IsOccupied);
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while updating Houses for Player : {0}, Exception : {1}", player.Name, e);
			}
		}
		
		/// <summary>
		/// Check if this player can be updated
		/// </summary>
		/// <param name="lastUpdate"></param>
		/// <returns></returns>
		private static bool PlayerNeedUpdate(long lastUpdate)
		{
			return (GameTimer.GetTickCount() - lastUpdate) >= GetPlayerWorldUpdateInterval;
		}
		
		private static bool StartPlayerUpdateTask(GameClient client, IDictionary<GameClient, Tuple<long, Task, Region>> clientsUpdateTasks, long begin)
		{
			var player = client.Player;
			// Check for existing Task
			Tuple<long, Task, Region> clientEntry;
			
			if(!clientsUpdateTasks.TryGetValue(client, out clientEntry))
			{
				// Client not in tasks, create it and run it !
				clientEntry = new Tuple<long, Task, Region>(begin, Task.Factory.StartNew(() => UpdatePlayerWorld(player)), player.CurrentRegion);
				
				// Register.
				clientsUpdateTasks.Add(client, clientEntry);
				return true;
			}
			else
			{
				// Get client entry data.
				long lastUpdate = clientEntry.Item1;
				Task taskEntry = clientEntry.Item2;
				Region lastRegion = clientEntry.Item3;
				
				//Check if task finished
				if (!taskEntry.IsCompleted)
				{
					// Check for how long
					if ((begin - lastUpdate) > GetPlayerWorldUpdateInterval)
					{
						if (log.IsWarnEnabled && (GameTimer.GetTickCount() - player.TempProperties.getProperty<long>("LAST_WORLD_UPDATE_THREAD_WARNING", 0) >= 1000))
						{
							log.WarnFormat("Player Update Task ({0}) Taking more than world update refresh rate : {1} ms (real {2} ms) - Task Status : {3}!", player.Name, GetPlayerWorldUpdateInterval, begin - lastUpdate, taskEntry.Status);
							player.TempProperties.setProperty("LAST_WORLD_UPDATE_THREAD_WARNING", GameTimer.GetTickCount());
						}
					}
					// Don't init this client.
					return false;
				}
				
				// Display Exception
				if (taskEntry.IsFaulted)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Error in World Update Thread, Player Task ({0})! Exception : {1}", player.Name, taskEntry.Exception);
				}
				
				// Region Refresh
				if (player.CurrentRegion != lastRegion)
				{
					lastUpdate = 0;
					lastRegion = player.CurrentRegion;
					client.GameObjectUpdateArray.Clear();
					client.HouseUpdateArray.Clear();
				}
				
				// If this player need update.
				if (PlayerNeedUpdate(lastUpdate))
				{
					// Update Time, Region and Create Task
					var newClientEntry = new Tuple<long, Task, Region>(begin, Task.Factory.StartNew(() => UpdatePlayerWorld(player)), lastRegion);
					// Register Tuple
					clientsUpdateTasks[client] = newClientEntry;
					return true;
				}
				
			}
			
			return false;
		}
		
		private static bool IsTaskCompleted(GameClient client, IDictionary<GameClient, Tuple<long, Task, Region>> clientsUpdateTasks)
		{
			Tuple<long, Task, Region> clientEntry;
			
			// Check for existing Task
			if(clientsUpdateTasks.TryGetValue(client, out clientEntry))
			{
				Task taskEntry = clientEntry.Item2;
				
				if (taskEntry != null)
					return taskEntry.IsCompleted;
			}
			
			return true;
		}
		
		/// <summary>
		/// This thread updates the NPCs and objects around the player at very short
		/// intervalls! But since the update is very quick the thread will
		/// sleep most of the time!
		/// </summary>
		public static void WorldUpdateThreadStart()
		{
			// Tasks Collection of running Player updates, with starting time.
			var clientsUpdateTasks = new Dictionary<GameClient, Tuple<long, Task, Region>>();
			
			bool running = true;
			
			if (log.IsInfoEnabled)
			{
				log.InfoFormat("World Update Thread Starting - ThreadId = {0}", Thread.CurrentThread.ManagedThreadId);
			}
			
			while (running)
			{
				try
				{
					// Start Time of the loop
					long begin = GameTimer.GetTickCount();
					
					// Get All Clients
					var clients = WorldMgr.GetAllClients();
					
					// Clean Tasks Dict on Client Exiting.
					foreach(GameClient cli in clientsUpdateTasks.Keys.ToArray())
					{
						if (cli == null)
							continue;
						
						GamePlayer player = cli.Player;
						
						bool notActive = cli.ClientState != GameClient.eClientState.Playing || player == null || player.ObjectState != GameObject.eObjectState.Active;
						bool notConnected = !clients.Contains(cli);
						
						if (notConnected || (notActive && IsTaskCompleted(cli, clientsUpdateTasks)))
						{
							clientsUpdateTasks.Remove(cli);
							cli.GameObjectUpdateArray.Clear();
							cli.HouseUpdateArray.Clear();
						}
					}
					
					// Browse all clients to check if they can be updated.
					for (int cl = 0; cl < clients.Count; cl++)
					{
						GameClient client = clients[cl];
						
						// Check that client is healthy
						if (client == null)
							continue;

						GamePlayer player = client.Player;
						
						if (client.ClientState == GameClient.eClientState.Playing && player == null)
						{
							if (log.IsErrorEnabled)
								log.Error("account has no active player but is playing, disconnecting! => " + client.Account.Name);
							
							// Disconnect buggy Client
							GameServer.Instance.Disconnect(client);
							continue;
						}
						
						// Check that player is active.
						if (client.ClientState != GameClient.eClientState.Playing || player == null || player.ObjectState != GameObject.eObjectState.Active)
							continue;
						
						// Start Update Task
						StartPlayerUpdateTask(client, clientsUpdateTasks, begin);
					}
					
					long took = GameTimer.GetTickCount() - begin;
					
					if (took >= 500)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("World Update Thread (NPC/Object update) took {0} ms", took);
					}

					// relaunch update thread every 100 ms to check if any player need updates.
					Thread.Sleep((int)Math.Max(1, 100 - took));
				}
				catch (ThreadInterruptedException)
				{
					if (log.IsInfoEnabled)
						log.Info("World Update Thread stopping...");
					
					running = false;
					break;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error in World Update (NPC/Object Update) Thread!", e);
				}
			}
		}
	}
}
