/*
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

//Thanks to Eden (Vico) for creating this - Edited by IST
using System;
using DOL.GS;
using DOL.Events;
using System.Threading;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using DOL.AI.Brain;
using DOL.GS.Movement;
using DOL.GS.Effects;
using DOL.GS.Spells;


namespace DOL.GS.GameEvents
{
	public static class RegionTimersResynch
	{
		const int UPDATE_INTERVAL = 15 * 1000; // 15 seconds to check freeze

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		static Timer m_timer;
		public static Stopwatch watch;
		static Dictionary<GameTimer.TimeManager, long> old_time = new Dictionary<GameTimer.TimeManager, long>();

		#region Initialization/Teardown

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (ServerProperties.Properties.USE_SYNC_UTILITY)
				Init();
		}
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args) 
		{
			if (ServerProperties.Properties.USE_SYNC_UTILITY)
				Stop();
		}

		public static void Init()
		{
			watch = new Stopwatch();
			watch.Start();
			foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
				old_time.Add(mgr, 0);

			m_timer = new Timer(new TimerCallback(Resynch), null, 0, UPDATE_INTERVAL);
		}

		public static void Stop()
		{
			if (m_timer != null)
				m_timer.Dispose();
		}

		#endregion

		private static void Resynch(object nullValue)
		{
			long syncTime = watch.ElapsedMilliseconds;

			//Check alive
			foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
			{
				if (old_time.ContainsKey(mgr) && old_time[mgr] > 0 && old_time[mgr] == mgr.CurrentTime)
				{
					if (log.IsErrorEnabled)
					{
						// Tolakram: Can't do StackTrace call here.  If thread is stopping will result in UAE app stop
						log.ErrorFormat("----- Found Frozen Region Timer -----\nName: {0} - Current Time: {1}", mgr.Name, mgr.CurrentTime);
					}

					//if(mgr.Running)
					try
					{
						if (!mgr.Stop())
						{
							log.ErrorFormat("----- Failed to Stop the TimeManager: {0}", mgr.Name);
						}
					}
					catch(Exception mex)
					{
						log.ErrorFormat("----- Errors while trying to stop the TimeManager: {0}\n{1}", mgr.Name, mex);
					}

					foreach (GameClient clients in WorldMgr.GetAllClients())
					{
						if (clients.Player == null || clients.ClientState == GameClient.eClientState.Linkdead)
						{
							if(log.IsErrorEnabled)
								log.ErrorFormat("----- Disconnected Client: {0}", clients.Account.Name);
							if (clients.Player != null)
							{
								clients.Player.SaveIntoDatabase();
								clients.Player.Quit(true);
							}
							clients.Out.SendPlayerQuit(true);
							clients.Disconnect();
							GameServer.Instance.Disconnect(clients);
							WorldMgr.RemoveClient(clients);
						}
					}

					if (!mgr.Start())
					{
						log.ErrorFormat("----- Failed to (re)Start the TimeManager: {0}", mgr.Name);
					}
					
                    foreach (Region reg in WorldMgr.GetAllRegions())
					{
						if (reg.TimeManager == mgr)
						{
							foreach (GameObject obj in reg.Objects)
							{
								//Restart Player regen & remove PvP immunity
								if (obj is GamePlayer)
								{
									GamePlayer plr = obj as GamePlayer;
									if (plr.IsAlive)
									{
										plr.StopHealthRegeneration();
										plr.StopPowerRegeneration();
										plr.StopEnduranceRegeneration();
										plr.StopCurrentSpellcast();
										plr.StartHealthRegeneration();
										plr.StartPowerRegeneration();
										plr.StartEnduranceRegeneration();
										plr.StartInvulnerabilityTimer(1000, null);

                                        
										try
										{
											foreach (IGameEffect effect in plr.EffectList)
											{
												var gsp = effect as GameSpellEffect;
												if (gsp != null)
													gsp.RestartTimers();
											}
										}
										catch(Exception e)
										{
											log.Error("Can't cancel immunty effect : "+e);
										}

										
									}
									// Warn Player
									plr.Client.Out.SendMessage("["+reg.Description+"] detected as frozen, restarting the zone.", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
								}
								
								//Restart Brains & Paths
								if (obj is GameNPC && (obj as GameNPC).Brain != null)
                                {
									GameNPC npc = obj as GameNPC;
									
									if(npc.Brain is IControlledBrain)
									{
										npc.Die(null);
									}
									else if(!(npc.Brain is BlankBrain))
									{
                                        npc.Brain.Stop();
										DOL.AI.ABrain brain = npc.Brain;
                                        npc.RemoveBrain(npc.Brain);
                                        //npc.Brain.Stop();
										if (npc.MaxSpeedBase > 0 && npc.PathID != null && npc.PathID != "" && npc.PathID != "NULL")
										{
											npc.StopMovingOnPath();
											PathPoint path = MovementMgr.LoadPath(npc.PathID);
											if (path != null)
											{
												npc.CurrentWayPoint = path;
												npc.MoveOnPath((short)path.MaxSpeed);
											}
										}
                                        try
										{
											npc.SetOwnBrain(brain);
											npc.Brain.Start();
										}
										catch(Exception e)
										{
											log.Error("Can't restart Brain in RegionTimerResynch, NPC Name = "+npc.Name
                                                +" X="+npc.Position.X+"/Y="+npc.Position.Y+"/Z="+npc.Position.Z+"/R="+npc.Position.RegionID+" "+e);
											try
											{
												npc.Die(null);
											}
											catch(Exception ee)
											{
												log.Error("Can't restart Brain and Kill NPC in RegionTimerResynch, NPC Name = "+npc.Name
                                                    +" X="+npc.Position.X+"/Y="+npc.Position.Y+"/Z="+npc.Position.Z+"/R="+npc.Position.RegionID+" "+ee);
											}
										}
									}
								}
							}
							
							//Restart Respawn Timers
							List<GameNPC> respawnings = new List<GameNPC>(reg.MobsRespawning.Keys);
							foreach(GameNPC deadMob in respawnings)
							{
								GameNPC mob = deadMob;
								if(mob != null)
									mob.StartRespawn();
							}
						}					
					}
					//RegionTimerUnfrozen(mgr, syncTime);
				}

				if (old_time.ContainsKey(mgr))
					old_time[mgr] = mgr.CurrentTime;
				else
					old_time.Add(mgr, mgr.CurrentTime);
			}
		}

		public delegate void RegionTimerHandler(GameTimer.TimeManager RestartedTimer, long SyncTime);
		//public static event RegionTimerHandler RegionTimerUnfrozen;
	}
}
