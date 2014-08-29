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
using System.Collections.Generic;

using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;

using log4net;

namespace net.freyad.keep
{
	/// <summary>
	/// Keep Data Manager is responsible of Loading Keep and Enabling Rules.
	/// It start after server startup to be sure all needed script are compiled.
	/// </summary>
	public static class KeepDataManager
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private static DOLEventHandler m_onStartEventHandler;
		private static DOLEventHandler m_onSaveEventHandler;
		private static DOLEventHandler m_onPlayerEnterEventHandler;
		
		private static Dictionary<long, IGameKeepData> m_gameKeepDict;
		private static Dictionary<int, Dictionary<ushort, long>> m_gameKeepRegionDict;
		
		/// <summary>
		/// Register Server Start and Save Handler on Script Startup.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				if (m_onStartEventHandler == null)
					m_onStartEventHandler = new DOLEventHandler(OnServerStarted);
				
				if (m_onSaveEventHandler == null)
					m_onSaveEventHandler = new DOLEventHandler(OnServerSave);
				
				GameEventMgr.AddHandler(GameServerEvent.Started, m_onStartEventHandler);
				GameEventMgr.AddHandler(GameServerEvent.WorldSave, m_onSaveEventHandler);
				
				if (log.IsInfoEnabled)
					log.InfoFormat("Keep Data Manager Started, will load Keeps on server startup...");
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while Registering Keep Data Script : {0}", ex);
				throw;
			}
			
		}
		
		/// <summary>
		/// Unload Keep Script, try any cleanup, last hope save ? -- Unregister Server Event.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptUnloadedEvent]
		public static void ScriptUnLoaded(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				GameEventMgr.RemoveHandler(GameServerEvent.Started, m_onStartEventHandler);
				GameEventMgr.RemoveHandler(GameServerEvent.WorldSave, m_onSaveEventHandler);
				if (m_onPlayerEnterEventHandler != null)
					GameEventMgr.RemoveHandler(RegionEvent.PlayerEnter, m_onPlayerEnterEventHandler);
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Errors While Cleaning Keep Data Manager : {0}", ex);
				throw;
			}
		}
		
		/// <summary>
		/// On Server Startup, Load Keep Data, Start Keep Rules.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnServerStarted(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				if (log.IsInfoEnabled)
					log.InfoFormat("Keep Data Server Startup Loading...");
				
				int count = 0;
				int componentcount = 0;
				int positiontemplatecount = 0;
				int positioncount = 0;
				
				// Load Records...
				IList<KeepData> records = GameServer.Database.SelectAllObjects<KeepData>();
				
				foreach(KeepData record in records)
				{
					KeepData currentKeep = record;
					
					try
					{
						if (!record.Enabled)
							continue;
						
						IGameKeepData keep = null;
						//We will search our assemblies for reference to classtype
						//it is not neccessary anymore to register new tables with the
						//server, it is done automatically!
						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							try
							{
								keep = (IGameKeepData)assembly.CreateInstance(currentKeep.RulesType);
								if (keep != null)
								{
									if (log.IsInfoEnabled)
										log.InfoFormat("New Keep Instanciated {0} - {1} - {2}", currentKeep.RulesType, currentKeep.KeepDataID, currentKeep.Name);
									break;
								}
							}
							catch (Exception ex)
							{
								if (log.IsDebugEnabled)
									log.DebugFormat("Could not instanciate IGameKeep {0}({1}) From Class '{2}' in assembly {3}, ex : {4}", currentKeep.KeepDataID, currentKeep.Name, currentKeep.RulesType, assembly.ToString(), ex);
								continue;
							}
						}
						
						if (keep == null)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Could not instanciate Keep ({0}) Id : {1}, Name {2}", currentKeep.RulesType, currentKeep.KeepDataID, currentKeep.Name);
							
							continue;
						}
						
						// add to general Keep Cache
						
						if (m_gameKeepDict == null)
							m_gameKeepDict = new Dictionary<long, IGameKeepData>();
						
						m_gameKeepDict.Add(currentKeep.KeepDataID, keep);
						
						// add to region / keep collection
						
						if (m_gameKeepRegionDict == null)
							m_gameKeepRegionDict = new Dictionary<int, Dictionary<ushort, long>>();
						
						if (!m_gameKeepRegionDict.ContainsKey(currentKeep.Region))
							m_gameKeepRegionDict.Add(currentKeep.Region, new Dictionary<ushort, long>());
						
						m_gameKeepRegionDict[currentKeep.Region].Add(currentKeep.KeepID, currentKeep.KeepDataID);
						
						// Keep Load ?
						keep.LoadFromDatabase(currentKeep);
						
						// Try adding Component to World before handling keep !
						bool allAddedToWorld = true;
						foreach (IGameKeepComponent component in keep.SentKeepComponents)
							if(!component.AddToWorld())
								allAddedToWorld = false;
						
						if (!allAddedToWorld && log.IsWarnEnabled)
						{
							log.WarnFormat("Not All Keep Component Added to World for {0}({1}) !", currentKeep.KeepDataID, currentKeep.Name);
						}
						
						count++;
						componentcount += keep.SentKeepComponents.Count;
						positiontemplatecount += keep.KeepObjects.Count;
						
						if (log.IsInfoEnabled)
							log.InfoFormat("Loaded Keep {0}, Components {1}, Templates {2}, Objects {3}", keep.Name, keep.SentKeepComponents.Count, currentKeep.Template != null ? currentKeep.Template.Length : 0, keep.KeepObjects.Count);
					}
					catch (Exception kex)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Could not flawlessly start Keep {2}({1}) : {0}", kex, currentKeep.Name, currentKeep.KeepDataID);
						
						continue;
					}

				}
				
				// Add Event Listener ?
				if (count > 0 && m_onPlayerEnterEventHandler == null)
				{
					m_onPlayerEnterEventHandler = new DOLEventHandler(OnPlayerEnterRegion);
					GameEventMgr.AddHandler(RegionEvent.PlayerEnter, m_onPlayerEnterEventHandler);
				}

				
				if (log.IsInfoEnabled)
					log.InfoFormat("Keep Data Startup finished, Loaded, keeps : {0}, component : {1}, template : {2}, position : {3}, New New Frontier Check : {4}", count, componentcount, positiontemplatecount, positioncount, CheckNewNewFrontierLayout());
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error While Loading Keeps Data at Server Startup : {0}", ex);
				throw;
			}
			
		}
		
		/// <summary>
		/// On Server Save, trigger objects database dump.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnServerSave(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.InfoFormat("Keep Data Server Save trigger, object saved : xx, object untouched : xx");
		}
		
		/// <summary>
		/// When player enters region, sends him keep data !
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnPlayerEnterRegion(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				Region entered = (Region)sender;
				
				if (!m_gameKeepRegionDict.ContainsKey(entered.ID))
					return;
				
				RegionPlayerEventArgs playerEntered = (RegionPlayerEventArgs)args;
				
				foreach (long dataID in m_gameKeepRegionDict[entered.ID].Values)
				{
					m_gameKeepDict[dataID].OnPlayerEnterRegion(playerEntered.Player);
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Could not send Keep Data to Player {0} Entering Region {1}, ex : {3}", ((RegionPlayerEventArgs)args).Player, ((Region)sender).ID, ex);
			}
		}
		
		/// <summary>
		/// Check if database layout is compatible with New New Frontiers
		/// </summary>
		/// <returns></returns>
		public static bool CheckNewNewFrontierLayout()
		{
			return false;
		}
	}
}
